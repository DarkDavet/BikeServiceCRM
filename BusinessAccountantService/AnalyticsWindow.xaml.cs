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
        public enum ChartDataType { Revenue, Parts, Profit, Orders }
        private ChartDataType _currentChartMode = ChartDataType.Revenue;

        private bool _showRevenue = true;  
        private bool _showParts = false;
        private bool _showProfit = false;
        private bool _showCount = false;
        public AnalyticsWindow()
        {
            InitializeComponent();
            MonthPicker.SelectedDate = DateTime.Now;

            UpdateCardVisuals(CardRev, _showRevenue, "#2980B9");
            UpdateCardVisuals(CardParts, _showParts, "#C0392B");
            UpdateCardVisuals(CardProfit, _showProfit, "#27AE60");
            UpdateCardVisuals(CardOrders, _showCount, "#8E44AD");

            LoadData(DateTime.Now);
            LoadYearlyData();
        }
        private void LoadData(DateTime date)
        {
            var dailyData = _repairManager.GetDailyStats(date);
            var series = new SeriesCollection();

            if (_showCount)
            {
                series.Add(new LineSeries
                {
                    Title = "Заказы",
                    Values = new ChartValues<double>(dailyData.Select(x => (double)x.dailyCount)),
                    Stroke = Brushes.BlueViolet,
                    PointGeometry = DefaultGeometries.Square,
                    DataLabels = true 
                });
            }
            else
            {
                if (_showRevenue) series.Add(new LineSeries { Title = "Выручка", Values = new ChartValues<double>(dailyData.Select(x => x.dailyRev)), Stroke = Brushes.DodgerBlue, Fill = Brushes.Transparent });
                if (_showParts) series.Add(new LineSeries { Title = "Запчасти", Values = new ChartValues<double>(dailyData.Select(x => x.dailyParts)), Stroke = Brushes.Tomato, Fill = Brushes.Transparent });
                if (_showProfit) series.Add(new LineSeries { Title = "Прибыль", Values = new ChartValues<double>(dailyData.Select(x => x.dailyRev - x.dailyParts)), Stroke = Brushes.MediumSeaGreen, Fill = Brushes.Transparent });
            }

            FinanceChart.Series = series;

            FinanceChart.AxisY.Clear();

            if (_showCount)
            {
                FinanceChart.AxisY.Add(new Axis
                {
                    Title = "Количество (шт.)",
                    LabelFormatter = value => value.ToString("N0") + " шт.",
                    Separator = new LiveCharts.Wpf.Separator { Step = 1 } 
                });
            }
            else
            {
                FinanceChart.AxisY.Add(new Axis
                {
                    Title = "Сумма (₽)",
                    LabelFormatter = value => value.ToString("N0") + " ₽"
                });
            }

            FinanceChart.AxisX.Clear();
            FinanceChart.AxisX.Add(new Axis
            {
                Labels = dailyData.Select(x => x.day).ToArray()
            });
        }


        private void LoadYearlyData()
        {
            var yearlyData = _repairManager.GetYearlyStats();

            string[] monthNames = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

            YearlyChart.Series = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Чистая прибыль",
            Values = new ChartValues<double>(yearlyData.Select(x => x.profit)),
            Fill = System.Windows.Media.Brushes.MediumSeaGreen
        }
    };

            YearlyChart.AxisX.Clear();
            YearlyChart.AxisX.Add(new Axis
            {
                Labels = yearlyData.Select(x => monthNames[int.Parse(x.month) - 1]).ToArray()
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

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var card = sender as Border;
            if (card == null || card.Tag == null) return;

            string type = card.Tag.ToString();

            if (type == "Orders")
            {
                _showCount = !_showCount;
                if (_showCount) { _showRevenue = _showParts = _showProfit = false; }
            }
            else
            {
                _showCount = false;

                if (type == "Revenue") _showRevenue = !_showRevenue;
                if (type == "Parts") _showParts = !_showParts;
                if (type == "Profit") _showProfit = !_showProfit;
            }

            UpdateCardVisuals(CardRev, _showRevenue, "#2980B9");
            UpdateCardVisuals(CardParts, _showParts, "#C0392B");
            UpdateCardVisuals(CardProfit, _showProfit, "#27AE60");
            UpdateCardVisuals(CardOrders, _showCount, "#8E44AD"); 

            if (MonthPicker.SelectedDate is DateTime date) LoadData(date);
        }

        private void UpdateCardVisuals(Border card, bool isActive, string colorHex)
        {
            if (card == null) return;
            var color = (Color)ColorConverter.ConvertFromString(colorHex);

            if (isActive)
            {
                card.BorderBrush = new SolidColorBrush(color);
                card.BorderThickness = new Thickness(2);
                card.Opacity = 1.0;
            }
            else
            {
                card.BorderBrush = Brushes.Transparent;
                card.BorderThickness = new Thickness(2); 
                card.Opacity = 0.5;
            }
        }


    }
}
