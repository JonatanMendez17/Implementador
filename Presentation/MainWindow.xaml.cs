using System.Windows;
using Microsoft.Extensions.Logging;
using Implementador.ViewModels;

namespace Implementador.Presentation
{
    public partial class MainWindow : Window
    {
        public MainWindow() : this(new MainViewModel(App.LoggerFactory.CreateLogger("Implementador")))
        {
        }

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Closed += (_, _) => viewModel.Dispose();
        }
    }
}

