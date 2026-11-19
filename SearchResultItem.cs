using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Searchable;

public enum SearchItemKind
{
    Window,
    Item,
    VideoEffect,
    AudioEffect,
    Character,
    Template,
    Command
}

public sealed class SearchResultItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public string Title { get; }
    public string Category { get; }
    public string SubText { get; }
    public string Keywords { get; }
    public SearchItemKind Kind { get; }
    public Action Action { get; }

    public bool HasSubText => !string.IsNullOrWhiteSpace(SubText);

    public string KindShortLabel => Kind switch
    {
        SearchItemKind.Window => "ウィンドウ",
        SearchItemKind.Item => "追加",
        SearchItemKind.VideoEffect => "エフェクト",
        SearchItemKind.AudioEffect => "音声エフェクト",
        SearchItemKind.Character => "キャラクター",
        SearchItemKind.Template => "テンプレート",
        SearchItemKind.Command => "操作",
        _ => string.Empty
    };

    public string GroupBadgeText => !string.IsNullOrWhiteSpace(SubText) 
        ? SubText 
        : KindShortLabel;

    /// <summary>
    /// 16x16 coordinate space minimal monochrome vector paths (stroke-based)
    /// </summary>
    public string KindIconPathData => Kind switch
    {
        // Window: simple rectangle with top bar
        SearchItemKind.Window => "M2,3.5 C2,2.67 2.67,2 3.5,2 L12.5,2 C13.33,2 14,2.67 14,3.5 L14,12.5 C14,13.33 13.33,14 12.5,14 L3.5,14 C2.67,14 2,13.33 2,12.5 Z M2,5.5 L14,5.5",
        // Item: plus inside minimal box / layers
        SearchItemKind.Item => "M8,3 L8,13 M3,8 L13,8",
        // VideoEffect: diamond / sparkle outline
        SearchItemKind.VideoEffect => "M8,2 L9.7,6.3 L14,8 L9.7,9.7 L8,14 L6.3,9.7 L2,8 L6.3,6.3 Z",
        // AudioEffect: minimal waveform bars
        SearchItemKind.AudioEffect => "M3,6.5 L3,9.5 M6,4 L6,12 M9,2.5 L9,13.5 M12,5.5 L12,10.5",
        // Character: minimal user silhouette
        SearchItemKind.Character => "M8,7.5 A2.5,2.5 0 1 1 8,2.5 A2.5,2.5 0 1 1 8,7.5 Z M3.5,13.5 C3.5,11 5.5,9.5 8,9.5 C10.5,9.5 12.5,11 12.5,13.5",
        // Template: folder outline (like reference screenshot)
        SearchItemKind.Template => "M2,4.5 C2,3.67 2.67,3 3.5,3 L6.3,3 L7.6,4.5 L12.5,4.5 C13.33,4.5 14,5.17 14,6 L14,11.5 C14,12.33 13.33,13 12.5,13 L3.5,13 C2.67,13 2,12.33 2,11.5 Z",
        // Command: chevron prompt >_
        SearchItemKind.Command => "M3.5,4.5 L7,8 L3.5,11.5 M8.5,11.5 L13,11.5",
        _ => "M3,8 L13,8"
    };

    public SearchResultItem(
        string title,
        string category,
        string subText,
        string keywords,
        SearchItemKind kind,
        Action action)
    {
        Title = title ?? string.Empty;
        Category = category ?? string.Empty;
        SubText = subText ?? string.Empty;
        Keywords = keywords ?? string.Empty;
        Kind = kind;
        Action = action;
    }
}
