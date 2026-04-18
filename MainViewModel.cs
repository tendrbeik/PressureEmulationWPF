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
using System.IO;
using System.Text.Json;
using FluentModbus;

namespace PressureEmulationWPF
{
    //TODO: Надо сделать код класса гибче, чтобы можно было изменить одну переменную и на графике отображались бы не 25 последних секунд, а 30 или 10
    internal class MainViewModel : INotifyPropertyChanged
    {
        #region Private-свойства класса
        //TODO: Надо посмотреть, где тут реально нужен модификатор readonly и оставить только там, где нужен
        private readonly string _configFileName = "config.json";
        //Описание полей для вкладки TabItem EmulationTab
        private readonly Random _random = new();
        private readonly ObservableCollection<ObservablePoint> _values = new ObservableCollection<ObservablePoint>();
        private readonly ObservableCollection<ObservablePoint> _valuesForDB = new ObservableCollection<ObservablePoint>();
        private double _upperPressureLimit = 150;
        private double _constantPressureValue = 300;
        private double _startPressureValue = 300;
        private double _pressureDelta = 4;

        private bool _randomPressureMode = true;
        private bool _constantPressureMode = false;
        private bool _constantChangingPressureMode = false;

        //Описание полей для вкладки TabItem SaveLastEmulationTab
        private string _emulationName = "Эмуляция";
        private DateTime _emulationDate = DateTime.Now;

        //Описание полей для вкладки TabItem WatchSavedEmulationTab
        private ObservableCollection<EmulationData> _emulations;
        private EmulationData _selectedEmulation;
        private readonly ObservableCollection<ObservablePoint> _valuesWSE = new ObservableCollection<ObservablePoint>();

        //Описание полей для вкладки TabItem ModbusSlave
        private string _slaveIP = "127.0.0.1";
        private string _slavePort = "502";
        private string _slaveID = "1";
        private ModbusTcpClient _client = new ModbusTcpClient();

        //Описание Action<string> делегата для вывода сообщений об ошибках на форму
        private readonly Action<string> _showError;

        //Токен отмены. Нужен для того, чтобы "убить" асинхронную эмуляцию.
        private CancellationTokenSource _cts;
        #endregion

        #region Геттеры и сеттеры
        //описание геттеров и сеттеров использующихся во вкладке TabItem EmulationTab
        public double UpperPressureLimit
        {
            get { return _upperPressureLimit; }
            set
            {
                _upperPressureLimit = value;
                OnPropertyChanged("UpperPressureLimit");
            }
        }

        public double ConstantPressureValue
        {
            get { return _constantPressureValue; }
            set
            {
                _constantPressureValue = value;
                OnPropertyChanged("ConstantPressureValue");
            }
        }

        public double StartPressureValue
        {
            get { return _startPressureValue; }
            set
            {
                _startPressureValue = value;
                OnPropertyChanged("StartPressureValue");
            }
        }

        public double PressureDelta
        {
            get { return _pressureDelta; }
            set
            {
                _pressureDelta = value;
                OnPropertyChanged("PressureDelta");
            }
        }

        public bool RandomPressureMode
        {
            get => _randomPressureMode;
            set
            {
                _randomPressureMode = value;
                OnPropertyChanged("RandomPressureMode");
            }
        }

        public bool ConstantPressureMode
        {
            get => _constantPressureMode;
            set
            {
                _constantPressureMode = value;
                OnPropertyChanged("ConstantPressureMode");
            }
        }

        public bool ConstantChangingPressureMode
        {
            get => _constantChangingPressureMode;
            set
            {
                _constantChangingPressureMode = value;
                OnPropertyChanged("ConstantChangingPressureMode");
            }
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
                _emulationName = value;
                OnPropertyChanged("EmulationName");
            }
        }
        public DateTime EmulationDate
        {
            get => _emulationDate;
            set
            {
                _emulationDate = value;
                OnPropertyChanged("EmulationDate");
            }
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
            //TODO: Надо проверить есть ли проверка на NULL где-нибудь в другом месте. Если есть, то тут надо убрать.
            get => _selectedEmulation;
            set
            {
                if (value == null) return;
                _selectedEmulation = value;
                OnPropertyChanged("SelectedEmulation");
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

        //Описание геттеров и сеттеров для вкладки TabItem ModbusSlave
        public string SlaveIP
        {
            get => _slaveIP;
            set
            {
                _slaveIP = value;
                OnPropertyChanged("SlaveIP");
            }
        }

        public string SlavePort
        {
            get => _slavePort;
            set
            {
                _slavePort = value;
                OnPropertyChanged("SlavePort");
            }
        }

        public string SlaveID
        {
            get => _slaveID;
            set
            {
                _slaveIP = value;
                OnPropertyChanged("SlaveID");
            }
        }
        #endregion


        public MainViewModel(Action<string> showError)
        {
            //При инициализации класса подгружаем значения из конфигурационного JSON файла
            ReadLastUserInputsFromJSON();

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
                    _ = RandomPressureEmulation(_cts.Token, _upperPressureLimit, 10, 1000);
                if (ConstantPressureMode)
                    _ = ConstantPressureEmulation(_cts.Token, _constantPressureValue, 10, 1000);
                if (ConstantChangingPressureMode)
                    _ = СonstantChangingPressureEmulation(_cts.Token, _startPressureValue, _pressureDelta, 10, 1000);
            });

            StopCommand = new MyCommand(_ =>
            {
                if (_cts == null)
                {
                    _showError("Нечего останавливать. Эмуляция не запущена.");
                    return;
                }
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
            _showError = showError;
        }


        #region Режимы эмуляции
        //TODO: Надо подумать, как лучше писать методы вроде трёх следующих. Они частично дублируют друг друга, но если их объединить, то
        //каждую итерацию цикла придётся выполнять лишний if или switch case. Короче надо искать решение для данной ситуации.
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
                _values.Add(new ObservablePoint(emulationTime, constantPressureValue));
                _valuesForDB.Add(new ObservablePoint(emulationTime, constantPressureValue));


                if (_values.Count > lastSecondsAmount * 1000 / delay) _values.RemoveAt(0);

                //Каждый раз, когда мы добавляем точку на график надо обновлять сепараторы.
                XAxes[0].CustomSeparators = GetSeparators(emulationTime, lastSecondsAmount, delay);
                emulationTime += delay / 1000;
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
            {
                _showError("В данный момент идёт эмуляция. Завершите эмуляцию и попробуйте сохранить её снова.");
                return;
            }
            if (_valuesForDB.Count == 0)
            {
                _showError("Коллекция точек графика пуста, поэтому сохранять нечего. Попробуйте сначала провести эмуляцию и потом сохраните её.");
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
        public void SaveUserInputsToJSON()
        {
            using (FileStream fs = new FileStream(_configFileName, FileMode.Create))
            {
                var configData = new ConfigData()
                {
                    UpperPressureLimit = UpperPressureLimit,
                    ConstantPressureValue = ConstantPressureValue,
                    PressureDelta = PressureDelta,
                    StartPressureValue = StartPressureValue,
                    EmulationName = EmulationName,
                    EmulationDateTime = EmulationDate,
                    RandomPressureMode = RandomPressureMode,
                    ConstantChangingPressureMode = ConstantChangingPressureMode,
                    ConstantPressureMode = ConstantPressureMode,
                };
                System.Text.Json.JsonSerializer.Serialize(fs, configData);
            }
        }

        private void ReadLastUserInputsFromJSON()
        {
            ConfigData? data;
            using (FileStream fs = new FileStream(_configFileName, FileMode.OpenOrCreate))
            {
                try
                {
                    data = System.Text.Json.JsonSerializer.Deserialize<ConfigData>(fs);
                }
                catch (JsonException)
                {
                    return;
                }
                if (data != null)
                {
                    UpperPressureLimit = data.UpperPressureLimit;
                    ConstantPressureValue = data.ConstantPressureValue;
                    PressureDelta = data.PressureDelta;
                    StartPressureValue = data.StartPressureValue;
                    EmulationName = data.EmulationName;
                    EmulationDate = data.EmulationDateTime;
                    ConstantPressureMode = data.ConstantPressureMode;
                    ConstantChangingPressureMode = data.ConstantChangingPressureMode;
                    RandomPressureMode = data.RandomPressureMode;
                }
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
