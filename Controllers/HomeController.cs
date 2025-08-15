using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.Models;
using MyApp.Persistence;

namespace MyApp.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly AppDbContext _context;
    public HomeController(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }

        [HttpGet("home-info")]
        public async Task<ActionResult<IEnumerable<Home>>> GetAll()
        {
            var sections = await _context.Home
                .Where(h => h.IsActive)
                .OrderBy(h => h.DisplayOrder)
                .ToListAsync();
            if (!sections.Any())
            {
            return NotFound("nothing");
        }
            return Ok(sections);
            
    }
}

}