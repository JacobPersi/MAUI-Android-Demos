#if ANDROID
using Android.Media;
#endif
using System;
using System.IO;

namespace MauiAudioRecorder.Shared;

public class AudioService
{
#if ANDROID
    private MediaRecorder? _recorder;
    private MediaPlayer? _player;
#endif
    private string? _filePath;

    public string? FilePath => _filePath;

    public void StartRecording()
    {
#if ANDROID
        _filePath = Path.Combine(FileSystem.CacheDirectory, "recording.amr");

        _recorder = new MediaRecorder();
        _recorder.SetAudioSource(AudioSource.Mic);
        _recorder.SetOutputFormat(OutputFormat.AmrNb);
        _recorder.SetAudioEncoder(AudioEncoder.AmrNb);
        _recorder.SetOutputFile(_filePath);

        _recorder.Prepare();
        _recorder.Start();
#endif
    }

    public void StopRecording()
    {
#if ANDROID
        if (_recorder != null)
        {
            _recorder.Stop();
            _recorder.Release();
            _recorder = null;
        }
#endif
    }

    public void StartPlayback()
    {
#if ANDROID
        if (_filePath == null || !File.Exists(_filePath)) return;

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
            _player.Stop();
            _player.Release();
            _player = null;
        }
#endif
    }
}
