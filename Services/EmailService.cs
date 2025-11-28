namespace ConsolidatedApi.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // TODO: Implement email sending functionality
            await Task.Delay(100); // Simulate async operation
            Console.WriteLine($"Email sent to {to}: {subject}");
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var subject = "Password Reset Request";
            var body = $"Click the following link to reset your password: {resetToken}";
            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailVerificationAsync(string email, string verificationToken)
        {
            var subject = "Email Verification";
            var body = $"Please verify your email by clicking: {verificationToken}";
            await SendEmailAsync(email, subject, body);
        }
    }
}