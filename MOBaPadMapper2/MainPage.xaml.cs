using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Linq;
using MOBaPadMapper2;

namespace MOBaPadMapper2;

public partial class MainPage : ContentPage
{
    private readonly ITouchInjector _touchInjector;
    private readonly MobaInputMapper _mapper;
    private readonly IGamepadInputService _gamepadInput;

    private ActionMapping? _aMapping;
    private ActionMapping? _rbMapping;

    public MainPage(ITouchInjector touchInjector, MobaInputMapper mapper, IGamepadInputService gamepadInput)
    {
        InitializeComponent();

        _touchInjector = touchInjector;
        _mapper = mapper;
        _gamepadInput = gamepadInput;

        Title = "MobaPad Mapper";

        // Subskrypcja stanu gamepada
        _gamepadInput.GamepadUpdated += OnGamepadUpdated;

        // Wczytaj mapowania do UI
        InitConfigFromMappings();
    }

    // ======================
    //  STATUS GAMEPADA + MAPOWANIE
    // ======================
    private async void OnGamepadUpdated(object? sender, GamepadState state)
    {
        // Aktualizacja tekstów na UI
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ButtonsStateLabel.Text = "Przyciski: " +
                                     (state.PressedButtons.Any()
                                         ? string.Join(", ", state.PressedButtons)
                                         : "(brak)");

            LeftStickLabel.Text = $"Left Stick: X={state.LeftStick.X:0.00}  Y={state.LeftStick.Y:0.00}";
            RightStickLabel.Text = $"Right Stick: X={state.RightStick.X:0.00}  Y={state.RightStick.Y:0.00}";
        });

        // Wyliczenie dotyku z mapera
        var displayInfo = DeviceDisplay.MainDisplayInfo;
        var width = displayInfo.Width / displayInfo.Density;
        var height = displayInfo.Height / displayInfo.Density;

        await _mapper.OnGamepadStateChanged(state, width, height);
    }

    // ======================
    //  TEST DOTYKU
    // ======================
    private async void OnTestTapClicked(object sender, EventArgs e)
    {
        var displayInfo = DeviceDisplay.MainDisplayInfo;
        var width = displayInfo.Width / displayInfo.Density;
        var height = displayInfo.Height / displayInfo.Density;

        await _touchInjector.TapAsync(width / 2, height / 2);
    }

    private async void OnTestSwipeClicked(object sender, EventArgs e)
    {
        var displayInfo = DeviceDisplay.MainDisplayInfo;
        var width = displayInfo.Width / displayInfo.Density;
        var height = displayInfo.Height / displayInfo.Density;

        await _touchInjector.SwipeAsync(width / 2, height / 2, width / 2 + 200, height / 2,
            TimeSpan.FromMilliseconds(150));
    }

    // ======================
    //  KONFIGURACJA MAPOWAŃ
    // ======================

    private void InitConfigFromMappings()
    {
        // Wymagane: w MobaInputMapper musi być public IList<ActionMapping> Mappings { get; }
        _aMapping = _mapper.Mappings.FirstOrDefault(m => m.TriggerButton == GamepadButton.A);
        _rbMapping = _mapper.Mappings.FirstOrDefault(m => m.TriggerButton == GamepadButton.RB);

        // A
        if (_aMapping != null)
        {
            ASliderX.Value = _aMapping.TargetX * 100;
            ASliderY.Value = _aMapping.TargetY * 100;
        }

        // RB
        if (_rbMapping != null)
        {
            RBSliderX.Value = _rbMapping.TargetX * 100;
            RBSliderY.Value = _rbMapping.TargetY * 100;
            RBUseRightStickSwitch.IsToggled = _rbMapping.UseRightStickForDirection;
        }

        UpdateAConfigLabel();
        UpdateRBConfigLabel();
    }

    // --- A: Tap ---

    private void OnAConfigSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        UpdateAConfigLabel();
    }

    private void UpdateAConfigLabel()
    {
        AConfigLabel.Text = $"Pozycja: X={ASliderX.Value:0}%  Y={ASliderY.Value:0}%";
    }

    private void OnApplyAConfigClicked(object sender, EventArgs e)
    {
        if (_aMapping == null) return;

        _aMapping.TargetX = ASliderX.Value / 100.0;
        _aMapping.TargetY = ASliderY.Value / 100.0;

        DisplayAlert("MobaPad", "Zastosowano konfigurację dla przycisku A.", "OK");
    }

    private void OnResetAConfigClicked(object sender, EventArgs e)
    {
        if (_aMapping == null) return;

        _aMapping.TargetX = 0.5;
        _aMapping.TargetY = 0.5;

        ASliderX.Value = 50;
        ASliderY.Value = 50;
        UpdateAConfigLabel();

        DisplayAlert("MobaPad", "Przywrócono domyślne ustawienia A.", "OK");
    }

    // --- RB: Hold + Aim ---

    private void OnRBConfigSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        UpdateRBConfigLabel();
    }

    private void UpdateRBConfigLabel()
    {
        RBConfigLabel.Text = $"Pozycja: X={RBSliderX.Value:0}%  Y={RBSliderY.Value:0}%";
    }

    private void OnApplyRBConfigClicked(object sender, EventArgs e)
    {
        if (_rbMapping == null) return;

        _rbMapping.TargetX = RBSliderX.Value / 100.0;
        _rbMapping.TargetY = RBSliderY.Value / 100.0;
        _rbMapping.UseRightStickForDirection = RBUseRightStickSwitch.IsToggled;

        DisplayAlert("MobaPad", "Zastosowano konfigurację dla przycisku RB.", "OK");
    }

    private void OnResetRBConfigClicked(object sender, EventArgs e)
    {
        if (_rbMapping == null) return;

        _rbMapping.TargetX = 0.8;
        _rbMapping.TargetY = 0.8;
        _rbMapping.UseRightStickForDirection = true;

        RBSliderX.Value = 80;
        RBSliderY.Value = 80;
        RBUseRightStickSwitch.IsToggled = true;
        UpdateRBConfigLabel();

        DisplayAlert("MobaPad", "Przywrócono domyślne ustawienia RB.", "OK");
    }
}
