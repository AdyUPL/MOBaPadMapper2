using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
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

    // muszą być typy generyczne
    private readonly Dictionary<View, Rect> _dragStartBounds = new();

    private readonly IGamepadInputService _gamepad;
    private readonly MobaInputMapper _mapper;

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

    public TestPage(IGamepadInputService gamepad, MobaInputMapper mapper)
    {
        InitializeComponent();

        _gamepad = gamepad;
        _mapper = mapper;

        // tu też musi być konkretny typ
        ActionTypePicker.ItemsSource = Enum
            .GetValues(typeof(ActionType))
            .Cast<ActionType>()
            .ToList();

        ActionTypePicker.SelectedIndexChanged += ActionTypePicker_SelectedIndexChanged;
        SizeSlider.ValueChanged += SizeSlider_ValueChanged;

        TestSurface.SizeChanged += (s, e) => RenderButtons();

        var bgTap = new TapGestureRecognizer();
        bgTap.Tapped += OnBackgroundTapped;
        TestSurface.GestureRecognizers.Add(bgTap);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _gamepad.GamepadUpdated += OnGamepadUpdated;
        RenderButtons();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _gamepad.GamepadUpdated -= OnGamepadUpdated;

        // zapisujemy mapowania
        if (_profile != null)
        {
            _mapper.UpdateMappings(_profile.Mappings);
        }
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

    // ====== REAKCJA NA GAMEPADA ======
    private void OnGamepadUpdated(object? sender, GamepadState state)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_profile == null || _profile.Mappings == null || _profile.Mappings.Count == 0)
                return;

            if (state.PressedButtons == null || state.PressedButtons.Count == 0)
                return;

            var pressed = state.PressedButtons.First();

            var mapping = _profile.Mappings.FirstOrDefault(m => m.TriggerButton == pressed);
            if (mapping == null)
                return;

            var view = TestSurface.Children
                .OfType<Label>()
                .FirstOrDefault(l => string.Equals(l.Text, mapping.TriggerButton.ToString(), StringComparison.Ordinal));

            if (view != null)
            {
                SelectMapping(mapping, view);
            }
        });
    }

    // ====== WYGLĄD PRZYCISKÓW – KOLORY JAK NA PADZIE ======
    private static Color GetButtonColor(GamepadButton button)
    {
        return button switch
        {
            GamepadButton.A => Color.FromArgb("#0DB45E"), // zielony
            GamepadButton.B => Color.FromArgb("#D83C3C"), // czerwony
            GamepadButton.X => Color.FromArgb("#2563EB"), // niebieski
            GamepadButton.Y => Color.FromArgb("#EAB308"), // żółty
            _ => Colors.DimGray
        };
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
            var baseColor = GetButtonColor(mapping.TriggerButton);

            var label = new Label
            {
                Text = mapping.TriggerButton.ToString(),
                TextColor = Colors.White,
                BackgroundColor = baseColor,
                WidthRequest = size,
                HeightRequest = size,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                Opacity = 0.85
            };

            var pan = new PanGestureRecognizer();
            pan.PanUpdated += (s, e) => OnButtonPanUpdated(label, e, mapping);
            label.GestureRecognizers.Add(pan);

            var tap = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            tap.Tapped += (s, e) => SelectMapping(mapping, label);
            label.GestureRecognizers.Add(tap);

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

        if (_selectedMapping == null)
        {
            ConfigPanel.IsVisible = false;
        }
    }

    private void SelectMapping(ActionMapping mapping, View view, bool fromRender = false)
    {
        _selectedMapping = mapping;
        _selectedView = view;

        foreach (var child in TestSurface.Children.OfType<Label>())
        {
            if (_profile != null)
            {
                var map = _profile.Mappings.FirstOrDefault(m => m.TriggerButton.ToString() == child.Text);
                if (map != null)
                {
                    child.BackgroundColor = GetButtonColor(map.TriggerButton);
                    child.Opacity = 0.85;
                }
            }
        }

        if (view is Label lbl)
        {
            lbl.Opacity = 1.0;
            lbl.BackgroundColor = GetButtonColor(mapping.TriggerButton);
        }

        ConfigPanel.IsVisible = true;

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
            if (_profile != null)
            {
                var map = _profile.Mappings.FirstOrDefault(m => m.TriggerButton.ToString() == child.Text);
                if (map != null)
                {
                    child.BackgroundColor = GetButtonColor(map.TriggerButton);
                    child.Opacity = 0.85;
                }
            }
        }

        ConfigPanel.IsVisible = false;
        SelectedButtonLabel.Text = "-";
    }

    private void OnBackgroundTapped(object? sender, TappedEventArgs e)
    {
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

        _selectedMapping.Size = e.NewValue;
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
                    var startBounds = AbsoluteLayout.GetLayoutBounds(view);

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

                    var centerX = bounds.X + buttonWidth / 2.0;
                    var centerY = bounds.Y + buttonHeight / 2.0;

                    mapping.TargetX = centerX / width;
                    mapping.TargetY = centerY / height;
                    break;
                }
        }
    }

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
