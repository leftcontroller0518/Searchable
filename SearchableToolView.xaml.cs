using System.Windows;
using System.Windows.Controls;

namespace Searchable;

public partial class SearchableToolView : UserControl
{
    public SearchableToolView()
    {
        InitializeComponent();
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        SearchableCommandPlugin.ShowSearchWindow();
    }
}

