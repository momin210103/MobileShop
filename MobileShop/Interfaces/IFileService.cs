namespace MobileShop.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    Task<List<string>> SaveFilesAsync(List<IFormFile> files, string folder);
    void DeleteFile(string filePath);
}