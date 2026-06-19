namespace VadkanRobotClient
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // register settingspage route
            Routing.RegisterRoute(nameof(RobotSettingsPage), typeof(RobotSettingsPage));
        }
    }
}