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

    private readonly Dictionary<View, Rect> _dragStartBounds = new();

    private readonly IGamepadInputService _gamepad;
    private readonly MobaInputMapper _mapper;

    // Kontrolki tworzone w kodzie
    private Label _gameNameLabel = null!;
    private Label _selectedButtonLabel = null!;
    private Picker _actionTypePicker = null!;
    private Slider _sizeSlider = null!;
    private AbsoluteLayout _testSurface = null!;
    private Grid _configPanel = null!;

    public GameProfile? Profile
    {
        get => _profile;
        set
        {
            _profile = value;
            if (_gameNameLabel != null)
                _gameNameLabel.Text = $"Gra/profil: {_profile?.Name ?? "-"}";
            RenderButtons();
        }
    }

    public TestPage(IGamepadInputService gamepad, MobaInputMapper mapper)
    {
        _gamepad = gamepad;
        _mapper = mapper;

        BuildLayout();
        SetupLogic();
    }

    private void BuildLayout()
    {
        BackgroundColor = Color.FromArgb("#111111");

        _testSurface = new AbsoluteLayout
        {
            BackgroundColor = Color.FromArgb("#202020"),
            Margin = new Thickness(10)
        };

        _configPanel = new Grid
        {
            BackgroundColor = Color.FromArgb("#181818"),
            Padding = new Thickness(10),
            IsVisible = false
        };

        _configPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _configPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _configPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _configPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _configPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _configPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        _gameNameLabel = new Label
        {
            Text = "Gra/profil: -",
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 18,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(_gameNameLabel, 0);
        _configPanel.Children.Add(_gameNameLabel);

        var selectedStack = new HorizontalStackLayout
        {
            Spacing = 8
        };
        var selectedText = new Label
        {
            Text = "Wybrany przycisk:",
            TextColor = Colors.White
        };
        _selectedButtonLabel = new Label
        {
            Text = "-",
            TextColor = Colors.Orange,
            FontAttributes = FontAttributes.Bold
        };
        selectedStack.Children.Add(selectedText);
        selectedStack.Children.Add(_selectedButtonLabel);
        Grid.SetRow(selectedStack, 1);
        _configPanel.Children.Add(selectedStack);

        var actionStack = new StackLayout
        {
            Margin = new Thickness(0, 10, 0, 0)
        };
        actionStack.Children.Add(new Label
        {
            Text = "Typ akcji",
            TextColor = Colors.White
        });
        _actionTypePicker = new Picker
        {
            Title = "Wybierz akcję"
        };
        actionStack.Children.Add(_actionTypePicker);
        Grid.SetRow(actionStack, 2);
        _configPanel.Children.Add(actionStack);

        var sizeStack = new StackLayout
        {
            Margin = new Thickness(0, 10, 0, 0)
        };
        sizeStack.Children.Add(new Label
        {
            Text = "Rozmiar przycisku",
            TextColor = Colors.White
        });
        _sizeSlider = new Slider
        {
            Minimum = 30,
            Maximum = 160,
            Value = 60
        };
        sizeStack.Children.Add(_sizeSlider);
        Grid.SetRow(sizeStack, 3);
        _configPanel.Children.Add(sizeStack);

        var infoStack = new StackLayout
        {
            Margin = new Thickness(0, 10, 0, 0),
            Spacing = 6
        };
        infoStack.Children.Add(new Label
        {
            Text = "Przeciągnij przyciski po lewej, aby zmienić ich pozycję.",
            TextColor = Colors.LightGray,
            FontSize = 12
        });
        infoStack.Children.Add(new Label
        {
            Text = "Kliknij w puste miejsce, aby odznaczyć.",
            TextColor = Colors.LightGray,
            FontSize = 12
        });
        Grid.SetRow(infoStack, 4);
        _configPanel.Children.Add(infoStack);

        var addButton = new Button
        {
            Text = "Dodaj mapowanie",
            Margin = new Thickness(0, 10, 0, 0)
        };
        addButton.Clicked += OnAddMappingClicked;
        Grid.SetRow(addButton, 5);
        _configPanel.Children.Add(addButton);

        var root = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }
            }
        };

        Grid.SetColumn(_testSurface, 0);
        root.Children.Add(_testSurface);

        Grid.SetColumn(_configPanel, 1);
        root.Children.Add(_configPanel);

        Content = root;
    }

    private void SetupLogic()
    {
        _actionTypePicker.ItemsSource = Enum
            .GetValues(typeof(ActionType))
            .Cast<ActionType>()
            .ToList();

        _actionTypePicker.SelectedIndexChanged += ActionTypePicker_SelectedIndexChanged;
        _sizeSlider.ValueChanged += SizeSlider_ValueChanged;

        _testSurface.SizeChanged += (s, e) => RenderButtons();

        var bgTap = new TapGestureRecognizer();
        bgTap.Tapped += OnBackgroundTapped;
        _testSurface.GestureRecognizers.Add(bgTap);
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

            var view = _testSurface.Children
                .OfType<Label>()
                .FirstOrDefault(l => string.Equals(l.Text, mapping.TriggerButton.ToString(), StringComparison.Ordinal));

            if (view != null)
            {
                SelectMapping(mapping, view);
            }
        });
    }

    private static Color GetButtonColor(GamepadButton button)
    {
        return button switch
        {
            GamepadButton.A => Color.FromArgb("#0DB45E"),
            GamepadButton.B => Color.FromArgb("#D83C3C"),
            GamepadButton.X => Color.FromArgb("#2563EB"),
            GamepadButton.Y => Color.FromArgb("#EAB308"),
            _ => Colors.DimGray
        };
    }

    private void RenderButtons()
    {
        if (_testSurface == null || _profile == null)
            return;

        if (_testSurface.Width <= 0 || _testSurface.Height <= 0)
            return;

        var currentSelected = _selectedMapping;

        _testSurface.Children.Clear();
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

            var x = mapping.TargetX * _testSurface.Width - size / 2.0;
            var y = mapping.TargetY * _testSurface.Height - size / 2.0;

            x = Math.Clamp(x, 0, _testSurface.Width - size);
            y = Math.Clamp(y, 0, _testSurface.Height - size);

            var rect = new Rect(x, y, size, size);
            AbsoluteLayout.SetLayoutBounds(label, rect);
            AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.None);

            _testSurface.Children.Add(label);

            if (currentSelected == mapping)
            {
                SelectMapping(mapping, label, fromRender: true);
            }
        }

        if (_selectedMapping == null)
        {
            _configPanel.IsVisible = false;
        }
    }

    private void SelectMapping(ActionMapping mapping, View view, bool fromRender = false)
    {
        _selectedMapping = mapping;
        _selectedView = view;

        foreach (var child in _testSurface.Children.OfType<Label>())
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

        _configPanel.IsVisible = true;

        _isUpdatingUi = true;
        _selectedButtonLabel.Text = mapping.TriggerButton.ToString();
        _actionTypePicker.SelectedItem = mapping.ActionType;
        _sizeSlider.Value = mapping.Size <= 0 ? 60 : mapping.Size;
        _isUpdatingUi = false;
    }

    private void ClearSelection()
    {
        _selectedMapping = null;
        _selectedView = null;

        foreach (var child in _testSurface.Children.OfType<Label>())
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

        _configPanel.IsVisible = false;
        _selectedButtonLabel.Text = "-";
    }

    private void OnBackgroundTapped(object? sender, TappedEventArgs e)
    {
        ClearSelection();
    }

    private void ActionTypePicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_selectedMapping == null || _isUpdatingUi)
            return;

        if (_actionTypePicker.SelectedItem is ActionType at)
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
        if (_testSurface == null)
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

                    var width = _testSurface.Width;
                    var height = _testSurface.Height;

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

                    var width = _testSurface.Width;
                    var height = _testSurface.Height;

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
