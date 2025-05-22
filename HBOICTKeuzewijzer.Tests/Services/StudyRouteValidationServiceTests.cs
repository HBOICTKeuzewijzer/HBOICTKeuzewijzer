using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Services;
using Xunit;

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
    }
}

