#if ANDROID
using Android.Media;
#endif
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MauiAudioRecorder.Shared;

public class AudioService
{
#if ANDROID
    private AudioRecord? _audioRecord;
    private MediaPlayer? _player;
#endif
    private string? _filePath;
    private bool _isRecording;
    private const int SampleRate = 16000;
    private const ChannelIn ChannelConfig = ChannelIn.Mono;
    private const Encoding AudioFormat = Encoding.Pcm16bit;

    public string? FilePath => _filePath;

    public void StartRecording()
    {
#if ANDROID
        _filePath = Path.Combine(FileSystem.CacheDirectory, "recording.wav");
        int bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelConfig, AudioFormat);
        
        _audioRecord = new AudioRecord(
            AudioSource.VoiceRecognition,
            SampleRate,
            ChannelConfig,
            AudioFormat,
            bufferSize);

        _isRecording = true;
        _audioRecord.StartRecording();

        Task.Run(() => RecordAudioAsync(bufferSize));
#endif
    }

    private async Task RecordAudioAsync(int bufferSize)
    {
#if ANDROID
        var tempFile = Path.Combine(FileSystem.CacheDirectory, "temp.pcm");
        using (var stream = File.OpenWrite(tempFile))
        {
            byte[] data = new byte[bufferSize];
            while (_isRecording && _audioRecord != null)
            {
                int read = _audioRecord.Read(data, 0, bufferSize);
                if (read > 0)
                {
                    await stream.WriteAsync(data, 0, read);
                }
            }
        }

        // Convert PCM to WAV
        if (File.Exists(tempFile))
        {
            WriteWavFile(tempFile, _filePath!);
            File.Delete(tempFile);
        }
#endif
    }

    public void StopRecording()
    {
#if ANDROID
        _isRecording = false;
        if (_audioRecord != null)
        {
            _audioRecord.Stop();
            _audioRecord.Release();
            _audioRecord = null;
        }
#endif
    }

    public void StartPlayback()
    {
#if ANDROID
        if (_filePath == null || !File.Exists(_filePath)) return;

        StopPlayback(); // Ensure any existing playback is stopped

        _player = new MediaPlayer();
        _player.SetDataSource(_filePath);
        _player.Prepare();
        _player.Start();
#endif
    }

    public void StopPlayback()
    {
#if ANDROID
        if (_player != null)
        {
            if (_player.IsPlaying)
                _player.Stop();
            _player.Release();
            _player = null;
        }
#endif
    }

#if ANDROID
    private void WriteWavFile(string pcmPath, string wavPath)
    {
        using var pcmStream = File.OpenRead(pcmPath);
        using var wavStream = File.OpenWrite(wavPath);
        
        long pcmLength = pcmStream.Length;
        long totalDataLen = pcmLength + 36;
        int channels = 1;
        long byteRate = SampleRate * channels * 16 / 8;

        // WAV Header
        wavStream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        wavStream.Write(BitConverter.GetBytes((int)totalDataLen), 0, 4);
        wavStream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);
        wavStream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
        wavStream.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size (16 for PCM)
        wavStream.Write(BitConverter.GetBytes((short)1), 0, 2); // AudioFormat (1 for PCM)
        wavStream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        wavStream.Write(BitConverter.GetBytes(SampleRate), 0, 4);
        wavStream.Write(BitConverter.GetBytes((int)byteRate), 0, 4);
        wavStream.Write(BitConverter.GetBytes((short)(channels * 16 / 8)), 0, 2); // BlockAlign
        wavStream.Write(BitConverter.GetBytes((short)16), 0, 2); // BitsPerSample
        wavStream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
        wavStream.Write(BitConverter.GetBytes((int)pcmLength), 0, 4);

        pcmStream.CopyTo(wavStream);
    }
#endif
}
