using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SzivarClubManager.ViewModels.Activities;

public sealed class NoActivityViewModel : ActivityViewModel
{
    public Bitmap Image => ImageSource;

    private static readonly Bitmap ImageSource = new(AssetLoader.Open(new Uri("avares://SzivarClubManager/Assets/sad-face-black.png")));
}