using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

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

            // Ustaw tekst początkowy profilu
            ActiveProfileLabel.Text = "Domyślny profil";

            // Tymczasowe profile – później podłączymy ProfilesRepository
            ProfilesPicker.Items.Add("Domyślny profil");
            ProfilesPicker.Items.Add("Profil 2");
            ProfilesPicker.SelectedIndex = 0;
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
            // Informacja diagnostyczna na ekranie
            MainThread.BeginInvokeOnMainThread(() =>
            {
                GamepadStatusLabel.Text =
                    $"Przycisk: {e.Button}  Stan: {(e.IsPressed ? "Down" : "Up")}";
            });

            // Faktyczne mapowanie przycisku na dotyk
            _ = _mapper.HandleButtonChangedAsync(e);
        }

        private async void OnTestButtonClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "Testowy przycisk działa.", "OK");
        }

        // 🔹 HANDLER dla SelectedIndexChanged w Pickerze
        private void OnProfileChanged(object sender, EventArgs e)
        {
            if (ProfilesPicker.SelectedIndex < 0)
                return;

            var selectedName = ProfilesPicker.Items[ProfilesPicker.SelectedIndex];
            ActiveProfileLabel.Text = selectedName;

            // Tu później podłączymy konkretne profile:
            // np. wczytanie z ProfilesRepository + _mapper.SetProfile(profile);
        }
    }
}
