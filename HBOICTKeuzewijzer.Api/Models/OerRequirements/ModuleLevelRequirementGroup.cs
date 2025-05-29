namespace HBOICTKeuzewijzer.Api.Models.OerRequirements
{
    /// <summary>
    /// Represents a group of module level requirements that must all be satisfied (AND).
    /// If at least one <see cref="ModuleLevelRequirementGroup"/> is satisfied, the overall requirement is met.
    /// </summary>
    public class ModuleLevelRequirementGroup
    {
        /// <summary>
        /// The list of level-based module requirements that must all be met.
        /// </summary>
        public List<ModuleLevelRequirement> ModuleLevelRequirements { get; set; }
    }
}