using Wpf.Ui.Abstractions;

namespace AutoHdrManager.Services;

public class PageService : INavigationViewPageProvider
{
    private readonly Dictionary<Type, object> _cache = new();

    public object? GetPage(Type pageType)
    {
        if (_cache.TryGetValue(pageType, out var cached))
            return cached;

        var page = Activator.CreateInstance(pageType);
        if (page is not null)
            _cache[pageType] = page;

        return page;
    }
}
