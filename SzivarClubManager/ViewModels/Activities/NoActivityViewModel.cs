using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SzivarClubManager.Services;

namespace SzivarClubManager.ViewModels.Activities;

public sealed class NoActivityViewModel(PopupService popupService) : ActivityViewModel(popupService)
{
    public Bitmap Image => ImageSource;

    private static readonly Bitmap ImageSource = new(AssetLoader.Open(new Uri("avares://SzivarClubManager/Assets/sad-face-black.png")));
}