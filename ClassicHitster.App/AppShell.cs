using ClassicHitster.App.Pages;

namespace ClassicHitster.App;

public sealed class AppShell : Shell
{
    public AppShell()
    {
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

        Routing.RegisterRoute(nameof(ScannerPage), typeof(ScannerPage));
        Routing.RegisterRoute(nameof(PlayerPage), typeof(PlayerPage));
        Routing.RegisterRoute(nameof(SongListPage), typeof(SongListPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));

        Items.Add(new ShellContent
        {
            Title = "Klassik Hitster",
            Route = nameof(ScannerPage),
            ContentTemplate = new DataTemplate(typeof(ScannerPage))
        });
    }
}
