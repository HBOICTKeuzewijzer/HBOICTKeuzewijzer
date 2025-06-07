using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Tests.Repositories
{
    public class RepositoryTests
    {
        private readonly DbContextOptions<AppDbContext> _options;


        public RepositoryTests()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddAsync_ShouldAddModuleToDatabase()
        {
            //Arrange
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "TestModule",
                Code = "TM01",
                ECs = 5,
                Level = 1,
                Required = true,
                IsPropaedeutic = true,
                OerId = Guid.NewGuid(),
            };

            using (var context = new AppDbContext(_options))
            {
                var repository = new Repository<Module>(context);

                //Act
                await repository.AddAsync(module);
            }

            //Assert
            using (var context = new AppDbContext(_options))
            {
                var result = await context.Modules.FindAsync(module.Id);
                Assert.NotNull(result);
                Assert.Equal("TestModule", result?.Name);
                Assert.Equal("TM01", result?.Code);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectModule()
        {
            //Arrange
            var moduleId = Guid.NewGuid();
            var module = new Module
            {
                Id = moduleId,
                Name = "GetById Testmodule",
                Code = "GBID01",
                ECs = 3,
                Level = 1,
                Required = false,
                IsPropaedeutic = false,
                OerId = Guid.NewGuid()
            };

            using (var context = new AppDbContext(_options))
            {
                context.Modules.Add(module);
                await context.SaveChangesAsync();
            }

            //Act
            Module? result;
            using (var context = new AppDbContext(_options))
            {
                var repository = new Repository<Module>(context);
                result = await repository.GetByIdAsync(moduleId);
            }

            //Assert
            Assert.NotNull(result);
            Assert.Equal("GetById Testmodule", result?.Name);
            Assert.Equal("GBID01", result?.Code);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateModuleInDatabase()
        {
            //Arrange
            var moduleId = Guid.NewGuid();
            var originalModule = new Module
            {
                Id = moduleId,
                Name = "Originele naam",
                Code = "ORIG01",
                ECs = 3,
                Level = 1,
                Required = false,
                IsPropaedeutic = true,
                OerId = Guid.NewGuid()
            };

            // save first
            using (var context = new AppDbContext(_options))
            {
                context.Modules.Add(originalModule);
                await context.SaveChangesAsync();
            }

            // execute update
            using (var context = new AppDbContext(_options))
            {
                var repository = new Repository<Module>(context);

                var updatedModule = new Module
                {
                    Id = moduleId,
                    Name = "Gewijzigde naam",
                    Code = "UPD01",
                    ECs = 6,
                    Level = 2,
                    Required = true,
                    IsPropaedeutic = false,
                    OerId = originalModule.OerId
                };

                // Act
                await repository.UpdateAsync(updatedModule);
            }

            // Assert
            using (var context = new AppDbContext(_options))
            {
                var result = await context.Modules.FindAsync(moduleId);
                Assert.NotNull(result);
                Assert.Equal("Gewijzigde naam", result?.Name);
                Assert.Equal("UPD01", result?.Code);
                Assert.Equal(6, result?.ECs);
                Assert.True(result?.Required);
            }
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveModuleFromDatabase()
        {
            // Arrange
            var moduleId = Guid.NewGuid();
            var module = new Module
            {
                Id = moduleId,
                Name = "Te verwijderen module",
                Code = "DEL01",
                ECs = 5,
                Level = 1,
                Required = false,
                IsPropaedeutic = false,
                OerId = Guid.NewGuid()
            };

            using (var context = new AppDbContext(_options))
            {
                context.Modules.Add(module);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(_options))
            {
                var repository = new Repository<Module>(context);
                await repository.DeleteAsync(moduleId);
            }

            // Assert
            using (var context = new AppDbContext(_options))
            {
                var result = await context.Modules.FindAsync(moduleId);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenModuleExists()
        {
            // Arrange
            var moduleId = Guid.NewGuid();
            var module = new Module
            {
                Id = moduleId,
                Name = "Bestaande module",
                Code = "EXIST01",
                ECs = 3,
                Level = 1,
                Required = true,
                IsPropaedeutic = false,
                OerId = Guid.NewGuid()
            };

            using (var context = new AppDbContext(_options))
            {
                context.Modules.Add(module);
                await context.SaveChangesAsync();
            }

            // Act & Assert
            using (var context = new AppDbContext(_options))
            {
                var repository = new Repository<Module>(context);
                var exists = await repository.ExistsAsync(moduleId);
                Assert.True(exists);
            }
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenModuleDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            using (var context = new AppDbContext(_options))
            {
                var repository = new Repository<Module>(context);

                // Act
                var exists = await repository.ExistsAsync(nonExistentId);

                // Assert
                Assert.False(exists);
            }
        }

        [Fact]
        public async Task GetPaginatedAsync_ShouldReturnCorrectPage()
        {
            //Arrange
            var oerId = Guid.NewGuid();

            using (var context = new AppDbContext(_options))
            {
                for (int i = 1; i <= 10; i++)
                {
                    context.Modules.Add(new Module
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Module {i}",
                        Code = $"MOD{i:D2}",
                        ECs = 3,
                        Level = 1,
                        Required = false,
                        IsPropaedeutic = false,
                        OerId = oerId
                    });
                }
                await context.SaveChangesAsync();
            }
            PaginatedResult<Module> result;
            using (var context = new AppDbContext(_options))
            {
                var repository = new Repository<Module>(context);
                var request = new GetAllRequestQuery
                {
                    Page = 2,
                    PageSize = 3
                };

                result = await repository.GetPaginatedAsync(request);
            }

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(3, result.PageSize);
            Assert.Equal(3, result.Items.Count());

            // Optioneel: check eerste item op pagina 2
            Assert.Contains(result.Items, m => m.Name == "Module 4");
        }

        [Fact]
        public async Task GetPaginatedAsync_ShouldReturnFilteredResults()
        {
            var oerId = Guid.NewGuid();

            using (var context = new AppDbContext(_options))
            {
                context.Modules.AddRange(new List<Module>
                {
                    new Module { Id = Guid.NewGuid(), Name = "Module Alpha", Code = "A01", ECs = 3, Level = 1, Required = false, IsPropaedeutic = false, OerId = oerId },
                    new Module { Id = Guid.NewGuid(), Name = "Security", Code = "B01", ECs = 3, Level = 1, Required = false, IsPropaedeutic = false, OerId = oerId },
                    new Module { Id = Guid.NewGuid(), Name = "Module Beta", Code = "C01", ECs = 3, Level = 1, Required = false, IsPropaedeutic = false, OerId = oerId }
                });

                await context.SaveChangesAsync();
            }

            PaginatedResult<Module> result;

            using (var context = new AppDbContext(_options))
            {
                var repository = new Repository<Module>(context);

                var request = new GetAllRequestQuery
                {
                    Filter = "Security",
                    Page = 1,
                    PageSize = 10
                };

                result = await repository.GetPaginatedAsync(request);
            }

            // Assert
            Assert.Single(result.Items); // Er moet maar 1 resultaat zijn
            Assert.Equal("Security", result.Items.First().Name);
        }

        [Fact]
        public async Task Query_ShouldReturnQueryableOfModules()
        {
            // Arrange
            var oerId = Guid.NewGuid();

            using (var context = new AppDbContext(_options))
            {
                context.Modules.AddRange(new[]
                {
                    new Module { Id = Guid.NewGuid(), Name = "Q1", Code = "Q001", ECs = 3, Level = 1, Required = true, IsPropaedeutic = false, OerId = oerId },
                    new Module { Id = Guid.NewGuid(), Name = "Q2", Code = "Q002", ECs = 5, Level = 2, Required = false, IsPropaedeutic = true, OerId = oerId }
                });
                await context.SaveChangesAsync();
            }

            // Act
            List<Module> result;
            using (var context = new AppDbContext(_options))
            {
                var repository = new Repository<Module>(context);
                var query = repository.Query();

                result = query.ToList(); // query uitvoeren
            }

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Name == "Q1");
            Assert.Contains(result, m => m.Name == "Q2");
        }

    }
}
