namespace OrderService.Domain.Validations;

public class OrderValidationException : Exception
{
    public OrderValidationException(string message) : base(message)
    {
    }
}
