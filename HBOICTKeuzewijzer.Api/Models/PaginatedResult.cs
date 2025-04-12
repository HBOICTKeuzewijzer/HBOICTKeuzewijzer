namespace HBOICTKeuzewijzer.Api.Models
{

    /// <summary>
    /// Gepagineerd resultaat voor API-responses.
    /// </summary>
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
