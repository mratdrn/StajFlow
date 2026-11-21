using System.Buffers.Binary;
using StajFlow.Api.Data;
using StajFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Services;

public class CompanyLogoService(ApplicationDbContext context) : ICompanyLogoService
{
    public const int MaxFileSize = 2 * 1024 * 1024;

    public async Task<CompanyLogoUploadResult> UploadAsync(string userId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length is <= 0 or > MaxFileSize)
            return new(null, "Logo boş olamaz ve en fazla 2 MB olabilir.");

        var companyId = await context.CompanyProfiles.Where(profile => profile.IdentityUserId == userId)
            .Select(profile => (int?)profile.Id).SingleOrDefaultAsync(cancellationToken);
        if (companyId is null) return new(null, ProfileNotFound: true);

        await using var input = file.OpenReadStream();
        using var output = new MemoryStream();
        var buffer = new byte[8192];
        int count;
        while ((count = await input.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (output.Length + count > MaxFileSize) return new(null, "Logo en fazla 2 MB olabilir.");
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
        }
        var data = output.ToArray();
        var contentType = DetectContentType(data);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (contentType is null || file.ContentType != contentType ||
            (contentType == "image/png" ? extension != ".png" : extension is not (".jpg" or ".jpeg")))
            return new(null, "Geçerli bir PNG veya JPG dosyası seçin. Görsel boyutları en fazla 4096 × 4096 piksel olabilir.");

        var version = Guid.NewGuid();
        var changed = await context.CompanyProfiles.Where(profile => profile.Id == companyId && profile.IdentityUserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(profile => profile.LogoData, data)
                .SetProperty(profile => profile.LogoContentType, contentType)
                .SetProperty(profile => profile.LogoVersion, version), cancellationToken);
        return changed == 0 ? new(null, ProfileNotFound: true) : new($"/api/companies/{companyId}/logo?v={version}");
    }

    public Task<CompanyLogoFile?> GetAsync(int companyId, CancellationToken cancellationToken) =>
        context.CompanyProfiles.AsNoTracking()
            .Where(profile => profile.Id == companyId && profile.LogoData != null && profile.LogoContentType != null)
            .Select(profile => new CompanyLogoFile(profile.LogoData!, profile.LogoContentType!))
            .SingleOrDefaultAsync(cancellationToken);

    private static string? DetectContentType(byte[] data)
    {
        ReadOnlySpan<byte> bytes = data;
        if (bytes.Length >= 45 && bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
        {
            if (BinaryPrimitives.ReadUInt32BigEndian(bytes[8..]) != 13 || !bytes.Slice(12, 4).SequenceEqual("IHDR"u8)) return null;
            var width = BinaryPrimitives.ReadUInt32BigEndian(bytes[16..]);
            var height = BinaryPrimitives.ReadUInt32BigEndian(bytes[20..]);
            if (width is 0 or > 4096 || height is 0 or > 4096) return null;
            var offset = 8;
            var hasImageData = false;
            while (offset <= bytes.Length - 12)
            {
                var length = BinaryPrimitives.ReadUInt32BigEndian(bytes[offset..]);
                if (length > bytes.Length - offset - 12) return null;
                var type = bytes.Slice(offset + 4, 4);
                if (type.SequenceEqual("IDAT"u8) && length > 0) hasImageData = true;
                if (type.SequenceEqual("IEND"u8))
                    return hasImageData && length == 0 && offset + 12 == bytes.Length ? "image/png" : null;
                offset += (int)length + 12;
            }
            return null;
        }
        if (bytes.Length < 12 || bytes[0] != 0xff || bytes[1] != 0xd8 || bytes[^2] != 0xff || bytes[^1] != 0xd9) return null;
        var position = 2;
        var hasFrame = false;
        while (position < bytes.Length - 2)
        {
            if (bytes[position++] != 0xff) return null;
            while (position < bytes.Length && bytes[position] == 0xff) position++;
            if (position >= bytes.Length) return null;
            var marker = bytes[position++];
            if (marker == 0xda) return hasFrame ? "image/jpeg" : null;
            if (position > bytes.Length - 2) return null;
            var length = BinaryPrimitives.ReadUInt16BigEndian(bytes[position..]);
            if (length < 2 || position + length > bytes.Length) return null;
            if (marker is 0xc0 or 0xc1 or 0xc2)
            {
                if (length < 8) return null;
                var height = BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 3)..]);
                var width = BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 5)..]);
                if (width is 0 or > 4096 || height is 0 or > 4096) return null;
                hasFrame = true;
            }
            position += length;
        }
        return null;
    }
}
