using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SzivarClubManager.ViewModels;

public class ConnectionStateViewModel : ViewModelBase
{
    public string State { get; }
    public Bitmap Image { get; }

    private ConnectionStateViewModel(string state, Bitmap image)
    {
        State = state;
        Image = image;
    }

    public static ConnectionStateViewModel Loading { get; } = new("Loading...", new Bitmap(AssetLoader.Open(new Uri("avares://SzivarClubManager/Assets/loading.gif"))));
    public static ConnectionStateViewModel Error { get; } = new("Error", new Bitmap(AssetLoader.Open(new Uri("avares://SzivarClubManager/Assets/error.png"))));
}