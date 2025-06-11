using APBD_example_test1_2025.Exceptions;
using APBD_example_test1_2025.Models.DTOs;
using APBD_example_test1_2025.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_example_test1_2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IDbService _dbService;
        public ProjectsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectByIdAsync(int id)
        {
            try
            {
                var project = await _dbService.GetProjectByIdAsync(id);
                return Ok(project);
            }
            catch (NotFoundException)
            {
                return NotFound($"Project with id {id} was not found");
            }
        }

        
    }
}
