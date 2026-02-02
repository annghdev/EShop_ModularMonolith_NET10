using Microsoft.AspNetCore.Http;

namespace Kernel.Application;

public interface IImageStorageService
{
    Task<FileMetadata> UploadAsync(IFormFile file, string resource, string entityId);
    Task DeleteFileAsync(string url);
    Task DeleteFilesAsync(string resource, string entityId);

    Task<IEnumerable<FileMetadata>> GetFileMetadatasByResourceAsync(string resource);
}
