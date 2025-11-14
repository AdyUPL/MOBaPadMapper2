namespace MOBaPadMapper2
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Rejestracja trasy do ekranu testowego
            Routing.RegisterRoute(nameof(TestPage), typeof(TestPage));
        }
    }
}
