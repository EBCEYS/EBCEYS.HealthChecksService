namespace EBCEYS.HealthChecksService.Extensions;

public static class StreamExtensions
{
    public static async Task WriteToFileAsync(this Stream stream, string path, CancellationToken token = default)
    {
        using var sw = File.CreateText(path);
        using StreamReader sr = new(stream);
        string? line;
        while ((line = await sr.ReadLineAsync(token)) != null) await sw.WriteLineAsync(line);
    }
}