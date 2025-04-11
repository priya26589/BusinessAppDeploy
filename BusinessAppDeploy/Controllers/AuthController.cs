using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Business.Data;
using Business.Dto;
using Business.Models;
using System.Linq;
using Business.Service;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace Banking_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BusinessContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly SubAdminServices _subAdminServices;

        public AuthController(BusinessContext context, IConfiguration configuration, EmailService emailService, SubAdminServices subAdminServices)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _subAdminServices = subAdminServices;
        }

        [HttpPost("login")]
        public IActionResult Login(Business.Models.LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest("Username or password cannot be empty.");

                string token = "";
                int roleId = 0;
                bool isPasswordChanged = true;                

                var adminUser = _context.AdminLoginRequests.FirstOrDefault(x => x.EmailId == request.Username);

                if (adminUser != null && ( adminUser.RoleId == 1 || adminUser.RoleId == 2))
                {
                    // Check if user is an Admin or Sub-Admin
                    roleId = adminUser.RoleId;
                    isPasswordChanged = adminUser.IsPasswordChanged;

                    // Compare password directly instead of hashing stored password again.
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminUser.AdminPassword);
                    if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), hashedPassword))
                        return Unauthorized("Invalid username or password.");

                    token = GenerateTokenforAdmin(adminUser);
                    return Ok(new { token, isPasswordChanged, roleId });
                }

                else
                {
                    // Check if user is a Business
                    var userBusiness = _context.Businesses.FirstOrDefault(u => u.EmailId == request.Username);
                    if (userBusiness != null)
                    {
                        roleId = userBusiness.RoleID;
                        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), userBusiness.Password))
                            return Unauthorized("Invalid username or password.");

                        token = GenerateBusinessToken(userBusiness);
                        return Ok(new { token, roleId });
                    }

                    // Check if user is a Customer
                    var userCustomer = _context.Customers.FirstOrDefault(x => x.Cus_EmailId == request.Username);
                    if (userCustomer != null)
                    {
                        roleId = userCustomer.RoleID;
                        if (!BCrypt.Net.BCrypt.Verify(request.Password, userCustomer.Cus_Password))
                            return Unauthorized("Invalid username or password.");

                        token = GenerateCustomerToken(userCustomer);
                        return Ok(new { token, roleId });
                    }
                }
                return Unauthorized("Invalid username or password.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private string GenerateBusinessToken(Busines business)
        {
            var claims = new[] {
                new Claim("EmailId", business.EmailId),
                new Claim("BusinessID", business.BusinessID.ToString()),
                new Claim("EmailId", business.EmailId.ToString()),
                new Claim("RoleID", business.RoleID.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateCustomerToken(Customer customer)
        {
            var claims = new[] {
                new Claim("Email", customer.Cus_EmailId),
                new Claim("Cus_Id", customer.Cus_Id.ToString()),
                new Claim("EmailId", customer.Cus_EmailId.ToString()),
                new Claim("RoleID", customer.RoleID.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(Business.Models.ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest("Email cannot be empty.");

                // Check if the user is a Business
                var userBusiness = _context.Businesses.FirstOrDefault(u => u.EmailId == request.Email);
                if (userBusiness != null)
                {
                    string token = GeneratePasswordResetToken(userBusiness.BusinessID.ToString(), "Business");
                    SendResetEmail(request.Email, token);
                    return Ok(new { message = "Password reset link has been sent to your email." });
                }

                // Check if the user is a Customer
                var userCustomer = _context.Customers.FirstOrDefault(u => u.Cus_EmailId == request.Email);
                if (userCustomer != null)
                {
                    string token = GeneratePasswordResetToken(userCustomer.Cus_Id.ToString(), "Customer");
                    SendResetEmail(request.Email, token);
                    return Ok("Password reset link has been sent to your email.");
                }

                return NotFound("User not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GeneratePasswordResetToken(string userId, string userType)
        {
            var claims = new[] {
            new Claim("UserID", userId),
            new Claim("UserType", userType),
            new Claim("ResetToken", Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(15), // Token valid for 15 minutes
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task SendResetEmail(string email, string token)
        {
            string resetLink = $"https://sasmita2622606.github.io/BusinessApplication/Reset-password?token={token}";

            string subject = "Password Reset Request";
            string body = $"Click the following link to reset your password: <a href='{resetLink}'>Reset Password</a>";

            // Implement your email sending logic here
            await _emailService.SendEmailForForgotPasswordAsync(email, subject, body);
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword(Business.Models.ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                    return BadRequest("Invalid request.");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(request.Token, tokenValidationParameters, out securityToken);
                var jwtToken = (JwtSecurityToken)securityToken;

                var userId = jwtToken.Claims.First(x => x.Type == "UserID").Value;
                var userType = jwtToken.Claims.First(x => x.Type == "UserType").Value;

                if (userType == "Business")
                {
                    var userBusiness = _context.Businesses.FirstOrDefault(u => u.BusinessID.ToString() == userId);
                    if (userBusiness == null) return NotFound("User not found.");

                    userBusiness.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                }
                else if (userType == "Customer")
                {
                    var userCustomer = _context.Customers.FirstOrDefault(u => u.Cus_Id.ToString() == userId);
                    if (userCustomer == null) return NotFound("User not found.");

                    userCustomer.Cus_Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                }
                else
                {
                    return BadRequest("Invalid user type.");
                }

                _context.SaveChanges();
                return Ok(new { message = "Password has been successfully reset." });
            }
            catch (SecurityTokenException)
            {
                return BadRequest("Invalid or expired token.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GenerateTokenforAdmin(AdminLoginRequest adminLogin)
        {
            var claims = new[] {
            new Claim("EmailId", adminLogin.EmailId),
            new Claim("Id", adminLogin.Id.ToString()),
            new Claim("RoleID", adminLogin.RoleId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
