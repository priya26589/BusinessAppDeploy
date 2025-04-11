using Business.Data;
using Business.Dto;
using Business.Models;
using Business.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Business.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly BusinessContext _context;
        public ILogger<CustomerController> _logger;
        private readonly IConfiguration _configuration;

        public CustomerController(ILogger<CustomerController> logger, BusinessContext context, HttpClient httpClient, IConfiguration configuration, GeocodingService geocodingService)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<bool>> Registercustomer(CustomerDto customerDto)
        {
            // Validate customer data
            if (customerDto == null || string.IsNullOrEmpty(customerDto.Cus_EmailId) || string.IsNullOrEmpty(customerDto.Cus_Password))
            {
                return BadRequest(new { message = "Invalid data. Please check the input." }); // HTTP 400 Bad Request
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(customerDto.Cus_Password);

            try
            {
                // Add new customer
                var customerObj = new Customer
                {
                    Cus_EmailId = customerDto.Cus_EmailId,
                    Cus_Password = hashedPassword,
                    Cus_Location = customerDto.Cus_Location,
                    Longitude = customerDto.Longitude,
                    Latitude = customerDto.Latitude,
                    RoleID = 4,
                    PinCode = customerDto.pinCode
                };
                _context.Customers.Add(customerObj);
                await _context.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("getcusdetailsbyid")]
        public async Task<ActionResult> getCustomerDetailByID(int cusId)
        {
            var customerData = await _context.Customers.Where(u => u.Cus_Id == cusId).ToListAsync();
            return Ok(customerData);
        }

        [HttpPut("updatecustomerdetails")]
        public async Task<ActionResult<bool>> UpdateCustomer([FromForm] CustomerDto customerDto)
        {
            if (customerDto == null || string.IsNullOrEmpty(customerDto.Cus_EmailId))
            {
                return BadRequest(new { message = "Invalid data. Please check the input." }); // HTTP 400 Bad Request
            }

            try
            {
                var existingCustomer = await _context.Customers.FindAsync(customerDto.Cus_Id);
                if (existingCustomer == null)
                {
                    return NotFound(new { message = "Customer not found." }); // HTTP 404 Not Found
                }

                // Update customer details
                existingCustomer.Cus_EmailId = customerDto.Cus_EmailId;
                existingCustomer.Cus_Location = customerDto.Cus_Location;
                existingCustomer.Longitude = customerDto.Longitude;
                existingCustomer.Latitude = customerDto.Latitude;

                // Update password only if provided
                if (!string.IsNullOrEmpty(customerDto.Cus_Password))
                {
                    existingCustomer.Cus_Password = BCrypt.Net.BCrypt.HashPassword(customerDto.Cus_Password);
                }

                _context.Customers.Update(existingCustomer);
                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExistsBusiness(string email)
        {
            //bool exists = await _context.Businesses.AnyAsync(u => u.EmailId == email);
            //return Ok(exists);
            bool exists = await _context.Businesses.AnyAsync(b => b.EmailId == email) ||
                  await _context.Customers.AnyAsync(c => c.Cus_EmailId == email);

            return Ok(exists);
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            var usercustomer = _context.Customers.FirstOrDefault(u => u.Cus_EmailId == loginRequest.Username && u.Cus_Password == loginRequest.Password);
            if (usercustomer == null)
                return Unauthorized();

            var token = GenerateToken(loginRequest.Username, loginRequest.RememberMe);
            return Ok(new LoginResponse
            {
                Token = token.Token,
                Expiration = token.Expiration
            });
        }
        private (string Token, DateTime Expiration) GenerateToken(string email, bool rememberMe)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = rememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(1); // Longer expiration for Remember Me

            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: new[] { new Claim(ClaimTypes.Email, email) },
            expires: expiration,
            signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }
    }
}
