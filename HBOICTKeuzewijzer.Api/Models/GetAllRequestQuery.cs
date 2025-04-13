namespace HBOICTKeuzewijzer.Api.Models
{
    public class GetAllRequestQuery
    {
        public string? Filter { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; } = "asc";
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
