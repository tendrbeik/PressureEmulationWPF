using PressureEmulationWPF.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace PressureEmulationWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel? _vm => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            var MVM = new MainViewModel(
                showError: msg => MessageBox.Show(
                    msg,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    )
                );
            DataContext = MVM;
            MVM.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MVM.UpperPressureLimit) ||
                e.PropertyName == nameof(MVM.ConstantPressureValue) ||
                e.PropertyName == nameof(MVM.PressureDelta) ||
                e.PropertyName == nameof(MVM.StartPressureValue) ||
                e.PropertyName == nameof(MVM.EmulationName) ||
                e.PropertyName == nameof(MVM.EmulationDate) ||
                e.PropertyName == nameof(MVM.ConstantPressureMode) ||
                e.PropertyName == nameof(MVM.ConstantChangingPressureMode) ||
                e.PropertyName == nameof(MVM.RandomPressureMode))
                    MVM.SaveUserInputsToJSON();
            };
        }

        private void PositiveDoubleParse_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                MessageBox.Show(e.Error.ErrorContent.ToString());
            }
        }

        private void SaveLastEmulation_Click(object sender, RoutedEventArgs e)
        {
            var errors = Validation.GetHasError(EmulationName) ||
                Validation.GetHasError(EmulationDate);

            if (errors)
            {
                MessageBox.Show("Исправьте ошибки в полях ввода имени эмуляции и её даты");
                return;
            }

            if (_vm?.SaveEmulationCommand?.CanExecute(null) == true)
            {
                _vm.SaveEmulationCommand.Execute(null);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool RPM = (bool)RandomPressureMode.IsChecked;
                bool CPM = (bool)ConstantPressureMode.IsChecked;
                bool CCPM = (bool)ConstantChangingPressureMode.IsChecked;
                if (RPM && Validation.GetHasError(UpperPressureLimit))
                {
                    MessageBox.Show("Исправьте ошибку в поле ввода верхнего предела давления, чтобы запустить эмуляцию случайного давления.");
                    return;
                }
                if (CPM && Validation.GetHasError(ConstantPressureValue))
                {
                    MessageBox.Show("Исправьте ошибку в поле ввода постоянного значения давления, чтобы запустить эмуляцию постоянного давления.");
                    return;
                }
                if (CCPM &&
                    (Validation.GetHasError(StartPressureValue) ||
                    Validation.GetHasError(PressureDelta)))
                {
                    MessageBox.Show("Исправьте ошибку в полях ввода стартового значения давления и шага изменения давления, чтобы запустить эмуляцию постоянно убывающего/растущего давления.");
                    return;
                }
            }
            catch (Exception ex)
            {
                //Я пока не уверен, что ошибка тут вообще может выпрыгнуть, поэтому пока не буду её специально обрабатывать
                MessageBox.Show(ex.ToString());
            }

            if (_vm?.StartCommand?.CanExecute(null) == true)
            {
                _vm.StartCommand.Execute(null);
            }
        }

        private void MSConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var errors = Validation.GetHasError(SlaveIP) ||
                Validation.GetHasError(SlavePort) ||
                Validation.GetHasError(SlaveID) ||
                Validation.GetHasError(InputRegisterAddress);

            if (errors)
            {
                MessageBox.Show("Исправьте ошибки в полях ввода для подключения к Modbus Slave");
                return;
            }
            //TODO: Сделать валидацию четырёх полей тут.
            if (_vm?.ConnectToSlaveCommand?.CanExecute(null) == true)
            {
                _vm.ConnectToSlaveCommand.Execute(null);
            }
        }
    }
}