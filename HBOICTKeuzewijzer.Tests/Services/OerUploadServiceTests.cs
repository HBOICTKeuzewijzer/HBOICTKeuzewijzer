using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HBOICTKeuzewijzer.Tests.Services
{
    public class OerUploadServiceTests
    {
        [Fact]
        public async Task SavePdfAsync_StoresFileAndReturnsRelativeUrl()
        {
            // Arrange
            var oer = new Oer
            {
                AcademicYear = "2024/2025"
            };

            var fileContent = "Fake PDF content";
            var fileName = "fake.pdf";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.FileName).Returns(fileName);
            formFileMock.Setup(f => f.Length).Returns(ms.Length);
            formFileMock.Setup(f => f.ContentType).Returns("application/pdf");
            formFileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                        .Returns<Stream, System.Threading.CancellationToken>((stream, _) => ms.CopyToAsync(stream));

            var service = new OerUploadService();

            // Act
            var result = await service.SavePdfAsync(oer, formFileMock.Object);

            // Assert
            Assert.StartsWith("/uploads/oer/OER-2024-2025-", result);
            Assert.EndsWith(".pdf", result);

            // Extra: Controleer of bestand echt is aangemaakt
            var savedFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", result.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            Assert.True(File.Exists(savedFilePath));

            // Cleanup
            File.Delete(savedFilePath);
        }
    }
}
