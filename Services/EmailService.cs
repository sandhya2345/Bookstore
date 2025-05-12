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
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link href='https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap' rel='stylesheet'>
    <style>
        body {{
            font-family: 'Inter', sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f4f6f8;
            color: #333;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.07);
            padding: 40px 30px;
        }}
        h1 {{
            font-size: 26px;
            color: #1a202c;
            margin-bottom: 20px;
        }}
        p {{
            font-size: 16px;
            color: #4a5568;
            line-height: 1.6;
        }}
        .order-summary {{
            background-color: #edf2f7;
            padding: 20px;
            border-radius: 8px;
            margin-top: 20px;
        }}
        .order-summary p {{
            margin: 5px 0;
        }}
        .highlight {{
            color: #2b6cb0;
            font-weight: 600;
        }}
        .button {{
            display: inline-block;
            background-color: #2b6cb0;
            color: #ffffff;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 6px;
            font-size: 16px;
            margin-top: 30px;
            transition: background-color 0.3s ease;
        }}
        .button:hover {{
            background-color: #1a4e80;
        }}
        .footer {{
            text-align: center;
            font-size: 13px;
            color: #a0aec0;
            margin-top: 40px;
        }}
        .footer a {{
            color: #2b6cb0;
            text-decoration: none;
        }}
        @media screen and (max-width: 600px) {{
            .email-container {{
                padding: 20px;
            }}
            h1 {{
                font-size: 22px;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <h1>Hi {customerName},</h1>
        <p>Thank you for your purchase from <strong>MetroBook</strong>! We're thrilled to have you as a customer. Your order has been successfully placed and is being processed.</p>

        <div class='order-summary'>
            <p><strong>Order Summary:</strong></p>
            <p><span class='highlight'>Total Amount:</span> Rs. {totalAmount:F2}</p>
            <p><span class='highlight'>Claim Code:</span> {claimCode}</p>
        </div>

        <p>Please keep your <strong>Claim Code</strong> safe. It will be required for tracking or pickup.</p>

        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} MetroBook. All rights reserved.</p>
            <p>Need help? <a href='mailto:support@metrobook.com'>Contact Support</a></p>
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
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link href='https://fonts.googleapis.com/css2?family=Inter:wght@400;600&display=swap' rel='stylesheet'>
    <style>
        body {{
            font-family: 'Inter', sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f4f6f8;
            color: #333;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.07);
            padding: 40px 30px;
        }}
        h3 {{
            font-size: 22px;
            color: #1a202c;
            margin-bottom: 20px;
        }}
        p {{
            font-size: 16px;
            color: #4a5568;
            line-height: 1.6;
        }}
        .info {{
            background-color: #edf2f7;
            padding: 20px;
            border-radius: 8px;
            margin-top: 20px;
        }}
        .info p {{
            margin: 6px 0;
        }}
        .label {{
            color: #2b6cb0;
            font-weight: 600;
        }}
        .footer {{
            text-align: center;
            font-size: 13px;
            color: #a0aec0;
            margin-top: 40px;
        }}
        .footer a {{
            color: #2b6cb0;
            text-decoration: none;
        }}
        @media screen and (max-width: 600px) {{
            .email-container {{
                padding: 20px;
            }}
            h3 {{
                font-size: 20px;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <h3>📦 New Order Received</h3>
        <div class='info'>
            <p><span class='label'>Customer:</span> {customerName} ({customerEmail})</p>
            <p><span class='label'>Claim Code:</span> {claimCode}</p>
            <p><span class='label'>Total Amount:</span> Rs. {totalAmount:F2}</p>
        </div>

        <p>This order requires processing. Please verify and begin fulfillment as soon as possible.</p>

        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} MetroBook. All rights reserved.</p>
            <p>Questions? <a href='mailto:support@metrobook.com'>Contact Support</a></p>
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
