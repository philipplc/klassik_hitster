using ClassicHitster.App.Services;
using ClassicHitster.Shared;

namespace ClassicHitster.App.Pages;

[QueryProperty(nameof(CardId), "id")]
public sealed class PlayerPage : ContentPage
{
    private readonly LocalAudioSongPlayer player = new();
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
    private bool isPlaying;
    private readonly ImageButton playPauseButton;

    public string? CardId
    {
        get => cardId;
        set
        {
            cardId = value is null ? null : Uri.UnescapeDataString(value);
            _ = LoadAndPlayAsync();
        }
    }

    public PlayerPage()
    {
        Title = "Karte";
        BackgroundColor = Color.FromArgb("#161219");
        Shell.SetNavBarIsVisible(this, false);

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
            HorizontalOptions = LayoutOptions.Fill,
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

        playPauseButton = new ImageButton
        {
            Source = "icon_play.png",
            BackgroundColor = Colors.Transparent,
            WidthRequest = 100,
            HeightRequest = 100,
            HorizontalOptions = LayoutOptions.Center
        };
        playPauseButton.Clicked += OnPlayPauseTapped;

        var revealButton = CreateSecondaryButton("Auflösung");
        revealButton.Clicked += (_, _) => RevealSolution();

        var scanAgainButton = CreateSecondaryButton("Neue Karte");
        scanAgainButton.Clicked += async (_, _) => await GoToScannerAsync();

        var bottomButtons = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 12,
            HorizontalOptions = LayoutOptions.Fill,
            Children =
            {
                revealButton.AssignToGridColumn(0),
                scanAgainButton.AssignToGridColumn(1)
            }
        };

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            Padding = new Thickness(24, 24, 24, 40),
            RowSpacing = 16,
            Children =
            {
                new BoxView { Opacity = 0 }.AssignToGridRow(0),
                revealLayout.AssignToGridRow(1),
                playPauseButton.AssignToGridRow(2),
                bottomButtons.AssignToGridRow(3)
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
    }

    private async Task LoadAndPlayAsync()
    {
        try
        {
            currentSong = await SongCatalog.GetSongByIdAsync(cardId);
            revealLayout.IsVisible = false;

            if (currentSong is null)
            {
                await DisplayAlert("Unbekannte Karte",
                    string.IsNullOrWhiteSpace(cardId)
                        ? "Keine Karten-ID übergeben."
                        : $"Die Karten-ID '{cardId}' steht nicht in songs.json.",
                    "OK");
                return;
            }

            await PlayCurrentSong();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async Task PlayCurrentSong()
    {
        if (currentSong is null) return;

        try
        {
            await player.PlayAsync(currentSong);
            isPlaying = true;
            isPaused = false;
            playPauseButton.Source = "icon_pause.png";
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

    private void OnPlayPauseTapped(object? sender, EventArgs e)
    {
        if (currentSong is null) return;

        if (!isPlaying)
        {
            _ = PlayCurrentSong();
            return;
        }

        if (isPaused)
        {
            player.Resume();
            isPaused = false;
            playPauseButton.Source = "icon_pause.png";
        }
        else
        {
            player.Pause();
            isPaused = true;
            playPauseButton.Source = "icon_play.png";
        }
    }

    private void RevealSolution()
    {
        if (currentSong is null) return;

        composerLabel.Text = $"Komponist: {currentSong.Composer}";

        var titleAndWork = currentSong.Work is not null
            && !string.Equals(currentSong.Title, currentSong.Work, StringComparison.Ordinal);
        workLabel.Text = titleAndWork
            ? $"Titel: {currentSong.Title}\nWerk: {currentSong.Work}"
            : $"Titel: {currentSong.Title}";

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
