namespace ECommerce.Contracts.Requests
{
    public class UpdateOrderStatusRequest
    {
        public required string Status { get; init; }
    }
}
