using DreamTrip.Classes;
using DreamTrip.Functions;
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
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Data;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для AnalyzeClients.xaml
    /// </summary>
    public partial class AnalyzeClients : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Аналитика по клиентам", "../../Resources/analyze_client.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        List<ClientABC> clientsABC;

        List<SolidColorBrush> chartColors;
        public Func<ChartPoint, string> PointLabel { get; set; }
        public SeriesCollection clientSeries { get; set; }
        #endregion

        #region Constructor
        public AnalyzeClients(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            PointLabel = chartPoint => string.Format("{0} чел.", chartPoint.Y);
            chartColors = new List<SolidColorBrush>()
            {
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007373")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00B1B1")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#61D8D8")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3DAFDB")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7192DF")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D64E0")),
            };

            try
            {
                LoadNewClientsCount();
                LoadClientsABC();
                LoadPieChart(2);
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                btnCancel_Click(btnCancel, new RoutedEventArgs());
                MainFunctions.AddLogRecord($"Unknown analyze clients load error: {ex.Message}");
            }

            if (MainFunctions.GetShowPrompts()) btnHelpInfo.Visibility = Visibility.Visible;
            else btnHelpInfo.Visibility = Visibility.Hidden;

        }
        #endregion

        #region LoadData
        void LoadNewClientsCount()
        {
            List<int> countPercent = Analytics.GetCurrentClients_CountPercent();
            tbNewClientCount.Text = countPercent[0].ToString();

            int percent = countPercent[1];
            if (percent >= 0)
            {
                tbNewClientPercent.Text = percent.ToString() + "%";
                tbNewClientPercent.Foreground = plgGreen.Fill;
                plgGreen.Visibility = Visibility.Visible;
                plgRed.Visibility = Visibility.Hidden;
            }
            else
            {
                tbNewClientPercent.Text = (-percent).ToString() + "%";
                tbNewClientPercent.Foreground = plgRed.Fill;
                plgGreen.Visibility = Visibility.Hidden;
                plgRed.Visibility = Visibility.Visible;
            }
        }

        void LoadPieChart(int groupType = 1)
        {
            clientSeries = new SeriesCollection();
            pcClientGroup.Series = null;

            switch (groupType)
            {
                case 1: //пол
                    clientSeries.Add(new PieSeries
                    {
                        FontSize = 16,
                        DataLabels = true,
                        LabelPoint = PointLabel,

                        Fill = chartColors[0],

                        Title = $"Мужчина",
                        Values = new ChartValues<ObservableValue> { new ObservableValue
                        (Convert.ToInt32(MainFunctions.NewQuery($"SELECT COUNT(*) FROM Client WHERE gender='М'").Rows[0][0].ToString())) },
                    });

                    clientSeries.Add(new PieSeries
                    {
                        FontSize = 16,
                        DataLabels = true,
                        LabelPoint = PointLabel,

                        Fill = chartColors[1],

                        Title = $"Женщина",
                        Values = new ChartValues<ObservableValue> { new ObservableValue
                        (Convert.ToInt32(MainFunctions.NewQuery($"SELECT COUNT(*) FROM Client WHERE gender='Ж'").Rows[0][0].ToString())) },
                    });

                    break;

                case 2: //возраст
                    int minAge = Convert.ToInt32(MainFunctions.NewQuery($"SELECT MIN(age) FROM Client").Rows[0][0].ToString());
                    int maxAge = Convert.ToInt32(MainFunctions.NewQuery($"SELECT MAX(age) FROM Client").Rows[0][0].ToString());

                    clientSeries.Add(new PieSeries
                    {
                        FontSize = 16,
                        DataLabels = true,
                        LabelPoint = PointLabel,

                        Fill = chartColors[0],

                        Title = $"{minAge}-24 лет",
                        Values = new ChartValues<ObservableValue> { new ObservableValue
                        (Convert.ToInt32(MainFunctions.NewQuery($"SELECT COUNT(*) FROM Client WHERE Age BETWEEN {minAge} AND 24").Rows[0][0].ToString())) },
                    });

                    clientSeries.Add(new PieSeries
                    {
                        FontSize = 16,
                        DataLabels = true,
                        LabelPoint = PointLabel,

                        Fill = chartColors[1],

                        Title = $"25-39 лет",
                        Values = new ChartValues<ObservableValue> { new ObservableValue
                        (Convert.ToInt32(MainFunctions.NewQuery($"SELECT COUNT(*) FROM Client WHERE Age BETWEEN 25 AND 39").Rows[0][0].ToString())) },
                    });

                    clientSeries.Add(new PieSeries
                    {
                        FontSize = 16,
                        DataLabels = true,
                        LabelPoint = PointLabel,

                        Fill = chartColors[2],

                        Title = $"40-59 лет",
                        Values = new ChartValues<ObservableValue> { new ObservableValue
                        (Convert.ToInt32(MainFunctions.NewQuery($"SELECT COUNT(*) FROM Client WHERE Age BETWEEN 40 AND 59").Rows[0][0].ToString())) },
                    });

                    clientSeries.Add(new PieSeries
                    {
                        FontSize = 16,
                        DataLabels = true,
                        LabelPoint = PointLabel,

                        Fill = chartColors[3],

                        Title = $"60-{maxAge} лет",
                        Values = new ChartValues<ObservableValue> { new ObservableValue
                        (Convert.ToInt32(MainFunctions.NewQuery($"SELECT COUNT(*) FROM Client WHERE Age BETWEEN 60 AND {maxAge}").Rows[0][0].ToString())) },
                    });

                    break;

                case 3: //работа
                    DataTable workData = MainFunctions.NewQuery($"SELECT COUNT(id_client), wf.short_name " +
                        $"FROM Client_work_info cwi " +
                        $"JOIN Work_field wf ON wf.id_work_field = cwi.id_work_field " +
                        $"WHERE wf.name != 'Отсутствует' AND wf.name != 'Другое' " +
                        $"GROUP BY cwi.id_work_field, wf.short_name ORDER BY COUNT(id_client) DESC");

                    int maxFieldsCount = 5;

                    List<int> counts = new List<int>();
                    List<string> names = new List<string>();

                    for (int i = 0; i < maxFieldsCount; i++)
                    {
                        counts.Add(Convert.ToInt32(workData.Rows[i][0].ToString()));
                        names.Add(workData.Rows[i][1].ToString());
                    }

                    if (workData.Rows.Count > maxFieldsCount)
                    {
                        int otherCount = 0;
                        names.Add("Другое");
                        for (int i = maxFieldsCount; i < workData.Rows.Count; i++)
                        {
                            otherCount += Convert.ToInt32(workData.Rows[i][0].ToString());
                        }
                        counts.Add(otherCount);
                    }

                    for (int i = 0; i < names.Count; i++)
                    {
                        clientSeries.Add(new PieSeries
                        {
                            FontSize = 16,
                            DataLabels = true,
                            LabelPoint = PointLabel,

                            Fill = chartColors[i%chartColors.Count],

                            Title = names[i% chartColors.Count],
                            Values = new ChartValues<ObservableValue> { new ObservableValue(counts[i]) },
                        });
                    }

                    break;
            }

            pcClientGroup.Series = clientSeries;
            //DataContext = this;
        }

        void LoadClientsABC()
        {
            clientsABC = Analytics.GetClientABCs();
            dtgClients.ItemsSource = null;
            dtgClients.ItemsSource = clientsABC;
        }
        #endregion

        #region Functions
        /// <summary>
        /// При событии "Скролл мыши"
        /// </summary>
        /// <param name="Delta">значение скролла</param>
        public void ScrollEvent(int Delta)
        {
            int dtgDelta = Delta * 2 / 7;
            if (Delta < 0) Delta = Delta / 40 + 2;
            else Delta = Delta / 40 - 2;


            
            if (dtgClients.IsMouseOver)
            {
                ScrollViewer scv = dtgClients.Template.FindName("DG_ScrollViewer", dtgClients) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - dtgDelta);
            }
        }

        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        private void tbClientFullName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int clientId = int.Parse((sender as TextBlock).Tag.ToString());
            parentTabItemLink.ItemUserControl = new EditClient(parentTabItemLink, this, thisPageParametres, clientId);
        }
        #endregion

        #region Changed
        private void cmbClientGroupType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //index : type
            //1 : пол
            //2 : возраст
            //3 : работа

            try
            {
                if (pcClientGroup != null)
                    if (cmbClientGroupType.SelectedIndex != -1)
                        LoadPieChart(cmbClientGroupType.SelectedIndex + 1);
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                MainFunctions.AddLogRecord($"Unknown error: {ex.Message}");
            }
        }

        #endregion


    }
}
