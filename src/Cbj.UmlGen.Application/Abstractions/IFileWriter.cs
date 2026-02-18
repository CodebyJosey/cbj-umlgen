namespace Cbj.UmlGen.Application.Abstractions;

/// <summary>
/// Writes generated files to disk.
/// </summary>
public interface IFileWriter
{
    /// <summary>
    /// Writes text content to a file path, creating directories if needed.
    /// </summary>
    Task WriteTextAsync(string path, string content, CancellationToken ct = default);
}