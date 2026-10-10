namespace ECommerce.Contracts.Responses
{
    public class CartResponse
    {
        public required Guid Id { get; init; }
        public required IEnumerable<CartItemResponse> Items { get; init; } = Enumerable.Empty<CartItemResponse>();
        public required decimal TotalAmount { get; init; }
    }

    public class CartItemResponse
    {
        public required Guid ProductId { get; init; }
        public required string ProductName { get; init; }
        public required decimal Price { get; init; }
        public required int Quantity { get; init; }
        public decimal LineTotal => Price * Quantity;
    }
}
