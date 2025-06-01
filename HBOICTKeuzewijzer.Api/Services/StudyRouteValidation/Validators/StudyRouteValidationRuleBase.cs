using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using Newtonsoft.Json;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public abstract class StudyRouteValidationRuleBase : IStudyRouteValidationRule
    {
        public abstract Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors);

        /// <summary>
        /// Parses the prerequisite JSON blob from the semester's module into a strongly typed <see cref="ModulePrerequisite"/>.
        /// </summary>
        /// <param name="semester">The semester containing the module whose prerequisite JSON should be parsed.</param>
        /// <param name="modulePrerequisite">
        /// The parsed <see cref="ModulePrerequisite"/> object if parsing was successful; otherwise, <c>null</c>.
        /// This is an <c>out</c> parameter used to return the parsed result.
        /// </param>
        /// <returns>
        /// <c>true</c> if parsing was successful and a prerequisite exists; otherwise, <c>false</c>.
        /// </returns>
        protected bool GetParsedPrerequisite(Semester semester, out ModulePrerequisite? modulePrerequisite)
        {
            if (semester.Module is null || string.IsNullOrEmpty(semester.Module.PrerequisiteJson))
            {
                modulePrerequisite = null;
                return false;
            }

            modulePrerequisite = JsonConvert.DeserializeObject<ModulePrerequisite>(semester.Module.PrerequisiteJson);
            return true;
        }

        /// <summary>
        /// Adds a validation error message to the specified error collection, organized by a unique key.
        /// This is a utility method that simplifies appending errors to a grouped error list structure.
        /// </summary>
        /// <param name="errorText">
        /// The descriptive error message to be added .
        /// </param>
        /// <param name="key">
        /// A unique identifier representing the context or field name the error is associated with like the semester id.
        /// If the key does not exist in the error collection, it will be added.
        /// </param>
        /// <param name="errors">
        /// The dictionary that holds lists of error messages, keyed by their associated field or context name.
        /// This dictionary will be modified by reference.
        /// </param>
        protected void AddError(string errorText, string key, Dictionary<string, List<string>> errors)
        {
            if (!errors.ContainsKey(key))
            {
                errors[key] = new List<string>();
            }

            errors[key].Add(errorText);
        }
    }
}