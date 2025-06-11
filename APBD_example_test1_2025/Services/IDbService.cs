using APBD_example_test1_2025.Models.DTOs;

namespace APBD_example_test1_2025.Services;

public interface IDbService
{
    public Task<ProjectDto> GetProjectByIdAsync(int id);
    public Task AddArtifactAsync(CreateArtifactWithProjectDto newArtifactProject);
}
