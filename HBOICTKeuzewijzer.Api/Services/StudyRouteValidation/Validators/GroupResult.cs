namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public class GroupResult<T> where T : class, new()
    {
        public List<T> Requirements { get; set; }
        public List<(T Requirement, bool Missing)> Failures { get; set; } = new();
        public bool GroupPassed { get; set; }
    }
}