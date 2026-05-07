using ClassicHitster.App.Pages;

namespace ClassicHitster.App;

public sealed class AppShell : Shell
{
    public AppShell()
    {
        Routing.RegisterRoute(nameof(ScannerPage), typeof(ScannerPage));
        Routing.RegisterRoute(nameof(PlayerPage), typeof(PlayerPage));
        Routing.RegisterRoute(nameof(SongListPage), typeof(SongListPage));

        Items.Add(new ShellContent
        {
            Title = "Classic Hitster",
            Route = nameof(MainPage),
            ContentTemplate = new DataTemplate(typeof(MainPage))
        });
    }
}
