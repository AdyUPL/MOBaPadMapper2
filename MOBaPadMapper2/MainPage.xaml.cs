using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace MOBaPadMapper2
{
    public partial class MainPage : ContentPage
    {
        private readonly IGamepadInputService _gamepad;
        private readonly MobaInputMapper _mapper;

        private List<GameProfile> _profiles = new();
        private GameProfile? _activeProfile;

        public MainPage(IGamepadInputService gamepad, MobaInputMapper mapper)
        {
            InitializeComponent();

            _gamepad = gamepad;
            _mapper = mapper;

            // 1️⃣ Wczytanie profili
            _profiles = ProfilesRepository.LoadProfiles();

            ProfilesPicker.Items.Clear();
            foreach (var p in _profiles)
            {
                ProfilesPicker.Items.Add(p.Name);
            }

            // 2️⃣ Ustaw domyślny profil
            if (_profiles.Count > 0)
            {
                _activeProfile = _profiles[0];
                ProfilesPicker.SelectedIndex = 0;
                ActiveProfileLabel.Text = _profiles[0].Name;
                _mapper.SetProfile(_profiles[0]);
            }
            else
            {
                ActiveProfileLabel.Text = "Brak profili";
            }
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
            // Info diagnostyczne na ekranie
            MainThread.BeginInvokeOnMainThread(() =>
            {
                GamepadStatusLabel.Text =
                    $"Przycisk: {e.Button}  Stan: {(e.IsPressed ? "Down" : "Up")}";
            });

            // Mapowanie przycisku na dotyk według aktywnego profilu
            _ = _mapper.HandleButtonChangedAsync(e);
        }

        private async void OnTestButtonClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "Testowy przycisk działa.", "OK");
        }

        private void OnProfileChanged(object sender, EventArgs e)
        {
            if (ProfilesPicker.SelectedIndex < 0 || ProfilesPicker.SelectedIndex >= _profiles.Count)
                return;

            var profile = _profiles[ProfilesPicker.SelectedIndex];

            _activeProfile = profile;
            ActiveProfileLabel.Text = profile.Name;
            _mapper.SetProfile(profile);
        }

        // 🔹 Wejście do ekranu konfiguracji
        private async void ConfigureButton_Clicked(object sender, EventArgs e)
        {
            if (_activeProfile == null)
                return;

            // Przejście do strony konfiguracji
            await Navigation.PushAsync(new ConfigPage(_activeProfile, _mapper));
        }
    }
}
