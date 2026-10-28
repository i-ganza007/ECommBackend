using ConsumerProject.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ConsumerProject.Services;

public class EmailSender
{
    private readonly SmtpSettings _settings;

    public EmailSender(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendOrderEmail(OrderEmailMessage order, CancellationToken ctx)
    {
        var msg = BuildMessage(order);

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.Host, _settings.Port, _settings.Security, ctx);
        if (!string.IsNullOrEmpty(_settings.Username))
        {
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password ?? string.Empty, ctx);
        }
        await smtp.SendAsync(msg, ctx);
        await smtp.DisconnectAsync(true, ctx);
    }

    private MimeMessage BuildMessage(OrderEmailMessage order)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        msg.To.Add(new MailboxAddress(order.ToName, order.ToEmail));

        var (subject, line) = order.OrderStatus switch
        {
            OrderStatus.Processed => ($"Your order {order.OrderId} is confirmed",
                "Good news, your order has been confirmed and is being prepared."),
            OrderStatus.Rejected => ($"Your order {order.OrderId} couldn't be completed",
                "Sorry, we couldn't complete your order because some items are out of stock. You haven't been charged."),
            OrderStatus.Cancelled => ($"Your order {order.OrderId} was cancelled",
                "Your order has been cancelled as you requested. You haven't been charged."),
            _ => ($"Update on your order {order.OrderId}",
                $"Your order is now {order.OrderStatus}.")
        };
        msg.Subject = subject;

        var body = new BodyBuilder
        {
            TextBody =
                $"Hi {order.ToName},\n\n{line}\n\n" +
                $"Order: {order.OrderId}\nPlaced: {order.CreatedDate:u}\nTotal: {order.TotalPrice:0.00}\n",
            HtmlBody =
                $"<p>Hi {System.Net.WebUtility.HtmlEncode(order.ToName)},</p><p>{line}</p>" +
                $"<p>Order: {order.OrderId}<br/>Placed: {order.CreatedDate:u}<br/>Total: {order.TotalPrice:0.00}</p>"
        };
        msg.Body = body.ToMessageBody();
        return msg;
    }
}
