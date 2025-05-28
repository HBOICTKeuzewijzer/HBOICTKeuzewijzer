namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public class AllGroupsResult<T> where T : class, new()
    {
        public List<GroupResult<T>> FailureResults { get; set; } = new();
        public bool AnyGroupPassed { get; set; }
    }
}