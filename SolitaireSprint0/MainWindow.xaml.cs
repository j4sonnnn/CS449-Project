using System.Windows;

namespace SolitaireSprint0
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            string mode = rbA.IsChecked == true ? "A" : "B";
            txtStatus.Text = $"Mode: {mode} | Popup: {chkShowPopup.IsChecked}";

            if (chkShowPopup.IsChecked == true)
                MessageBox.Show($"Mode {mode} selected.");
        }
    }
}
