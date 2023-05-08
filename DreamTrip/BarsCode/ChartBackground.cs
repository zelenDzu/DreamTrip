using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace DreamTrip.BarsCode
{

    /// <summary>
    /// Фон для вывода диаграмм
    /// </summary>
    internal class ChartBackground : Grid
    {
        private SolidColorBrush _bg = new SolidColorBrush(Color.FromArgb(255, 180, 200, 180));
        private Grid _grid = new Grid();
        private Grid _paddinggrid = new Grid();

        public  UIElementCollection ChartItems
        {
            get => _paddinggrid.Children;
        }


        public ChartBackground()
        {
            Background = new SolidColorBrush(Color.FromArgb(150, 190, 220, 190));

            Children.Add(_paddinggrid);
            _paddinggrid.Margin = new System.Windows.Thickness(0);
        }



        public void DrawLine()
        {
            for (int i = 0; i <= 6; i++)
            {
                Line line = new Line();
                line.X1 = 10;
                line.Y1 = -i * 50 + _paddinggrid.Margin.Bottom;
                line.X2 = 450;
                line.Y2 = -i * 50 + _paddinggrid.Margin.Bottom;
                line.Stroke = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
                line.StrokeDashArray = new DoubleCollection() { 5 };
                line.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                line.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                Children.Add(line);
            }
        }
    }
}
