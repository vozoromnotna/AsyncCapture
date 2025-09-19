using System.Windows;

namespace AsyncCapture.Wpf.Hist
{
    /// <summary>
    /// Логика взаимодействия для HistWindow.xaml
    /// </summary>
    public partial class HistWindow : Window
    {
        HistVM vm;
        public HistWindow(HistFilter filter)
        {
            
            InitializeComponent();

            vm = new HistVM(filter, WP_Plot);
            this.DataContext = vm;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.Dispose();
            vm = null;
        }
    }
}
