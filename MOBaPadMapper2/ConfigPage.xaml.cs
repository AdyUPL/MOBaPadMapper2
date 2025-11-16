using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace MOBaPadMapper2;

public partial class ConfigPage : ContentPage
{
    private readonly GameProfile _profile;
    private readonly MobaInputMapper _mapper;
    private readonly IGamepadInputService _gamepad;

    public ConfigPage(GameProfile profile, MobaInputMapper mapper, IGamepadInputService gamepad)
    {
        InitializeComponent();

        _profile = profile;
        _mapper = mapper;
        _gamepad = gamepad;

        ProfileNameLabel.Text = $"Konfiguracja: {_profile.Name}";
        DebugLabel.Text = $"Mappings: {_profile.Mappings.Count}";

        // Podpinamy listę
        MappingsList.ItemsSource = _profile.Mappings;
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

    // Podświetlanie kafelka dla wciśniętego przycisku pada
    private void OnGamepadButtonChanged(object? sender, GamepadButtonEventArgs e)
    {
        if (!e.IsPressed)
            return;

        var mapping = _profile.Mappings.FirstOrDefault(m => m.Matches(e));
        if (mapping == null)
            return;

        // Na razie tylko info tekstowe – bez szukania konkretnych kontrolek
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DebugLabel.Text = $"Mappings: {_profile.Mappings.Count} | Last: {mapping.TriggerButton} ({e.Button})";
        });
    }

}
