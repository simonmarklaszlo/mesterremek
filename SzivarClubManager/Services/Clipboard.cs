using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace SzivarClubManager.Services;

public static class Clipboard
{
    public static async Task CopyText(string? text)
    {
        IClipboard? clipboard = GetClipboard();
        if (clipboard is null) return;

        await clipboard.SetTextAsync(text);
    }

    private static IClipboard? GetClipboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow;

            return window?.Clipboard;
        }

        return null;
    }
}