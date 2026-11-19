using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Searchable;

public sealed class SearchableViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? RequestClose;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private List<SearchResultItem> allItems = new();
    public ObservableCollection<SearchResultItem> FilteredItems { get; } = new();

    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set
        {
            if (searchText != value)
            {
                searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }

    private SearchResultItem? selectedItem;
    public SearchResultItem? SelectedItem
    {
        get => selectedItem;
        set
        {
            if (selectedItem != value)
            {
                selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
            }
        }
    }

    public bool HasSelection => SelectedItem != null;

    private string selectedCategoryFilter = "すべて";
    public string SelectedCategoryFilter
    {
        get => selectedCategoryFilter;
        set
        {
            if (selectedCategoryFilter != value)
            {
                selectedCategoryFilter = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }

    public ObservableCollection<string> CategoryFilters { get; } = new()
    {
        "すべて",
        "ウィンドウ",
        "オブジェクト",
        "映像エフェクト",
        "音声エフェクト",
        "キャラクター",
        "テンプレート",
        "コマンド"
    };

    public SearchableViewModel()
    {
        ReloadCatalog();
    }

    public void ReloadCatalog()
    {
        try
        {
            allItems = ItemCatalog.BuildCatalog();
            ApplyFilter();
        }
        catch
        {
        }
    }

    public void ApplyFilter()
    {
        var query = SearchText?.Trim() ?? string.Empty;
        var terms = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        IEnumerable<SearchResultItem> filtered = allItems;

        if (SelectedCategoryFilter != "すべて")
        {
            filtered = SelectedCategoryFilter switch
            {
                "ウィンドウ" => filtered.Where(x => x.Kind == SearchItemKind.Window),
                "オブジェクト" => filtered.Where(x => x.Kind == SearchItemKind.Item),
                "映像エフェクト" => filtered.Where(x => x.Kind == SearchItemKind.VideoEffect),
                "音声エフェクト" => filtered.Where(x => x.Kind == SearchItemKind.AudioEffect),
                "キャラクター" => filtered.Where(x => x.Kind == SearchItemKind.Character),
                "テンプレート" => filtered.Where(x => x.Kind == SearchItemKind.Template),
                "コマンド" => filtered.Where(x => x.Kind == SearchItemKind.Command),
                _ => filtered
            };
        }

        if (terms.Length > 0)
        {
            filtered = filtered
                .Select(item => new { Item = item, Score = CalculateScore(item, terms) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Item);
        }

        FilteredItems.Clear();
        foreach (var item in filtered.Take(100))
        {
            FilteredItems.Add(item);
        }

        if (FilteredItems.Count > 0)
        {
            SelectedItem = FilteredItems[0];
        }
        else
        {
            SelectedItem = null;
        }
    }

    private static int CalculateScore(SearchResultItem item, string[] terms)
    {
        int totalScore = 0;

        foreach (var term in terms)
        {
            int termScore = 0;

            if (item.Title.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                termScore += item.Title.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 100 : 50;
            }
            if (item.Category.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                termScore += 20;
            }
            if (item.SubText.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                termScore += 15;
            }
            if (item.Keywords.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                termScore += 10;
            }

            if (termScore == 0)
            {
                return 0; // All terms must match somewhere
            }

            totalScore += termScore;
        }

        return totalScore;
    }

    public void ExecuteSelected()
    {
        if (SelectedItem != null)
        {
            var action = SelectedItem.Action;
            RequestClose?.Invoke();
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Execute error: {ex.Message}");
            }
        }
    }

    public void SelectPrevious()
    {
        if (FilteredItems.Count == 0) return;
        int index = SelectedItem != null ? FilteredItems.IndexOf(SelectedItem) : 0;
        index = Math.Max(0, index - 1);
        SelectedItem = FilteredItems[index];
    }

    public void SelectNext()
    {
        if (FilteredItems.Count == 0) return;
        int index = SelectedItem != null ? FilteredItems.IndexOf(SelectedItem) : -1;
        index = Math.Min(FilteredItems.Count - 1, index + 1);
        SelectedItem = FilteredItems[index];
    }
}

