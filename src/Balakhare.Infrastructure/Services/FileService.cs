using Microsoft.AspNetCore.Http;

namespace Balakhare.Infrastructure.Services;

public interface IFileService
{
    Task<string?> SaveFileAsync(IFormFile file);
    bool DeleteFile(string? path);
}

public class FileService : IFileService
{
    private readonly string _uploadFolder;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public FileService()
    {
        _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_uploadFolder))
        {
            Directory.CreateDirectory(_uploadFolder);
        }
    }

    public async Task<string?> SaveFileAsync(IFormFile file)
    {
        if (file.Length > MaxFileSize)
        {
            throw new Exception("حجم فایل بیش از حد مجاز (۱۰ مگابایت) است.");
        }

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(_uploadFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{fileName}";
    }

    public bool DeleteFile(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        var fileName = Path.GetFileName(path);
        var filePath = Path.Combine(_uploadFolder, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }
}
