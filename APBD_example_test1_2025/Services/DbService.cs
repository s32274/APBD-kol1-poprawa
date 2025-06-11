using APBD_example_test1_2025.Exceptions;
using APBD_example_test1_2025.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD_example_test1_2025.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    public async Task<ProjectDto> GetProjectByIdAsync(int id)
    {
        var query =
            @"SELECT pres.ProjectId,
                     pres.Objective,
                     pres.StartDate,
                     pres.EndDate,
                     art.Name,
                     art.OriginDate,
                     inst.InstitutionId,
                     inst.Name,
                     inst.FoundedYear,
                     st.FirstName,
                     st.LastName,
                     st.HireDate,
                     sta.Role
            FROM Preservation_Project pres
            JOIN Artifact art ON pres.ArtifactId = art.ArtifactId
            JOIN Institution inst ON art.InstitutionId = inst.InstitutionId
            JOIN Staff_Assignment sta ON pres.ProjectId = sta.ProjectId
            JOIN Staff st ON st.StaffId = sta.StaffId
            WHERE pres.ProjectId = @id
            ";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@id", id);
        var reader = await command.ExecuteReaderAsync();
        
        ProjectDto? projectDto = null;
        
        while (await reader.ReadAsync())
        {
            // Create DTO
            if (projectDto is null)
            {
                projectDto = new ProjectDto()
                {
                    ProjectId = reader.GetInt32(0),
                    Objective = reader.GetString(1),
                    StartDate = reader.GetDateTime(2),
                    EndDate =  await reader.IsDBNullAsync(3) ? null : reader.GetDateTime(3),
                    Artifact = new ArtifactDto()
                    {
                        Name = reader.GetString(4),
                        OriginDate = reader.GetDateTime(5),
                        Institution = new InstitutionDto()
                        {
                            InstitutionId = reader.GetInt32(6),
                            Name = reader.GetString(7),
                            FoundedYear = reader.GetInt32(8)
                        }
                    },
                    StaffAssignments = new List<StaffAssignmentDto>()
                };
                
            }
            
            projectDto.StaffAssignments.Add(new StaffAssignmentDto()
            {
                FirstName = reader.GetString(9),
                LastName = reader.GetString(10),
                HireDate = reader.GetDateTime(11),
                Role = reader.GetString(12)
            });
        }
        
        if (projectDto is null)
        {
            throw new NotFoundException($"Project with id {id} was not found.");
        }
        
        return projectDto;
    }

    public async Task AddArtifactAsync(CreateArtifactWithProjectDto newArtifactProject)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
    
        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Artifact WHERE ArtifactId = @ArtifactId";
            command.Parameters.AddWithValue("ArtifactId", newArtifactProject.Artifact.ArtifactId);
            var artifactExists = await command.ExecuteScalarAsync();
            if (artifactExists is not null)
                throw new ConflictException($"Artifact with id {newArtifactProject.Artifact.ArtifactId} already exists");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Preservation_Project WHERE ProjectId = @ProjectId";
            command.Parameters.AddWithValue("ProjectId", newArtifactProject.Project.ProjectId);
            var projectExists = await command.ExecuteScalarAsync();
            if (projectExists is not null)
                throw new ConflictException($"Project with id {newArtifactProject.Project.ProjectId} already exists");
            
            // czy istnieje dana instytucja, do której przypisze się nowy artefakt? sprawdzenie
            command.Parameters.Clear();
            command.CommandText = "SELECT InstitutionId FROM Institution WHERE InstitutionId = @InstitutionId";
            command.Parameters.AddWithValue("InstitutionId", newArtifactProject.Artifact.InstitutionId);
            
            Console.WriteLine("Institution Id: " + newArtifactProject.Artifact.InstitutionId);
            Console.WriteLine("Artifact Id: " + newArtifactProject.Artifact.ArtifactId);
            Console.WriteLine("Artifact Name: " + newArtifactProject.Artifact.Name);
            var instituionExists = await command.ExecuteScalarAsync();
            if (instituionExists is null)
                throw new NotFoundException($"Institution with id {newArtifactProject.Artifact.InstitutionId} wasn't found");
            
            
            
            await command.ExecuteNonQueryAsync();
            
            command.Parameters.Clear();
            command.CommandText = @"INSERT INTO Artifact(ArtifactId, Name, OriginDate, InstitutionId)
              VALUES(@ArtifactId, @Name, @OriginDate, @InstitutionId);";
    
            command.Parameters.AddWithValue("@ArtifactId", newArtifactProject.Artifact.ArtifactId);
            command.Parameters.AddWithValue("@Name", newArtifactProject.Artifact.Name);
            command.Parameters.AddWithValue("@OriginDate", newArtifactProject.Artifact.OriginDate);
            command.Parameters.AddWithValue("@InstitutionId", newArtifactProject.Artifact.InstitutionId);
            await command.ExecuteNonQueryAsync();
        
            command.Parameters.Clear();
            command.CommandText = @"INSERT INTO Preservation_Project(ProjectId, ArtifactId, StartDate, EndDate, Objective)
              VALUES(@ProjectId, @ArtifactId, @StartDate, @EndDate, @Objective);";
    
            command.Parameters.AddWithValue("@ProjectId", newArtifactProject.Project.ProjectId);
            command.Parameters.AddWithValue("@ArtifactId", newArtifactProject.Artifact.ArtifactId);
            command.Parameters.AddWithValue("@StartDate", newArtifactProject.Project.StartDate);
            command.Parameters.AddWithValue("@Objective", newArtifactProject.Project.Objective);
            if (newArtifactProject.Project.EndDate is not null)
            {
                command.Parameters.AddWithValue("@EndDate", newArtifactProject.Project.EndDate);
            }
            else
            {
                command.Parameters.AddWithValue("@EndDate", DBNull.Value);
            }
            
            
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}