using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Searchable;

public partial class SearchWindow : Window
{
    private readonly SearchableViewModel viewModel;
    private bool isClosing = false;

    public SearchWindow(SearchableViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        DataContext = viewModel;

        viewModel.RequestClose += () =>
        {
            Dispatcher.Invoke(SafeClose);
        };

        Loaded += (s, e) =>
        {
            CenterOnCurrentWindow();
            SearchInputBox.Focus();
        };

        Deactivated += (s, e) =>
        {
            // Close when clicking outside
            SafeClose();
        };
    }

    private void SafeClose()
    {
        if (isClosing) return;
        isClosing = true;
        try
        {
            Close();
        }
        catch
        {
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        isClosing = true;
    }

    private void CenterOnCurrentWindow()
    {
        var mainWindow = Application.Current?.MainWindow;
        if (mainWindow != null && mainWindow.IsVisible && mainWindow.WindowState != WindowState.Minimized)
        {
            Left = mainWindow.Left + (mainWindow.ActualWidth - Width) / 2;
            Top = mainWindow.Top + Math.Max(40, (mainWindow.ActualHeight - Height) / 3);
        }
        else
        {
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 3;
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            if (ScopeComboBox.IsDropDownOpen)
            {
                ScopeComboBox.IsDropDownOpen = false;
            }
            else
            {
                SafeClose();
            }
        }
        else if (e.Key == Key.Down)
        {
            e.Handled = true;
            viewModel.SelectNext();
            ScrollIntoView();
        }
        else if (e.Key == Key.Up)
        {
            e.Handled = true;
            viewModel.SelectPrevious();
            ScrollIntoView();
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            viewModel.ExecuteSelected();
        }
        else if (e.Key == Key.Tab)
        {
            e.Handled = true;
            CycleCategoryFilter((Keyboard.Modifiers & ModifierKeys.Shift) != 0 ? -1 : 1);
        }
    }

    private void CycleCategoryFilter(int delta)
    {
        var count = viewModel.CategoryFilters.Count;
        if (count == 0) return;
        var current = viewModel.CategoryFilters.IndexOf(viewModel.SelectedCategoryFilter);
        var next = (current + delta + count) % count;
        viewModel.SelectedCategoryFilter = viewModel.CategoryFilters[next];
    }

    private void FilterActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(viewModel.SearchText))
        {
            viewModel.SearchText = string.Empty;
            SearchInputBox.Focus();
        }
        else
        {
            ScopeComboBox.IsDropDownOpen = !ScopeComboBox.IsDropDownOpen;
        }
    }

    private void ResultItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBoxItem item && item.DataContext is SearchResultItem result)
        {
            viewModel.SelectedItem = result;
            viewModel.ExecuteSelected();
            e.Handled = true;
        }
    }

    private void ScrollIntoView()
    {
        if (viewModel.SelectedItem != null)
        {
            ResultsListBox.ScrollIntoView(viewModel.SelectedItem);
        }
    }
}
