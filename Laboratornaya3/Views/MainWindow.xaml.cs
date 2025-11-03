using Laboratornaya3.ViewModels;
using System.Windows;

namespace Laboratornaya3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(); // VM со статичной картой
        }
    }
}