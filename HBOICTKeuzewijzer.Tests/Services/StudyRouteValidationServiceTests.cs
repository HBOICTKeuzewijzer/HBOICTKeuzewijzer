using AutoFixture;
using FluentAssertions;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using HBOICTKeuzewijzer.Api.Services;
using Newtonsoft.Json;
using System.Collections;
using HBOICTKeuzewijzer.Api.Services.StudyRouteValidation;
using System;

namespace HBOICTKeuzewijzer.Tests.Services
{
    public class StudyRouteValidationServiceTests
    {
        private readonly IFixture _fixture;
        private readonly TestModules _testModules;

        public StudyRouteValidationServiceTests()
        {
            _testModules = new TestModules();

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
        public void ValidateRoute_ReturnValidationError_WhenRouteOnlyContainsOnePModule()
        {
            // Arrange
            var invalidRoute = _fixture.Build<StudyRoute>()
                .Without(s => s.ApplicationUser)
                .Without(s => s.Semesters)
                .Create();

            var faultySemester = TestHelpers.CreateSemester(2, new Module
            {
                ECs = 30,
                Level = 2,
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    Propaedeutic = true
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps, 30));
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(1, new Module
            {
                ECs = 30,
                Level = 1,
                Name = "Test module 1"
            }));
            invalidRoute.Semesters.Add(faultySemester);

            var sut = new StudyRouteValidationService([new PropaedeuticRule()]);

            // Act
            var result = sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemester.Id.ToString());
            result.Errors[faultySemester.Id.ToString()].Length.Should().Be(2);
            result.Errors[faultySemester.Id.ToString()][0].Should()
                .Be($"Module: {faultySemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 2 modules uit de P fase, 1 gevonden.");
            result.Errors[faultySemester.Id.ToString()][1].Should()
                .Be($"Module: {faultySemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 60 ec's behaald in de P fase, huidige ec's 30");
        }

        [Fact]
        public void ValidateRoute_ReturnValidationError_WhenRouteOnlyContainsZeroPModules()
        {
            // Arrange
            var invalidRoute = _fixture.Build<StudyRoute>()
                .Without(s => s.ApplicationUser)
                .Without(s => s.Semesters)
                .Create();

            var faultySemester = TestHelpers.CreateSemester(2, new Module
            {
                ECs = 30,
                Level = 2,
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    Propaedeutic = true
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, new Module
            {
                ECs = 30,
                Level = 1,
                Name = "Test module"
            }));
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(1, new Module
            {
                ECs = 30,
                Level = 1,
                Name = "Test module"
            }));
            invalidRoute.Semesters.Add(faultySemester);

            var sut = new StudyRouteValidationService([new PropaedeuticRule()]);

            // Act
            var result = sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemester.Id.ToString());
            result.Errors[faultySemester.Id.ToString()].Length.Should().Be(2);
            result.Errors[faultySemester.Id.ToString()][0].Should()
                .Be($"Module: {faultySemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 2 modules uit de P fase, 0 gevonden.");
            result.Errors[faultySemester.Id.ToString()][1].Should()
                .Be($"Module: {faultySemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 60 ec's behaald in de P fase, huidige ec's 0");
        }

        [Fact]
        public void ValidateRoute_ReturnValidationError_WhenRouteHasModuleInWrongSemester()
        {
            // Arrange
            var invalidRoute = _fixture.Build<StudyRoute>()
                .Without(s => s.ApplicationUser)
                .Without(s => s.Semesters)
                .Create();

            var faultySemester = TestHelpers.CreateSemester(0, new Module
            {
                Name = "Test module",
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    SemesterConstraint = SemesterConstraint.Second
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(faultySemester);
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(1, new Module
            {
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    SemesterConstraint = SemesterConstraint.Second
                })
            }));

            var sut = new StudyRouteValidationService([new SemesterConstraintRule()]);

            // Act
            var result = sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemester.Id.ToString());
            result.Errors[faultySemester.Id.ToString()].Length.Should().Be(1);
            result.Errors[faultySemester.Id.ToString()][0].Should()
                .Be($"Module: {faultySemester.Module!.Name} kan alleen plaatsvinden in semester {SemesterConstraint.First + 1}.");
        }

        [Fact]
        public void ValidateRoute_ReturnMultipleValidationErrors_WhenRouteHasMultipleModulesInWrongSemester()
        {
            // Arrange
            var invalidRoute = _fixture.Build<StudyRoute>()
                .Without(s => s.ApplicationUser)
                .Without(s => s.Semesters)
                .Create();

            var faultySemesterOne = TestHelpers.CreateSemester(0, new Module
            {
                Name = "Test module",
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    SemesterConstraint = SemesterConstraint.Second
                })
            });

            var faultySemesterTwo = TestHelpers.CreateSemester(1, new Module
            {
                Name = "Test module",
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    SemesterConstraint = SemesterConstraint.First
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(faultySemesterOne);
            invalidRoute.Semesters.Add(faultySemesterTwo);

            var sut = new StudyRouteValidationService([new SemesterConstraintRule()]);

            // Act
            var result = sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(2);
            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors.Keys.Should().Contain(faultySemesterTwo.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(1);
            result.Errors[faultySemesterOne.Id.ToString()][0].Should()
                .Be($"Module: {faultySemesterOne.Module!.Name} kan alleen plaatsvinden in semester {SemesterConstraint.First + 1}.");
            result.Errors[faultySemesterTwo.Id.ToString()].Length.Should().Be(1);
            result.Errors[faultySemesterTwo.Id.ToString()][0].Should()
                .Be($"Module: {faultySemesterTwo.Module!.Name} kan alleen plaatsvinden in semester {SemesterConstraint.Second + 1}.");
        }

        [Fact]
        public void ValidateRoute_ReturnValidationError_WhenRouteDoesNotMeetEcRequirement()
        {
            // Arrange
            var invalidRoute = _fixture.Build<StudyRoute>()
                .Without(s => s.ApplicationUser)
                .Without(s => s.Semesters)
                .Create();

            var faultySemesterOne = TestHelpers.CreateSemester(0, new Module
            {
                Name = "Test module",
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    EcRequirements = new List<EcRequirement>
                    {
                        new EcRequirement
                        {
                            RequiredAmount = 60
                        }
                    }
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps));
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(1, _testModules.BeherenVanEenVerandertraject, 10));
            invalidRoute.Semesters.Add(faultySemesterOne);

            // niet voldaan aan 50ec requirement
            // niet voldaan aan 50ec requirement uit de p fase


            var sut = new StudyRouteValidationService([new EcRequirementRule()]);

            // Act
            var result = sut.ValidateRoute(invalidRoute);
        }
        // hier ga ik werken aan YearConstraints and available from year!!!!!!!!!!

        [Theory]
        [ClassData(typeof(ValidStudyRouteData))]
        public void ValidateStudyRoute_ReturnNull_WithValidStudyRoutes(StudyRoute studyRoute)
        {
            // Arrange
            var sut = new StudyRouteValidationService();

            // Act
            var result = sut.ValidateRoute(studyRoute);

            // Assert
            result.Should().BeNull();
        }
    }
}

public static class TestHelpers
{
    public static Semester CreateSemester(int index, Module module, int acquiredECs = 30)
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
}

public class ValidStudyRouteData : IEnumerable<object[]>
{
    private TestModules _testModules;

    public ValidStudyRouteData()
    {
        _testModules = new TestModules();
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
                    TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    TestHelpers.CreateSemester(1, _testModules.BeherenVanEenVerandertraject),
                    TestHelpers.CreateSemester(2, _testModules.OoSoftwareDesignDevelopment),
                    TestHelpers.CreateSemester(3, _testModules.WebDevelopment),
                    TestHelpers.CreateSemester(4, _testModules.QualitySoftwareDevelopment),
                    TestHelpers.CreateSemester(5, _testModules.MultidisciplinaireOpdracht),
                    TestHelpers.CreateSemester(6, _testModules.DataScience),
                    TestHelpers.CreateSemester(7, _testModules.AfstuderenSE),
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
                    TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    TestHelpers.CreateSemester(1, _testModules.BeherenVanEenVerandertraject),
                    TestHelpers.CreateSemester(2, _testModules.OoSoftwareDesignDevelopment),
                    TestHelpers.CreateSemester(3, _testModules.WebDevelopment),
                    TestHelpers.CreateSemester(4, _testModules.ManagementOfIt),
                    TestHelpers.CreateSemester(5, _testModules.Stage),
                    TestHelpers.CreateSemester(6, _testModules.MultidisciplinaireOpdracht),
                    TestHelpers.CreateSemester(7, _testModules.AfstuderenSE),
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
                    TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    TestHelpers.CreateSemester(1, _testModules.BeherenVanEenVerandertraject),
                    TestHelpers.CreateSemester(2, _testModules.HybridCloudInfrastructure),
                    TestHelpers.CreateSemester(3, _testModules.CloudArchitectureAutomation),
                    TestHelpers.CreateSemester(4, _testModules.OoSoftwareDesignDevelopment),
                    TestHelpers.CreateSemester(5, _testModules.MultidisciplinaireOpdracht),
                    TestHelpers.CreateSemester(6, _testModules.Stage),
                    TestHelpers.CreateSemester(7, _testModules.AfstuderenIDS),
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
                    TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    TestHelpers.CreateSemester(1, _testModules.BeherenVanEenVerandertraject),
                    TestHelpers.CreateSemester(2, _testModules.HybridCloudInfrastructure),
                    TestHelpers.CreateSemester(3, _testModules.CloudArchitectureAutomation),
                    TestHelpers.CreateSemester(4, _testModules.AppliedItSecurity),
                    TestHelpers.CreateSemester(5, _testModules.MultidisciplinaireOpdracht),
                    TestHelpers.CreateSemester(6, _testModules.Stage),
                    TestHelpers.CreateSemester(7, _testModules.AfstuderenIDS),
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
                    TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    TestHelpers.CreateSemester(1, _testModules.BeherenVanEenVerandertraject),
                    TestHelpers.CreateSemester(2, _testModules.BusinessProcessManagement),
                    TestHelpers.CreateSemester(3, _testModules.DataScience),
                    TestHelpers.CreateSemester(4, _testModules.ManagementOfIt),
                    TestHelpers.CreateSemester(5, _testModules.CloudArchitectureAutomation),
                    TestHelpers.CreateSemester(6, _testModules.MultidisciplinaireOpdracht),
                    TestHelpers.CreateSemester(7, _testModules.AfstuderenBIM),
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
                    TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps),
                    TestHelpers.CreateSemester(1, _testModules.BeherenVanEenVerandertraject),
                    TestHelpers.CreateSemester(2, _testModules.BusinessProcessManagement),
                    TestHelpers.CreateSemester(3, _testModules.DataScience),
                    TestHelpers.CreateSemester(4, _testModules.AppliedItSecurity),
                    TestHelpers.CreateSemester(5, _testModules.MultidisciplinaireOpdracht),
                    TestHelpers.CreateSemester(6, _testModules.Stage),
                    TestHelpers.CreateSemester(7, _testModules.AfstuderenBIM),
                }
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


public class TestModules
{
    #region Fields

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

    #endregion

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
