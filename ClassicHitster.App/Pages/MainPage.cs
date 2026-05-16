using ClassicHitster.App.Services;
using ClassicHitster.Shared;

namespace ClassicHitster.App.Pages;

public sealed class MainPage : ContentPage
{
    private readonly Label subtitleLabel;

    public MainPage()
    {
        Title = "Einstellungen";
        BackgroundColor = Color.FromArgb("#161219");

        var titleLabel = new Label
        {
            Text = "Klassik Hitster",
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

        var listButton = CreateSecondaryButton("Songliste anzeigen");
        listButton.Clicked += async (_, _) => await Shell.Current.GoToAsync(nameof(SongListPage));

        var manualButton = CreateSecondaryButton("ID manuell eingeben");
        manualButton.Clicked += EnterIdManually;

        var hintLabel = new Label
        {
            Text = "Ablauf: Karte ziehen → QR-Code scannen → Song abspielen → raten → Auflösung anzeigen.",
            TextColor = Color.FromArgb("#B8ABC8"),
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var backButton = new Button
        {
            Text = "← Zurück",
            FontSize = 16,
            TextColor = Colors.White,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Start,
            Padding = new Thickness(0)
        };
        backButton.Clicked += async (_, _) => await Shell.Current.Navigation.PopAsync();

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(24, 48, 24, 24),
                Spacing = 18,
                Children =
                {
                    backButton,
                    titleLabel,
                    subtitleLabel,
                    new BoxView { HeightRequest = 10, Opacity = 0 },
                    listButton,
                    manualButton,
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

    private async void EnterIdManually(object? sender, EventArgs e)
    {
        var value = await DisplayPromptAsync("Karten-ID", "ID oder kompletten QR-Code-Inhalt eingeben:", "Öffnen", "Abbrechen");
        var cardId = CardPayload.TryExtractCardId(value);
        if (cardId is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(PlayerPage)}?id={Uri.EscapeDataString(cardId)}");
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
