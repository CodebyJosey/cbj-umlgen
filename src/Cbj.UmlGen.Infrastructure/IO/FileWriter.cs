using Cbj.UmlGen.Application.Abstractions;

namespace Cbj.UmlGen.Infrastructure.IO;

public sealed class FileWriter : IFileWriter
{
    /// <inheritdoc/>
    public async Task WriteTextAsync(string path, string content, CancellationToken ct = default)
    {
        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllTextAsync(path, content, ct);
    }
}