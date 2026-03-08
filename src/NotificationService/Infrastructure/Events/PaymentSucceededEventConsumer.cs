using MassTransit;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Contracts.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Events;

public class PaymentSucceededEventConsumer(INotificationService notificationService, ILogger<PaymentSucceededEventConsumer> logger) : IConsumer<PaymentSucceededEvent>
{
    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received PaymentSucceededEvent: {Payment}", JsonSerializer.Serialize(message));

        await notificationService.SendNotificationAsync(new Notification(message));
    }
}
