using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SzivarClubManager.ViewModels.Activities.Page;
using PageActivityView = SzivarClubManager.Views.Activities.Page.PageActivityView;

namespace SzivarClubManager.ViewLocators;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class PageActivityViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is not PageActivityViewModel) return null;

        Type type = typeof(PageActivityView);

        return (Control)Activator.CreateInstance(type)!;
    }

    public bool Match(object? data)
    {
        return data is PageActivityViewModel;
    }
}