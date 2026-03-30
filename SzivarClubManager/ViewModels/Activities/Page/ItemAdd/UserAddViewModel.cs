using System;
using SzivarClubManager.Services;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemAdd;

public class UserAddViewModel : ItemAddViewModel
{
    protected override bool CanAdd => throw new NotSupportedException();
    public UserAddViewModel(PopupService popupService) : base(popupService) => throw new NotSupportedException();
    protected override void ResetFields() => throw new NotSupportedException();
    protected override void AddNewItemToChanges() => throw new NotSupportedException();
}