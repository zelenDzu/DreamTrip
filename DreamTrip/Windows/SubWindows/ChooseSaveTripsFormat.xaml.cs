using DreamTrip.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using DreamTrip.Functions;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChooseSaveTripsFormat.xaml
    /// </summary>
    public partial class ChooseSaveTripsFormat : Window
    {
        #region Variables
        ObservableCollection<Trip> TripsList { get; set; } = new ObservableCollection<Trip>();
        #endregion

        #region Constructor
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="tempTripsList">список поездок</param>
        public ChooseSaveTripsFormat(ObservableCollection<Trip> tempTripsList)
        {
            InitializeComponent();
            TripsList = tempTripsList;
        }
        #endregion

        #region ButtonsClick
        /// <summary>
        /// Выгрузить в формате pdf
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPdf_Click(object sender, RoutedEventArgs e)
        {
            gridFileLoad.Visibility = Visibility.Visible;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                try
                {
                    Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
                    Microsoft.Office.Interop.Word.Document document = word.Documents.Add();
                    Microsoft.Office.Interop.Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Microsoft.Office.Interop.Word.Range tableRange = tableParagraph.Range;
                    Microsoft.Office.Interop.Word.Table tripsTable = document.Tables.Add(tableRange, TripsList.Count + 1, 10);
                    tripsTable.Borders.InsideLineStyle = tripsTable.Borders.OutsideLineStyle =
                    Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;
                    tripsTable.Range.Cells.VerticalAlignment = Microsoft.Office.Interop.Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;


                    Microsoft.Office.Interop.Word.Range cellRange;
                    cellRange = tripsTable.Cell(1, 1).Range;
                    cellRange.Text = "Клиент";
                    cellRange.Cells.Width = 60;
                    cellRange = tripsTable.Cell(1, 2).Range;
                    cellRange.Text = "Тур";
                    cellRange.Cells.Width = 50;
                    cellRange = tripsTable.Cell(1, 3).Range;
                    cellRange.Text = "Местоположение";
                    cellRange.Cells.Width = 60;
                    cellRange = tripsTable.Cell(1, 4).Range;
                    cellRange.Text = "Комната";
                    cellRange.Cells.Width = 60;
                    cellRange = tripsTable.Cell(1, 5).Range;
                    cellRange.Text = "Питание";
                    cellRange.Cells.Width = 50;
                    cellRange = tripsTable.Cell(1, 6).Range;
                    cellRange.Text = "Сервисы";
                    cellRange.Cells.Width = 50;
                    cellRange = tripsTable.Cell(1, 7).Range;
                    cellRange.Text = "Начало";
                    cellRange.Cells.Width = 40;
                    cellRange = tripsTable.Cell(1, 8).Range;
                    cellRange.Text = "Конец";
                    cellRange.Cells.Width = 50;
                    cellRange = tripsTable.Cell(1, 9).Range;
                    cellRange.Text = "Статус";
                    cellRange.Cells.Width = 42;
                    cellRange = tripsTable.Cell(1, 10).Range;
                    cellRange.Text = "Стоимость";
                    cellRange.Cells.Width = 45;

                    tripsTable.Rows[1].Range.Bold = 1;
                    tripsTable.Rows[1].Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    for (int i = 0; i < TripsList.Count; i++)
                    {
                        var currentTrip = TripsList[i];

                        cellRange = tripsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentTrip.ClientName;
                        cellRange.Cells.Width = 60;


                        cellRange = tripsTable.Cell(i + 2, 2).Range;
                        cellRange.Text = currentTrip.TourName.ToString();
                        cellRange.Cells.Width = 50;


                        cellRange = tripsTable.Cell(i + 2, 3).Range;
                        cellRange.Text = currentTrip.Location;
                        cellRange.Cells.Width = 60;


                        cellRange = tripsTable.Cell(i + 2, 4).Range;
                        cellRange.Text = currentTrip.RoomType;
                        cellRange.Cells.Width = 60;


                        cellRange = tripsTable.Cell(i + 2, 5).Range;
                        cellRange.Text = currentTrip.FeedType.ToString();
                        cellRange.Cells.Width = 50;


                        cellRange = tripsTable.Cell(i + 2, 6).Range;
                        cellRange.Text = currentTrip.Services.ToString();
                        cellRange.Cells.Width = 50;


                        cellRange = tripsTable.Cell(i + 2, 7).Range;
                        cellRange.Text = currentTrip.StartDate.ToString();
                        cellRange.Cells.Width = 40;


                        cellRange = tripsTable.Cell(i + 2, 8).Range;
                        cellRange.Text = currentTrip.EndDate;
                        cellRange.Cells.Width = 50;


                        cellRange = tripsTable.Cell(i + 2, 9).Range;
                        cellRange.Text = currentTrip.Status.ToString();
                        cellRange.Cells.Width = 42;

                        cellRange = tripsTable.Cell(i + 2, 10).Range;
                        cellRange.Text = currentTrip.TotalPrice.ToString() + " руб.";
                        cellRange.Cells.Width = 45;
                    }

                    word.Selection.Tables[1].Rows.Alignment = Microsoft.Office.Interop.Word.WdRowAlignment.wdAlignRowRight;

                    System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.FileName = null;
                    saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    saveFileDialog.Filter = $"pdf-файл(*.pdf)|*.pdf|Все файлы (*.*)|*.*";
                    saveFileDialog.ShowDialog();
                    if (saveFileDialog.FileName != null && saveFileDialog.FileName != "")
                    {
                        string fileName = saveFileDialog.FileName;
                        if (!fileName.Contains($".pdf"))
                        {
                            if (fileName.Contains(".")) fileName = fileName.Remove(fileName.IndexOf('.'));
                            fileName += $".pdf";
                        }

                        document.SaveAs2(fileName, Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);

                        MainFunctions.AddLogRecord($"Trips saved in PDF");
                        (new Message("Успех", "Данные успешно выгружены")).ShowDialog();

                    }
                }
                catch (Exception ex)
                {
                    new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                    MainFunctions.AddLogRecord($"Unknown error: {ex.Message}");
                }

                gridFileLoad.Visibility = Visibility.Hidden;
                ((DispatcherTimer)c).Stop();
            });

        }

        /// <summary>
        /// Выгрузить в формате csv
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCsv_Click(object sender, RoutedEventArgs e)
        {
            gridFileLoad.Visibility = Visibility.Visible;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                try
                {
                    Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                    Microsoft.Office.Interop.Excel.Workbook book = excel.Workbooks.Add(System.Reflection.Missing.Value);
                    Microsoft.Office.Interop.Excel.Worksheet sheet = (Microsoft.Office.Interop.Excel.Worksheet)book.ActiveSheet;
                    sheet.Columns.EntireColumn.ColumnWidth = 30;
                    sheet.Rows.EntireRow.RowHeight = 20;
                    sheet.Columns.AutoFit();

                    string[] vs = new string[] { "Клиент",
                                        "Тур",
                                        "Местоположение",
                                        "Комната",
                                        "Питание",
                                        "Сервисы",
                                        "Начало",
                                        "Конец",
                                        "Статус",
                                        "Стоимость"};

                    string[,] dat = new string[10, TripsList.Count];
                    int counter = 0;
                    for (int i = 0; i < TripsList.Count; i++)
                    {
                        counter = 0;
                        dat[counter, i] = TripsList[i].ClientName.ToString();
                        counter += 1;
                        dat[counter, i] = TripsList[i].TourName.ToString();
                        counter += 1;
                        dat[counter, i] = TripsList[i].Location.ToString();
                        counter += 1;
                        dat[counter, i] = TripsList[i].RoomType.ToString();
                        counter += 1;
                        dat[counter, i] = TripsList[i].FeedType.ToString();
                        counter += 1;
                        dat[counter, i] = TripsList[i].Services.ToString();
                        counter += 1;
                        dat[counter, i] = TripsList[i].StartDate.ToString();
                        counter += 1;
                        dat[counter, i] = TripsList[i].EndDate.ToString();
                        counter += 1;
                        dat[counter, i] = TripsList[i].Status.ToString();
                        counter += 1;
                        dat[counter, i] = TripsList[i].TotalPrice.ToString() + " руб.";
                    }


                    for (int i = 0; i < 10; i++)
                    {
                        sheet.Range["A1"].Offset[0, i].Value = vs[i];
                    }


                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < TripsList.Count; j++)
                        {
                            sheet.Range["A1"].Offset[j + 1, i].Value = dat[i, j];
                        }
                    }

                    System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.FileName = null;
                    saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    saveFileDialog.Filter = $"csv-файл(*.csv)|*.csv|Все файлы (*.*)|*.*";
                    saveFileDialog.ShowDialog();
                    if (saveFileDialog.FileName != null && saveFileDialog.FileName != "")
                    {
                        string fileName = saveFileDialog.FileName;
                        if (!fileName.Contains($".csv"))
                        {
                            if (fileName.Contains(".")) fileName = fileName.Remove(fileName.IndexOf('.'));
                            fileName += $".csv";
                        }

                        book.SaveAs(fileName);

                        MainFunctions.AddLogRecord($"Trips saved in CSV");
                        (new Message("Успех", "Данные успешно выгружены")).ShowDialog();
                    }

                    sheet.Range["A1"].Offset[0, 10].Font.Bold = true;
                    excel.Visible = true;
                    book.Activate();
                }
                catch (Exception ex)
                {
                    new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                    MainFunctions.AddLogRecord($"Unknown error: {ex.Message}");
                }

                gridFileLoad.Visibility = Visibility.Hidden;
                ((DispatcherTimer)c).Stop();
            });
        }

        /// <summary>
        /// Отправить на печать в word
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            gridFileLoad.Visibility = Visibility.Visible;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                try
                {
                    Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
                    Microsoft.Office.Interop.Word.Document document = word.Documents.Add();
                    Microsoft.Office.Interop.Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Microsoft.Office.Interop.Word.Range tableRange = tableParagraph.Range;
                    Microsoft.Office.Interop.Word.Table tripsTable = document.Tables.Add(tableRange, TripsList.Count + 1, 10);
                    tripsTable.Borders.InsideLineStyle = tripsTable.Borders.OutsideLineStyle =
                    Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;
                    tripsTable.Range.Cells.VerticalAlignment = Microsoft.Office.Interop.Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    Microsoft.Office.Interop.Word.Range cellRange;
                    cellRange = tripsTable.Cell(1, 1).Range;
                    cellRange.Text = "Клиент";
                    cellRange.Cells.Width = 60;
                    cellRange = tripsTable.Cell(1, 2).Range;
                    cellRange.Text = "Тур";
                    cellRange.Cells.Width = 50;
                    cellRange = tripsTable.Cell(1, 3).Range;
                    cellRange.Text = "Местоположение";
                    cellRange.Cells.Width = 60;
                    cellRange = tripsTable.Cell(1, 4).Range;
                    cellRange.Text = "Тип комнаты";
                    cellRange.Cells.Width = 60;
                    cellRange = tripsTable.Cell(1, 5).Range;
                    cellRange.Text = "Тип питания";
                    cellRange.Cells.Width = 50;
                    cellRange = tripsTable.Cell(1, 6).Range;
                    cellRange.Text = "Сервисы";
                    cellRange.Cells.Width = 50;
                    cellRange = tripsTable.Cell(1, 7).Range;
                    cellRange.Text = "Начало";
                    cellRange.Cells.Width = 40;
                    cellRange = tripsTable.Cell(1, 8).Range;
                    cellRange.Text = "Конец";
                    cellRange.Cells.Width = 50;
                    cellRange = tripsTable.Cell(1, 9).Range;
                    cellRange.Text = "Статус";
                    cellRange.Cells.Width = 42;
                    cellRange = tripsTable.Cell(1, 10).Range;
                    cellRange.Text = "Стоимость";
                    cellRange.Cells.Width = 45;

                    tripsTable.Rows[1].Range.Bold = 1;
                    tripsTable.Rows[1].Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    for (int i = 0; i < TripsList.Count; i++)
                    {
                        var currentTrip = TripsList[i];

                        cellRange = tripsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentTrip.ClientName.ToString();
                        cellRange.Cells.Width = 60;


                        cellRange = tripsTable.Cell(i + 2, 2).Range;
                        cellRange.Text = currentTrip.TourName.ToString();
                        cellRange.Cells.Width = 50;


                        cellRange = tripsTable.Cell(i + 2, 3).Range;
                        cellRange.Text = currentTrip.Location;
                        cellRange.Cells.Width = 60;


                        cellRange = tripsTable.Cell(i + 2, 4).Range;
                        cellRange.Text = currentTrip.RoomType;
                        cellRange.Cells.Width = 60;


                        cellRange = tripsTable.Cell(i + 2, 5).Range;
                        cellRange.Text = currentTrip.FeedType.ToString();
                        cellRange.Cells.Width = 50;


                        cellRange = tripsTable.Cell(i + 2, 6).Range;
                        cellRange.Text = currentTrip.Services.ToString();
                        cellRange.Cells.Width = 50;


                        cellRange = tripsTable.Cell(i + 2, 7).Range;
                        cellRange.Text = currentTrip.StartDate.ToString();
                        cellRange.Cells.Width = 40;


                        cellRange = tripsTable.Cell(i + 2, 8).Range;
                        cellRange.Text = currentTrip.EndDate;
                        cellRange.Cells.Width = 50;


                        cellRange = tripsTable.Cell(i + 2, 9).Range;
                        cellRange.Text = currentTrip.Status.ToString();
                        cellRange.Cells.Width = 42;

                        cellRange = tripsTable.Cell(i + 2, 10).Range;
                        cellRange.Text = currentTrip.TotalPrice.ToString() + " руб.";
                        cellRange.Cells.Width = 45;

                    }



                    //код на случай, если появится необходимость сохранять файл перед его открытием

                    //System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    //saveFileDialog.FileName = null;
                    //saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    //saveFileDialog.Filter = $"docx-файл(*.docx)|*.docx|Все файлы (*.*)|*.*";
                    //saveFileDialog.ShowDialog();
                    //if (saveFileDialog.FileName != null && saveFileDialog.FileName != "")
                    //{
                    //    string fileName = saveFileDialog.FileName;
                    //    if (!fileName.Contains($".docx"))
                    //    {
                    //        if (fileName.Contains(".")) fileName = fileName.Remove(fileName.IndexOf('.'));
                    //        fileName += $".docx";
                    //    }

                    //    document.SaveAs2(fileName);

                    //    (new Message("Успех", "Данные успешно выгружены")).ShowDialog();
                    //}

                    (new Message("Успех", "Данные успешно выгружены")).ShowDialog();

                    word.Selection.Tables[1].Rows.Alignment = Microsoft.Office.Interop.Word.WdRowAlignment.wdAlignRowRight;
                    word.Visible = true;

                    Microsoft.Office.Interop.Word.Dialog printDialog = word.Application.Dialogs[Microsoft.Office.Interop.Word.WdWordDialog.wdDialogFilePrint];
                    if (printDialog.Show() == 1)
                    {
                        document.PrintOut();
                        MainFunctions.AddLogRecord($"Trips printed");
                    }
                }
                catch (Exception ex)
                {
                    new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                    MainFunctions.AddLogRecord($"Unknown error: {ex.Message}");
                }

                gridFileLoad.Visibility = Visibility.Hidden;
                ((DispatcherTimer)c).Stop();
            });
        }
        #endregion
    }
}
