using Business.Data;
using Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Business.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessRatingController : ControllerBase
    {
        private readonly BusinessContext _context;

        public BusinessRatingController(BusinessContext context)
        {
            _context = context;
        }

        // POST: api/BusinessRatings/Add
        [HttpPost("Add")]
        public async Task<IActionResult> AddBusinessRating([FromBody] BusinessRatings rating)
        {
            if (rating == null)
            {
                return BadRequest("Invalid rating data.");
            }

            // Check if the BusinessID exists
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessID == rating.BusinessID);
            if (!businessExists)
            {
                return NotFound("Business not found.");
            }
            var alreadyRated = await _context.BusinessRatings.AnyAsync(br => br.BusinessID == rating.BusinessID && br.RatedBy == rating.RatedBy);
            if (alreadyRated)
            {
                return BadRequest("Already have submitted rating for the business, Thank You!");
            }
            rating.DateTime = DateTime.UtcNow; // Set current timestamp

            _context.BusinessRatings.Add(rating);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/BusinessRatings/{id}
        [HttpGet("{businessId}")]
        public async Task<IActionResult> GetBusinessRating(int businessId)
        {
            var ratings = await _context.BusinessRatings
         .Where(r => r.BusinessID == businessId)
         .ToListAsync();

            if (!ratings.Any()) // Check if there are no ratings
            {
                return Ok(new { message = "No ratings found for this business.." });
            }

            return Ok(ratings);
        }
    }
}
