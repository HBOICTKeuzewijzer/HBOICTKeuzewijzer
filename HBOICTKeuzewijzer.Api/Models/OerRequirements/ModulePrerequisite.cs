using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;

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
        public List<EcRequirement> EcRequirements { get; set; }

        /// <summary>
        /// Specifies prerequisite modules that a student must have taken or completed.
        /// Each <see cref="ModuleRequirementGroup"/> represents an OR group.
        /// Within a group, all <see cref="ModuleRequirement"/>s must be fulfilled (AND).
        /// </summary>
        public List<ModuleRequirementGroup> ModuleRequirementGroups { get; set; }

        /// <summary>
        /// Specifies requirements based on the level of modules the student has completed or followed.
        /// Each <see cref="ModuleLevelRequirementGroup"/> represents an OR group.
        /// Within a group, all <see cref="ModuleLevelRequirement"/>s must be fulfilled (AND).
        /// </summary>
        public List<ModuleLevelRequirementGroup> ModuleLevelRequirementGroups { get; set; }
    }

    /// <summary>
    /// Represents a credit (EC) requirement.
    /// </summary>
    public class EcRequirement
    {
        /// <summary>
        /// The number of ECs required.
        /// </summary>
        public int RequiredAmount { get; set; }

        /// <summary>
        /// Indicates whether the ECs must be from the propaedeutic phase (P).
        /// </summary>
        public bool Propaedeutic { get; set; }
    }

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

    /// <summary>
    /// Specifies the semester in which a module is available.
    /// </summary>
    public enum SemesterConstraint
    {
        /// <summary>
        /// Module can be followed in the first semester.
        /// </summary>
        First,

        /// <summary>
        /// Module can be followed in the second semester.
        /// </summary>
        Second
    }

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


class Test
{
    public Test()
    {
        var MdoModule = new Module();
        MdoModule.Id = new Guid();

        var OOSDModule = new Module();
        OOSDModule.Id = new Guid();

        var WebDevModule = new Module();
        WebDevModule.Id = new Guid();

        var QSDModule = new Module();
        QSDModule.Id = new Guid();

        var examplePrerequisite = new ModulePrerequisite
        {
            Propaedeutic = true,
            SemesterConstraint = SemesterConstraint.First,
            EcRequirements = new List<EcRequirement>
            {
                new EcRequirement
                {
                    Propaedeutic = true,
                    RequiredAmount = 50
                },
                new EcRequirement
                {
                    RequiredAmount = 120
                }
            },
            ModuleRequirementGroups = new List<ModuleRequirementGroup>
            {
                new ModuleRequirementGroup
                {
                    ModuleRequirements = new List<ModuleRequirement>
                    {
                        new ModuleRequirement
                        {
                            RelevantModuleId = MdoModule.Id,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 30
                            }
                        },
                        new ModuleRequirement
                        {
                            RelevantModuleId = OOSDModule.Id,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 30
                            }
                        },
                        new ModuleRequirement
                        {
                            RelevantModuleId = WebDevModule.Id,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 30
                            }
                        }
                    }
                },
                new ModuleRequirementGroup
                {
                    ModuleRequirements = new List<ModuleRequirement>
                    {
                        new ModuleRequirement
                        {
                            RelevantModuleId = MdoModule.Id,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 30
                            }
                        },
                        new ModuleRequirement
                        {
                            RelevantModuleId = OOSDModule.Id
                        },
                        new ModuleRequirement
                        {
                            RelevantModuleId = QSDModule.Id,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 10
                            }
                        }
                    }
                }
            },
            ModuleLevelRequirementGroups = new List<ModuleLevelRequirementGroup>
            {
                new ModuleLevelRequirementGroup
                {
                    ModuleLevelRequirements = new List<ModuleLevelRequirement>
                    {
                        new ModuleLevelRequirement
                        {
                            Level = 2
                        },
                        new ModuleLevelRequirement
                        {
                            Level = 2
                        }
                    }
                },
                new ModuleLevelRequirementGroup
                {
                    ModuleLevelRequirements = new List<ModuleLevelRequirement>
                    {
                        new ModuleLevelRequirement
                        {
                            Level = 1
                        },
                        new ModuleLevelRequirement
                        {
                            Level = 1,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 10
                            }
                        },
                        new ModuleLevelRequirement
                        {
                            Level = 2
                        }
                    }
                }
            }
        };
    }
}