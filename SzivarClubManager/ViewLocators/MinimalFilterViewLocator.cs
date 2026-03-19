using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;
using SzivarClubManager.Views.Activities.Page.MinimalFilter;

namespace SzivarClubManager.ViewLocators;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class MinimalFilterViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is not IMinimalFilterViewModel) return null;

        Type type = typeof(MinimalFilterView);
        MinimalFilterView view = new MinimalFilterView
        {
            DataContext = param
        };

        return view;
    }

    public bool Match(object? data)
    {
        return data is IMinimalFilterViewModel;
    }
}