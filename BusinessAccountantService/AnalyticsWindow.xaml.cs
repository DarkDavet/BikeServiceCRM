using BusinessAccountantService.Managers;
using LiveCharts;
using LiveCharts.Wpf;
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
using System.Windows.Shapes;

namespace BusinessAccountantService
{
    /// <summary>
    /// Interaction logic for AnalyticsWindow.xaml
    /// </summary>
    public partial class AnalyticsWindow : Window
    {
        private RepairManager _repairManager => ((MainWindow)Application.Current.MainWindow)._repairManager;
        public AnalyticsWindow()
        {
            InitializeComponent();
            MonthPicker.SelectedDate = DateTime.Now;
            LoadData(DateTime.Now);
        }
        private void LoadData(DateTime date)
        {
            var stats = _repairManager.GetStatsByMonth(date);
            RevText.Text = $"{stats.rev:N0} ₽";
            PartsText.Text = $"{stats.parts:N0} ₽";
            ProfitText.Text = $"{stats.prof:N0} ₽";
            CountText.Text = stats.count.ToString();

            var dailyData = _repairManager.GetDailyRevenue(date);

            FinanceChart.Series = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Выручка",
            Values = new ChartValues<double>(dailyData.Select(x => x.dailyRev)),
            PointGeometry = DefaultGeometries.Circle,
            Stroke = Brushes.DodgerBlue,
            Fill = Brushes.Transparent 
        }
    };

            FinanceChart.AxisX.Clear();
            FinanceChart.AxisX.Add(new Axis
            {
                Title = "Дни месяца",
                Labels = dailyData.Select(x => x.day).ToArray()
            });
        }

        private void MonthPicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            var datePicker = sender as DatePicker;
            if (datePicker == null) return;

            var popup = datePicker.Template.FindName("PART_Popup", datePicker) as System.Windows.Controls.Primitives.Popup;
            if (popup != null && popup.Child is System.Windows.Controls.Calendar calendar)
            {
                calendar.DisplayMode = CalendarMode.Year;

                calendar.DisplayModeChanged += (s, args) =>
                {
                    if (calendar.DisplayMode == CalendarMode.Month)
                    {
                        datePicker.SelectedDate = calendar.DisplayDate;
                        datePicker.IsDropDownOpen = false;
                    }
                };
            }
        }

        private void MonthPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MonthPicker.SelectedDate is DateTime date)
            {
                LoadData(date);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var textBox = MonthPicker.Template.FindName("PART_TextBox", MonthPicker) as TextBox;
                    if (textBox != null)
                    {
                        textBox.Text = date.ToString("MMMM yyyy");
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}
