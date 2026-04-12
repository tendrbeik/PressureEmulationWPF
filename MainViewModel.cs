using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LiteDB;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PressureEmulationWPF
{
    //TODO: Надо сделать код класса гибче, чтобы можно было изменить одну переменную и на графике отображались бы не 25 последних секунд, а 30 или 10
    //TODO: Надо почистить код класса от мусора.
    internal class MainViewModel : INotifyPropertyChanged
    {
        #region Private-свойства класса
        //TODO: Надо узнать зачем тут вообще Readonly, если мы и так задали
        //модификатор доступа private. Возможно придётся убрать этот модификтор.
        //описание свойств использующихся во вкладке TabItem EmulationTab
        //Описание полей для вкладки TabItem EmulationTab
        private readonly Random _random = new();
        private readonly ObservableCollection<ObservablePoint> _values = new ObservableCollection<ObservablePoint>();
        private readonly ObservableCollection<ObservablePoint> _valuesForDB = new ObservableCollection<ObservablePoint>();
        private double _higherPressureLimit = 150;
        private double _constantPressureValue = 300;
        private double _startPressureValue = 300;
        private double _pressureDelta = 4;

        private bool _randomPressureMode = true;
        private bool _constantPressureMode = false;
        private bool _constantChangingPressureMode = false;

        //Описание полей для вкладки TabItem SaveLastEmulationTab
        private string _emulationName = "Эмуляция";
        private DateTime _emulationDate = DateTime.Now;

        //Описание полей для вкладки TabItem WatchSavedEmulation
        private ObservableCollection<EmulationData> _emulations;
        private EmulationData _selectedEmulation;
        private readonly ObservableCollection<ObservablePoint> _valuesWSE = new ObservableCollection<ObservablePoint>();

        //Описание Action<string> делегата для вывода сообщений об ошибках на форму
        private readonly Action<string> _showError;

        //Токен отмены. Нужен для того, чтобы "убить" асинхронную эмуляцию.
        private CancellationTokenSource _cts;
        #endregion

        #region Геттеры и сеттеры
        //описание геттеров и сеттеров использующихся во вкладке TabItem EmulationTab
        public double HigherPressureLimit
        {
            get { return _higherPressureLimit; }
            set
            {
                if (value <= 0)
                {
                    _showError("Вы неправильно задали верхний предел давления. Он задаётся неотрицательным дробным числом по следующему образцу - \"19.99\"");
                    return;
                }
                _higherPressureLimit = value;
            }
        }

        public double ConstantPressureValue
        {
            get { return _constantPressureValue; }
            set
            {
                if (value <= 0) {
                    _showError("Вы неправильно задали постоянное значение давления. Оно задаётся неотрицательным дробным числом по следующему образцу - \"19.99\"");
                    return; }
                _constantPressureValue = value;
            }
        }

        public double StartPressureValue
        {
            get { return _startPressureValue; }
            set
            {
                if (value <= 0)
                {
                    _showError("Вы неправильно задали стартовое значение давления. Оно задаётся неотрицательным дробным числом по следующему образцу - \"19.99\"");
                    return;
                }
                _startPressureValue = value;
            }
        }

        public double PressureDelta
        {
            get { return _pressureDelta; }
            set
            {
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



        //Описание геттеров и сеттеров для вкладки TabItem SaveLastEmulationTab
        public string EmulationName
        {
            get => _emulationName;
            set
            {
                if (value.Length == 0) return;
                _emulationName = value;
            }
        }
        public DateTime EmulationDate
        {
            get => _emulationDate;
            set => _emulationDate = value;
        }
        public ICommand SaveEmulationCommand { get; }

        //Описание геттеров и сеттеров для вкладки TabItem WatchSavedEmulation
        public ObservableCollection<EmulationData> Emulations
        {
            get => _emulations;
            set
            {
                _emulations = value;
                OnPropertyChanged("Emulations");
            }
        }

        public EmulationData SelectedEmulation
        {
            get => _selectedEmulation;
            set
            {
                if (value == null) return;
                _selectedEmulation = value;
            }
        }

        public ICommand DrawGraphCommand { get; }

        public ObservableCollection<ISeries> SeriesWSE
        {
            get;
            set;
        }
        public List<Axis> XAxesWSE { get; set; }
        public List<Axis> YAxesWSE { get; set; }
        #endregion


        public MainViewModel(Action<string> showError)
        {
            Series = [
                new LineSeries<ObservablePoint>
            {
                Values = _values,
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0
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
                if (IsReading)
                {
                    _showError("В данный момент идёт эмуляция. Завершите эмуляцию и сможете запустить следующую.");
                    return;
                }
                IsReading = true;
                //TODO: потенциальная уязвимость. Надо подумать может ли где-то быть прыдыдущий токен.
                //_cts?.Cancel();
                _cts = new CancellationTokenSource();
                if (RandomPressureMode)
                    _ = RandomPressureEmulation(_cts.Token, _higherPressureLimit, 10, 1000);
                if (ConstantPressureMode)
                    _ = ConstantPressureEmulation(_cts.Token, _constantPressureValue, 10, 1000);
                if (ConstantChangingPressureMode)
                    _ = СonstantChangingPressureEmulation(_cts.Token, _startPressureValue, _pressureDelta, 10, 1000);
            });

            StopCommand = new MyCommand(_ =>
            {
                _cts.Cancel();
                IsReading = false;
            });

            SaveEmulationCommand = new MyCommand(_ =>
            {
                SaveEmulation();
            });

            //Описываем всё что нужно для работы TabItem WatchSavedEmulation
            SeriesWSE = [
                new LineSeries<ObservablePoint>
            {
                Values = _valuesWSE,
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0
            }
            ];

            XAxesWSE = new List<Axis>{
                new Axis()
                {
                    Name = "Время (секунды)",
                    NamePaint = new SolidColorPaint(SKColors.Black),

                    LabelsPaint = new SolidColorPaint(SKColors.Blue),
                    TextSize = 10,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }
                }
            };

            YAxesWSE = new List<Axis>{
                new Axis()
                {
                    Name = "Давление (условные единицы)"
                }
            };

            var emulationsFromDB = LoadEmulationsFromDB();
            _emulations = new ObservableCollection<EmulationData>(emulationsFromDB);
            DrawGraphCommand = new MyCommand(_ =>
            {
                var chartData = ConvertValuesToObservableCollection(_selectedEmulation.Values);
                DrawChart(chartData);
            });

            //инициализируем делегат
            //TODO: Надо припомнить что такое делегат и точнее коммент написать
            _showError = showError;
        }


        #region Режимы эмуляции
        //TODO: Надо разобраться с какой буквы начинать писать имена параметров метода.
        //TODO: Надо подумать стоит ли бить метод ReadData на три подметода. В целом программа и так работать будет, но этот вопрос мне не даёт покоя
        private async Task RandomPressureEmulation(CancellationToken token, double higherPressureLimit, int lastSecondsAmount, int delay)
        {
            // to keep this sample simple, we run the next infinite loop 
            // in a real application you should stop the loop/task when the view is disposed 
            _values.Clear();
            _valuesForDB.Clear();
            double emulationTime = 0;


            while (!token.IsCancellationRequested)
            {
                // Because we are updating the chart from a different thread 
                // we need to use a lock to access the chart data. 
                // this is not necessary if your changes are made on the UI thread. 
                //lock (Sync)
                //{
                var value = _random.NextDouble() * higherPressureLimit;
                _values.Add(new ObservablePoint(emulationTime, value));
                _valuesForDB.Add(new ObservablePoint(emulationTime, value));


                if (_values.Count > lastSecondsAmount * 1000 / delay) _values.RemoveAt(0);

                // we need to update the separators every time we add a new point 
                XAxes[0].CustomSeparators = GetSeparators(emulationTime, lastSecondsAmount, delay);
                emulationTime += delay / 1000;
                //}
                await Task.Delay(delay);
            }
        }

        private async Task ConstantPressureEmulation(CancellationToken token, double constantPressureValue, int lastSecondsAmount, int delay)
        {
            // to keep this sample simple, we run the next infinite loop 
            // in a real application you should stop the loop/task when the view is disposed 
            _values.Clear();
            _valuesForDB.Clear();
            double emulationTime = 0;


            while (!token.IsCancellationRequested)
            {
                //TODO: Перевести английские комментарии
                // Because we are updating the chart from a different thread 
                // we need to use a lock to access the chart data. 
                // this is not necessary if your changes are made on the UI thread. 
                //lock (Sync)
                //{
                _values.Add(new ObservablePoint(emulationTime, constantPressureValue));
                _valuesForDB.Add(new ObservablePoint(emulationTime, constantPressureValue));


                if (_values.Count > lastSecondsAmount * 1000 / delay) _values.RemoveAt(0);

                // we need to update the separators every time we add a new point 
                XAxes[0].CustomSeparators = GetSeparators(emulationTime, lastSecondsAmount, delay);
                emulationTime += delay / 1000;
                //}
                await Task.Delay(delay);
            }
        }

        private async Task СonstantChangingPressureEmulation(CancellationToken token, double startPressureValue, double pressureDelta, int lastSecondsAmount, int delay)
        {
            // to keep this sample simple, we run the next infinite loop 
            // in a real application you should stop the loop/task when the view is disposed 
            _values.Clear();
            _valuesForDB.Clear();
            double emulationTime = 0;
            if (ConstantChangingPressureMode)
            {
                _values.Add(new ObservablePoint(emulationTime, startPressureValue));
                _valuesForDB.Add(new ObservablePoint(emulationTime, startPressureValue));
                emulationTime += delay / 1000d;
                XAxes[0].CustomSeparators = GetSeparators(emulationTime, lastSecondsAmount, delay);
                await Task.Delay(delay);
            }


            while (!token.IsCancellationRequested)
            {
                // Because we are updating the chart from a different thread 
                // we need to use a lock to access the chart data. 
                // this is not necessary if your changes are made on the UI thread. 
                //lock (Sync)
                //{   
                double? val = _values.Last().Y + pressureDelta;
                val = val > 0 ? val : 0;
                _values.Add(new ObservablePoint(emulationTime, val));
                _valuesForDB.Add(new ObservablePoint(emulationTime, val));

                if (_values.Count > lastSecondsAmount * 1000d / delay) _values.RemoveAt(0);

                // we need to update the separators every time we add a new point 
                XAxes[0].CustomSeparators = GetSeparators(emulationTime, lastSecondsAmount, delay);
                emulationTime += delay / 1000d;
                //}
                await Task.Delay(delay);
            }
        }
        #endregion

        #region Методы для работы с LiteDB
        private void SaveEmulation()
        {
            if (IsReading)
            //TODO: Прописать сообщение об ошибке в будущем ERRORMESSAGE
            {
                _showError("В данный момент идёт эмуляция. Завершите эмуляцию и попробуйте сохранить её снова.");
                return;
            }
            using (var db = new LiteDatabase(@"Data.db"))
            {
                var col = db.GetCollection<EmulationData>("Emulations");

                var emulation = new EmulationData
                {
                    Name = _emulationName,
                    Date = _emulationDate,
                    Values = ConvertValuesToMyPoint(_valuesForDB)
                };

                col.EnsureIndex(x => x.Name);
                if (col.Find(x => x.Name.Equals(_emulationName)).Any())
                //TODO: Прописать сообщение об ошибке в будущем ERRORMESSAGE
                {
                    _showError($"Элемент с именем {_emulationName} уже существует в базе данных!");
                    return;
                }

                col.Insert(emulation);

                //EmualtionData result = col.Find(x => x.Name.Equals(_emulationName)).First();
            }
            //Обновляем коллекцию Emulations
            var emulationsFromDB = LoadEmulationsFromDB();
            Emulations = new ObservableCollection<EmulationData>(emulationsFromDB);
        }

        private List<EmulationData> LoadEmulationsFromDB()
        {
            using (var db = new LiteDatabase(@"Data.db"))
            {
                var emulationsCol = db.GetCollection<EmulationData>("Emulations");
                var result = emulationsCol.FindAll().ToList();
                return result;
            }
        }

        private List<MyPoint> ConvertValuesToMyPoint(ObservableCollection<ObservablePoint> values)
        {
            List<MyPoint> resultValues = new List<MyPoint>();
            foreach (var value in values)
            {
                MyPoint point = new MyPoint();
                point.X = value.X;
                point.Y = value.Y;
                resultValues.Add(point);
            }
            return resultValues;
        }

        private ObservableCollection<ObservablePoint> ConvertValuesToObservableCollection(List<MyPoint> values)
        {
            ObservableCollection<ObservablePoint> resultValues = new ObservableCollection<ObservablePoint>();
            foreach (var value in values)
            {
                resultValues.Add(new ObservablePoint(value.X, value.Y));
            }
            return resultValues;
        }
        #endregion

        private void DrawChart(ObservableCollection<ObservablePoint> values)
        {
            if (values == null) return;
            _valuesWSE.Clear();
            foreach (var value in values)
            {
                _valuesWSE.Add(value);
            }
        }
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
