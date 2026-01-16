using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Kernel.Application;
using Kernel.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public class CloudinaryService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly InfrasDbContext _context;

    public CloudinaryService(IOptions<CloudinarySettings> config, InfrasDbContext context)
    {
        var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
        _cloudinary = new Cloudinary(acc);
        _context = context;
    }

    public async Task<IEnumerable<FileMetadata>> GetFileMetadatasByResourceAsync(string resource)
    {
        return await _context.FileMetadata.Where(f => f.Resource == resource).ToListAsync();
    }

    public async Task<FileMetadata> UploadAsync(IFormFile file, string resource, string entityId)
    {
        // validate
        if (file.Length == 0)
        {
            throw new InputFormatException("File length = 0!");
        }

        var uploadResult = await UploadToCloudAsync(file, $"FastVocab_{resource}");

        if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            throw new ExternalServiceException("Upload file to Cloudinary fail!");

        // Save
        var metadata = new FileMetadata
        {
            Url = uploadResult.SecureUrl.AbsoluteUri,
            Resource = resource,
            EntityId = entityId,
            Format = uploadResult.Format ?? string.Empty,
            FileName = uploadResult.OriginalFilename ?? string.Empty,
            ExternalId = uploadResult.PublicId,
            Type = uploadResult.Type ?? string.Empty,
            Folder = uploadResult.AssetFolder ?? string.Empty,
        };

        _context.FileMetadata.Add(metadata);
        await _context.SaveChangesAsync();

        return metadata;
    }

    public async Task DeleteFilesAsync(string resource, string entityId)
    {
        var files = await _context.FileMetadata.Where(f => f.Resource == resource && f.EntityId == entityId).ToListAsync();

        foreach (var metadata in files)
        {
            await DeleteFromCloudAsync(metadata.ExternalId);
        }

        _context.FileMetadata.RemoveRange(files);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFileAsync(string url)
    {
        var metadata = await _context.FileMetadata.FirstOrDefaultAsync(f => f.Url == url);

        if (metadata == null)
        {
            throw new InfastructureException("File url not found");
        }

        var result = await DeleteFromCloudAsync(metadata.ExternalId);

        if (!result)
        {
            throw new ExternalServiceException("Delete file from Cloudinary fail");
        }

        _context.FileMetadata.Remove(metadata);
        await _context.SaveChangesAsync();
    }

    private async Task<ImageUploadResult> UploadToCloudAsync(IFormFile file, string folder)
    {
        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = true
        };

        return await _cloudinary.UploadAsync(uploadParams);
    }

    private async Task<bool> DeleteFromCloudAsync(string publicId)
    {
        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);
        return result.Result == "ok";
    }
}