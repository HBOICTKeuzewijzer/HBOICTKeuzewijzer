namespace HBOICTKeuzewijzer.Api.Models.OerRequirements
{
    /// <summary>
    /// Represents a prerequisite module and its optional EC requirement.
    /// </summary>
    public class ModuleRequirement
    {
        /// <summary>
        /// The module that is required.
        /// </summary>
        public Guid RelevantModuleId { get; set; }

        /// <summary>
        /// Optional EC requirement associated with this module.
        /// Null indicates that the student just needs to have participated.
        /// </summary>
        public EcRequirement? EcRequirement { get; set; }
    }
}