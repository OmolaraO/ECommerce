namespace ECommerce.Contracts.Responses
{
    public class OrderResponse
    {
        public required Guid Id { get; init; }
        public required Guid UserId { get; init; }
        public required string Status { get; init; }
        public required decimal TotalAmount { get; init; }
        public required DateTime CreatedAt { get; init; }
        public required IEnumerable<OrderItemResponse> Items { get; init; } = Enumerable.Empty<OrderItemResponse>();
    }

    public class OrderItemResponse
    {
        public required Guid ProductId { get; init; }
        public required string ProductName { get; init; }
        public required int Quantity { get; init; }
        public required decimal PriceAtPurchase { get; init; }
    }
}
