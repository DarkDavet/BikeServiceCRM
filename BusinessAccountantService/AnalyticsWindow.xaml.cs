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
using System.Windows.Threading;
using System.Globalization;

namespace BusinessAccountantService
{
    /// <summary>
    /// Interaction logic for AnalyticsWindow.xaml
    /// </summary>
    public partial class AnalyticsWindow : Window
    {
        private StatsManager _statsManager => ((MainWindow)Application.Current.MainWindow)._statsManager;
        private ExpensesManager _expensesManager => ((MainWindow)Application.Current.MainWindow)._expensesManager;
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
        }
        private void LoadData(DateTime date)
        {
            var dailyData = _statsManager.GetDailyStats(date);

            decimal totalRev = dailyData.Sum(x => x.dailyRev);
            decimal totalParts = dailyData.Sum(x => x.dailyParts);
            decimal totalProfit = totalRev - totalParts;
            int totalCount = dailyData.Sum(x => x.dailyCount);

            RevText.Text = $"{totalRev:N0} ₽";
            PartsText.Text = $"{totalParts:N0} ₽";
            ProfitText.Text = $"{totalProfit:N0} ₽";
            CountText.Text = totalCount.ToString();

            MonthLabel.Text = date.ToString("MMMM yyyy", new CultureInfo("ru-RU")).ToUpper();

            var series = new SeriesCollection();

            if (_showCount)
            {
                series.Add(new LineSeries
                {
                    Title = "Заказы",
                    Values = new ChartValues<decimal>(dailyData.Select(x => (decimal)x.dailyCount)),
                    Stroke = Brushes.BlueViolet,
                    PointGeometry = DefaultGeometries.Square,
                    DataLabels = true 
                });
            }
            else
            {
                if (_showRevenue) series.Add(new LineSeries { Title = "Выручка", Values = new ChartValues<decimal>(dailyData.Select(x => x.dailyRev)), Stroke = Brushes.DodgerBlue, Fill = Brushes.Transparent });
                if (_showParts) series.Add(new LineSeries { Title = "Запчасти", Values = new ChartValues<decimal>(dailyData.Select(x => x.dailyParts)), Stroke = Brushes.Tomato, Fill = Brushes.Transparent });
                if (_showProfit) series.Add(new LineSeries { Title = "Прибыль", Values = new ChartValues<decimal>(dailyData.Select(x => x.dailyRev - x.dailyParts)), Stroke = Brushes.MediumSeaGreen, Fill = Brushes.Transparent });
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
                    LabelFormatter = value => value.ToString("N2") + " ₽"
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
            DateTime selectedDate = MonthPicker.SelectedDate ?? DateTime.Now;
            int year = selectedDate.Year;

            // Обновляем надпись над графиком года
            YearLabel.Text = $"{year} ГОД";
            var yearlyData = _statsManager.GetYearlyStats(year);

            var series = new SeriesCollection();
            string[] monthNames = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

            if (_showCount)
            {
                series.Add(new ColumnSeries
                {
                    Title = "Заказы",
                    Values = new ChartValues<double>(yearlyData.Select(x => (double)x.count)),
                    Fill = Brushes.BlueViolet,
                    DataLabels = true
                });
            }
            else
            {
                if (_showRevenue) series.Add(new ColumnSeries { Title = "Выручка", Values = new ChartValues<decimal>(yearlyData.Select(x => x.rev)), Fill = Brushes.DodgerBlue });
                if (_showParts) series.Add(new ColumnSeries { Title = "Запчасти", Values = new ChartValues<decimal>(yearlyData.Select(x => x.parts)), Fill = Brushes.Tomato });
                if (_showProfit) series.Add(new ColumnSeries { Title = "Прибыль", Values = new ChartValues<decimal>(yearlyData.Select(x => x.prof)), Fill = Brushes.MediumSeaGreen });
            }

            YearlyChart.Series = series;

            YearlyChart.AxisY.Clear();
            if (_showCount)
            {
                YearlyChart.AxisY.Add(new Axis
                {
                    Title = "Заказы (шт.)",
                    LabelFormatter = value => value.ToString("N0") + " шт.",
                    Separator = new LiveCharts.Wpf.Separator { Step = 1 }
                });
            }
            else
            {
                YearlyChart.AxisY.Add(new Axis
                {
                    Title = "Сумма (₽)",
                    LabelFormatter = value => value.ToString("N2") + " ₽"
                });
            }

            YearlyChart.AxisX.Clear();
            YearlyChart.AxisX.Add(new Axis
            {
                Labels = yearlyData.Select(x => monthNames[int.Parse(x.month) - 1]).ToArray(),
                Separator = new LiveCharts.Wpf.Separator { Step = 1 }
            });

            var yearlyExpenses = _expensesManager.GetExpensesByYear(year);
            var pieSeries = new SeriesCollection();

            foreach (var stat in yearlyExpenses)
            {
                pieSeries.Add(new PieSeries
                {
                    Title = stat.Category,
                    Values = new ChartValues<decimal> { stat.Amount },
                    DataLabels = true,
                    LabelPoint = p => $"{p.Y:N2} ₽"
                });
            }
            YearlyExpensesPieChart.Series = pieSeries;
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
                RefreshAll();

                Dispatcher.BeginInvoke(new Action(() => {
                    var textBox = MonthPicker.Template.FindName("PART_TextBox", MonthPicker) as TextBox;
                    if (textBox != null) textBox.Text = date.ToString("MMMM yyyy", new CultureInfo("ru-RU"));
                }), DispatcherPriority.Background);
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
            RefreshGraphs(); 
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource is TabControl)
            {
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            if (MainTabControl == null || MonthPicker?.SelectedDate == null) return;
            DateTime date = MonthPicker.SelectedDate.Value;

            // 1. Обновляем всё легкое (рамки карточек и линейные графики)
            RefreshGraphs();

            // 2. Обновляем тяжелые диаграммы (сработают только при смене даты/таба)
            LoadExpensesStructure(date);
            LoadYearlyExpensesPie(date.Year);

            // 3. Обновляем цифры в карточках (зависит от того, месяц это или год)
            if (MainTabControl.SelectedIndex == 0) // Вкладка месяца
            {
                var dailyData = _statsManager.GetDailyStats(date);
                UpdateCardTexts(
                    dailyData.Sum(x => x.dailyRev),
                    dailyData.Sum(x => x.dailyParts),
                    dailyData.Sum(x => x.dailyRev - x.dailyParts),
                    dailyData.Sum(x => x.dailyCount)
                );
            }
            else // Вкладка года
            {
                var yearlyData = _statsManager.GetYearlyStats(date.Year);
                UpdateCardTexts(
                    yearlyData.Sum(x => x.rev),
                    yearlyData.Sum(x => x.parts),
                    yearlyData.Sum(x => x.prof),
                    yearlyData.Sum(x => x.count)
                );
            }
        }


        private void RefreshGraphs()
        {
            if (MonthPicker?.SelectedDate == null) return;
            DateTime date = MonthPicker.SelectedDate.Value;
 
            UpdateCardVisuals(CardRev, _showRevenue, "#2980B9");
            UpdateCardVisuals(CardParts, _showParts, "#C0392B");
            UpdateCardVisuals(CardProfit, _showProfit, "#27AE60");
            UpdateCardVisuals(CardOrders, _showCount, "#8E44AD");

            // 2. Обновляем только ГРАФИКИ (LoadExpensesStructure НЕ ВЫЗЫВАЕМ)
            LoadData(date);
            LoadYearlyData();
        }


        private void UpdateCardTexts(decimal rev, decimal parts, decimal prof, int count)
        {
            RevText.Text = $"{rev:N2} ₽";
            PartsText.Text = $"{parts:N2} ₽";
            ProfitText.Text = $"{prof:N2} ₽";
            CountText.Text = count.ToString();
        }

        private void LoadExpensesStructure(DateTime date)
        {
            // 1. Для таблицы — берем историю (каждую запись отдельно)
            ExpensesDetailGrid.ItemsSource = _expensesManager.GetExpensesHistory(date);

            // 2. Для круговой диаграммы — по-прежнему используем группировку по категориям
            var categoryStats = _expensesManager.GetExpensesByCategory(date);
            var pieSeries = new SeriesCollection();
            foreach (var stat in categoryStats)
            {
                pieSeries.Add(new PieSeries
                {
                    Title = stat.Category,
                    Values = new ChartValues<decimal> { stat.Amount },
                    DataLabels = false,
                    LabelPoint = point => $"{point.SeriesView.Title}: {point.Y:N2} ₽"
                });
            }
            ExpensesPieChart.Series = pieSeries;
        }

        private void LoadYearlyExpensesPie(int year)
        {
            // Получаем сгруппированные данные по категориям за весь год
            var yearlyExpenses = _expensesManager.GetExpensesByYear(year);

            var pieSeries = new SeriesCollection();

            foreach (var stat in yearlyExpenses)
            {
                pieSeries.Add(new PieSeries
                {
                    Title = stat.Category,
                    Values = new ChartValues<decimal> { stat.Amount },
                    DataLabels = false,
                    LabelPoint = p => $"{p.SeriesView.Title}: {p.Y:N2} ₽"
                });
            }

            // Привязываем к новой диаграмме на второй вкладке
            YearlyExpensesPieChart.Series = pieSeries;
        }

    }
}
