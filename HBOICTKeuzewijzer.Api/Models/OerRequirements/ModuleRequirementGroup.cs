namespace HBOICTKeuzewijzer.Api.Models.OerRequirements
{
    /// <summary>
    /// Represents a group of module prerequisites that must all be satisfied (AND).
    /// If at least one <see cref="ModuleRequirementGroup"/> is satisfied, the overall requirement is met.
    /// </summary>
    public class ModuleRequirementGroup
    {
        /// <summary>
        /// The list of module requirements that must all be met.
        /// </summary>
        public List<ModuleRequirement> ModuleRequirements { get; set; }
    }
}