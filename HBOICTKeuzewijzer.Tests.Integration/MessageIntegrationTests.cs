using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Tests.Integration.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HBOICTKeuzewijzer.Tests.Integration;

public class MessageIntegrationTests
{
    public class Post : MessageIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin", true)]
        [InlineData("ModuleAdmin", true)]
        [InlineData("User", true)]
        [InlineData("SystemAdmin", false)]
        [InlineData("ModuleAdmin", false)]
        [InlineData("User", false)]
        public async Task PostMessage_CreatesEntryInDatabase(string role, bool asSlb)
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
                SlbApplicationUserId = asSlb ? user.Id : otherUser.Id,
                StudentApplicationUserId = asSlb ? otherUser.Id : user.Id
            };
            await SeedHelper.SeedAsync(factory.Services, chat);

            var newMessage = new Message
            {
                Id = Guid.NewGuid(),
                MessageText = "Hello world",
                SenderApplicationUserId = user.Id
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"/Chat/{chat.Id}/Message")
            {
                Content = JsonContent.Create(newMessage)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);
            request.Headers.Add("X-User-Id", userExternalId);
            request.Headers.Add("X-User-Email", user.Email);
            request.Headers.Add("X-User-Name", user.DisplayName);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location!.AbsolutePath.Should().Contain($"/Chat/{chat.Id}/Message");

            Message? saved;
            await using (var context = factory.CreateDbContext())
            {
                saved = await context.Messages.FindAsync(newMessage.Id);
            }

            saved.Should().NotBeNull();
            saved!.MessageText.Should().Be("Hello world");
            saved.ChatId.Should().Be(chat.Id);
            saved.SenderApplicationUserId.Should().Be(user.Id);
        }
    }

    public class Get : MessageIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin", true)]
        [InlineData("ModuleAdmin", true)]
        [InlineData("User", true)]
        [InlineData("SystemAdmin", false)]
        [InlineData("ModuleAdmin", false)]
        [InlineData("User", false)]
        public async Task GetMessage_ReturnsMessageBasedOnId(string role, bool asSlb)
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
                SlbApplicationUserId = asSlb ? user.Id : otherUser.Id,
                StudentApplicationUserId = asSlb ? otherUser.Id : user.Id
            };
            await SeedHelper.SeedAsync(factory.Services, chat);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chat.Id,
                MessageText = "Test message",
                SenderApplicationUserId = user.Id,
                SentAt = DateTime.UtcNow,
                SlbRead = false,
                StudentRead = false
            };
            await SeedHelper.SeedAsync(factory.Services, message);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/Chat/{chat.Id}/Message/{message.Id}");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);
            request.Headers.Add("X-User-Id", userExternalId);
            request.Headers.Add("X-User-Email", user.Email);
            request.Headers.Add("X-User-Name", user.DisplayName);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Use JsonDocument to inspect the returned JSON
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            root.GetProperty("id").GetGuid().Should().Be(message.Id);
            root.GetProperty("chatId").GetGuid().Should().Be(chat.Id);
            root.GetProperty("messageText").GetString().Should().Be("Test message");
            root.GetProperty("senderApplicationUserId").GetGuid().Should().Be(user.Id);
            root.GetProperty("slbRead").GetBoolean().Should().BeFalse();
            root.GetProperty("studentRead").GetBoolean().Should().BeFalse();
        }
    }


    public class Put : MessageIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin", true)]
        [InlineData("ModuleAdmin", true)]
        [InlineData("User", true)]
        [InlineData("SystemAdmin", false)]
        [InlineData("ModuleAdmin", false)]
        [InlineData("User", false)]
        public async Task PutMessage_UpdatesMessageInDatabase(string role, bool asSlb)
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
                SlbApplicationUserId = asSlb ? user.Id : otherUser.Id,
                StudentApplicationUserId = asSlb ? otherUser.Id : user.Id
            };
            await SeedHelper.SeedAsync(factory.Services, chat);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chat.Id,
                MessageText = "Old message",
                SenderApplicationUserId = user.Id,
                SentAt = DateTime.UtcNow,
                SlbRead = false,
                StudentRead = false
            };
            await SeedHelper.SeedAsync(factory.Services, message);

            var updatedMessage = new Message
            {
                Id = message.Id,
                ChatId = chat.Id,
                MessageText = "Updated message",
                SenderApplicationUserId = user.Id,
                SentAt = DateTime.UtcNow,
                SlbRead = true,
                StudentRead = true
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/Chat/{chat.Id}/Message/{message.Id}")
            {
                Content = JsonContent.Create(updatedMessage)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);
            request.Headers.Add("X-User-Id", userExternalId);
            request.Headers.Add("X-User-Email", user.Email);
            request.Headers.Add("X-User-Name", user.DisplayName);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            Message? saved;
            await using (var context = factory.CreateDbContext())
            {
                saved = await context.Messages.FindAsync(message.Id);
            }

            saved.Should().NotBeNull();
            saved!.MessageText.Should().Be("Updated message");
            saved.SlbRead.Should().BeTrue();
            saved.StudentRead.Should().BeTrue();
        }
    }

    public class Delete : MessageIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin", true)]
        [InlineData("ModuleAdmin", true)]
        [InlineData("User", true)]
        [InlineData("SystemAdmin", false)]
        [InlineData("ModuleAdmin", false)]
        [InlineData("User", false)]
        public async Task DeleteMessage_RemovesMessageFromDatabase(string role, bool asSlb)
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
                SlbApplicationUserId = asSlb ? user.Id : otherUser.Id,
                StudentApplicationUserId = asSlb ? otherUser.Id : user.Id
            };
            await SeedHelper.SeedAsync(factory.Services, chat);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chat.Id,
                MessageText = "Delete me",
                SenderApplicationUserId = user.Id,
                SentAt = DateTime.UtcNow,
                SlbRead = false,
                StudentRead = false
            };
            await SeedHelper.SeedAsync(factory.Services, message);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/Chat/{chat.Id}/Message/{message.Id}");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);
            request.Headers.Add("X-User-Id", userExternalId);
            request.Headers.Add("X-User-Email", user.Email);
            request.Headers.Add("X-User-Name", user.DisplayName);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await using (var context = factory.CreateDbContext())
            {
                var deleted = await context.Messages.FindAsync(message.Id);
                deleted.Should().BeNull();
            }
        }
    }
}
