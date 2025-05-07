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
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                margin: 0;
                padding: 0;
            }}
            .container {{
                width: 100%;
                max-width: 600px;
                margin: 0 auto;
                background-color: #ffffff;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            }}
            h1 {{
                color: #333333;
                font-size: 24px;
            }}
            p {{
                color: #666666;
                font-size: 16px;
            }}
            .btn {{
                background-color: #1b6ec2;
                color: #ffffff;
                padding: 12px 20px;
                text-align: center;
                border-radius: 4px;
                text-decoration: none;
                display: inline-block;
                font-size: 16px;
                margin-top: 20px;
            }}
            .footer {{
                font-size: 12px;
                text-align: center;
                color: #888888;
                margin-top: 30px;
            }}
            .footer a {{
                color: #1b6ec2;
                text-decoration: none;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h1>Thank You for Your Purchase, {customerName}!</h1>
            <p>Your order has been placed successfully with MetroBook.</p>

            <h2>Order Details:</h2>
            <p><strong>Total Amount:</strong> Rs. {totalAmount:F2}</p>
            <p><strong>Claim Code:</strong> {claimCode}</p>

            <p>Please keep your claim code safe. It will be required for pickup or order tracking.</p>

            <a href='#' class='btn'>Track Your Order</a>

            <div class='footer'>
                <p>&copy; {DateTime.Now.Year} MetroBook | All rights reserved</p>
                <p>Need help? Contact us at <a href='mailto:support@metrobook.com'>support@metrobook.com</a></p>
            </div>
        </div>
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
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                margin: 0;
                padding: 0;
            }}
            .container {{
                width: 100%;
                max-width: 600px;
                margin: 0 auto;
                background-color: #ffffff;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            }}
            h3 {{
                color: #333333;
                font-size: 20px;
            }}
            p {{
                color: #666666;
                font-size: 16px;
            }}
            .footer {{
                font-size: 12px;
                text-align: center;
                color: #888888;
                margin-top: 30px;
            }}
            .footer a {{
                color: #1b6ec2;
                text-decoration: none;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h3>New Order Placed</h3>
            <p><strong>Customer:</strong> {customerName} ({customerEmail})</p>
            <p><strong>Claim Code:</strong> {claimCode}</p>
            <p><strong>Total Amount:</strong> Rs. {totalAmount:F2}</p>

            <div class='footer'>
                <p>&copy; {DateTime.Now.Year} MetroBook | All rights reserved</p>
                <p>If you have any questions, please contact us at <a href='mailto:support@metrobook.com'>support@metrobook.com</a>.</p>
            </div>
        </div>
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
