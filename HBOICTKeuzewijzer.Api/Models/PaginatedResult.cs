namespace HBOICTKeuzewijzer.Api.Models
{

    /// <summary>
    /// Paginated result for API responses. In wish fulfillment for Jarne ;p
    /// </summary>
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
