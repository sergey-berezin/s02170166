using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace WpfView
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new ViewModel();

            this.DataContext = viewModel;

            ClassesOfImagesListBox.SelectionChanged += RefreshGridContext;

            CommandBinding commandBinding = new CommandBinding
            {
                Command = UserCommands.LaunchProcessingCommand
            };
            commandBinding.CanExecute += this.CanLaunchProcessing;
            commandBinding.Executed += this.LaunchProcessing;
            this.CommandBindings.Add(commandBinding);

            commandBinding = new CommandBinding
            {
                Command = ApplicationCommands.Open
            };
            commandBinding.Executed += this.ChooseDirectory;
            this.CommandBindings.Add(commandBinding);

            commandBinding = new CommandBinding
            {
                Command = ApplicationCommands.Stop
            };
            commandBinding.CanExecute += this.CanInterruptProcessing;
            commandBinding.Executed += this.InterruptProcessing;
            this.CommandBindings.Add(commandBinding);
        }

        private void LaunchProcessing(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.Start();
        }

        private void CanLaunchProcessing(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !viewModel.IsProcessingNow;
        }

        private void ChooseDirectory(object sender, ExecutedRoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if ((bool)dialog.ShowDialog())
                viewModel.CurrentChosenDirectory = dialog.SelectedPath;
        }

        private void InterruptProcessing(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.Stop();
        }

        private void CanInterruptProcessing(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = viewModel.IsProcessingNow;
        }

        public void RefreshGridContext(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                CurrentChosenClassGrid.DataContext = e.AddedItems[0];
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (viewModel.IsProcessingNow)
            {
                viewModel.Stop();
            }
        }
    }
}
