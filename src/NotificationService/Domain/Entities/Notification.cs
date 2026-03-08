using NotificationService.Domain.Validations;
using Shared.Contracts.Events;

namespace NotificationService.Domain.Entities;

public class Notification
{
    public int NotificationId { get; set; }
    public PaymentSucceededEvent Message { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    private Notification()
    {
        
    }

    public Notification(PaymentSucceededEvent message)
    {
        Message = message;
        CreatedAt = DateTime.UtcNow;
    }
}
