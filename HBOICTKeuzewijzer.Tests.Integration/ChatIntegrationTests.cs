using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Tests.Integration.Shared;

namespace HBOICTKeuzewijzer.Tests.Integration;

public class ChatIntegrationTests
{
    public class Post : ChatIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin")]
        [InlineData("ModuleAdmin")]
        [InlineData("User")]
        public async Task PostChat_CreatesEntryInDatabase(string role)
        {
            using var factory = new TestAppFactory();

            var userExternalId = Guid.NewGuid().ToString();
            var otherUserExternalId = Guid.NewGuid().ToString();

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                ExternalId = userExternalId,
                Email = $"{role.ToLower()}@test.nl",
                DisplayName = $"{role} User"
            };
            await SeedHelper.SeedAsync(factory.Services, user);
            await SeedHelper.SeedAsync(factory.Services, new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = user.Id,
                Role = Enum.Parse<Role>(role)
            });

            var otherUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                ExternalId = otherUserExternalId,
                Email = $"other_{role.ToLower()}@test.nl",
                DisplayName = $"Other {role} User"
            };
            await SeedHelper.SeedAsync(factory.Services, otherUser);
            await SeedHelper.SeedAsync(factory.Services, new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = otherUser.Id,
                Role = Role.User
            });

            var newChat = new Chat
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = user.Id,
                StudentApplicationUserId = otherUser.Id
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/Chat")
            {
                Content = JsonContent.Create(newChat)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);
            request.Headers.Add("X-User-Id", userExternalId);
            request.Headers.Add("X-User-Email", user.Email);
            request.Headers.Add("X-User-Name", user.DisplayName);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location!.AbsolutePath.Should().Contain($"/Chat/{newChat.Id}");

            Chat? saved;
            await using (var context = factory.CreateDbContext())
            {
                saved = await context.Chats.FindAsync(newChat.Id);
            }

            saved.Should().NotBeNull();
            saved!.SlbApplicationUserId.Should().Be(user.Id);
            saved.StudentApplicationUserId.Should().Be(otherUser.Id);
        }
    }

    public class Get : ChatIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin")]
        [InlineData("ModuleAdmin")]
        [InlineData("User")]
        public async Task GetChat_ReturnsChatBasedOnId(string role)
        {
            using var factory = new TestAppFactory();

            var userExternalId = Guid.NewGuid().ToString();
            var otherUserExternalId = Guid.NewGuid().ToString();

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                ExternalId = userExternalId,
                Email = $"{role.ToLower()}@test.nl",
                DisplayName = $"{role} User"
            };
            await SeedHelper.SeedAsync(factory.Services, user);
            await SeedHelper.SeedAsync(factory.Services, new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = user.Id,
                Role = Enum.Parse<Role>(role)
            });

            var otherUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                ExternalId = otherUserExternalId,
                Email = $"other_{role.ToLower()}@test.nl",
                DisplayName = $"Other {role} User"
            };
            await SeedHelper.SeedAsync(factory.Services, otherUser);
            await SeedHelper.SeedAsync(factory.Services, new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = otherUser.Id,
                Role = Role.User
            });

            var chat = new Chat
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = user.Id,
                StudentApplicationUserId = otherUser.Id
            };
            await SeedHelper.SeedAsync(factory.Services, chat);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/Chat/{chat.Id}");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);
            request.Headers.Add("X-User-Id", userExternalId);
            request.Headers.Add("X-User-Email", user.Email);
            request.Headers.Add("X-User-Name", user.DisplayName);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            root.GetProperty("id").GetGuid().Should().Be(chat.Id);
            root.GetProperty("slbApplicationUserId").GetGuid().Should().Be(user.Id);
            root.GetProperty("studentApplicationUserId").GetGuid().Should().Be(otherUser.Id);
        }
    }

    public class Delete : ChatIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin")]
        [InlineData("ModuleAdmin")]
        [InlineData("User")]
        public async Task DeleteChat_RemovesChatFromDatabase(string role)
        {
            using var factory = new TestAppFactory();

            var userExternalId = Guid.NewGuid().ToString();
            var otherUserExternalId = Guid.NewGuid().ToString();

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                ExternalId = userExternalId,
                Email = $"{role.ToLower()}@test.nl",
                DisplayName = $"{role} User"
            };
            await SeedHelper.SeedAsync(factory.Services, user);
            await SeedHelper.SeedAsync(factory.Services, new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = user.Id,
                Role = Enum.Parse<Role>(role)
            });

            var otherUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                ExternalId = otherUserExternalId,
                Email = $"other_{role.ToLower()}@test.nl",
                DisplayName = $"Other {role} User"
            };
            await SeedHelper.SeedAsync(factory.Services, otherUser);
            await SeedHelper.SeedAsync(factory.Services, new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = otherUser.Id,
                Role = Role.User
            });

            var chat = new Chat
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = user.Id,
                StudentApplicationUserId = otherUser.Id
            };
            await SeedHelper.SeedAsync(factory.Services, chat);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/Chat/{chat.Id}");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);
            request.Headers.Add("X-User-Id", userExternalId);
            request.Headers.Add("X-User-Email", user.Email);
            request.Headers.Add("X-User-Name", user.DisplayName);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await using (var context = factory.CreateDbContext())
            {
                var chatDeleted = await context.Chats.FindAsync(chat.Id);
                chatDeleted.Should().BeNull();
            }
        }
    }
}
