namespace HBOICTKeuzewijzer.Api.Models.OerRequirements
{
    /// <summary>
    /// Represents a requirement for a module of a specific level to have been followed or completed.
    /// May also have an associated EC requirement.
    /// </summary>
    public class ModuleLevelRequirement
    {
        /// <summary>
        /// The required level of the module (e.g., 1 = year 1, 2 = year 2/3).
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Optional EC requirement associated with the level requirement.
        /// </summary>
        public EcRequirement? EcRequirement { get; set; }
    }
}