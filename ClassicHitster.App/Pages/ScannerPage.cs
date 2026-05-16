using Microsoft.Maui.Controls.Shapes;
using ClassicHitster.Shared;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace ClassicHitster.App.Pages;

public sealed class ScannerPage : ContentPage
{
    private readonly CameraBarcodeReaderView cameraView;
    private readonly Label statusLabel;
    private bool isNavigating;

    public ScannerPage()
    {
        Title = "QR-Code scannen";
        BackgroundColor = Color.FromArgb("#161219");
        Shell.SetNavBarIsVisible(this, false);

        statusLabel = new Label
        {
            Text = "Kamera auf den QR-Code der Karte richten.",
            TextColor = Colors.White,
            FontSize = 15,
            HorizontalTextAlignment = TextAlignment.Center
        };

        cameraView = new CameraBarcodeReaderView
        {
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.TwoDimensional,
                AutoRotate = true,
                Multiple = false
            },
            IsDetecting = false,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        cameraView.BarcodesDetected += OnBarcodesDetected;

        var torchButton = new ImageButton
        {
            Source = "icon_flashlight.png",
            BackgroundColor = Color.FromArgb("#342843"),
            CornerRadius = 22,
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = 8
        };
        torchButton.Clicked += (_, _) => cameraView.IsTorchOn = !cameraView.IsTorchOn;

        var settingsButton = new ImageButton
        {
            Source = "icon_settings.png",
            BackgroundColor = Color.FromArgb("#342843"),
            CornerRadius = 22,
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = 8
        };
        settingsButton.Clicked += async (_, _) => await Shell.Current.GoToAsync(nameof(MainPage));

        var bottomBar = new HorizontalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            Children = { torchButton, settingsButton }
        };

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            Children =
            {
                cameraView.AssignToGridRow(0),
                new Border
                {
                    BackgroundColor = Color.FromArgb("#CC161219"),
                    Padding = new Thickness(16),
                    StrokeShape = new RoundRectangle { CornerRadius = 20 },
                    Margin = new Thickness(16),
                    Content = new VerticalStackLayout
                    {
                        Spacing = 12,
                        Children =
                        {
                            statusLabel,
                            bottomBar
                        }
                    }
                }.AssignToGridRow(1)
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        isNavigating = false;

        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        if (status == PermissionStatus.Granted)
        {
            statusLabel.Text = "Kamera auf den QR-Code der Karte richten.";
            cameraView.IsDetecting = true;
        }
        else
        {
            statusLabel.Text = "Kamerazugriff wurde verweigert. Du kannst die ID manuell eingeben.";
            cameraView.IsDetecting = false;
        }
    }

    protected override void OnDisappearing()
    {
        cameraView.IsDetecting = false;
        base.OnDisappearing();
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (isNavigating)
        {
            return;
        }

        var value = e.Results.FirstOrDefault()?.Value;
        var cardId = CardPayload.TryExtractCardId(value);
        if (cardId is null)
        {
            return;
        }

        isNavigating = true;
        cameraView.IsDetecting = false;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync($"{nameof(PlayerPage)}?id={Uri.EscapeDataString(cardId)}");
        });
    }

}

internal static class ViewGridExtensions
{
    public static T AssignToGridRow<T>(this T view, int row) where T : View
    {
        Grid.SetRow(view, row);
        return view;
    }
}
