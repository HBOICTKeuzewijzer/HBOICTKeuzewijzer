using AutoFixture;
using FluentAssertions;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using HBOICTKeuzewijzer.Api.Services;
using Newtonsoft.Json;
using System.Collections;

namespace HBOICTKeuzewijzer.Tests.Services
{
    public class StudyRouteValidationServiceTests
    {
        private readonly IFixture _fixture;

        public StudyRouteValidationServiceTests()
        {
            _fixture = new Fixture();

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void ValidateRoute_ThrowsArgumentNullException_WhenPassedRouteIsNull()
        {
            var sut = new StudyRouteValidationService();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            sut.Invoking(s => s.ValidateRoute(null))
                .Should().Throw<ArgumentNullException>();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact]
        public void ValidateRoute_ReturnsNull_WhenSemestersIsNull()
        {
            var route = _fixture.Build<StudyRoute>()
                .With(r => r.Semesters, (List<Semester>?)null)
                .Create();

            var sut = new StudyRouteValidationService();

            sut.ValidateRoute(route).Should().BeNull();
        }

        [Fact]
        public void ValidateRoute_ReturnsNull_WhenSemestersIsEmpty()
        {
            var route = _fixture.Build<StudyRoute>()
                .With(r => r.Semesters, new List<Semester>())
                .Create();

            var sut = new StudyRouteValidationService();

            sut.ValidateRoute(route).Should().BeNull();
        }

        [Fact]
        public void ValidateRoute_RetursNull_WhenGivenValidRoute()
        {
            var semesters = _fixture.CreateMany<Semester>(8).ToList();

            var route = _fixture.Build<StudyRoute>()
                .With(r => r.Semesters, semesters)
                .Create();

            var sut = new StudyRouteValidationService();

            sut.ValidateRoute(route).Should().BeNull();
        }

        [Theory]
        [ClassData(typeof(ValidStudyRouteData))]
        public void Test(StudyRoute studyRoute)
        {

        }
    }
}

public class ValidStudyRouteData : IEnumerable<object[]>
{
    private TestModules _testModules;

    public ValidStudyRouteData()
    {
        _testModules = new TestModules();
    }

    private Semester CreateSemester(int index, Module module, int acquiredECs = 30)
    {
        var newSemester = new Semester
        {
            Id = Guid.NewGuid(),
            Index = index,
            Module = module,
            ModuleId = module.Id,
            AcquiredECs = acquiredECs
        };

        return newSemester;
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new StudyRoute
            {
                Id = Guid.NewGuid(),
                Semesters = new List<Semester>
                {
                    CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    CreateSemester(1, _testModules.BeherenVanEenVerandertraject, 20),
                    CreateSemester(2, _testModules.OoSoftwareDesignDevelopment, 20),
                    CreateSemester(3, _testModules.WebDevelopment, 20),
                    CreateSemester(4, _testModules.QualitySoftwareDevelopment, 20),
                    CreateSemester(5, _testModules.MultidisciplinaireOpdracht, 20),
                    CreateSemester(6, _testModules.DataScience, 20),
                    CreateSemester(7, _testModules.AfstuderenSE, 20),
                }
            }
        };

        yield return new object[]
        {
            new StudyRoute
            {
                Id = Guid.NewGuid(),
                Semesters = new List<Semester>
                {
                    CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    CreateSemester(1, _testModules.BeherenVanEenVerandertraject, 20),
                    CreateSemester(2, _testModules.OoSoftwareDesignDevelopment, 20),
                    CreateSemester(3, _testModules.WebDevelopment, 20),
                    CreateSemester(4, _testModules.ManagementOfIt, 20),
                    CreateSemester(5, _testModules.Stage, 20),
                    CreateSemester(6, _testModules.MultidisciplinaireOpdracht, 20),
                    CreateSemester(7, _testModules.AfstuderenSE, 20),
                }
            }
        };

        yield return new object[]
        {
            new StudyRoute
            {
                Id = Guid.NewGuid(),
                Semesters = new List<Semester>
                {
                    CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    CreateSemester(1, _testModules.BeherenVanEenVerandertraject, 20),
                    CreateSemester(2, _testModules.HybridCloudInfrastructure, 20),
                    CreateSemester(3, _testModules.CloudArchitectureAutomation, 20),
                    CreateSemester(4, _testModules.OoSoftwareDesignDevelopment, 20),
                    CreateSemester(5, _testModules.MultidisciplinaireOpdracht, 20),
                    CreateSemester(6, _testModules.Stage, 20),
                    CreateSemester(7, _testModules.AfstuderenIDS, 20),
                }
            }
        };

        yield return new object[]
        {
            new StudyRoute
            {
                Id = Guid.NewGuid(),
                Semesters = new List<Semester>
                {
                    CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    CreateSemester(1, _testModules.BeherenVanEenVerandertraject, 20),
                    CreateSemester(2, _testModules.HybridCloudInfrastructure, 20),
                    CreateSemester(3, _testModules.CloudArchitectureAutomation, 20),
                    CreateSemester(4, _testModules.AppliedItSecurity, 20),
                    CreateSemester(5, _testModules.MultidisciplinaireOpdracht, 20),
                    CreateSemester(6, _testModules.Stage, 20),
                    CreateSemester(7, _testModules.AfstuderenIDS, 20),
                }
            }
        };

        yield return new object[]
        {
            new StudyRoute
            {
                Id = Guid.NewGuid(),
                Semesters = new List<Semester>
                {
                    CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    CreateSemester(1, _testModules.BeherenVanEenVerandertraject, 20),
                    CreateSemester(2, _testModules.BusinessProcessManagement, 20),
                    CreateSemester(3, _testModules.DataScience, 20),
                    CreateSemester(4, _testModules.ManagementOfIt, 20),
                    CreateSemester(5, _testModules.CloudArchitectureAutomation, 20),
                    CreateSemester(6, _testModules.MultidisciplinaireOpdracht, 20),
                    CreateSemester(7, _testModules.AfstuderenBIM, 20),
                }
            }
        };

        yield return new object[]
        {
            new StudyRoute
            {
                Id = Guid.NewGuid(),
                Semesters = new List<Semester>
                {
                    CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    CreateSemester(1, _testModules.BeherenVanEenVerandertraject, 20),
                    CreateSemester(2, _testModules.BusinessProcessManagement, 20),
                    CreateSemester(3, _testModules.DataScience, 20),
                    CreateSemester(4, _testModules.AppliedItSecurity, 20),
                    CreateSemester(5, _testModules.MultidisciplinaireOpdracht, 20),
                    CreateSemester(6, _testModules.Stage, 20),
                    CreateSemester(7, _testModules.AfstuderenBIM, 20),
                }
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


public class TestModules
{
    private new Dictionary<string, Module> Modules { get; set; }

    private const string bedrijfsProcessenDynamischeWebapps = "Bedrijfsprocessen en dynamische webapplicaties";
    private const string beherenVanEenVerandertraject = "Beheren van een verandertraject";
    private const string ooSoftwareDesignDevelopment = "OO Software Design & Development";
    private const string webDevelopment = "Web Development";
    private const string businessProcessManagement = "Business Process Management";
    private const string dataScience = "Data Science";
    private const string hybridCloudInfrastructure = "Hybrid Cloud Infrastructure";
    private const string cloudArchitectureAutomation = "Cloud Architecture and Automation";
    private const string qualitySoftwareDevelopment = "Quality in Software Development";
    private const string managementOfIt = "Management of IT";
    private const string appliedItSecurity = "Applied IT Security";
    private const string multidisciplinaireOpdracht = "Multidisciplinaire opdracht";
    private const string stage = "Stage";
    private const string afstuderenSE = "Afstuderen SE";
    private const string afstuderenBIM = "Afstuderen BIM";
    private const string afstuderenIDS = "Afstuderen IDS";

    public Module BedrijfsProcessenDynamischeWebapps => Modules[bedrijfsProcessenDynamischeWebapps];
    public Module BeherenVanEenVerandertraject => Modules[beherenVanEenVerandertraject];
    public Module OoSoftwareDesignDevelopment => Modules[ooSoftwareDesignDevelopment];
    public Module WebDevelopment => Modules[webDevelopment];
    public Module BusinessProcessManagement => Modules[businessProcessManagement];
    public Module DataScience => Modules[dataScience];
    public Module HybridCloudInfrastructure => Modules[hybridCloudInfrastructure];
    public Module CloudArchitectureAutomation => Modules[cloudArchitectureAutomation];
    public Module QualitySoftwareDevelopment => Modules[qualitySoftwareDevelopment];
    public Module ManagementOfIt => Modules[managementOfIt];
    public Module AppliedItSecurity => Modules[appliedItSecurity];
    public Module MultidisciplinaireOpdracht => Modules[multidisciplinaireOpdracht];
    public Module Stage => Modules[stage];
    public Module AfstuderenSE => Modules[afstuderenSE];
    public Module AfstuderenBIM => Modules[afstuderenBIM];
    public Module AfstuderenIDS => Modules[afstuderenIDS];

    public TestModules()
    {
        Modules = new Dictionary<string, Module>();

        Modules.Add(bedrijfsProcessenDynamischeWebapps, new Module
        {
            Id = Guid.NewGuid(),
            Name = bedrijfsProcessenDynamischeWebapps,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                YearConstraints = [1]
            }),
            IsPropaedeutic = true,
            ECs = 30,
            Level = 1,
            Locked = true,
            LockedSemester = 1
        });

        Modules.Add(beherenVanEenVerandertraject, new Module
        {
            Id = Guid.NewGuid(),
            Name = beherenVanEenVerandertraject,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                YearConstraints = [1],
                ModuleRequirementGroups = [
                    new ModuleRequirementGroup {
                        ModuleRequirements = [
                            new ModuleRequirement{
                                RelevantModuleId = Modules[bedrijfsProcessenDynamischeWebapps].Id
                            }
                        ]
                    }
                ]
            }),
            IsPropaedeutic = true,
            ECs = 30,
            Level = 1
        });

        Modules.Add(ooSoftwareDesignDevelopment, new Module
        {
            Id = Guid.NewGuid(),
            Name = ooSoftwareDesignDevelopment,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 2,
                SemesterConstraint = SemesterConstraint.First,
                EcRequirements = [
                    new EcRequirement
                    {
                        Propaedeutic = true,
                        RequiredAmount = 50
                    }
                ]
            }),
            ECs = 30,
            Level = 2
        });

        Modules.Add(webDevelopment, new Module
        {
            Id = Guid.NewGuid(),
            Name = webDevelopment,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 2,
                SemesterConstraint = SemesterConstraint.Second,
                EcRequirements = 
                [
                    new EcRequirement
                    {
                        Propaedeutic = true,
                        RequiredAmount = 50
                    }
                ],
                ModuleRequirementGroups = [
                    new ModuleRequirementGroup {
                        ModuleRequirements = [
                            new ModuleRequirement {
                                RelevantModuleId = Modules[ooSoftwareDesignDevelopment].Id
                            }
                        ]
                    }
                ]
            }),
            ECs = 30,
            Level = 2
        });

        Modules.Add(businessProcessManagement, new Module
        {
            Id = Guid.NewGuid(),
            Name = businessProcessManagement,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 2,
                SemesterConstraint = SemesterConstraint.First,
                EcRequirements = [
                    new EcRequirement
                    {
                        Propaedeutic = true,
                        RequiredAmount = 50
                    }
                ]
            }),
            ECs = 30,
            Level = 2
        });

        Modules.Add(dataScience, new Module
        {
            Id = Guid.NewGuid(),
            Name = dataScience,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 2,
                SemesterConstraint = SemesterConstraint.Second,
                EcRequirements = [
                    new EcRequirement
                    {
                        Propaedeutic = true,
                        RequiredAmount = 50
                    }
                ]
            }),
            ECs = 30,
            Level = 2
        });

        Modules.Add(hybridCloudInfrastructure, new Module
        {
            Id = Guid.NewGuid(),
            Name = hybridCloudInfrastructure,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 2,
                SemesterConstraint = SemesterConstraint.First,
                EcRequirements = [
                    new EcRequirement
                    {
                        Propaedeutic = true,
                        RequiredAmount = 50
                    }
                ]
            }),
            ECs = 30,
            Level = 2
        });

        Modules.Add(cloudArchitectureAutomation, new Module
        {
            Id = Guid.NewGuid(),
            Name = cloudArchitectureAutomation,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 2,
                SemesterConstraint = SemesterConstraint.Second,
                EcRequirements = [
                    new EcRequirement
                    {
                        Propaedeutic = true,
                        RequiredAmount = 50
                    }
                ]
            }),
            ECs = 30,
            Level = 2
        });

        Modules.Add(qualitySoftwareDevelopment, new Module
        {
            Id = Guid.NewGuid(),
            Name = qualitySoftwareDevelopment,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 3,
                SemesterConstraint = SemesterConstraint.First,
                ModuleRequirementGroups = [
                    new ModuleRequirementGroup
                    {
                        ModuleRequirements = [
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[ooSoftwareDesignDevelopment].Id
                            }
                        ]
                    }
                ]
            }),
            ECs = 30,
            Level = 3
        });

        Modules.Add(managementOfIt, new Module
        {
            Id = Guid.NewGuid(),
            Name = managementOfIt,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 3,
                SemesterConstraint = SemesterConstraint.First
            }),
            ECs = 30,
            Level = 3
        });

        Modules.Add(appliedItSecurity, new Module
        {
            Id = Guid.NewGuid(),
            Name = appliedItSecurity,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 3,
                SemesterConstraint = SemesterConstraint.First
            }),
            ECs = 30,
            Level = 3
        });

        Modules.Add(multidisciplinaireOpdracht, new Module
        {
            Id = Guid.NewGuid(),
            Name = multidisciplinaireOpdracht,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 3,
                ModuleLevelRequirementGroups = [
                    new ModuleLevelRequirementGroup
                    {
                        ModuleLevelRequirements = [
                            new ModuleLevelRequirement
                            {
                                Level = 2
                            },
                            new ModuleLevelRequirement
                            {
                                Level = 2
                            }
                        ]
                    }
                ]
            }),
            ECs = 30,
            Level = 3
        });

        Modules.Add(stage, new Module
        {
            Id = Guid.NewGuid(),
            Name = stage,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                AvailableFromYear = 3
            }),
            ECs = 30,
            Level = 3
        });

        Modules.Add(afstuderenSE, new Module
        {
            Id = Guid.NewGuid(),
            Name = afstuderenSE,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                Propaedeutic = true,
                AvailableFromYear = 3,
                ModuleLevelRequirementGroups = [
                    new ModuleLevelRequirementGroup
                    {
                        ModuleLevelRequirements = [
                            new ModuleLevelRequirement
                            {
                                Level = 2
                            },
                            new ModuleLevelRequirement
                            {
                                Level = 2
                            }
                        ]
                    }
                ],
                ModuleRequirementGroups = [
                    new ModuleRequirementGroup
                    {
                        ModuleRequirements = [
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[multidisciplinaireOpdracht].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[ooSoftwareDesignDevelopment].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[webDevelopment].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            }
                        ]
                    },
                    new ModuleRequirementGroup
                    {
                        ModuleRequirements = [
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[multidisciplinaireOpdracht].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[ooSoftwareDesignDevelopment].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[qualitySoftwareDevelopment].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            }
                        ]
                    }
                ]
            }),
            ECs = 30,
            Level = 4
        });

        Modules.Add(afstuderenIDS, new Module
        {
            Id = Guid.NewGuid(),
            Name = afstuderenIDS,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                Propaedeutic = true,
                AvailableFromYear = 3,
                ModuleLevelRequirementGroups = [
                    new ModuleLevelRequirementGroup
                    {
                        ModuleLevelRequirements = [
                            new ModuleLevelRequirement
                            {
                                Level = 2
                            },
                            new ModuleLevelRequirement
                            {
                                Level = 2
                            }
                        ]
                    }
                ],
                ModuleRequirementGroups = [
                    new ModuleRequirementGroup
                    {
                        ModuleRequirements = [
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[multidisciplinaireOpdracht].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[hybridCloudInfrastructure].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[cloudArchitectureAutomation].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            }
                        ]
                    },
                    new ModuleRequirementGroup
                    {
                        ModuleRequirements = [
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[multidisciplinaireOpdracht].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[hybridCloudInfrastructure].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[appliedItSecurity].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            }
                        ]
                    }
                ]
            }),
            ECs = 30,
            Level = 4
        });

        Modules.Add(afstuderenBIM, new Module
        {
            Id = Guid.NewGuid(),
            Name = afstuderenBIM,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                Propaedeutic = true,
                AvailableFromYear = 3,
                ModuleLevelRequirementGroups = [
                    new ModuleLevelRequirementGroup
                    {
                        ModuleLevelRequirements = [
                            new ModuleLevelRequirement
                            {
                                Level = 2
                            },
                            new ModuleLevelRequirement
                            {
                                Level = 2
                            }
                        ]
                    }
                ],
                ModuleRequirementGroups = [
                    new ModuleRequirementGroup
                    {
                        ModuleRequirements = [
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[multidisciplinaireOpdracht].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[businessProcessManagement].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            },
                            new ModuleRequirement
                            {
                                RelevantModuleId = Modules[dataScience].Id,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 30
                                }
                            }
                        ]
                    }
                ]
            }),
            ECs = 30,
            Level = 4
        });
    }
}
