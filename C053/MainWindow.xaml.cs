using Microsoft.Win32;
using System.Linq;
using System.Windows;
using System.IO;

namespace C053
{
    public class Transaction
    {
        public string Betrag { get; set; } = string.Empty;
        public string Waehrung { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Strasse { get; set; } = string.Empty;
        public string Nummer { get; set; } = string.Empty;
        public string PLZ { get; set; } = string.Empty;
        public string Stadt { get; set; } = string.Empty;
        public string Land { get; set; } = string.Empty;
        public string Kommentar { get; set; } = string.Empty;
        public string Konto_IBAN { get; set; } = string.Empty;
        public string Konto_Bank { get; set; } = string.Empty;
        public string Konto_Adresse { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path;
        public MainWindow()
        {
            InitializeComponent();
            path = string.Empty;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ".tar.gz Dateien (*.tar.gz)|*.tar.gz";

            if (openFileDialog.ShowDialog() == true)
                path = openFileDialog.FileName;

            LoadFile(path);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[]? droppedFiles = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }

            if ((null == droppedFiles) || (!droppedFiles.Any())) { return; }
            if (droppedFiles.Length > 1) { MessageBox.Show(this, "Bitte nur eine Datei gleichzeitig ins Programm ziehen.", "Drag and Drop Fehler", MessageBoxButton.OK, MessageBoxImage.Error); return; };

            path = droppedFiles[0];

            LoadFile(path);
        }

        private void LoadFile(string path)
        {
            if (!TransactionLoader.IsValidPath(path))
                MessageBox.Show(this, "Ungültige Datei, bitte eine .tar.gz Datei auswählen.", "Datei ungültig", MessageBoxButton.OK, MessageBoxImage.Error);

            Title = "C053 - " + Path.GetFileName(path);

            var transactions = TransactionLoader.LoadCamtFile(path);
            dataGrid.ItemsSource = transactions;

            helpLabel.Visibility = Visibility.Collapsed;
        }
    }
}