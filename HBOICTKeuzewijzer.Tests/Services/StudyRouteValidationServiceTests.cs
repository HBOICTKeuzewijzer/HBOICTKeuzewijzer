using AutoFixture;
using FluentAssertions;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using HBOICTKeuzewijzer.Api.Services.StudyRouteValidation;
using HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators;
using Newtonsoft.Json;
using System.Collections;
using System.Text;

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

        public List<IStudyRouteValidationRule> GetValidators()
        {
            return
            [
                new PropaedeuticRule(),
                new PropaedeuticRule(),
                new EcRequirementRule(),
                new ModuleRequirementRule(id =>
                {
                    var module = _testModules.GetById(id);
                    return Task.FromResult<Module?>(module);
                })
            ];
        }

        [Fact]
        public async Task ValidateRoute_ThrowsArgumentNullException_WhenPassedRouteIsNull()
        {
            var sut = new StudyRouteValidationService(GetValidators());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            await sut.Awaiting(s => s.ValidateRoute(null))
                .Should().ThrowAsync<ArgumentNullException>();
#pragma warning restore CS8625
        }


        [Fact]
        public async Task ValidateRoute_ReturnsNull_WhenSemestersIsNull()
        {
            var route = _fixture.Build<StudyRoute>()
                .With(r => r.Semesters, (List<Semester>?)null)
                .Create();

            var sut = new StudyRouteValidationService(GetValidators());

            var result = await sut.ValidateRoute(route);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateRoute_ReturnsNull_WhenSemestersIsEmpty()
        {
            var route = _fixture.Build<StudyRoute>()
                .With(r => r.Semesters, new List<Semester>())
                .Create();

            var sut = new StudyRouteValidationService(GetValidators());

            var result = await sut.ValidateRoute(route);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationError_WhenRouteOnlyContainsOnePModule()
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
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemester.Id.ToString());
            result.Errors[faultySemester.Id.ToString()].Length.Should().Be(2);
            result.Errors[faultySemester.Id.ToString()][0].Should()
                .Be($"Module: {faultySemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 2 modules uit de P fase, 1 gevonden.");
            result.Errors[faultySemester.Id.ToString()][1].Should()
                .Be($"Module: {faultySemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 60 ec's behaald in de P fase, huidige ec's 30.");
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationError_WhenRouteOnlyContainsZeroPModules()
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
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemester.Id.ToString());
            result.Errors[faultySemester.Id.ToString()].Length.Should().Be(2);
            result.Errors[faultySemester.Id.ToString()][0].Should()
                .Be($"Module: {faultySemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 2 modules uit de P fase, 0 gevonden.");
            result.Errors[faultySemester.Id.ToString()][1].Should()
                .Be($"Module: {faultySemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 60 ec's behaald in de P fase, huidige ec's 0.");
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationError_WhenRouteHasModuleInWrongSemester()
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
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemester.Id.ToString());
            result.Errors[faultySemester.Id.ToString()].Length.Should().Be(1);
            result.Errors[faultySemester.Id.ToString()][0].Should()
                .Be($"Module: {faultySemester.Module!.Name} kan alleen plaatsvinden in semester {(int)SemesterConstraint.First + 1}.");
        }

        [Fact]
        public async Task ValidateRoute_ReturnMultipleValidationErrors_WhenRouteHasMultipleModulesInWrongSemester()
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
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(2);
            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors.Keys.Should().Contain(faultySemesterTwo.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(1);
            result.Errors[faultySemesterOne.Id.ToString()][0].Should()
                .Be($"Module: {faultySemesterOne.Module!.Name} kan alleen plaatsvinden in semester {(int)SemesterConstraint.First + 1}.");
            result.Errors[faultySemesterTwo.Id.ToString()].Length.Should().Be(1);
            result.Errors[faultySemesterTwo.Id.ToString()][0].Should()
                .Be($"Module: {faultySemesterTwo.Module!.Name} kan alleen plaatsvinden in semester {(int)SemesterConstraint.Second + 1}.");
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationError_WhenRouteDoesNotMeetEcRequirement()
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

            var sut = new StudyRouteValidationService([new EcRequirementRule()]);

            // Act
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(1);
            result.Errors[faultySemesterOne.Id.ToString()][0].Should()
                .Be($"Module: {faultySemesterOne.Module!.Name} verwacht dat uit voorgaande modules minimaal 60 ec zijn behaald, huidige behaalde ec's is 40.");
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationErrors_WhenRouteDoesNotMeetEcRequirement()
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
                            RequiredAmount = 70
                        }
                    }
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps));
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(1, _testModules.BeherenVanEenVerandertraject, 10));
            invalidRoute.Semesters.Add(faultySemesterOne);

            var sut = new StudyRouteValidationService([new EcRequirementRule()]);

            // Act
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(2);
            result.Errors[faultySemesterOne.Id.ToString()][0].Should()
                .Be($"Module: {faultySemesterOne.Module!.Name} verwacht dat uit voorgaande modules minimaal 70 ec zijn behaald, huidige behaalde ec's is 40.");
            result.Errors[faultySemesterOne.Id.ToString()][1].Should()
                .Be($"Module: {faultySemesterOne.Module!.Name} verwacht dat uit voorgaande modules minimaal 70 ec behaalbaar zijn, huidige behaalbare ec's is 60.");
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationErrorsForP_WhenRouteDoesNotMeetEcRequirement()
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
                            RequiredAmount = 60,
                            Propaedeutic = true
                        }
                    }
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps));
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.ManagementOfIt));
            invalidRoute.Semesters.Add(faultySemesterOne);

            var sut = new StudyRouteValidationService([new EcRequirementRule()]);

            // Act
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(2);
            result.Errors[faultySemesterOne.Id.ToString()][0].Should()
                .Be($"Module: {faultySemesterOne.Module!.Name} verwacht dat uit propedeuse minimaal 60 ec zijn behaald, huidige behaalde ec's is 30.");
            result.Errors[faultySemesterOne.Id.ToString()][1].Should()
                .Be($"Module: {faultySemesterOne.Module!.Name} verwacht dat uit propedeuse minimaal 60 ec behaalbaar zijn, huidige behaalbare ec's is 30.");
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationError_WhenRouteDoesNotMeetModuleRequirements()
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
                    ModuleRequirementGroups = [
                        new ModuleRequirementGroup {
                            ModuleRequirements = [
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.OoSoftwareDesignDevelopment.Id
                                },
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.WebDevelopment.Id
                                }
                            ]
                        },
                        new ModuleRequirementGroup
                        {
                            ModuleRequirements = [
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.OoSoftwareDesignDevelopment.Id,
                                },
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.QualitySoftwareDevelopment.Id
                                },
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.BedrijfsProcessenDynamischeWebapps.Id,
                                    EcRequirement = new EcRequirement
                                    {
                                        RequiredAmount = 30
                                    }
                                }
                            ]
                        }
                    ]
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps, 20));
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.ManagementOfIt));
            invalidRoute.Semesters.Add(faultySemesterOne);

            var sut = new StudyRouteValidationService([new ModuleRequirementRule(id =>
                {
                    var module = _testModules.GetById(id);
                    return Task.FromResult<Module?>(module);
                })]);

            // Act
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(1);

            var errors = result.Errors[faultySemesterOne.Id.ToString()];

            var sb = new StringBuilder();
            sb.AppendLine("Module: Test module verwacht dat een van de volgende groepen modules aanwezig is in de voorgaande semesters:");
            sb.AppendLine();
            sb.AppendLine("Groep 1:");
            sb.AppendLine($"- {_testModules.OoSoftwareDesignDevelopment.Name} niet gevonden.");
            sb.AppendLine($"- {_testModules.WebDevelopment.Name} niet gevonden.");
            sb.AppendLine();
            sb.AppendLine("Groep 2:");
            sb.AppendLine($"- {_testModules.OoSoftwareDesignDevelopment.Name} niet gevonden.");
            sb.AppendLine($"- {_testModules.QualitySoftwareDevelopment.Name} niet gevonden.");
            sb.AppendLine($"- {_testModules.BedrijfsProcessenDynamischeWebapps.Name} wel gevonden maar voldoet niet aan de behaalde ec eis van 30.");

            errors[0].Should()
                .Be(sb.ToString());
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationError_WhenRouteDoesNotMeetModuleRequirementsEvenIfOneMatches()
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
                    ModuleRequirementGroups = [
                        new ModuleRequirementGroup {
                            ModuleRequirements = [
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.OoSoftwareDesignDevelopment.Id
                                },
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.WebDevelopment.Id
                                },
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.BedrijfsProcessenDynamischeWebapps.Id
                                }
                            ]
                        }
                    ]
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps, 20));
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.ManagementOfIt));
            invalidRoute.Semesters.Add(faultySemesterOne);

            var sut = new StudyRouteValidationService([new ModuleRequirementRule(id =>
                {
                    var module = _testModules.GetById(id);
                    return Task.FromResult<Module?>(module);
                })]);

            // Act
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(1);

            var errors = result.Errors[faultySemesterOne.Id.ToString()];

            var sb = new StringBuilder();
            sb.AppendLine("Module: Test module verwacht dat de volgende groep modules aanwezig is in de voorgaande semesters:");
            sb.AppendLine();
            sb.AppendLine("Groep 1:");
            sb.AppendLine($"- {_testModules.OoSoftwareDesignDevelopment.Name} niet gevonden.");
            sb.AppendLine($"- {_testModules.WebDevelopment.Name} niet gevonden.");
            sb.AppendLine($"- {_testModules.BedrijfsProcessenDynamischeWebapps.Name} gevonden.");

            errors[0].Should()
                .Be(sb.ToString());
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationError_WhenRouteDoesNotMeetModuleLevelRequirements()
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
                    ModuleLevelRequirementGroups = [
                        new ModuleLevelRequirementGroup
                        {
                            ModuleLevelRequirements = [
                                new ModuleLevelRequirement
                                {
                                    Level = 1
                                },
                                new ModuleLevelRequirement
                                {
                                    Level = 1
                                },
                                new ModuleLevelRequirement
                                {
                                    Level = 1
                                }
                            ]
                        },
                        new ModuleLevelRequirementGroup
                        {
                            ModuleLevelRequirements = [
                                new ModuleLevelRequirement
                                {
                                    Level = 1,
                                    EcRequirement = new EcRequirement
                                    {
                                        RequiredAmount = 30
                                    }
                                },
                                new ModuleLevelRequirement
                                {
                                    Level = 1,
                                    EcRequirement = new EcRequirement
                                    {
                                        RequiredAmount = 30
                                    }
                                }
                            ]
                        },
                    ]
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps, 30));
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BeherenVanEenVerandertraject, 20));
            invalidRoute.Semesters.Add(faultySemesterOne);

            var sut = new StudyRouteValidationService([new ModuleLevelRequirementRule()]);

            // Act
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(1);

            var errors = result.Errors[faultySemesterOne.Id.ToString()];

            var sb = new StringBuilder();
            sb.AppendLine("Module: Test module verwacht dat een van de volgende groepen module niveaus aanwezig is in de voorgaande semesters:");
            sb.AppendLine();
            sb.AppendLine("Groep 1:");
            sb.AppendLine("- Niveau 1 gevonden.");
            sb.AppendLine("- Niveau 1 gevonden.");
            sb.AppendLine("- Niveau 1 niet gevonden.");
            sb.AppendLine();
            sb.AppendLine("Groep 2:");
            sb.AppendLine("- Niveau 1 gevonden.");
            sb.AppendLine("- Niveau 1 wel gevonden maar gevonden voldoet niet aan de behaalde ec eis van 30.");


            errors[0].Should()
                .Be(sb.ToString());
        }

        [Fact]
        public async Task ValidateRoute_ReturnValidationError_WhenRouteDoesNotMeetModuleLevelRequirementsEvenIfLastOneMatches()
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
                                },
                                new ModuleLevelRequirement
                                {
                                    Level = 1
                                }
                            ]
                        }
                    ]
                })
            });

            invalidRoute.Semesters = new List<Semester>();
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps, 30));
            invalidRoute.Semesters.Add(TestHelpers.CreateSemester(0, _testModules.BeherenVanEenVerandertraject, 20));
            invalidRoute.Semesters.Add(faultySemesterOne);

            var sut = new StudyRouteValidationService([new ModuleLevelRequirementRule()]);

            // Act
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(1);
            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(1);

            var errors = result.Errors[faultySemesterOne.Id.ToString()];

            var sb = new StringBuilder();
            sb.AppendLine("Module: Test module verwacht dat de volgende groep module niveaus aanwezig is in de voorgaande semesters:");
            sb.AppendLine();
            sb.AppendLine("Groep 1:");
            sb.AppendLine("- Niveau 2 niet gevonden.");
            sb.AppendLine("- Niveau 2 niet gevonden.");
            sb.AppendLine("- Niveau 1 gevonden.");

            errors[0].Should()
                .Be(sb.ToString());
        }

        [Fact]
        public Task ValidateYearconstraint_ReturnValidationError_Whenyearnotmeettherequirements()
        {
            var semester = TestHelpers.CreateSemester(0, new Module
            {
                Name = "Test module met year constraint",
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    YearConstraints = new List<int> { 2 },
                    AvailableFromYear = 2
                })
            });

            var route = new StudyRoute
            {
                Semesters = new List<Semester> { semester }
            };

            var sut = new StudyRouteValidationService(new List<IStudyRouteValidationRule>
            {
                new YearRequirementRule()
            });

            var result = sut.ValidateRoute(route).GetAwaiter().GetResult();

            result.Should().NotBeNull();
            result.Errors.Should().ContainKey(semester.Id.ToString());
            result.Errors[semester.Id.ToString()].Should().Contain(m => m.Contains("mag alleen gevolgd worden in jaar"));
            result.Errors[semester.Id.ToString()].Should().Contain(m => m.Contains("beschikbaar vanaf jaar"));

            return Task.CompletedTask; 
        }


        /// <summary>
        /// Sanity check if valid routes are still being accepted with all rules implemented.
        /// </summary>
        /// <param name="studyRoute"></param>
        /// <returns></returns>
        [Theory]
        [ClassData(typeof(ValidStudyRouteData))]
        public async Task ValidateStudyRoute_ReturnNull_WithValidStudyRoutes(StudyRoute studyRoute)
        {
            // Arrange
            var sut = new StudyRouteValidationService(GetValidators());

            // Act
            var result = await sut.ValidateRoute(studyRoute);

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Sanity check if each type of rule still writes an error when they are all supposed to.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ValidateRoute_ReturnsAllValidationErrors_WhenRouteViolatesAllRules()
        {
            // Arrange
            var invalidRoute = _fixture.Build<StudyRoute>()
                .Without(s => s.ApplicationUser)
                .Without(s => s.Semesters)
                .Create();

            // Module that violates all rules
            var faultySemesterOne = TestHelpers.CreateSemester(0, new Module
            {
                Name = "Comprehensive Test Module one",
                Level = 2,
                ECs = 30,
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    Propaedeutic = true,
                    SemesterConstraint = SemesterConstraint.Second,
                    EcRequirements = new List<EcRequirement>
                    {
                        new EcRequirement { RequiredAmount = 60 },
                        new EcRequirement { RequiredAmount = 60, Propaedeutic = true }
                    },
                    ModuleLevelRequirementGroups =
                    [
                        new ModuleLevelRequirementGroup
                        {
                            ModuleLevelRequirements =
                            [
                                new ModuleLevelRequirement
                                {
                                    Level = 1
                                },
                                new ModuleLevelRequirement
                                {
                                    Level = 2
                                }
                            ]
                        },
                        new ModuleLevelRequirementGroup
                        {
                            ModuleLevelRequirements =
                            [
                                new ModuleLevelRequirement
                                {
                                    Level = 1,
                                    EcRequirement = new EcRequirement
                                    {
                                        RequiredAmount = 30
                                    }
                                }
                            ]
                        }
                    ]
                })
            });

            var faultySemesterTwo = TestHelpers.CreateSemester(0, new Module
            {
                Name = "Comprehensive Test Module two",
                Level = 2,
                ECs = 30,
                PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
                {
                    Propaedeutic = true,
                    SemesterConstraint = SemesterConstraint.Second,
                    EcRequirements = new List<EcRequirement>
                    {
                        new EcRequirement { RequiredAmount = 60 },
                        new EcRequirement { RequiredAmount = 60, Propaedeutic = true }
                    },
                    ModuleRequirementGroups =
                    [
                        new ModuleRequirementGroup
                        {
                            ModuleRequirements =
                            [
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.WebDevelopment.Id
                                },
                                new ModuleRequirement
                                {
                                    RelevantModuleId = _testModules.ManagementOfIt.Id,
                                    EcRequirement = new EcRequirement
                                    {
                                        RequiredAmount = 30
                                    }
                                }
                            ]
                        }
                    ]
                })
            });

            invalidRoute.Semesters = new List<Semester>
            {
                // These will partially satisfy things but still fail on each rule
                TestHelpers.CreateSemester(0, _testModules.BedrijfsProcessenDynamischeWebapps, 20),
                TestHelpers.CreateSemester(0, _testModules.BeherenVanEenVerandertraject, 10),
                faultySemesterOne,
                faultySemesterTwo
            };

            var sut = new StudyRouteValidationService([
                new PropaedeuticRule(),
                new SemesterConstraintRule(),
                new EcRequirementRule(),
                new ModuleRequirementRule(id =>
                {
                    var module = _testModules.GetById(id);
                    return Task.FromResult<Module?>(module);
                }),
                new ModuleLevelRequirementRule()
            ]);

            // Act
            var result = await sut.ValidateRoute(invalidRoute);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeNull();
            result.Errors.Count.Should().Be(2);

            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors[faultySemesterOne.Id.ToString()].Length.Should().Be(5);
            var errorsSemesterOne = result.Errors[faultySemesterOne.Id.ToString()];
            errorsSemesterOne[0].Should().Be($"Module: {faultySemesterOne.Module!.Name} verwacht een voltooide propedeuse, minimaal 60 ec's behaald in de P fase, huidige ec's 30.");
            errorsSemesterOne[1].Should().Be($"Module: {faultySemesterOne.Module!.Name} kan alleen plaatsvinden in semester 1.");
            errorsSemesterOne[2].Should().Be($"Module: {faultySemesterOne.Module!.Name} verwacht dat uit voorgaande modules minimaal 60 ec zijn behaald, huidige behaalde ec's is 30.");
            errorsSemesterOne[3].Should().Be($"Module: {faultySemesterOne.Module!.Name} verwacht dat uit propedeuse minimaal 60 ec zijn behaald, huidige behaalde ec's is 30.");

            var sbSemesterOne = new StringBuilder();
            sbSemesterOne.AppendLine($"Module: {faultySemesterOne.Module!.Name} verwacht dat een van de volgende groepen module niveaus aanwezig is in de voorgaande semesters:");
            sbSemesterOne.AppendLine();
            sbSemesterOne.AppendLine("Groep 1:");
            sbSemesterOne.AppendLine("- Niveau 1 gevonden.");
            sbSemesterOne.AppendLine("- Niveau 2 niet gevonden.");
            sbSemesterOne.AppendLine();
            sbSemesterOne.AppendLine("Groep 2:");
            sbSemesterOne.AppendLine("- Niveau 1 wel gevonden maar gevonden voldoet niet aan de behaalde ec eis van 30.");

            errorsSemesterOne[4].Should().Be(sbSemesterOne.ToString());

            result.Errors.Keys.Should().Contain(faultySemesterOne.Id.ToString());
            result.Errors[faultySemesterTwo.Id.ToString()].Length.Should().Be(4);
            var errorsSemesterTwo = result.Errors[faultySemesterTwo.Id.ToString()];
            errorsSemesterTwo[0].Should().Be($"Module: {faultySemesterTwo.Module!.Name} verwacht een voltooide propedeuse, minimaal 60 ec's behaald in de P fase, huidige ec's 30.");
            errorsSemesterTwo[1].Should().Be($"Module: {faultySemesterTwo.Module!.Name} kan alleen plaatsvinden in semester 1.");
            errorsSemesterTwo[2].Should().Be($"Module: {faultySemesterTwo.Module!.Name} verwacht dat uit propedeuse minimaal 60 ec zijn behaald, huidige behaalde ec's is 30.");

            var sbSemesterTwo = new StringBuilder();
            sbSemesterTwo.AppendLine($"Module: {faultySemesterTwo.Module!.Name} verwacht dat de volgende groep modules aanwezig is in de voorgaande semesters:");
            sbSemesterTwo.AppendLine();
            sbSemesterTwo.AppendLine("Groep 1:");
            sbSemesterTwo.AppendLine("- Web Development niet gevonden.");
            sbSemesterTwo.AppendLine("- Management of IT niet gevonden.");

            errorsSemesterTwo[3].Should().Be(sbSemesterTwo.ToString());
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
                Id = new Guid("392e1cd7-6374-490d-a473-fc1d404538ce"),
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
                Id = new Guid("413b3f8c-f0d9-405e-8331-9b8be4bc967f"),
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
                Id = new Guid("8d1adff6-b31b-4bc0-9e37-9997dc211cb0"),
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
                Id = new Guid("55bd3618-f85a-4379-9756-4fa6a8c94ce8"),
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
                Id = new Guid("2f595dde-0314-4291-a185-03e315e9c3ee"),
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
                Id = new Guid("60af565a-dab0-444a-a616-7e1bd6313506"),
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

    public Module? GetById(Guid id)
    {
        return Modules.Values.FirstOrDefault(m => m.Id == id);
    }

    public TestModules()
    {
        Modules = new Dictionary<string, Module>();

        Modules.Add(bedrijfsProcessenDynamischeWebapps, new Module
        {
            Id = new Guid("62c7e4c9-b64a-4e2e-b40c-2fffa6c589f5"),
            Name = bedrijfsProcessenDynamischeWebapps,
            PrerequisiteJson = JsonConvert.SerializeObject(new ModulePrerequisite
            {
                YearConstraints = [1]
            }),
            IsPropaedeutic = true,
            ECs = 30,
            Level = 1,
            Required = true,
            RequiredSemester = 1
        });

        Modules.Add(beherenVanEenVerandertraject, new Module
        {
            Id = new Guid("4658a381-45bc-4fcb-bcc4-6a90bd0ba8d0"),
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
            Id = new Guid("d5dbabd3-b94d-487a-86fb-00b9393ae099"),
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
            Id = new Guid("fcf7409f-f161-4457-b3cf-1f93cd8b5a6a"),
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
            Id = new Guid("70361855-035f-4f49-bc39-f3cb8f1c0e58"),
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
            Id = new Guid("61ef8516-1b98-40b8-aa91-cdc954a5cc1f"),
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
            Id = new Guid("28f907ed-1a05-4957-82e8-4ec7f27b6d08"),
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
            Id = new Guid("191b7c83-402c-48de-b543-12b7ab8349cd"),
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
            Id = new Guid("3f45991c-cc88-4d8d-b184-057f5e58376a"),
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
            Id = new Guid("e0ee9325-196e-45cf-b500-78b1e874f2b5"),
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
            Id = new Guid("9ff00680-8725-4d35-9fe9-3ac7f51cba39"),
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
            Id = new Guid("e3026336-e59e-41fd-a84b-dfa24f355cb9"),
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
                                Level = 2,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 60
                                }
                            },
                            new ModuleLevelRequirement
                            {
                                Level = 2,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 60
                                }
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
            Id = new Guid("43ee338a-0c0a-4b33-8338-a8e3ad9ed0e0"),
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
            Id = new Guid("8d779309-80df-43ee-8c5a-d35b86c255d8"),
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
                                Level = 2,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 60
                                }
                            },
                            new ModuleLevelRequirement
                            {
                                Level = 2,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 60
                                }
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
            Id = new Guid("16a71c27-8ecb-4078-9b5b-caf349459d17"),
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
                                Level = 2,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 60
                                }
                            },
                            new ModuleLevelRequirement
                            {
                                Level = 2,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 60
                                }
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
            Id = new Guid("ea99db39-6fc0-4b13-b9a2-3c708816a85c"),
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
                                Level = 2,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 60
                                }
                            },
                            new ModuleLevelRequirement
                            {
                                Level = 2,
                                EcRequirement = new EcRequirement
                                {
                                    RequiredAmount = 60
                                }
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
