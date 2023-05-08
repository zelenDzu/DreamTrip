using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;



namespace DreamTrip.Charts
{

    /// <summary>
    /// График типа "Круг"
    /// </summary>
    internal class PieChart : Chart
    {

        private List<string> infoList = new List<string>();
        private List<SolidColorBrush> colorsList = new List<SolidColorBrush>()
        {
            new SolidColorBrush(Color.FromArgb(255,58,166,208)),
            new SolidColorBrush(Color.FromArgb(255,52,209,178)),
            new SolidColorBrush(Color.FromArgb(255,0,163,131)),
            new SolidColorBrush(Color.FromArgb(255,34,96,120)),
            new SolidColorBrush(Color.FromArgb(255,49,137,236)),
            new SolidColorBrush(Color.FromArgb(255,0,101,97)),
            new SolidColorBrush(Color.FromArgb(255,0,155,149)),
            new SolidColorBrush(Color.FromArgb(255,29,116,113)),
            new SolidColorBrush(Color.FromArgb(255,98,177,208)),
            new SolidColorBrush(Color.FromArgb(255,2,76,104)),
            new SolidColorBrush(Color.FromArgb(255,94,209,186)),
            new SolidColorBrush(Color.FromArgb(255,7,118,160)),
            new SolidColorBrush(Color.FromArgb(255,31,122,104)),
            new SolidColorBrush(Color.FromArgb(255,92,205,201)),
            new SolidColorBrush(Color.FromArgb(255,0,106,85)),
            new SolidColorBrush(Color.FromArgb(255,51,205,199)),
        };

        public override void AddValue(double value, string info)
        {
            List<StoredValues> listValues = СalculateSectorAngle(value);
            infoList.Add(info);

            // Удалим все элементы перед созданием новых актуальных.
            Clear();

            // Размещение секторов вычисленного размера для создания Pie Chart.
            for (int i = 0; i < listValues.Count; i++)
            {
                StoredValues sv = listValues[i];

                // Каждый Path-элемент будет хранить данные сектора для последующих вычислений.
                Path p = CreateSector(sv.Degree, sv.Offset, sv.Value);
                _ = ChartBackground.Children.Add(p);


                // Числовые значения секторов диска.
                Label label = new Label()
                {
                    Content = infoList[i]
                };

                // Цветовые метки перед числовыми
                Rectangle r = new Rectangle()
                {
                    Width = 16,
                    Height = 12,
                    Fill = p.Fill,
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };

                StackPanel sp = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                sp.Children.Add(r);
                sp.Children.Add(label);
                Canvas.SetLeft(sp, 10);
                Canvas.SetTop(sp, 20 * i);
                _ = ChartBackground.Children.Add(sp);
            }

            /*
            // Это важные пояснительные строчки:
            // как формируется смещение в градусах.
            Path path = CreatePath(45, 0, Brushes.Red);
            path.Tag = value;
            _canvas.Children.Add(path);

            path = CreatePath(15, 45, Brushes.Blue);
            _canvas.Children.Add(path);

            path = CreatePath(300, 60, Brushes.GreenYellow);
            _canvas.Children.Add(path);
            */
        }


        public override void Clear()
        {
            ChartBackground.Children.Clear();
        }


        #region Private

        /// <summary>
        /// Создание сектора диска с собственным цветом.
        /// </summary>
        /// <param name="degree">угол сектора в градусах</param>
        /// <param name="offset">угол смещение на величину угла предыдущего сектора</param>
        /// <param name="value">абсолютное значение пункта графика</param>
        /// <returns></returns>
        private Path CreateSector(double degree, double offset, double value)
        {
            Random random = new Random();

            Path path = new Path()
            {
                StrokeThickness = 5,
                Stroke = Brushes.White,


                Fill = colorsList[0],

                Data = new PathGeometry()
                {
                    Figures = new PathFigureCollection()
                    {
                        SectorGeometry(degree, offset)
                    }
                },

                // Каждый Path будет хранить свои координаты.
                Tag = new StoredValues()
                {
                    Degree = degree,
                    Offset = offset,
                    Value = value
                }
            };

            colorsList.RemoveAt(0);



            return path;
        }


        /// <summary>
        /// Геометрия сектора
        /// </summary>
        /// <param name="degree">угол сектора</param>
        /// <param name="offset">смещение на угол предыдущего сектора</param>
        /// <returns></returns>
        private PathFigure SectorGeometry(double degree, double offset)
        {
            // Радиус круговой диаграммы в зависимости от высоты поля графика.
            double _radius = ChartBackground.ActualHeight / 2 - PaddingChart;

            bool islarge = false;

            // Если угол больше половины диска формируем большую дугу.
            if (degree > 180) islarge = true;

            // Исключаем совпадение начальной точки с конечной,
            // иначе сектор не отразится.
            if (degree >= 360) degree = 359.999;

            // Центр диска.
            Point centerPoint = new Point(ChartBackground.ActualWidth / 2, ChartBackground.ActualHeight / 2);

            // Начальная точка сектора диска - самая нижняя точка диска.
            Point startPoint = new Point(centerPoint.X, centerPoint.Y + _radius);

            // Конечная точка дуги. Точка подлежащая вращению.
            // Начинается от начала диска, от самой нижней точки.
            // Вращение по часовой стрелке.
            Point endPoint = startPoint;

            // Поворачиваем на угол смещения стартовую точку.
            RotateTransform rotateStartPoint = new RotateTransform(offset)
            {
                CenterX = centerPoint.X,
                CenterY = centerPoint.Y
            };
            startPoint = rotateStartPoint.Transform(startPoint);

            // Поворачиваем на заданный угол конечную точку,
            // на угол относительно начальной точки фигуры.
            RotateTransform rotateEndPoint = new RotateTransform(offset + degree)
            {
                CenterX = centerPoint.X,
                CenterY = centerPoint.Y
            };
            endPoint = rotateEndPoint.Transform(endPoint);

            // Фигура предсталяющая отдельный сектор на диске.
            PathFigure sector = new PathFigure()
            {
                StartPoint = startPoint,
                Segments = new PathSegmentCollection()
                {
                    new ArcSegment()
                    {
                        Point = endPoint,
                        Size = new Size(_radius, _radius),
                        SweepDirection = SweepDirection.Clockwise,
                        IsLargeArc = islarge,
                        IsStroked = true
                    },

                    new PolyLineSegment()
                    {
                        // Начинаем линию с конечной точки дуги, чтобы
                        // точки выстраивались в логичную, последовательную кривую.
                        Points = new PointCollection() { endPoint, centerPoint, startPoint },
                        IsStroked = false
                    }
                }
            };

            return sector;
        }


        /// <summary>
        /// Вычисление угловых значений для каждого сектора.
        /// </summary>
        /// <param name="value">абсолютное значение</param>
        /// <returns></returns>
        private List<StoredValues> СalculateSectorAngle(double value)
        {
            // Получаем абсолютные значения секторов текущего диска, 
            // для вычисления новых угловых размеров секторов 
            // после добавления ещё одного сектора.
            List<StoredValues> listValues = ChartBackground.Children.OfType<Path>().Select(p => (StoredValues)p.Tag).ToList();

            // Добавление в список нового абсолютного значения. 
            StoredValues d = new StoredValues();
            d.Value = value;
            listValues.Add(d);

            // Сортировка по возрастанию значения.
            listValues = listValues.OrderBy(p => p.Value).ToList();

            // Сумма всех абсолютных значений секторов.
            double sum = listValues.Select(p => p.Value).Sum();

            // Общий знаменатель для вычисления градусов поворота
            // и углового смещения для каждого значения.
            double denominator = sum / 360;

            for (int i = 0; i < listValues.Count; i++)
            {
                // Точность угла снижаем до сотых долей,
                // чтобы исключить артефакты при рисовании сектора.
                double degree = Math.Round(listValues[i].Value / denominator, 2);
                listValues[i].Degree = degree;

                double offset = 0;
                if (i > 0)
                {
                    // Угловое смещение следующего сектра формируется на данных
                    // предыдущего.
                    offset = listValues[i - 1].Degree + listValues[i - 1].Offset;
                }

                listValues[i].Offset = offset;
            }

            return listValues;
        }

        #endregion


        /// <summary>
        /// Совокупность хранимых значений в каждом Path-элементе.
        /// </summary>
        internal class StoredValues
        {
            // Угол сектора диска и угловое смещение от
            // предыдущего сектора в градусах.
            public double Degree;
            public double Offset;

            // Абсолютное значение
            public double Value;
        }

    }

}
