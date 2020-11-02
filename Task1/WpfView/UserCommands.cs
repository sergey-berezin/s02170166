using System.Windows.Input;

namespace WpfView
{
    public class UserCommands
    {
        public static RoutedCommand LaunchProcessingCommand { get; set; }
        static UserCommands()
        {
            LaunchProcessingCommand = new RoutedCommand("LaunchProcessingCommand", typeof(MainWindow));
        }
    }
}