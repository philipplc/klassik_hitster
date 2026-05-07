using Microsoft.Maui.Controls.Shapes;
using ClassicHitster.App.Services;
using ClassicHitster.Shared;

namespace ClassicHitster.App.Pages;

public sealed class SongListPage : ContentPage
{
    private readonly VerticalStackLayout listLayout;

    public SongListPage()
    {
        Title = "Songliste";
        BackgroundColor = Color.FromArgb("#161219");

        listLayout = new VerticalStackLayout
        {
            Spacing = 12,
            Padding = new Thickness(18)
        };

        Content = new ScrollView { Content = listLayout };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadListAsync();
    }

    private async Task LoadListAsync()
    {
        listLayout.Children.Clear();

        IReadOnlyList<SongCard> songs;
        try
        {
            songs = await SongCatalog.GetSongsAsync();
        }
        catch (Exception ex)
        {
            listLayout.Children.Add(new Label
            {
                Text = "Songliste konnte nicht geladen werden: " + ex.Message,
                TextColor = Colors.White
            });
            return;
        }

        listLayout.Children.Add(new Label
        {
            Text = $"{songs.Count} Karten",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });

        foreach (var song in songs.OrderBy(song => song.Year).ThenBy(song => song.Composer).ThenBy(song => song.Title))
        {
            var card = new Border
            {
                Padding = new Thickness(14),
                BackgroundColor = Color.FromArgb("#241B30"),
                Stroke = Color.FromArgb("#3D3150"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Content = new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label
                        {
                            Text = $"{song.YearDisplay} · {song.Composer}",
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                            FontSize = 16
                        },
                        new Label
                        {
                            Text = song.Title,
                            TextColor = Color.FromArgb("#D6C9E6"),
                            FontSize = 14
                        },
                        new Label
                        {
                            Text = "ID: " + song.Id,
                            TextColor = Color.FromArgb("#9F91B1"),
                            FontSize = 12
                        }
                    }
                }
            };

            listLayout.Children.Add(card);
        }
    }
}
