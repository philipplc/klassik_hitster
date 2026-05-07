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

        var torchButton = new Button
        {
            Text = "Licht an/aus",
            FontSize = 15,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#342843"),
            CornerRadius = 16,
            HeightRequest = 48
        };
        torchButton.Clicked += (_, _) => cameraView.IsTorchOn = !cameraView.IsTorchOn;

        var manualButton = new Button
        {
            Text = "ID manuell eingeben",
            FontSize = 15,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#342843"),
            CornerRadius = 16,
            HeightRequest = 48
        };
        manualButton.Clicked += EnterIdManually;

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            Children =
            {
                cameraView,
                new Border
                {
                    BackgroundColor = Color.FromArgb("#CC161219"),
                    Padding = new Thickness(16),
                    StrokeShape = new RoundRectangle { CornerRadius = 20 },
                    Margin = new Thickness(16),
                    Content = new VerticalStackLayout
                    {
                        Spacing = 10,
                        Children =
                        {
                            statusLabel,
                            torchButton,
                            manualButton
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
}

internal static class ViewGridExtensions
{
    public static T AssignToGridRow<T>(this T view, int row) where T : View
    {
        Grid.SetRow(view, row);
        return view;
    }
}
