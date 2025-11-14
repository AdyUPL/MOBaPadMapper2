using Microsoft.Maui.Controls;

namespace MOBaPadMapper2;

[QueryProperty(nameof(Profile), "profile")]
public partial class TestPage : ContentPage
{
    public GameProfile? Profile { get; set; }

    public TestPage()
    {
        Content = new Label
        {
            Text = "Ekran konfiguracji przycisków będzie tutaj 🙂",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
    }
}
