using APBD_example_test1_2025.Exceptions;
using APBD_example_test1_2025.Models.DTOs;
using APBD_example_test1_2025.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_example_test1_2025.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ArtifactsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public ArtifactsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        
        public async Task<IActionResult> AddArtifactAsync(CreateArtifactWithProjectDto newArtifactProject)
        {
            try
            {
                await _dbService.AddArtifactAsync(newArtifactProject);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }

            return Created(Request.Path.Value ?? "api/artifacts", newArtifactProject);
        }
    }
}