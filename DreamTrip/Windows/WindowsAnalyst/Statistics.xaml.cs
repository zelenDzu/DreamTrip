using DreamTrip.Charts;
using DreamTrip.Classes;
using DreamTrip.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using System.Windows.Threading;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для Statistics.xaml
    /// </summary>
    public partial class Statistics : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Статистика", "../../Resources/statistics.png" };
        UserControl previousPage;
        string[] previousPageParametres;


        ObservableCollection<Tour> ToursList { get; set; } = new ObservableCollection<Tour>();
        #endregion

        #region Constructor
        public Statistics(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            LoadTours();
            LoadDatesPeriod();
            double[] sizes = MainFunctions.MenuLink.GetWidthHeight();
            WindowSizeChanged(sizes[0], sizes[1]);
        }
        #endregion

        #region LoadData
        /// <summary>
        /// Загрузить тура
        /// </summary>
        private void LoadTours()
        {
            DataTable dataTours = MainFunctions.NewQuery("SELECT id_tour, name FROM Tour ORDER BY id_tour");
            for (int i = 0; i < dataTours.Rows.Count; i++)
            {
                Tour newTour = new Tour()
                {
                    TourId = int.Parse(dataTours.Rows[i][0].ToString()),
                    Name = dataTours.Rows[i][1].ToString(),
                };
                ToursList.Add(newTour);
                cmbTour.Items.Add(newTour);
            }
        }
        
        /// <summary>
        /// Загрузить даты периода
        /// </summary>
        private void LoadDatesPeriod()
        {
            tbxStartDate.Text = CommonFunctions.ConvertDateToText(DateTime.Today.AddDays(-30));

            tbxEndDate.Text = CommonFunctions.ConvertDateToText(DateTime.Today.AddDays(30));
        }
        #endregion

        #region Functions
        /// <summary>
        /// При изменении фильтра
        /// </summary>
        private void FilterChanged()
        {
            if (cmbStatisticsType != null && cmbTour != null && tbxStartDate != null &&
                tbxEndDate != null && cmbClientsStatisticsType != null)
            {
                switch (cmbStatisticsType.SelectedIndex)
                {
                    case 1:
                        if (cmbTour.SelectedIndex != 0 && CommonFunctions.isTextDateValid(tbxStartDate.Text) && CommonFunctions.isTextDateValid(tbxEndDate.Text)
                            && (Convert.ToDateTime(tbxEndDate.Text) >= Convert.ToDateTime(tbxStartDate.Text)))
                            borderSearch.IsEnabled = true;
                        else
                            borderSearch.IsEnabled = false;
                        break;

                    case 2:
                        if (cmbClientsStatisticsType.SelectedIndex != 0)
                            borderSearch.IsEnabled = true;
                        else
                            borderSearch.IsEnabled = false;
                        break;

                    default:
                        borderSearch.IsEnabled = false;
                        break;
                }
            }
        }

        /// <summary>
        /// При событии "Скролл мыши"
        /// </summary>
        /// <param name="Delta"></param>
        public void ScrollEvent(int Delta)
        {
            int dtgDelta = Delta * 2 / 7;
            if (Delta < 0) Delta = Delta / 40 + 2;
            else Delta = Delta / 40 - 2;


            //if (lstField.IsDropDownOpen && lstField.IsMouseOver)
            //{

            //    ScrollViewer scv = lstField.Template.FindName("cmbScroll", lstField) as ScrollViewer;
            //    scv.ScrollToVerticalOffset(scv.VerticalOffset - Delta);
            //}

            //if (lstPost.IsDropDownOpen && lstPost.IsMouseOver)
            //{
            //    ScrollViewer scv = lstPost.Template.FindName("cmbScroll", lstPost) as ScrollViewer;
            //    scv.ScrollToVerticalOffset(scv.VerticalOffset - Delta);
            //}

            //if (dtgClients.IsMouseOver)
            //{
            //    ScrollViewer scv = dtgClients.Template.FindName("DG_ScrollViewer", dtgClients) as ScrollViewer;
            //    scv.ScrollToVerticalOffset(scv.VerticalOffset - dtgDelta);
            //}


        }

        /// <summary>
        /// При изменении размеров окна
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void WindowSizeChanged(double width, double height)
        {
            this.Width = width - 48.8;
            mainGrid.Width = this.Width - 270;
        }

        /// <summary>
        /// Создание нового графика
        /// </summary>
        /// <param name="chart">график</param>
        /// <param name="values">значения графика</param>
        /// <param name="infos">подписи к значениям графика</param>
        private static void CreateChart(Chart chart, List<int> values, List<string> infos)
        {
            chart.Clear();

            for (int i = 0; i < values.Count; i++)
            {
                chart.AddValue(values[i], infos[i]);
            }
        }

        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            tbLineChartTitle.Visibility = Visibility.Hidden;
            gridForChart.Children.OfType<Canvas>().ToList().ForEach(p => gridForChart.Children.Remove(p));

            Chart chart = null;

            List<int> values = new List<int>();
            List<string> infos = new List<string>();

            if (gridToursStatistics.Visibility == Visibility.Visible)
            {
                chart = new LineChart();
                if (cmbTour.SelectedIndex != 0 && CommonFunctions.isTextDateValid(tbxStartDate.Text) && CommonFunctions.isTextDateValid(tbxEndDate.Text) &&
                    Convert.ToDateTime(tbxEndDate.Text) > Convert.ToDateTime(tbxStartDate.Text))
                {
                    tbLineChartTitle.Visibility = Visibility.Visible;
                    int tourId = (cmbTour.SelectedItem as Tour).TourId;
                    int totalDaysCount = Convert.ToInt32((Convert.ToDateTime(tbxEndDate.Text) - Convert.ToDateTime(tbxStartDate.Text)).TotalDays);

                    List<string> datesList = new List<string>();
                    List<int> valuesList = new List<int>();

                    for (int i = 0; i < totalDaysCount; i++)
                    {
                        string currentDate = CommonFunctions.ConvertDateToText(Convert.ToDateTime(tbxStartDate.Text).AddDays(i));
                        string sqlCurrentDate = currentDate.Remove(4, 1).Remove(6, 1);

                        int tripsCount = int.Parse(MainFunctions.NewQuery($"SELECT COUNT(id_client) FROM Trip " +
                            $"WHERE id_tour = {tourId} " +
                            $"AND start_date <= '{sqlCurrentDate}' " +
                            $"AND end_date >= '{sqlCurrentDate}' ").Rows[0][0].ToString());

                        datesList.Add(CommonFunctions.ConvertDateToText(Convert.ToDateTime(currentDate)));
                        valuesList.Add(tripsCount);
                    }

                    if (datesList.Count > 0)
                    {
                        infos.Add(datesList[0]);
                        values.Add(valuesList[0]);
                        for (int i = 1; i < datesList.Count; i++)
                        {
                            if (valuesList[i] != values[values.Count - 1])
                            {
                                infos.Add(datesList[i]);
                                values.Add(valuesList[i]);
                            }
                        }

                        if (values.Count == 1)
                        {
                            values.Add(values[0]);
                            infos.Add(tbxEndDate.Text);
                        }

                        if (infos[infos.Count - 1] != tbxEndDate.Text)
                        {
                            values.Add(values[values.Count - 1]);
                            infos.Add(tbxEndDate.Text);
                        }

                        if (infos.Count > 10)
                        {
                            for (int i = 0; i < infos.Count; i++)
                            {
                                infos[i] = "";
                            }
                        }
                    }
                    else
                    {
                        values.Add(0);
                        values.Add(0);
                        infos.Add(tbxStartDate.Text);
                        infos.Add(tbxEndDate.Text);
                    }

                }
                else
                {
                    (new Message("Ошибка", "Неверно выбраны параметры для составления статистики", false, false)).ShowDialog();
                    return;
                }
            }
            if (gridClientsStatistics.Visibility == Visibility.Visible)
            {
                chart = new PieChart();
                switch (cmbClientsStatisticsType.SelectedIndex)
                {
                    case 1:
                        infos.Add("до 25");
                        infos.Add("25-40");
                        infos.Add("40-55");
                        infos.Add("после 55");
                        values.Add(0);
                        values.Add(0);
                        values.Add(0);
                        values.Add(0);
                        DataTable agesData = MainFunctions.NewQuery($"SELECT COUNT(id_client),age FROM Client GROUP BY age ORDER BY age");
                        for (int i = 0; i < agesData.Rows.Count; i++)
                        {
                            if (int.Parse(agesData.Rows[i][1].ToString()) < 25)
                            {
                                values[0] += int.Parse(agesData.Rows[i][0].ToString());
                                continue;
                            }
                            if (int.Parse(agesData.Rows[i][1].ToString()) < 40)
                            {
                                values[1] += int.Parse(agesData.Rows[i][0].ToString());
                                continue;
                            }
                            if (int.Parse(agesData.Rows[i][1].ToString()) < 55)
                            {
                                values[2] += int.Parse(agesData.Rows[i][0].ToString());
                                continue;
                            }
                            if (int.Parse(agesData.Rows[i][1].ToString()) >= 55)
                            {
                                values[3] += int.Parse(agesData.Rows[i][0].ToString());
                                continue;
                            }
                        }
                        break;

                    case 2:
                        DataTable genderData = MainFunctions.NewQuery($"SELECT COUNT(id_client),gender FROM Client GROUP BY gender");
                        for (int i = 0; i < genderData.Rows.Count; i++)
                        {
                            infos.Add(genderData.Rows[i][1].ToString());
                            values.Add(int.Parse(genderData.Rows[i][0].ToString()));
                        }

                        break;

                    case 3:
                        DataTable workFieldData = MainFunctions.NewQuery($"SELECT COUNT(id_client), wf.[name]  " +
                            $"FROM Client_work_info cwi " +
                            $"JOIN Work_field wf ON wf.id_work_field = cwi.id_work_field " +
                            $"GROUP BY cwi.id_work_field, wf.[name] ORDER BY COUNT(id_client)");

                        int rowsCount = workFieldData.Rows.Count;
                        if (rowsCount > 4) rowsCount = 4;

                        for (int i = 0; i < rowsCount; i++)
                        {
                            infos.Add(workFieldData.Rows[i][1].ToString());
                            values.Add(int.Parse(workFieldData.Rows[i][0].ToString()));
                        }

                        if (workFieldData.Rows.Count > 4)
                        {
                            infos.Add("Другие");
                            int otherClientsCount = 0;
                            for (int i = 4; i < workFieldData.Rows.Count; i++)
                            {
                                otherClientsCount += int.Parse(workFieldData.Rows[i][0].ToString());
                            }

                            values.Add(otherClientsCount);
                        }

                        break;

                    default:
                        (new Message("Ошибка", "Неверно выбраны параметры для составления статистики", false, false)).ShowDialog();
                        return;
                }
            }

            gridForChart.Children.Add(chart.ChartBackground);

            gridForChart.UpdateLayout();


            CreateChart(chart, values, infos);
        }
        #endregion

        #region TextChanged
        private void tbxStartDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonFunctions.CheckDate(sender);

            if (CommonFunctions.isTextDateValid(tbxStartDate.Text))
            {
                tbWrongStartDate.Visibility = Visibility.Hidden;
            }
            else
                tbWrongStartDate.Visibility = Visibility.Visible;

            FilterChanged();
        }

        private void tbxEndDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonFunctions.CheckDate(sender);

            if (CommonFunctions.isTextDateValid(tbxEndDate.Text))
            {
                tbWrongEndDate.Visibility = Visibility.Hidden;

                if (CommonFunctions.isTextDateValid(tbxStartDate.Text))
                {
                    if (Convert.ToDateTime(tbxEndDate.Text) < Convert.ToDateTime(tbxStartDate.Text))
                        tbWrongEndDate.Visibility = Visibility.Visible;
                }
            }
            else
                tbWrongEndDate.Visibility = Visibility.Visible;


            FilterChanged();
        }
        #endregion

        #region SelectionChanged
        private void cmbStatisticsType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridToursStatistics != null && gridClientsStatistics != null)
            {
                switch (cmbStatisticsType.SelectedIndex)
                {
                    case 1:
                        gridToursStatistics.Visibility = Visibility.Visible;
                        gridClientsStatistics.Visibility = Visibility.Hidden;
                        break;

                    case 2:
                        gridClientsStatistics.Visibility = Visibility.Visible;
                        gridToursStatistics.Visibility = Visibility.Hidden;
                        break;

                    default:
                        gridToursStatistics.Visibility = Visibility.Hidden;
                        gridClientsStatistics.Visibility = Visibility.Hidden;
                        break;
                }
            }
        }

        private void ComboBoxSelectionChanged(object sender, int index)
        {
            (sender as ComboBox).SelectedIndex = index;
        }

        private void cmbTour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterChanged();
        }

        private void cmbClientsStatisticsType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        #endregion

        #region DropDowns
        private void cmbStatisticsType_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        private void cmbClientsStatisticsType_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        private void cmbTour_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        private void cmbTour_DropDownOpened(object sender, EventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                ScrollViewer scv = cmbTour.Template.FindName("cmbScroll", cmbTour) as ScrollViewer;
                scv.ScrollToTop();
                ((DispatcherTimer)c).Stop();
            });
        }
        #endregion

    }
}
