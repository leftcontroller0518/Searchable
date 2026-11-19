using System;
using YukkuriMovieMaker.Plugin;

namespace Searchable;

public class SearchableToolPlugin : IToolPlugin
{
    public string Name => "Searchable";

    public Type ViewModelType => typeof(SearchableToolViewModel);

    public Type ViewType => typeof(SearchableToolView);
}

