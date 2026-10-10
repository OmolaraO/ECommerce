namespace ECommerce.Contracts.Responses
{
    public class OrdersResponse
    {
        public required IEnumerable<OrderResponse> Items { get; init; } = Enumerable.Empty<OrderResponse>();
    }
}
