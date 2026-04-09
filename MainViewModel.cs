using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace PressureEmulationWPF
{
    //TODO: Надо сделать код класса гибче, чтобы можно было изменить одну переменную и на графике отображались бы не 25 последних секунд, а 30 или 10
    //TODO: Надо почистить код класса от мусора.
    internal class MainViewModel
    {
        //TODO: Надо узнать зачем тут вообще Readonly, если мы и так задали
        //модификатор доступа private. Возможно придётся убрать этот модификтор.
        private readonly Random _random = new();
        private readonly List<ObservablePoint> _values = [];
        //private Axis[] _customAxis;
        //private readonly DateTimeAxis _customAxis;
        private double _emulationTime;

        public ObservableCollection<ISeries> Series { get; set; }

        public List<Axis> XAxes { get; set; }

        public List<Axis> YAxes { get; set; }

        public object Sync { get; } = new object();

        public bool IsReading { get; set; }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public MainViewModel()
        {
            Series = [
                new LineSeries<ObservablePoint>
            {
                Values = _values,
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null
            }
            ];

            //_customAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            //{
            //    CustomSeparators = GetSeparators(),
            //    AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            //    SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(100))
            //};
            XAxes = new List<Axis>{
                new Axis()
                {
                    Name = "Время (секунды)",
                    NamePaint = new SolidColorPaint(SKColors.Black),

                    LabelsPaint = new SolidColorPaint(SKColors.Blue),
                    TextSize = 10,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 },
                    CustomSeparators = GetSeparators()
                }
            };

            YAxes = new List<Axis>{
                new Axis()
                {
                    Name = "Давление (условные единицы)"
                }
            };


            //XAxis = [_customAxis];

            StartCommand = new MyCommand(_ =>
            {
                IsReading = true;
                _ = ReadData();
            });

            StopCommand = new MyCommand(_ =>
            {
                IsReading = false;
            });
        }

        private async Task ReadData()
        {
            // to keep this sample simple, we run the next infinite loop 
            // in a real application you should stop the loop/task when the view is disposed 
            _values.Clear();
            _emulationTime = 0;

            while (IsReading)
            {
                await Task.Delay(1000);

                // Because we are updating the chart from a different thread 
                // we need to use a lock to access the chart data. 
                // this is not necessary if your changes are made on the UI thread. 
                lock (Sync)
                {
                    _values.Add(new ObservablePoint(_emulationTime, _random.Next(0, 10)));
                    //_values.Add(new DateTimePoint(DateTime.Now, 300));
                    //_values.Add(new DateTimePoint(DateTime.Now, _values.Last().Value + 1));

                    if (_values.Count > 25) _values.RemoveAt(0);

                    // we need to update the separators every time we add a new point 
                    XAxes[0].CustomSeparators = GetSeparators();
                    _emulationTime += 1;
                }
            }
        }

        private double[] GetSeparators()
        {
            double[] seps = new double[25];
            for (int i = 0; i < seps.Length; i++)
            {
                seps[i] = _emulationTime - i;
            }
            return seps;
        }
    }
}
