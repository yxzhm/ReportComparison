using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace ReportComparison
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var vm = this.DataContext as ViewModel;
            if (vm.Model.SelectedProfile.CompareStrategyAppendColumnNames.Contains(e.Column.Header.ToString()))
            {
                Style newCellStyle = new Style(typeof(DataGridCell), e.Column.CellStyle);
                newCellStyle.Setters.Add(new Setter(DataGridCell.ForegroundProperty, new Binding(e.Column.Header.ToString()) { Converter = new RedValues() }));
                e.Column.CellStyle = newCellStyle;

            }
        }
    }


}
