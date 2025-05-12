using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Services
{
    public class OerUploadService
    {
        public async Task<string> SavePdfAsync(Oer oer, IFormFile file)
        {
            // Opslaan in wwwroot/uploads/oer/
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "oer");
            Directory.CreateDirectory(uploadsFolder);

            // Jaar toevoegen aan de filename voor extra duidelijkheid
            var sanitizedYear = string.IsNullOrWhiteSpace(oer.AcademicYear) ? "unknown-year" : oer.AcademicYear.Replace("/", "-");
            var fileName = $"OER-{sanitizedYear}-{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // URL opslaan (relatief pad vanaf de server root)
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/oer/{fileName}";
        }
    }
}
