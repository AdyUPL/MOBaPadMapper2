namespace MOBaPadMapper2
{
    using Microsoft.Maui.Devices;

    public partial class MainPage : ContentPage
    {
        private readonly IGamepadInputService _gamepad;
        private readonly MobaInputMapper _mapper;

        public MainPage(IGamepadInputService gamepad, MobaInputMapper mapper)
        {
            InitializeComponent();

            _gamepad = gamepad;
            _mapper = mapper;

            _gamepad.GamepadUpdated += OnGamepadUpdated;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _gamepad.GamepadUpdated -= OnGamepadUpdated;
        }

        private void GamePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Tu możesz później dodać logikę wyboru profilu gry.
            // Na razie zostaw puste, żeby tylko zniknął błąd kompilacji.
        }

        private async void OnGamepadUpdated(object? sender, GamepadState state)
        {
            // Rozmiar widoku – jeśli 0, bierzemy fizyczny ekran
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
