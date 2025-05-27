namespace HBOICTKeuzewijzer.Api.Models;

public class ModulePrerequisite
{
    public bool Propaedeutic { get; set; }
    public SemesterConstraint SemesterConstraint { get; set; }
    public List<EcRequirement> EcRequirements { get; set; }
    public List<ModuleRequirementGroup> ModuleRequirementGroups { get; set; }
    public List<ModuleLevelRequirementGroup> ModuleLevelRequirementGroups { get; set; }
}

public class EcRequirement
{
    public int RequiredAmount { get; set; }
    public bool Propaedeutic { get; set; }
}

public class ModuleRequirement
{
    public Module RelevantModule { get; set; }
    public bool CompletionRequired { get; set; }
}

public class ModuleRequirementGroup
{
    public List<ModuleRequirement> ModuleRequirements { get; set; }
}

public class ModuleLevelRequirementGroup
{
    public List<ModuleLevelRequirement> ModuleLevelRequirements { get; set; }
}

public enum SemesterConstraint
{
    FIRST,
    SECOND
}

public class ModuleLevelRequirement
{
    public int Level { get; set; }
}

public class Test
{
    public Test()
    {
        var MdoModule = new Module();
        var OOSDModule = new Module();
        var WebDevModule = new Module();
        var QSDModule = new Module();

        var seAfstuderenVoorbeeld = new ModulePrerequisite
        {
            Propaedeutic = true,
            SemesterConstraint = SemesterConstraint.FIRST,
            ModuleRequirementGroups = new List<ModuleRequirementGroup>
            {
                new ModuleRequirementGroup
                {
                    ModuleRequirements = new List<ModuleRequirement>
                    {
                        new ModuleRequirement
                        {
                            RelevantModule = MdoModule,
                            CompletionRequired = true
                        },
                        new ModuleRequirement
                        {
                            RelevantModule = OOSDModule,
                            CompletionRequired = true
                        },
                        new ModuleRequirement
                        {
                            RelevantModule = WebDevModule,
                            CompletionRequired = true
                        }
                    }
                },
                new ModuleRequirementGroup
                {
                    ModuleRequirements = new List<ModuleRequirement>
                    {
                        new ModuleRequirement
                        {
                            RelevantModule = MdoModule,
                            CompletionRequired = true
                        },
                        new ModuleRequirement
                        {
                            RelevantModule = OOSDModule,
                            CompletionRequired = true
                        },
                        new ModuleRequirement
                        {
                            RelevantModule = QSDModule,
                            CompletionRequired = true
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
                }
            }
        };
    }
}