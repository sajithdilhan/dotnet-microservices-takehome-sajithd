namespace NotificationService.Domain.Validations
{
    [Serializable]
    internal class NotificationValidationException : Exception
    {
        public NotificationValidationException(string? message) : base(message)
        {
        }
    }
}