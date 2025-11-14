using Microsoft.Maui.Devices;
using System;

namespace MOBaPadMapper2
{
    public partial class MainPage : ContentPage
    {
        private readonly IGamepadInputService _gamepad;
        private readonly MobaInputMapper _mapper;

        public MainPage(IGamepadInputService gamepad, MobaInputMapper mapper)
        {
            InitializeComponent();

            _gamepad = gamepad;
            _mapper = mapper;

            // Na start – jeden domyślny profil
            ActiveProfileLabel.Text = "Domyślny profil";

            _gamepad.GamepadUpdated += OnGamepadUpdated;
        }

        private async void ConfigureButton_Clicked(object sender, EventArgs e)
        {
            // TODO: tutaj później nawigacja do ekranu konfiguracji (TestPage)
            await DisplayAlert("Konfiguracja",
                "Ekran konfiguracji przycisków dodamy w kolejnym kroku.",
                "OK");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _gamepad.GamepadUpdated -= OnGamepadUpdated;
        }

        private async void OnGamepadUpdated(object? sender, GamepadState state)
        {
            double width = Width;
            double height = Height;

            if (width <= 0 || height <= 0)
            {
                var info = DeviceDisplay.Current.MainDisplayInfo;
                width = info.Width / info.Density;
                height = info.Height / info.Density;
            }

            await _mapper.OnGamepadStateChanged(state, width, height);
        }
    }
}
