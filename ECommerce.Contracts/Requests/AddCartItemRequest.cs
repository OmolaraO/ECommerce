namespace ECommerce.Contracts.Requests
{
    public class AddCartItemRequest
    {
        public required Guid ProductId { get; init; }
        public int Quantity { get; init; } = 1;
    }
}
