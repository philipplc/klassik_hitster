using ClassicHitster.App.Services;

namespace ClassicHitster.App.Pages;

public sealed class MainPage : ContentPage
{
    private readonly Label subtitleLabel;

    public MainPage()
    {
        Title = "Classic Hitster";
        BackgroundColor = Color.FromArgb("#161219");

        var titleLabel = new Label
        {
            Text = "Classic Hitster",
            FontSize = 34,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center
        };

        subtitleLabel = new Label
        {
            Text = "Lade Songliste ...",
            FontSize = 16,
            TextColor = Color.FromArgb("#D6C9E6"),
            HorizontalTextAlignment = TextAlignment.Center
        };

        var scanButton = CreatePrimaryButton("QR-Code scannen");
        scanButton.Clicked += async (_, _) => await Shell.Current.GoToAsync(nameof(ScannerPage));

        var demoButton = CreateSecondaryButton("Demo-Karte öffnen");
        demoButton.Clicked += OpenDemoCard;

        var listButton = CreateSecondaryButton("Songliste anzeigen");
        listButton.Clicked += async (_, _) => await Shell.Current.GoToAsync(nameof(SongListPage));

        var hintLabel = new Label
        {
            Text = "Ablauf: Karte ziehen → QR-Code scannen → Song abspielen → raten → Auflösung anzeigen.",
            TextColor = Color.FromArgb("#B8ABC8"),
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(24, 48, 24, 24),
                Spacing = 18,
                Children =
                {
                    titleLabel,
                    subtitleLabel,
                    new BoxView { HeightRequest = 10, Opacity = 0 },
                    scanButton,
                    demoButton,
                    listButton,
                    new BoxView { HeightRequest = 16, Opacity = 0 },
                    hintLabel
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var songs = await SongCatalog.GetSongsAsync();
            subtitleLabel.Text = $"{songs.Count} Karten geladen";
        }
        catch (Exception ex)
        {
            subtitleLabel.Text = "Songliste konnte nicht geladen werden: " + ex.Message;
        }
    }

    private async void OpenDemoCard(object? sender, EventArgs e)
    {
        try
        {
            var firstSong = (await SongCatalog.GetSongsAsync()).FirstOrDefault();
            if (firstSong is null)
            {
                await DisplayAlert("Keine Songs", "Die Songliste ist leer.", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(PlayerPage)}?id={Uri.EscapeDataString(firstSong.Id)}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private static Button CreatePrimaryButton(string text)
    {
        return new Button
        {
            Text = text,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#6E3BD4"),
            CornerRadius = 18,
            HeightRequest = 56
        };
    }

    private static Button CreateSecondaryButton(string text)
    {
        return new Button
        {
            Text = text,
            FontSize = 16,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#342843"),
            CornerRadius = 18,
            HeightRequest = 52
        };
    }
}
