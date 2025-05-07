using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore.Models;

namespace OnlineBookStore.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public EmailService(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task SendOrderEmailAsync(string customerEmail, string claimCode, decimal totalAmount, string customerName = "Customer")
        {
            // -------------------------------
            // 1. Customer email
            // -------------------------------
            var customerMessage = new MimeMessage();
            customerMessage.From.Add(new MailboxAddress(_config["EmailSettings:FromName"], _config["EmailSettings:FromEmail"]));
            customerMessage.To.Add(MailboxAddress.Parse(customerEmail));
            customerMessage.Subject = "Your MetroBook Order Confirmation";

            var customerHtml = $@"
                <html>
                    <body>
                        <h2>Thank You for Your Purchase, {customerName}!</h2>
                        <p>Your order has been placed successfully.</p>
                        <p><strong>Claim Code:</strong> {claimCode}</p>
                        <p><strong>Total Amount:</strong> Rs. {totalAmount:F2}</p>
                    </body>
                </html>";

            customerMessage.Body = new TextPart("html") { Text = customerHtml };

            // -------------------------------
            // 2. Get staff emails from DB
            // -------------------------------
            var staffEmails = await _context.Users
                .Where(u => u.Role == UserRole.Staff && u.IsActive)
                .Select(u => u.Email)
                .ToListAsync();

            // -------------------------------
            // 3. Staff email
            // -------------------------------
            var staffMessage = new MimeMessage();
            staffMessage.From.Add(new MailboxAddress(_config["EmailSettings:FromName"], _config["EmailSettings:FromEmail"]));

            foreach (var email in staffEmails)
            {
                staffMessage.To.Add(MailboxAddress.Parse(email));
            }

            staffMessage.Subject = $"[Order Alert] Claim Code for {customerName}";

            var staffHtml = $@"
                <html>
                    <body>
                        <h3>New Order Placed</h3>
                        <p><strong>Customer:</strong> {customerName} ({customerEmail})</p>
                        <p><strong>Claim Code:</strong> {claimCode}</p>
                        <p><strong>Total Amount:</strong> Rs. {totalAmount:F2}</p>
                    </body>
                </html>";

            staffMessage.Body = new TextPart("html") { Text = staffHtml };

            // -------------------------------
            // 4. Send both emails
            // -------------------------------
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:SmtpPort"]),
                MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _config["EmailSettings:SmtpUsername"],
                _config["EmailSettings:SmtpPassword"]);

            await client.SendAsync(customerMessage);
            await client.SendAsync(staffMessage);
            await client.DisconnectAsync(true);
        }
    }
}
