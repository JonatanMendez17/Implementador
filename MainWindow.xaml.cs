using System.Windows;
using Microsoft.Extensions.Logging;
using ImplementadorCUAD.ViewModels;

namespace ImplementadorCUAD
{
    public partial class MainWindow : Window
    {
        public MainWindow() : this(new MainViewModel(App.LoggerFactory.CreateLogger<MainViewModel>()))
        {
        }

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
