using MauiAudioRecorder.Shared;

namespace MauiAudioRecorderUI;

public partial class MainPage : ContentPage
{
    private readonly AudioService _audioService = new();
    private readonly PermissionService _permissionService = new();

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await UpdatePermissionsUi();
    }

    private async Task UpdatePermissionsUi()
    {
        bool granted = await _permissionService.IsRecordingPermissionGranted();
        PermissionsBtn.BackgroundColor = granted ? Colors.Green : Colors.Red;
        PermissionsBtn.Text = granted ? "Permissions [OK]" : "Request Permissions";
        RecordBtn.IsEnabled = granted;
    }

    private async void OnPermissionsClicked(object? sender, EventArgs e)
    {
        bool granted = await _permissionService.IsRecordingPermissionGranted();
        if (!granted)
        {
            var status = await _permissionService.RequestRecordingPermission();
            if (status != PermissionStatus.Granted)
            {
                // Cross-launch to settings as requested
                _permissionService.OpenSettings();
            }
        }
        await UpdatePermissionsUi();
    }

    private void OnRecordClicked(object? sender, EventArgs e)
    {
        try
        {
            _audioService.StartRecording();
            StatusLabel.Text = "Recording...";
            RecordBtn.IsEnabled = false;
            StopBtn.IsEnabled = true;
            PlayBtn.IsEnabled = false;
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private void OnStopClicked(object? sender, EventArgs e)
    {
        _audioService.StopRecording();
        StatusLabel.Text = "Stopped. Saved to cache.";
        RecordBtn.IsEnabled = true;
        StopBtn.IsEnabled = false;
        PlayBtn.IsEnabled = true;
    }

    private void OnPlayClicked(object? sender, EventArgs e)
    {
        _audioService.StartPlayback();
        StatusLabel.Text = "Playing...";
    }
}
