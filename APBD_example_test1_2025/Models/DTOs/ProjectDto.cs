namespace APBD_example_test1_2025.Models.DTOs;

public class ProjectDto
{
    public int ProjectId { get; set; }
    public string Objective { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ArtifactDto Artifact { get; set; } = null!;
    public List<StaffAssignmentDto> StaffAssignments { get; set; } = new List<StaffAssignmentDto>();
}

public class ArtifactDto
{
    public string Name { get; set; } = null!;
    public DateTime OriginDate { get; set; }
    public InstitutionDto Institution { get; set; } = null!;
}

public class InstitutionDto
{
    public int InstitutionId { get; set; }
    public string Name { get; set; } = null!;
    public int FoundedYear { get; set; }
}

public class StaffAssignmentDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime HireDate { get; set; }
    public string Role { get; set; } = null!;
}