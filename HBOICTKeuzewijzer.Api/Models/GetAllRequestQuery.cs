using System.Linq.Expressions;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class GetAllRequestQuery<T>
    {
        public string? Filter { get; set; }
        public string? SortColumn { get; set; }
        public Direction SortDirection { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public Expression<Func<T, object>>? SortExpression { get; set; }

        public void SetSortExpression()
        {
            if (!string.IsNullOrEmpty(SortColumn))
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, SortColumn);
                var conversion = Expression.Convert(property, typeof(object));
                SortExpression = Expression.Lambda<Func<T, object>>(conversion, parameter);
            }
        }
    }
}
