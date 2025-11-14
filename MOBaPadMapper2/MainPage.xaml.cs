using Microsoft.Maui;
using System.Collections.ObjectModel;

namespace MOBaPadMapper2;

public partial class MainPage : ContentPage
{
    private readonly ObservableCollection<GameProfile> _games = new();
    private GameProfile? _selectedGame;

    public MainPage()
    {
        InitializeComponent();

        // Przykładowa gra startowa z 2 mapowaniami – możesz zostawić albo usunąć
        var defaultProfile = new GameProfile("Domyślna gra");
        defaultProfile.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.A,
            ActionType = ActionType.Tap,
            TargetX = 0.5,
            TargetY = 0.8,
            Size = 60
        });
        defaultProfile.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.RB,
            ActionType = ActionType.HoldAndAim,
            TargetX = 0.8,
            TargetY = 0.5,
            UseRightStickForDirection = true,
            Size = 60
        });

        _games.Add(defaultProfile);

        GamePicker.ItemsSource = _games;
        GamePicker.ItemDisplayBinding = new Binding(nameof(GameProfile.Name));
        GamePicker.SelectedItem = defaultProfile;

        UpdateSelectedGame();

        ControllerNameLabel.Text = "Kontroler: (do podpięcia z IGamepadInputService)";
    }

    private void UpdateSelectedGame()
    {
        _selectedGame = GamePicker.SelectedItem as GameProfile;
        ConfigButton.IsEnabled = _selectedGame != null;
    }

    private void GamePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateSelectedGame();
    }

    private async void OnAddGameClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Nowa gra", "Podaj nazwę gry:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var profile = new GameProfile(name);
        _games.Add(profile);
        GamePicker.SelectedItem = profile;
    }

    private async void OnOpenTestClicked(object sender, EventArgs e)
    {
        if (_selectedGame == null) return;

        await Shell.Current.GoToAsync(nameof(TestPage), true, new Dictionary<string, object>
        {
            ["profile"] = _selectedGame
        });
    }
}
