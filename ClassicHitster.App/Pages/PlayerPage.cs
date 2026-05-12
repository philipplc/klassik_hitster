using ClassicHitster.App.Services;
using ClassicHitster.Shared;

namespace ClassicHitster.App.Pages;

[QueryProperty(nameof(CardId), "id")]
public sealed class PlayerPage : ContentPage
{
    private readonly LocalAudioSongPlayer player = new();
    private readonly Label titleLabel;
    private readonly Label statusLabel;
    private readonly VerticalStackLayout revealLayout;
    private readonly Label composerLabel;
    private readonly Label workLabel;
    private readonly Label yearLabel;
    private readonly Label eraLabel;
    private readonly Label performerLabel;
    private readonly Label notesLabel;

    private SongCard? currentSong;
    private string? cardId;
    private bool isPaused;
    private Button? pauseButton;

    public string? CardId
    {
        get => cardId;
        set
        {
            cardId = value is null ? null : Uri.UnescapeDataString(value);
            _ = LoadSongAsync();
        }
    }

    public PlayerPage()
    {
        Title = "Karte";
        BackgroundColor = Color.FromArgb("#161219");

        titleLabel = new Label
        {
            Text = "Karte wird geladen ...",
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center
        };

        statusLabel = new Label
        {
            Text = "Noch nicht abgespielt.",
            FontSize = 15,
            TextColor = Color.FromArgb("#D6C9E6"),
            HorizontalTextAlignment = TextAlignment.Center
        };

        var playButton = CreatePrimaryButton("▶ Abspielen");
        playButton.Clicked += Play;

        pauseButton = CreateSecondaryButton("⏸ Pause");
        pauseButton.Clicked += (_, _) =>
        {
            if (isPaused)
            {
                player.Resume();
                isPaused = false;
                pauseButton.Text = "⏸ Pause";
                statusLabel.Text = "Spielt ab. Jetzt raten: Jahr / Komponist / Titel.";
            }
            else
            {
                player.Pause();
                isPaused = true;
                pauseButton.Text = "▶ Weiter";
                statusLabel.Text = "Pausiert.";
            }
        };

        var stopButton = CreateSecondaryButton("⏹ Stop");
        stopButton.Clicked += (_, _) =>
        {
            player.Stop();
            isPaused = false;
            pauseButton!.Text = "⏸ Pause";
            statusLabel.Text = "Gestoppt.";
        };

        var revealButton = CreateSecondaryButton("Auflösung anzeigen");
        revealButton.Clicked += (_, _) => RevealSolution();

        var scanAgainButton = CreateSecondaryButton("Neue Karte scannen");
        scanAgainButton.Clicked += async (_, _) => await GoToScannerAsync();

        composerLabel = CreateRevealLabel();
        workLabel = CreateRevealLabel();
        yearLabel = CreateRevealLabel();
        eraLabel = CreateRevealLabel();
        performerLabel = CreateRevealLabel();
        notesLabel = CreateRevealLabel();

        revealLayout = new VerticalStackLayout
        {
            IsVisible = false,
            Spacing = 8,
            Padding = new Thickness(18),
            BackgroundColor = Color.FromArgb("#241B30"),
            Children =
            {
                new Label
                {
                    Text = "Auflösung",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                },
                composerLabel,
                workLabel,
                yearLabel,
                eraLabel,
                performerLabel,
                notesLabel
            }
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(24, 36, 24, 24),
                Spacing = 16,
                Children =
                {
                    titleLabel,
                    statusLabel,
                    new BoxView { HeightRequest = 8, Opacity = 0 },
                    playButton,
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Star)
                        },
                        ColumnSpacing = 12,
                        Children =
                        {
                            pauseButton.AssignToGridColumn(0),
                            stopButton.AssignToGridColumn(1)
                        }
                    },
                    revealButton,
                    revealLayout,
                    scanAgainButton
                }
            }
        };
    }

    protected override void OnDisappearing()
    {
        player.Stop();
        base.OnDisappearing();
    }

    private static async Task GoToScannerAsync()
    {
        await Shell.Current.Navigation.PopToRootAsync();
        await Shell.Current.GoToAsync(nameof(ScannerPage));
    }

    private async Task LoadSongAsync()
    {
        try
        {
            currentSong = await SongCatalog.GetSongByIdAsync(cardId);
            revealLayout.IsVisible = false;

            if (currentSong is null)
            {
                titleLabel.Text = "Unbekannte Karte";
                statusLabel.Text = string.IsNullOrWhiteSpace(cardId)
                    ? "Keine Karten-ID übergeben."
                    : $"Die Karten-ID '{cardId}' steht nicht in songs.json.";
                return;
            }

            titleLabel.Text = "Karte erkannt";
            statusLabel.Text = "Drücke Abspielen. Die Lösung bleibt verborgen.";
        }
        catch (Exception ex)
        {
            titleLabel.Text = "Fehler";
            statusLabel.Text = ex.Message;
        }
    }

    private async void Play(object? sender, EventArgs e)
    {
        if (currentSong is null)
        {
            await DisplayAlert("Keine Karte", "Es wurde kein gültiger Song geladen.", "OK");
            return;
        }

        try
        {
            await player.PlayAsync(currentSong);
            isPaused = false;
            pauseButton!.Text = "⏸ Pause";
            statusLabel.Text = "Spielt ab. Jetzt raten: Jahr / Komponist / Titel.";
        }
        catch (FileNotFoundException)
        {
            await DisplayAlert("Audiodatei fehlt", $"Die Datei '{currentSong.AudioFile}' liegt nicht in Resources/Raw.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Audiofehler", ex.Message, "OK");
        }
    }

    private void RevealSolution()
    {
        if (currentSong is null)
        {
            return;
        }

        composerLabel.Text = $"Komponist: {currentSong.Composer}";

        var titleAndWork = currentSong.Work is not null
            && !string.Equals(currentSong.Title, currentSong.Work, StringComparison.Ordinal);
        if (titleAndWork)
        {
            workLabel.Text = $"Titel: {currentSong.Title}\nWerk: {currentSong.Work}";
        }
        else
        {
            workLabel.Text = $"Titel: {currentSong.Title}";
        }

        yearLabel.Text = $"Jahr: {currentSong.YearDisplay}";
        eraLabel.Text = string.IsNullOrWhiteSpace(currentSong.Era) ? string.Empty : $"Epoche: {currentSong.Era}";
        performerLabel.Text = string.IsNullOrWhiteSpace(currentSong.Performer) ? string.Empty : $"Aufnahme/Interpret: {currentSong.Performer}";
        notesLabel.Text = string.IsNullOrWhiteSpace(currentSong.Notes) ? string.Empty : $"Notiz: {currentSong.Notes}";

        eraLabel.IsVisible = !string.IsNullOrWhiteSpace(currentSong.Era);
        performerLabel.IsVisible = !string.IsNullOrWhiteSpace(currentSong.Performer);
        notesLabel.IsVisible = !string.IsNullOrWhiteSpace(currentSong.Notes);
        revealLayout.IsVisible = true;
    }

    private static Label CreateRevealLabel()
    {
        return new Label
        {
            FontSize = 15,
            TextColor = Color.FromArgb("#E9DEF8")
        };
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

internal static class GridColumnExtensions
{
    public static T AssignToGridColumn<T>(this T view, int column) where T : View
    {
        Grid.SetColumn(view, column);
        return view;
    }
}
