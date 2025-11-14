using System;
using Microsoft.Maui.Dispatching;

namespace MOBaPadMapper2;

public partial class MainPage : ContentPage
{
    private readonly IGamepadInputService _gamepad;
    private readonly MobaInputMapper _mapper;

    public MainPage(IGamepadInputService gamepad, MobaInputMapper mapper)
    {
        InitializeComponent();

        _gamepad = gamepad;
        _mapper = mapper;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _gamepad.ButtonChanged += OnGamepadButtonChanged;
    }

    protected override void OnDisappearing()
    {
        _gamepad.ButtonChanged -= OnGamepadButtonChanged;
        base.OnDisappearing();
    }

    private void OnGamepadButtonChanged(object? sender, GamepadButtonEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            GamepadStatusLabel.Text =
                $"Przycisk: {e.Button}  Stan: {(e.IsPressed ? "Down" : "Up")}";
        });
    }

    private void OnTestButtonClicked(object sender, EventArgs e)
    {
        DisplayAlert("Info", "Testowy przycisk działa.", "OK");
    }
}
