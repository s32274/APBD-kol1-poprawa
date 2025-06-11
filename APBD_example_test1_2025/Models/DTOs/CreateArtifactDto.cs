namespace APBD_example_test1_2025.Models.DTOs;

public class CreateArtifactWithProjectDto
{
    public CreateArtifactDto Artifact { get; set; } = null!;
    public CreateProjectDto Project { get; set; } = null!;
}

public class CreateArtifactDto
{
    public int ArtifactId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime OriginDate { get; set; }
    public int InstitutionId { get; set; }
}

public class CreateProjectDto
{
    public int ProjectId { get; set; }
    public string Objective { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}