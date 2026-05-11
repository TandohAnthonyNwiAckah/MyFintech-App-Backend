using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TANA.Data;
using TANA.Models;

namespace TANA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AppDBContext _context;

        public TestController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/test
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "API is working!", status = 200 });
        }

        // GET: api/test/db
        [HttpGet("db")]
        public IActionResult TestDb()
        {
            try
            {
                // simple query to test DB
                var tables = _context.Database.GetDbConnection().GetSchema("Tables");
                return Ok(new { message = "Database connected!", tables = tables.Rows.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Database connection failed", error = ex.Message }
                );
            }
        }
    }
}
