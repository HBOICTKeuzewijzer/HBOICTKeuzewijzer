using FluentAssertions;
using HBOICTKeuzewijzer.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Tests.Integration.Shared;
using Microsoft.Identity.Client;
using System.Text.Json;

namespace HBOICTKeuzewijzer.Tests.Integration
{
    public class ModuleIntegrationTests
    {

        [Fact]
        public async Task GET_retrieves_Modules()
        {
            using var application = new TestAppFactory();
            using var client = application.Client;

            var response = await client.GetAsync("/Module");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GET_ById_Retrieves_A_Module()
        {

            using var application = new TestAppFactory();
            using var client = application.Client;

            var testModule = new Module
            {
                Id = Guid.NewGuid(),
                Name = "test",
                Code = "ICT-001",
                Description = "testmodule",
                ECs = 5,
                Level = 1,
                Required = false,
                IsPropaedeutic = true,
                Oer = new Oer { Id = Guid.NewGuid(), AcademicYear = "24/25" }
            };

            await SeedHelper.SeedAsync(application.Services, testModule);

            var response = await client.GetAsync($"/Module/{testModule.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task POST_Module_AddsModule()
        {
            using var application = new TestAppFactory();
            using var client = application.Client;

            var testModule = new Module
            {
                Id = Guid.NewGuid(),
                Name = "test1",
                Code = "ICT-002",
                Description = "testmodule1",
                ECs = 5,
                Level = 1,
                Required = false,
                IsPropaedeutic = true,
                Oer = new Oer { Id = Guid.NewGuid(), AcademicYear = "24/25" }
            };

            await SeedHelper.SeedAsync(application.Services, testModule);

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            };

            var content = new StringContent(
                JsonSerializer.Serialize(testModule, options),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/Module", content);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
}
