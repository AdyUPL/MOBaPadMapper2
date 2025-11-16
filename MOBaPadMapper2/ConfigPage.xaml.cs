using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace MOBaPadMapper2;

public partial class ConfigPage : ContentPage
{
    private readonly GameProfile _profile;
    private readonly MobaInputMapper _mapper;

    public ConfigPage(GameProfile profile, MobaInputMapper mapper)
    {
        InitializeComponent();

        _profile = profile;
        _mapper = mapper;

        ProfileNameLabel.Text = $"Konfiguracja: {_profile.Name}";

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Jeśli Canvas ma już rozmiar – rysujemy od razu
        if (Canvas.Width > 0 && Canvas.Height > 0)
        {
            RenderButtons();
        }
        else
        {
            // W przeciwnym razie – rysujemy przy pierwszej zmianie rozmiaru,
            // i od razu wypisujemy liczbę mapowań dla debugowania
            Canvas.SizeChanged += CanvasOnSizeChangedOnce;
        }
    }

    private void CanvasOnSizeChangedOnce(object? sender, EventArgs e)
    {
        Canvas.SizeChanged -= CanvasOnSizeChangedOnce;
        RenderButtons();
    }

    private void RenderButtons()
    {
        Canvas.Children.Clear();

        if (Canvas.Width <= 0 || Canvas.Height <= 0)
            return;

        if (_profile.Mappings.Count == 0)
        {
            var info = new Label
            {
                Text = "Brak mapowań w profilu",
                TextColor = Colors.LightGray,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            Canvas.Children.Add(info);
            AbsoluteLayout.SetLayoutBounds(info, new Rect(0, 0, Canvas.Width, Canvas.Height));
            AbsoluteLayout.SetLayoutFlags(info, AbsoluteLayoutFlags.None);
            return;
        }

        foreach (var mapping in _profile.Mappings)
        {
            var label = mapping.TriggerButton?.ToString() ?? "?";

            var size = mapping.Size > 0 ? mapping.Size : 60;
            var radius = size / 2;

            var button = new Button
            {
                Text = label,
                WidthRequest = size,
                HeightRequest = size,
                CornerRadius = (int)radius,
                BackgroundColor = Colors.DarkOrange,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
            };

            button.BindingContext = mapping;

            // 👉 Normalizacja współrzędnych na zakres [0..1]
            mapping.TargetX = Math.Clamp(mapping.TargetX, 0.0, 1.0);
            mapping.TargetY = Math.Clamp(mapping.TargetY, 0.0, 1.0);

            // Wyliczamy środek
            var centerX = mapping.TargetX * Canvas.Width;
            var centerY = mapping.TargetY * Canvas.Height;

            // Margines, żeby kółko nie wyszło poza ekran
            var minX = radius;
            var maxX = Canvas.Width - radius;
            var minY = radius;
            var maxY = Canvas.Height - radius;

            centerX = Math.Clamp(centerX, minX, maxX);
            centerY = Math.Clamp(centerY, minY, maxY);

            var x = centerX - radius;
            var y = centerY - radius;

            AbsoluteLayout.SetLayoutBounds(button, new Rect(x, y, size, size));
            AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayoutFlags.None);

            // Gest przeciągania
            var pan = new PanGestureRecognizer();
            double startX = 0, startY = 0;

            pan.PanUpdated += (s, e) =>
            {
                if (e.StatusType == GestureStatus.Started)
                {
                    var bounds = AbsoluteLayout.GetLayoutBounds(button);
                    startX = bounds.X;
                    startY = bounds.Y;
                }
                else if (e.StatusType == GestureStatus.Running)
                {
                    var newX = startX + e.TotalX;
                    var newY = startY + e.TotalY;

                    // Też przycinamy, żeby nie dało się wypchnąć kółka poza Canvas
                    var clampedCenterX = Math.Clamp(newX + radius, minX, maxX);
                    var clampedCenterY = Math.Clamp(newY + radius, minY, maxY);

                    newX = clampedCenterX - radius;
                    newY = clampedCenterY - radius;

                    AbsoluteLayout.SetLayoutBounds(button, new Rect(newX, newY, size, size));
                }
                else if (e.StatusType == GestureStatus.Completed)
                {
                    var bounds = AbsoluteLayout.GetLayoutBounds(button);
                    var newCenterX = bounds.X + bounds.Width / 2;
                    var newCenterY = bounds.Y + bounds.Height / 2;

                    mapping.TargetX = newCenterX / Canvas.Width;
                    mapping.TargetY = newCenterY / Canvas.Height;

                    _mapper.SetProfile(_profile);
                }
            };

            button.GestureRecognizers.Add(pan);

            Canvas.Children.Add(button);
        }
    }
}
