using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOBaPadMapper2;

[QueryProperty(nameof(Profile), "profile")]
public partial class TestPage : ContentPage
{
    private GameProfile? _profile;

    private double _lastWidth;
    private double _lastHeight;

    private ActionMapping? _selectedMapping;
    private View? _selectedView;
    private bool _isUpdatingUi;

    // startowe położenie dla drag & drop
    private readonly Dictionary<View, Rect> _dragStartBounds = new();

    public GameProfile? Profile
    {
        get => _profile;
        set
        {
            _profile = value;
            GameNameLabel.Text = $"Gra: {_profile?.Name ?? "-"}";
            RenderButtons();
        }
    }

    public TestPage()
    {
        InitializeComponent();

        // lista dostępnych działań
        ActionTypePicker.ItemsSource = Enum.GetValues(typeof(ActionType)).Cast<ActionType>().ToList();
        ActionTypePicker.SelectedIndexChanged += ActionTypePicker_SelectedIndexChanged;

        SizeSlider.ValueChanged += SizeSlider_ValueChanged;

        // kiedy powierzchnia dostanie rozmiar – spróbuj narysować przyciski
        TestSurface.SizeChanged += (s, e) => RenderButtons();

        // kliknięcie w tło = odznaczenie
        var bgTap = new TapGestureRecognizer();
        bgTap.Tapped += OnBackgroundTapped;
        TestSurface.GestureRecognizers.Add(bgTap);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RenderButtons();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (Math.Abs(width - _lastWidth) > 0.5 || Math.Abs(height - _lastHeight) > 0.5)
        {
            _lastWidth = width;
            _lastHeight = height;
            RenderButtons();
        }
    }

    private void RenderButtons()
    {
        if (TestSurface == null || _profile == null)
            return;

        if (TestSurface.Width <= 0 || TestSurface.Height <= 0)
            return;

        var currentSelected = _selectedMapping;

        TestSurface.Children.Clear();
        _dragStartBounds.Clear();

        foreach (var mapping in _profile.Mappings)
        {
            var size = mapping.Size <= 0 ? 60 : mapping.Size;

            var label = new Label
            {
                Text = mapping.TriggerButton.ToString(),
                TextColor = Colors.White,
                BackgroundColor = Colors.DimGray,
                WidthRequest = size,
                HeightRequest = size,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold
            };

            // pan – przeciąganie
            var pan = new PanGestureRecognizer();
            pan.PanUpdated += (s, e) => OnButtonPanUpdated(label, e, mapping);
            label.GestureRecognizers.Add(pan);

            // tap – zaznaczenie przycisku
            var tap = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            tap.Tapped += (s, e) =>
            {
                // żeby kliknięcie w przycisk nie wywoływało tapu z tła
                //e.Handle = true;
                SelectMapping(mapping, label);
            };
            label.GestureRecognizers.Add(tap);

            // Pozycjonowanie – TargetX/Y (0..1 – środek)
            var x = mapping.TargetX * TestSurface.Width - size / 2.0;
            var y = mapping.TargetY * TestSurface.Height - size / 2.0;

            x = Math.Clamp(x, 0, TestSurface.Width - size);
            y = Math.Clamp(y, 0, TestSurface.Height - size);

            var rect = new Rect(x, y, size, size);
            AbsoluteLayout.SetLayoutBounds(label, rect);
            AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.None);

            TestSurface.Children.Add(label);

            if (currentSelected == mapping)
            {
                SelectMapping(mapping, label, fromRender: true);
            }
        }

        // jeśli po odrysowaniu nie ma zaznaczonego przycisku → schowaj panel
        if (_selectedMapping == null)
        {
            ConfigPanel.IsVisible = false;
        }
    }

    private void SelectMapping(ActionMapping mapping, View view, bool fromRender = false)
    {
        _selectedMapping = mapping;
        _selectedView = view;

        // podświetlenie
        foreach (var child in TestSurface.Children.OfType<Label>())
        {
            child.Opacity = 0.7;
            child.BackgroundColor = Colors.DimGray;
        }

        if (view is Label lbl)
        {
            lbl.Opacity = 1.0;
            lbl.BackgroundColor = Colors.Orange;
        }

        // pokazujemy panel
        ConfigPanel.IsVisible = true;

        // aktualizacja panelu ustawień
        _isUpdatingUi = true;
        SelectedButtonLabel.Text = mapping.TriggerButton.ToString();
        ActionTypePicker.SelectedItem = mapping.ActionType;
        SizeSlider.Value = mapping.Size <= 0 ? 60 : mapping.Size;
        _isUpdatingUi = false;
    }

    private void ClearSelection()
    {
        _selectedMapping = null;
        _selectedView = null;

        foreach (var child in TestSurface.Children.OfType<Label>())
        {
            child.Opacity = 1.0;
            child.BackgroundColor = Colors.DimGray;
        }

        ConfigPanel.IsVisible = false;
        SelectedButtonLabel.Text = "-";
    }

    private void OnBackgroundTapped(object? sender, TappedEventArgs e)
    {
        // kliknięcie w puste miejsce = odznaczenie
        ClearSelection();
    }

    private void ActionTypePicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_selectedMapping == null || _isUpdatingUi)
            return;

        if (ActionTypePicker.SelectedItem is ActionType at)
        {
            _selectedMapping.ActionType = at;
        }
    }

    private void SizeSlider_ValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_selectedMapping == null || _isUpdatingUi)
            return;

        // 1) zaktualizuj model
        _selectedMapping.Size = e.NewValue;

        // 2) przebuduj przyciski – ale zachowamy zaznaczenie
        RenderButtons();
    }

    private void OnButtonPanUpdated(View view, PanUpdatedEventArgs e, ActionMapping mapping)
    {
        if (TestSurface == null)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                {
                    // zapamiętujemy startowe położenie
                    var startBounds = AbsoluteLayout.GetLayoutBounds(view);

                    // jeżeli brak szerokości/wysokości z layoutu – użyj rozmiaru z mappingu
                    if (startBounds.Width <= 0 || startBounds.Height <= 0)
                    {
                        var size = mapping.Size <= 0 ? 60 : mapping.Size;
                        startBounds = new Rect(startBounds.X, startBounds.Y, size, size);
                    }

                    _dragStartBounds[view] = startBounds;
                    break;
                }

            case GestureStatus.Running:
                {
                    if (!_dragStartBounds.TryGetValue(view, out var startBounds))
                        return;

                    var width = TestSurface.Width;
                    var height = TestSurface.Height;

                    if (width <= 0 || height <= 0)
                        return;

                    var newX = startBounds.X + e.TotalX;
                    var newY = startBounds.Y + e.TotalY;

                    var buttonWidth = startBounds.Width;
                    var buttonHeight = startBounds.Height;

                    var maxX = width - buttonWidth;
                    var maxY = height - buttonHeight;

                    newX = Math.Clamp(newX, 0, maxX);
                    newY = Math.Clamp(newY, 0, maxY);

                    AbsoluteLayout.SetLayoutBounds(view, new Rect(newX, newY, buttonWidth, buttonHeight));
                    break;
                }

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                {
                    if (!_dragStartBounds.TryGetValue(view, out var startBounds))
                        return;

                    _dragStartBounds.Remove(view);

                    var width = TestSurface.Width;
                    var height = TestSurface.Height;

                    if (width <= 0 || height <= 0)
                        return;

                    var bounds = AbsoluteLayout.GetLayoutBounds(view);
                    var buttonWidth = bounds.Width;
                    var buttonHeight = bounds.Height;

                    // zapisujemy pozycję jako środki (proporcje 0..1)
                    var centerX = bounds.X + buttonWidth / 2.0;
                    var centerY = bounds.Y + buttonHeight / 2.0;

                    mapping.TargetX = centerX / width;
                    mapping.TargetY = centerY / height;

                    break;
                }
        }
    }

    // Dodawanie nowego przycisku bezpośrednio z ekranu konfiguracyjnego
    private async void OnAddMappingClicked(object sender, EventArgs e)
    {
        if (_profile == null)
            return;

        var buttonName = await DisplayActionSheet(
            "Przycisk pada",
            "Anuluj",
            null,
            Enum.GetNames(typeof(GamepadButton)));

        if (string.IsNullOrEmpty(buttonName) || buttonName == "Anuluj")
            return;

        var actionName = await DisplayActionSheet(
            "Działanie",
            "Anuluj",
            null,
            nameof(ActionType.Tap),
            nameof(ActionType.Swipe),
            nameof(ActionType.HoldAndAim));

        if (string.IsNullOrEmpty(actionName) || actionName == "Anuluj")
            return;

        var mapping = new ActionMapping
        {
            TriggerButton = Enum.Parse<GamepadButton>(buttonName),
            ActionType = Enum.Parse<ActionType>(actionName),
            TargetX = 0.5,
            TargetY = 0.5,
            Size = 60
        };

        _profile.Mappings.Add(mapping);
        _selectedMapping = mapping;

        RenderButtons();
    }
}
