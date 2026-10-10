namespace ECommerce.Contracts.Requests
{
    public class CreateProductRequest
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required decimal Price { get; init; }
        public required int StockQuantity { get; init; }
        public required string Category { get; init; }
    }
}
