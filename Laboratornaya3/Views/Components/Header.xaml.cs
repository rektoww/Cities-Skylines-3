using Laboratornaya3.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Laboratornaya3.Views.Components
{
    /// <summary>
    /// Логика взаимодействия для Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        public Header()
        {
            InitializeComponent();
        }

        // исправить привязку
        private void ClearBuildings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("допилить");
        }

        // тоже исправить привязку
        private void BudgetPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.GameState.ShowFinanceInfoCommand.Execute(null);
        }
    }
}
