using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OnlineBookStore.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOrderEmailAsync(string toEmail, string claimCode, decimal totalAmount)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["EmailSettings:FromName"],
                _config["EmailSettings:FromEmail"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Your MetroBook Order Confirmation";

            // HTML email content
            var htmlBody = $@"
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
                            color: white;
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
                        <h1>Thank You for Your Purchase!</h1>
                        <p>Dear Customer,</p>
                        <p>Thank you for shopping with MetroBook! Your order has been successfully processed.</p>
                        
                        <h2>Order Summary:</h2>
                        <p><strong>Total Amount:</strong> Rs. {totalAmount:F2}</p>
                        <p><strong>Your Claim Code:</strong> {claimCode}</p>

                        <p>Please keep your claim code safe, as it will be required for order pickup or tracking.</p>

                        <a href='#' class='btn'>Track Your Order</a>

                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} MetroBook | All rights reserved</p>
                            <p>If you have any questions, please contact us at <a href='mailto:{_config["EmailSettings:FromEmail"]}'>support@metrobook.com</a>.</p>
                        </div>
                    </div>
                </body>
            </html>";

            message.Body = new TextPart("html")
            {
                Text = htmlBody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:SmtpPort"]),
                MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _config["EmailSettings:SmtpUsername"],
                _config["EmailSettings:SmtpPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
