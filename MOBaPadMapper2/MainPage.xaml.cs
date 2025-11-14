using Microsoft.Maui.Devices;
using System.Collections.Generic;

namespace MOBaPadMapper2
{
    public partial class MainPage : ContentPage
    {
        private readonly IGamepadInputService _gamepad;
        private readonly MobaInputMapper _mapper;

        private List<GameProfile> _profiles = new();
        private GameProfile? _currentProfile;

        public MainPage(IGamepadInputService gamepad, MobaInputMapper mapper)
        {
            InitializeComponent();

            _gamepad = gamepad;
            _mapper = mapper;

            _profiles = ProfilesRepository.LoadProfiles();

            ProfilePicker.ItemsSource = _profiles;
            ProfilePicker.ItemDisplayBinding = new Binding(nameof(GameProfile.Name));

            if (_profiles.Count > 0)
            {
                ProfilePicker.SelectedIndex = 0;
                SetCurrentProfile(_profiles[0]);
            }

            _gamepad.GamepadUpdated += OnGamepadUpdated;
        }

        private void SetCurrentProfile(GameProfile profile)
        {
            _currentProfile = profile;
            ActiveProfileLabel.Text = profile.Name;
            _mapper.UpdateMappings(profile.Mappings);
        }

        private void ProfilePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProfilePicker.SelectedItem is GameProfile profile)
            {
                SetCurrentProfile(profile);
            }
        }

        private async void ConfigureButton_Clicked(object sender, EventArgs e)
        {
            if (_currentProfile == null)
                return;

            await Shell.Current.GoToAsync(nameof(TestPage),
                new Dictionary<string, object>
                {
                    ["profile"] = _currentProfile
                });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _gamepad.GamepadUpdated -= OnGamepadUpdated;
        }

        private async void OnGamepadUpdated(object? sender, GamepadState state)
        {
            double width = this.Width;
            double height = this.Height;

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
