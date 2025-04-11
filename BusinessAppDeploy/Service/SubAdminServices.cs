using System.Security.Cryptography;
using System.Text;

namespace Business.Service
{
    public class SubAdminServices
    {
        private readonly EmailService _emailService;
        public SubAdminServices(EmailService emailService)
        {
            _emailService = emailService;
        }
        public async Task AddSubAdminAsync(string email)
        {
            // Generate a random password
            var password = GenerateRandomPassword(8);

            // Create email body
            var emailBody = $"<p>Hi there,</p><p>Your account has been added as sub-admin successfully.</p><p>Here are your default login credentials:</p><p><b>Password: {password}</b><p>You can login now to provide ratings and comments for the business owners and customers.</p><p>Thank You,</p><p>Business Application</p></p>";

            // Send the email
            await _emailService.SendEmailAsync(email, "Welcome as Sub-admin", emailBody);
        }

        // Method to generate a random password
        private string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            StringBuilder password = new StringBuilder();
            using (var rng = new RNGCryptoServiceProvider()) // need to create a static method for random number generating
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                for (int i = 0; i < randomBytes.Length; i++)
                {
                    password.Append(validChars[randomBytes[i] % validChars.Length]);
                }
            }
            return password.ToString();
        }
        public async Task<string> GetEmailSubAdmin(string email)
        {
            // Generate a random password
            var password = GenerateRandomPassword(8);

            // Create email body
            var emailBody = $"<p>Hi there,</p><p>Your account has been added as sub-admin successfully.</p><p>Here are your default login credentials:</p><p><b>Password: {password}</b><p>You can login now to provide ratings and comments for the business owners and customers.</p><p>Thank You,</p><p>Business Application</p></p>";

            // Send the email
            await _emailService.SendEmailAsync(email, "Welcome as Sub-admin", emailBody);

            return password;
        }
    }
}
