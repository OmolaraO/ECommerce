namespace ECommerce.Contracts.Responses
{
    public class ProductsResponse
    {
        public required IEnumerable<ProductResponse> Items { get; init; } = Enumerable.Empty<ProductResponse>();
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required int Total { get; init; }
        public bool HasNextPage => Total > Page * PageSize;
    }
}
