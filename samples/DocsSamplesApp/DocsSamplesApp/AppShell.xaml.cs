namespace DocsSamplesApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register all demo pages as routes for Shell navigation
            RouteRegistration.RegisterRoutes();
        }
    }
}
