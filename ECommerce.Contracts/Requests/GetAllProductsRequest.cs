namespace ECommerce.Contracts.Requests
{
    public class GetAllProductsRequest
    {
        public string? Name { get; init; }
        public string? Category { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
