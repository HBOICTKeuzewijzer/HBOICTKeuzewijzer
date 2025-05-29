namespace HBOICTKeuzewijzer.Api.Models.OerRequirements
{
    /// <summary>
    /// Represents the full set of prerequisites required to enroll in a module.
    /// Typically populated from the database. All modules have this class, though it may contain no requirements.
    /// </summary>
    public class ModulePrerequisite
    {
        /// <summary>
        /// Indicates whether the student must have completed their propaedeutic phase (P) to take the module.
        /// </summary>
        public bool Propaedeutic { get; set; }

        /// <summary>
        /// Optional constraint specifying in which semester the module may be taken.
        /// </summary>
        public SemesterConstraint? SemesterConstraint { get; set; }

        /// <summary>
        /// Specifies required earned credits (ECs) a student must have.
        /// Requirements can be general or specific to the propaedeutic phase.
        /// Examples:
        /// - 50 ECs from the propaedeutic phase.
        /// - 210 ECs total.
        /// Both types can be present simultaneously.
        /// </summary>
        public List<EcRequirement>? EcRequirements { get; set; }

        /// <summary>
        /// Specifies prerequisite modules that a student must have taken or completed.
        /// Each <see cref="ModuleRequirementGroup"/> represents an OR group.
        /// Within a group, all <see cref="ModuleRequirement"/>s must be fulfilled (AND).
        /// </summary>
        public List<ModuleRequirementGroup>? ModuleRequirementGroups { get; set; }

        /// <summary>
        /// Specifies requirements based on the level of modules the student has completed or followed.
        /// Each <see cref="ModuleLevelRequirementGroup"/> represents an OR group.
        /// Within a group, all <see cref="ModuleLevelRequirement"/>s must be fulfilled (AND).
        /// </summary>
        public List<ModuleLevelRequirementGroup>? ModuleLevelRequirementGroups { get; set; }

        /// <summary>
        /// Specifies in which school years a module is available. e.g. year 2, year 3
        /// </summary>
        public List<int>? YearConstraints { get; set; }

        /// <summary>
        /// Specifies from which school year onward this module is available.
        /// </summary>
        public int? AvailableFromYear { get; set; }
    }
}
