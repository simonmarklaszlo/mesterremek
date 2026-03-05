using System;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemAdd;

public class UserAddViewModel : ItemAddViewModel<User>
{
    protected override bool CanAdd => throw new NotSupportedException();
    public UserAddViewModel() => throw new NotSupportedException();
    protected override void ResetFields() => throw new NotSupportedException();
    protected override void AddNewItemToChanges() => throw new NotSupportedException();
}