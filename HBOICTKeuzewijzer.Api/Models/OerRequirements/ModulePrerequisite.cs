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

        /// <summary>
        /// Specifies in which school years a module is available. e.g. year 2, year 3
        /// </summary>
        public List<int> YearConstraints { get; set; }

        /// <summary>
        /// Specifies from which school year onward this module is available.
        /// </summary>
        public int AvailableFromYear { get; set; }
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
            // P completed
            Propaedeutic = true,
            // Needs to be done in the first semester
            SemesterConstraint = SemesterConstraint.First,
            // Has credit requirements
            EcRequirements = new List<EcRequirement>
            {
                // 50 credits from P
                new EcRequirement
                {
                    Propaedeutic = true,
                    RequiredAmount = 50
                },
                // And 120 in total including those from P
                new EcRequirement
                {
                    RequiredAmount = 120
                }
            },
            // Requires some modules before following this one
            ModuleRequirementGroups = new List<ModuleRequirementGroup>
            {
                // Either this
                new ModuleRequirementGroup
                {
                    ModuleRequirements = new List<ModuleRequirement>
                    {
                        // Requires to have received 30 credits from MDO module
                        new ModuleRequirement
                        {
                            RelevantModuleId = MdoModule.Id,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 30
                            }
                        },
                        // And requires to have received 30 credits from OOSD module
                        new ModuleRequirement
                        {
                            RelevantModuleId = OOSDModule.Id,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 30
                            }
                        },
                        // And requires to have received 30 credits from WebDev module
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
                // Or this
                new ModuleRequirementGroup
                {
                    ModuleRequirements = new List<ModuleRequirement>
                    {
                        // Requires to have received 30 credits from MDO module
                        new ModuleRequirement
                        {
                            RelevantModuleId = MdoModule.Id,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 30
                            }
                        },
                        // And requires to have participated in OOSD module, regardless of received credits
                        new ModuleRequirement
                        {
                            RelevantModuleId = OOSDModule.Id
                        },
                        // And requires to have received 10 credits from QSD module
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
            // Requires some modules of specific level before following this one
            ModuleLevelRequirementGroups = new List<ModuleLevelRequirementGroup>
            {
                // Either this
                new ModuleLevelRequirementGroup
                {
                    ModuleLevelRequirements = new List<ModuleLevelRequirement>
                    {
                        // Requires to have participated in a module of level 2
                        new ModuleLevelRequirement
                        {
                            Level = 2
                        },
                        // And participated in a different module of level 2
                        new ModuleLevelRequirement
                        {
                            Level = 2
                        }
                    }
                },
                // Or this
                new ModuleLevelRequirementGroup
                {
                    ModuleLevelRequirements = new List<ModuleLevelRequirement>
                    {
                        // Requires to have participated in a module of level 1
                        new ModuleLevelRequirement
                        {
                            Level = 1
                        },
                        // And have received 10 credits in a module of level 1
                        new ModuleLevelRequirement
                        {
                            Level = 1,
                            EcRequirement = new EcRequirement
                            {
                                RequiredAmount = 10
                            }
                        },
                        // And to have participated in a module of level 2
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