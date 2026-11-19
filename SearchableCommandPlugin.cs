using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Settings;

namespace Searchable;

public class SearchableCommandPlugin : ICommandPlugin
{
    private static SearchWindow? activeWindow;

    public string Name => "Searchable";

    public IEnumerable<PluginCommandBase> Commands { get; } = new[]
    {
        new OpenSearchableCommand()
    };

    public static void ShowSearchWindow()
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            if (activeWindow != null && activeWindow.IsLoaded)
            {
                activeWindow.Activate();
                return;
            }

            var vm = new SearchableViewModel();
            var win = new SearchWindow(vm);
            activeWindow = win;

            win.Closed += (s, e) =>
            {
                if (activeWindow == win)
                {
                    activeWindow = null;
                }
            };

            win.Show();
            win.Activate();
        });
    }

    private sealed class OpenSearchableCommand : PluginCommand
    {
        public override ICommandId<NoParameter> Id { get; } = new PluginCommandId<NoParameter>("Searchable.OpenSearchWindow");

        public override string Name => "Searchable 検索バーを開く";

        public override Key DefaultKey => Key.F;

        public override ModifierKeys DefaultModifierKeys => ModifierKeys.Control | ModifierKeys.Shift;

        public override bool CanExecute() => true;

        public override void Execute()
        {
            ShowSearchWindow();
        }
    }
}
