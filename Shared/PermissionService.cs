using Android.Content;
using Android.Provider;

namespace MauiAudioRecorder.Shared;

public class PermissionService
{
    public async Task<bool> IsRecordingPermissionGranted()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        return status == PermissionStatus.Granted;
    }

    public async Task<PermissionStatus> RequestRecordingPermission()
    {
        return await Permissions.RequestAsync<Permissions.Microphone>();
    }

    public void OpenSettings()
    {
#if ANDROID
        var context = Android.App.Application.Context;
        var intent = new Intent(Settings.ActionApplicationDetailsSettings);
        var uri = Android.Net.Uri.FromParts("package", context.PackageName, null);
        intent.SetData(uri);
        intent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
#endif
    }
}
