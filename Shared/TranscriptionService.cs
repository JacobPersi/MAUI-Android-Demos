using Whisper.net;
using Whisper.net.Ggml;

namespace MauiAudioRecorder.Shared;

public class TranscriptionService
{
    private readonly string _modelPath;

    public TranscriptionService()
    {
        _modelPath = Path.Combine(FileSystem.AppDataDirectory, "ggml-base.bin");
    }

    public async Task<bool> EnsureModelExistsAsync()
    {
        if (File.Exists(_modelPath))
        {
            return true;
        }

        try
        {
            // Using the Default instance as GetGgmlModelAsync is not static
            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);
            using var fileStream = File.OpenWrite(_modelPath);
            await modelStream.CopyToAsync(fileStream);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<string> TranscribeAsync(string audioFilePath)
    {
        if (!File.Exists(_modelPath))
        {
            bool downloaded = await EnsureModelExistsAsync();
            if (!downloaded) return "Error: Could not download Whisper model.";
        }

        if (!File.Exists(audioFilePath))
        {
            return "Error: Audio file not found.";
        }

        try
        {
            using var whisperFactory = WhisperFactory.FromPath(_modelPath);
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            using var fileStream = File.OpenRead(audioFilePath);
            // Whisper.net actually has a built-in wave provider or we can use the stream directly
            // Let's try to use the stream as most processors do, or use the explicit class if it exists.
            // If WaveParser is not found, let's try to use the processor directly with the stream.
            
            var result = "";
            await foreach (var segment in processor.ProcessAsync(fileStream))
            {
                result += segment.Text;
            }

            return result;
        }
        catch (Exception ex)
        {
            return $"Error during transcription: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}\n\nInner Exception:\n{ex.InnerException?.Message}";
        }
    }
}
