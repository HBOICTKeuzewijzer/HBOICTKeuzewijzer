using HBOICTKeuzewijzer.Api.Models;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HBOICTKeuzewijzer.Tests.Integration.Shared;
using Azure;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Tests.Integration;

public class CategoryIntegrationTests
{
    public class Post : CategoryIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin")]
        [InlineData("ModuleAdmin")]
        public async Task PostCategory_CreatesEntryInDatabase(string role)
        {
            using var factory = new TestAppFactory();

            var newCategory = new Category
            {
                Id = Guid.NewGuid(),
                Value = "Test Category",
                AccentColor = "#fff",
                PrimaryColor = "#aaa",
                Position = 1
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/Category")
            {
                Content = JsonContent.Create(newCategory)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location!.AbsolutePath.Should().Be($"/Category/{newCategory.Id}");

            Category? saved;
            await using (var context = factory.CreateDbContext())
            {
                saved = await context.Categories.FindAsync(newCategory.Id);
            }

            saved.Should().NotBeNull();
            saved!.Value.Should().Be("Test Category");
            saved!.AccentColor.Should().Be("#fff");
            saved!.Position.Should().Be(1);
            saved!.PrimaryColor.Should().Be("#aaa");
        }

        [Fact]
        public async Task PostCategory_RespondsWithUnauthorized_WhenNotAuthenticated()
        {
            using var factory = new TestAppFactory();

            var category = new Category
            {
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category updated"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"/Category")
            {
                Content = JsonContent.Create(category)
            };

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("User")]
        [InlineData("Student")]
        [InlineData("SLB")]
        public async Task PostCategory_RespondsWithForbidden_WhenRolesAreNotCorrect(string role)
        {
            using var factory = new TestAppFactory();

            var category = new Category
            {
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category updated"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"/Category")
            {
                Content = JsonContent.Create(category)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    public class Put : CategoryIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin")]
        [InlineData("ModuleAdmin")]
        public async Task PutCategory_UpdatesCategoryInDatabase(string role)
        {
            using var factory = new TestAppFactory();

            var categoryId = Guid.NewGuid();

            var oldCategory = new Category
            {
                Id = categoryId,
                AccentColor = "#fff",
                PrimaryColor = "#000",
                Position = 1,
                Value = "Test category"
            };

            await SeedHelper.SeedAsync(factory.Services, oldCategory);

            var updatedCategory = new Category
            {
                Id = categoryId,
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category updated"
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{updatedCategory.Id}")
            {
                Content = JsonContent.Create(updatedCategory)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            Category? saved;
            await using (var context = factory.CreateDbContext())
            {
                saved = await context.Categories.FindAsync(updatedCategory.Id);
            }

            saved.Should().NotBeNull();
            saved!.Value.Should().Be("Test category updated");
            saved!.AccentColor.Should().Be("#aaa");
            saved!.Position.Should().Be(1);
            saved!.PrimaryColor.Should().Be("#fff");
        }

        [Theory]
        [InlineData("SystemAdmin")]
        [InlineData("ModuleAdmin")]
        public async Task PutCategory_ShouldNotUpdateModules(string role)
        {
            using var factory = new TestAppFactory();

            var categoryId = Guid.NewGuid();
            var oerId = Guid.NewGuid();

            await SeedHelper.SeedAsync(factory.Services, new Oer
            {
                AcademicYear = "24/25",
                Id = oerId
            });
            await SeedHelper.SeedAsync(factory.Services, new Category
            {
                Id = categoryId,
                AccentColor = "#fff",
                PrimaryColor = "#000",
                Position = 1,
                Value = "Test category"
            });

            var updatedCategory = new Category
            {
                Id = categoryId,
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category updated",
                Modules = new List<Module>
            {
                new Module
                {
                    CategoryId = categoryId,
                    Code = "12345",
                    Description = "niks",
                    ECs = 30,
                    Level = 2,
                    Name = "Test module 1",
                    OerId = oerId
                }
            }
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{updatedCategory.Id}")
            {
                Content = JsonContent.Create(updatedCategory)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            List<Module> modules;
            await using (var context = factory.CreateDbContext())
            {
                modules = await context.Modules.ToListAsync();
            }

            modules.Should().NotBeNull();
            modules.Should().NotContain(m => m.Code == "12345");
        }

        [Theory]
        [InlineData("SystemAdmin")]
        [InlineData("ModuleAdmin")]
        public async Task PutCategory_RespondsWithBadRequest_WhenIdDoesNotMatchCategory(string role)
        {
            using var factory = new TestAppFactory();

            var category = new Category
            {
                Id = Guid.NewGuid(),
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category updated"
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{/*invalid id*/Guid.NewGuid()}")
            {
                Content = JsonContent.Create(category)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PutCategory_RespondsWithUnauthorized_WhenNotAuthenticated()
        {
            using var factory = new TestAppFactory();

            var category = new Category
            {
                Id = Guid.NewGuid(),
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category updated"
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{category.Id}")
            {
                Content = JsonContent.Create(category)
            };

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("User")]
        [InlineData("Student")]
        [InlineData("SLB")]
        public async Task PutCategory_RespondsWithForbidden_WhenRolesAreNotCorrect(string role)
        {
            using var factory = new TestAppFactory();

            var category = new Category
            {
                Id = Guid.NewGuid(),
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category updated"
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{category.Id}")
            {
                Content = JsonContent.Create(category)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    public class Get : CategoryIntegrationTests
    {
        [Fact]
        public async Task GetCategory_ReturnsCategoryBasedOnId()
        {
            using var factory = new TestAppFactory();

            var categoryId = Guid.NewGuid();

            var category = new Category
            {
                Id = categoryId,
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category"
            };

            await SeedHelper.SeedAsync(factory.Services, category);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/Category/{category.Id}")
            {
                Content = JsonContent.Create(category)
            };

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var returnedCategory = await response.Content.ReadFromJsonAsync<Category>();

            returnedCategory.Should().NotBeNull();
            returnedCategory!.Id.Should().Be(category.Id);
            returnedCategory.Value.Should().Be(category.Value);
            returnedCategory.AccentColor.Should().Be(category.AccentColor);
            returnedCategory.PrimaryColor.Should().Be(category.PrimaryColor);
            returnedCategory.Position.Should().Be(category.Position);
        }

        [Fact]
        public async Task GetCategory_RespondsWithNotFound_WhenCategoryIdIsUnknown()
        {
            using var factory = new TestAppFactory();

            var categoryId = Guid.NewGuid();

            var category = new Category
            {
                Id = categoryId,
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category"
            };

            await SeedHelper.SeedAsync(factory.Services, category);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/Category/{Guid.NewGuid()}")
            {
                Content = JsonContent.Create(category)
            };

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCategories_ReturnsAllCategoriesOrderedByPosition()
        {
            using var factory = new TestAppFactory();

            var categoryOne = new Category
            {
                Id = Guid.NewGuid(),
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category"
            };

            var categoryTwo = new Category
            {
                Id = Guid.NewGuid(),
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 2,
                Value = "Test category"
            };

            var categoryThree = new Category
            {
                Id = Guid.NewGuid(),
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category"
            };

            await SeedHelper.SeedRangeAsync(factory.Services, [categoryOne, categoryThree, categoryTwo]);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/Category");

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var returnedCategories = await response.Content.ReadFromJsonAsync<List<Category>>();
            returnedCategories.Should().HaveCount(3);
            returnedCategories
                .Select(c => c.Position)
                .Should()
                .BeInAscendingOrder();
        }
    }
    
    public class Delete : CategoryIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin")]
        [InlineData("ModuleAdmin")]
        public async Task DeleteCategory_RemovesCategoryFromDatabase(string role)
        {
            using var factory = new TestAppFactory();

            var deleteCategoryId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            await SeedHelper.SeedAsync(factory.Services, new Category
            {
                Id = deleteCategoryId,
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category"
            });

            await SeedHelper.SeedAsync(factory.Services, new Category
            {
                Id = categoryId,
                AccentColor = "#aaa",
                PrimaryColor = "#fff",
                Position = 1,
                Value = "Test category"
            });

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/Category/{deleteCategoryId}");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            List<Category> currentCategories;
            await using (var context = factory.CreateDbContext())
            {
                currentCategories = await context.Categories.ToListAsync();
            }

            currentCategories.Should().NotBeNull();
            currentCategories.Should().NotContain(c => c.Id == deleteCategoryId);
            currentCategories.Should().Contain(c => c.Id == categoryId);
        }

        [Theory]
        [InlineData("SystemAdmin")]
        [InlineData("ModuleAdmin")]
        public async Task DeleteCategory_RespondsWithNotFound_WhenCategoryIdIsUnknown(string role)
        {
            using var factory = new TestAppFactory();

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/Category/{Guid.NewGuid()}");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("User")]
        [InlineData("Student")]
        [InlineData("SLB")]
        public async Task DeleteCategory_RespondsWithForbidden_WhenRolesAreNotCorrect(string role)
        {
            using var factory = new TestAppFactory();

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/Category/{Guid.NewGuid()}");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteCategory_RespondsWithUnauthorized_WhenNotAuthenticated()
        {
            using var factory = new TestAppFactory();

            var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{Guid.NewGuid()}");

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
