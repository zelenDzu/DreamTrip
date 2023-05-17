using DreamTrip.Classes;
using DreamTrip.Functions;
using System;
using System.Collections.Generic;
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
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Globalization;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для AnalyzeClients.xaml
    /// </summary>
    public partial class AnalyzeTrips : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Аналитика по турам", "../../Resources/analyze_tour.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        List<Tour> currentTours;

        public SeriesCollection ToursSeries { get; set; }
        public string[] LabelsMonths { get; set; }
        public Func<int, string> ValuesIncome { get; set; }
        public Func<int, string> ValuesCount { get; set; }
        public Func<double, string> LabelFormat { get; set; }
        
        #endregion

        #region Constructor
        public AnalyzeTrips(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            LoadTopTours();
            LoadIncome();
            LoadTripsCount();
            LoadTourTypes();

            LoadFirstChart();
        }
        #endregion

        #region LoadData
        void LoadTourTypes()
        {

            DataTable tourTypesData = MainFunctions.NewQuery($"SELECT * FROM Type_of_tour");

            for (int i = 0; i < tourTypesData.Rows.Count; i++)
            {
                TourType newType = new TourType()
                {
                    TourTypeId = Convert.ToInt32(tourTypesData.Rows[i][0].ToString()),
                    Name = tourTypesData.Rows[i][1].ToString()

                };

                cmbTourType.Items.Add(newType);
            }


            cmbTourType.SelectedIndex = 0;
        }

        void LoadTours()
        {
            if (currentTours != null) currentTours.Clear();
            currentTours = new List<Tour>();
            if (cmbTour.Items.Count > 0) cmbTour.Items.Clear();
            cmbTour.ItemsSource = null;

            int tourTypeId = (cmbTourType.SelectedItem as TourType).TourTypeId;

            DataTable tourData = MainFunctions.NewQuery($"SELECT id_tour, name FROM Tour WHERE id_tour IN " +
                $"(SELECT id_tour FROM Tour_types WHERE id_type = {tourTypeId})");

            for (int i = 0; i < tourData.Rows.Count; i++)
            {
                currentTours.Add(new Tour()
                {
                    TourId = Convert.ToInt32(tourData.Rows[i][0].ToString()),
                    Name = tourData.Rows[i][1].ToString()
                });
            }

            if (currentTours.Count == 0)
            {
                cmbTour.Items.Add(new ComboBoxItem()
                {
                    Content = new TextBlock()
                    {
                        Text = "Нет туров данного типа",
                        Foreground = Brushes.Gray,
                    },

                    IsEnabled = false,
                    Visibility = Visibility.Collapsed,
                }) ;

                cmbTour.SelectedIndex = 0;
            }
            else
            {
                cmbTour.ItemsSource = currentTours;
                cmbTour.SelectedIndex = 0;
            }

            
        }

        void LoadTopTours()
        {
            TextBlock[] textBoxesNames = new TextBlock[3] { tbTopTour1, tbTopTour2, tbTopTour3 };
            TextBlock[] textBoxesCounts = new TextBlock[3] { tbTopTourCount1, tbTopTourCount2, tbTopTourCount3 };

            for (int i = 0; i < 3; i++)
            {
                textBoxesNames[i].Text = "";
                textBoxesCounts[i].Text = "";
            }

            List<string> toursNameCount = Analytics.GetTopTours();

            for (int i = 0; i < toursNameCount.Count; i++)
            {
                textBoxesNames[i].Text = toursNameCount[i].Split(',')[0];
                textBoxesCounts[i].Text = toursNameCount[i].Split(',')[1];
            }

        }

        void LoadIncome()
        {
            List<int> incomePercent = Analytics.GetCurrentTrips_IncomePercent();
            tbIncome.Text = incomePercent[0].ToString("### ### ###") + "₽";

            int percent = incomePercent[1];
            if (percent >= 0)
            {
                tbIncomePercent.Text = percent.ToString() + "%";
                tbIncomePercent.Foreground = plgGreenIncome.Fill;
                plgGreenIncome.Visibility = Visibility.Visible;
                plgRedIncome.Visibility = Visibility.Hidden;
            }
            else
            {
                tbIncomePercent.Text = (-percent).ToString() + "%";
                tbIncomePercent.Foreground = plgRedIncome.Fill;
                plgGreenIncome.Visibility = Visibility.Hidden;
                plgRedIncome.Visibility = Visibility.Visible;
            }
            

        }

        void LoadTripsCount()
        {
            List<int> countPercent = Analytics.GetCurrentTrips_CountPercent();
            tbTripsCount.Text = countPercent[0].ToString();

            int percent = countPercent[1];
            if (percent >= 0)
            {
                tbTripsPercent.Text = percent.ToString() + "%";
                tbTripsPercent.Foreground = plgGreenIncome.Fill;
                plgGreenTrips.Visibility = Visibility.Visible;
                plgRedTrips.Visibility = Visibility.Hidden;
            }
            else
            {
                tbTripsPercent.Text = (-percent).ToString() + "%";
                tbTripsPercent.Foreground = plgRedIncome.Fill;
                plgGreenTrips.Visibility = Visibility.Hidden;
                plgRedTrips.Visibility = Visibility.Visible;
            }
        }

        void LoadFirstChart()
        {
            int bestTourId = Convert.ToInt32(MainFunctions.NewQuery($"SELECT TOP 1 id_tour " +
                $"FROM Trip " +
                $"GROUP BY id_tour " +
                $"ORDER BY SUM(total_price) DESC").Rows[0][0].ToString());

            int bestTourTypeId = Convert.ToInt32(MainFunctions.NewQuery($"SELECT TOP 1 id_Type " +
                $"FROM Tour_types " +
                $"WHERE id_tour = {bestTourId}").Rows[0][0].ToString());

            for (int i = 1; i < cmbTourType.Items.Count; i++)
            {
                if ((cmbTourType.Items[i] as TourType).TourTypeId == bestTourTypeId)
                {
                    cmbTourType.SelectedItem = cmbTourType.Items[i];

                    for (int j = 0; j < cmbTour.Items.Count; j++)
                    {
                        if ((cmbTour.Items[j] as Tour).TourId == bestTourId)
                        {
                            cmbTour.SelectedItem = cmbTour.Items[j];
                            break;
                        }
                    }

                    break;
                }
            }
        }

        void LoadTourChart(int tourId = 0)
        {
            DataContext = null;

            ValuesIncome = value => value.ToString("C");
            ValuesCount = value => value.ToString("N");
            LabelFormat = value => value.ToString() + "₽";

            List<string> months = new List<string>();
            List<int> tripsCounts = new List<int>();
            List<int> tourIncome = new List<int>();

            for (int i = 0; i < 12; i++)
            {
                months.Add(Analytics.GetMonthName(i));
                tripsCounts.Add(Analytics.GetTourTripsCount(tourId, i));
                tourIncome.Add(Analytics.GetTourIncome(tourId, i));
            }
            

            months.Reverse();
            tripsCounts.Reverse();
            tourIncome.Reverse();

            LabelsMonths = months.ToArray<string>();

            ToursSeries = new SeriesCollection();

            if (tripsCounts.Max() == 0)
            {
                chartAxisYCount = new Axis()
                {
                    //Labels = new List<string>(){ "0","1"},
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 1
                    }
                };

                chartAxisYIncome = new Axis()
                {
                    //Labels = new List<string>() { "0", "10000" },
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 10000
                    }
                };
            }
            else
            {
                ToursSeries.Add(new ColumnSeries
                {
                    Title = "Выручка",
                    Values = new ChartValues<int>(tourIncome),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#61D8D8")),
                    MaxColumnWidth = 60,
                    ScalesYAt = 0,
                });

                ToursSeries.Add(new LineSeries
                {
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3DAFDB")),
                    Fill = Brushes.Transparent,
                    Title = "Кол-во поездок",
                    Values = new ChartValues<int>(tripsCounts),
                    StrokeThickness = 5,
                    ScalesYAt = 1,
                    DataLabels = true,
                });
            }
            


            DataContext = this;
            if (chartAxisYIncome!=null) chartAxisYIncome.LabelFormatter = LabelFormat;
        }
        #endregion

        #region Functions

        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        #endregion

        #region Changed
        private void cmbTourType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cmbTourType.SelectedItem as TourType)!=null) LoadTours();
        }

        private void cmbTour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cmbTour.SelectedItem as Tour) != null) LoadTourChart((cmbTour.SelectedItem as Tour).TourId);
            else LoadTourChart();
        }


        #endregion

    }
}
