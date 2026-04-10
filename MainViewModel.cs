using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace PressureEmulationWPF
{
    //TODO: Надо сделать код класса гибче, чтобы можно было изменить одну переменную и на графике отображались бы не 25 последних секунд, а 30 или 10
    //TODO: Надо почистить код класса от мусора.
    internal class MainViewModel
    {
        #region Private-свойства класса
        //TODO: Надо узнать зачем тут вообще Readonly, если мы и так задали
        //модификатор доступа private. Возможно придётся убрать этот модификтор.
        private readonly Random _random = new();
        private readonly List<ObservablePoint> _values = [];
        private double _higherPressureLimit = 150;
        private double _constantPressureValue = 300;
        private double _startPressureValue = 300;
        private double _pressureDelta = 4;

        private bool _randomPressureMode = true;
        private bool _constantPressureMode = false;
        private bool _constantChangingPressureMode = false;
        #endregion

        #region Геттеры и сеттеры
        public double HigherPressureLimit
        {
            get { return _higherPressureLimit; }
            set
            {
                if (value <= 0) return;
                _higherPressureLimit = value;
            }
        }

        public double ConstantPressureValue
        {
            get { return _constantPressureValue; }
            set
            {
                if (value <= 0) return;
                _constantPressureValue = value;
            }
        }

        public double StartPressureValue
        {
            get { return _startPressureValue; }
            set
            {
                if (value <= 0) return;
                _startPressureValue = value;
            }
        }

        public double PressureDelta
        {
            get { return _pressureDelta; }
            set
            {
                //if () return;
                _pressureDelta = value;
            }
        }

        public bool RandomPressureMode
        {
            get => _randomPressureMode;
            set => _randomPressureMode = value;
        }

        public bool ConstantPressureMode
        {
            get => _constantPressureMode;
            set => _constantPressureMode = value;
        }

        public bool ConstantChangingPressureMode
        {
            get => _constantChangingPressureMode;
            set => _constantChangingPressureMode = value;
        }
        public ObservableCollection<ISeries> Series { get; set; }
        public List<Axis> XAxes { get; set; }
        public List<Axis> YAxes { get; set; }
        //public object Sync { get; } = new object();
        public bool IsReading { get; set; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        #endregion


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

            XAxes = new List<Axis>{
                new Axis()
                {
                    Name = "Время (секунды)",
                    NamePaint = new SolidColorPaint(SKColors.Black),

                    LabelsPaint = new SolidColorPaint(SKColors.Blue),
                    TextSize = 10,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 },
                    CustomSeparators = GetSeparators(0, 10, 1000)
                }
            };

            YAxes = new List<Axis>{
                new Axis()
                {
                    Name = "Давление (условные единицы)"
                }
            };

            StartCommand = new MyCommand(_ =>
            {
                if (IsReading) return;
                IsReading = true;
                if (RandomPressureMode)
                    _ = RandomPressureEmulation(_higherPressureLimit, 10, 1000);
                if (ConstantPressureMode)
                    _ = ConstantPressureEmulation(_constantPressureValue, 10, 1000);
                if (ConstantChangingPressureMode)
                    _ = СonstantChangingPressureEmulation(_startPressureValue, _pressureDelta, 10, 1000);
            });

            StopCommand = new MyCommand(_ =>
            {
                IsReading = false;
            });
        }


        #region Режимы эмуляции
        //TODO: Надо разобраться с какой буквы начинать писать имена параметров метода.
        //TODO: Надо подумать стоит ли бить метод ReadData на три подметода. В целом программа и так работать будет, но этот вопрос мне не даёт покоя
        private async Task RandomPressureEmulation(double higherPressureLimit, int lastSecondsAmount, int delay)
        {
            // to keep this sample simple, we run the next infinite loop 
            // in a real application you should stop the loop/task when the view is disposed 
            _values.Clear();
            double emulationTime = 0;


            while (IsReading)
            {
                // Because we are updating the chart from a different thread 
                // we need to use a lock to access the chart data. 
                // this is not necessary if your changes are made on the UI thread. 
                //lock (Sync)
                //{
                _values.Add(new ObservablePoint(emulationTime, _random.NextDouble() * higherPressureLimit));


                if (_values.Count > lastSecondsAmount * 1000 / delay) _values.RemoveAt(0);

                // we need to update the separators every time we add a new point 
                XAxes[0].CustomSeparators = GetSeparators(emulationTime, lastSecondsAmount, delay);
                emulationTime += delay / 1000;
                //}
                await Task.Delay(delay);
            }
        }

        private async Task ConstantPressureEmulation(double constantPressureValue, int lastSecondsAmount, int delay)
        {
            // to keep this sample simple, we run the next infinite loop 
            // in a real application you should stop the loop/task when the view is disposed 
            _values.Clear();
            double emulationTime = 0;


            while (IsReading)
            {
                //TODO: Перевести английские комментарии
                // Because we are updating the chart from a different thread 
                // we need to use a lock to access the chart data. 
                // this is not necessary if your changes are made on the UI thread. 
                //lock (Sync)
                //{
                _values.Add(new ObservablePoint(emulationTime, constantPressureValue));


                if (_values.Count > lastSecondsAmount * 1000 / delay) _values.RemoveAt(0);

                // we need to update the separators every time we add a new point 
                XAxes[0].CustomSeparators = GetSeparators(emulationTime, lastSecondsAmount, delay);
                emulationTime += delay / 1000;
                //}
                await Task.Delay(delay);
            }
        }

        private async Task СonstantChangingPressureEmulation(double startPressureValue, double pressureDelta, int lastSecondsAmount, int delay)
        {
            // to keep this sample simple, we run the next infinite loop 
            // in a real application you should stop the loop/task when the view is disposed 
            _values.Clear();
            double emulationTime = 0;
            if (ConstantChangingPressureMode)
            {
                _values.Add(new ObservablePoint(emulationTime, startPressureValue));
                emulationTime += delay / 1000d;
                await Task.Delay(delay);
            }


            while (IsReading)
            {
                // Because we are updating the chart from a different thread 
                // we need to use a lock to access the chart data. 
                // this is not necessary if your changes are made on the UI thread. 
                //lock (Sync)
                //{   
                double? val = _values.Last().Y + pressureDelta;
                _values.Add(new ObservablePoint(emulationTime, val > 0 ? val : 0));


                if (_values.Count > lastSecondsAmount * 1000d / delay) _values.RemoveAt(0);

                // we need to update the separators every time we add a new point 
                XAxes[0].CustomSeparators = GetSeparators(emulationTime, lastSecondsAmount, delay);
                emulationTime += delay / 1000d;
                //}
                await Task.Delay(delay);
            }
        }
        #endregion


        private double[] GetSeparators(double emulationTime, double lastSecondsAmount, int delay)
        {
            int sepsAmount = (int)(lastSecondsAmount * 1000d / (double)delay);
            double[] seps = new double[sepsAmount];
            for (int i = 0; i < seps.Length; i++)
            {
                seps[i] = emulationTime - delay * i / 1000d;
            }
            return seps;
        }
    }
}
