using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Services;

public interface IOerUploadService
{
    Task<string> SavePdfAsync(Oer oer, IFormFile file);
}