using NAudio.Wave;
using System;
using System.IO;
using System.Windows;

namespace enregistrement
{
    public partial class MainWindow : Window
    {
        private WaveInEvent waveIn;
        private WaveFileWriter writer;
        private string outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\enregistrement\\";

        public MainWindow()
        {
            InitializeComponent();
            stopButton.IsEnabled = false;
            LoadRecordingDevices();
        }

        private void LoadRecordingDevices()
        {
            for (int deviceId = 0; deviceId < WaveIn.DeviceCount; deviceId++)
            {
                var deviceInfo = WaveIn.GetCapabilities(deviceId);
                comboBoxDevices.Items.Add($"{deviceInfo.ProductName}");
            }

            if (comboBoxDevices.Items.Count > 0)
            {
                comboBoxDevices.SelectedIndex = 0; // Par défaut, sélectionnez le premier périphérique
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            // Assurez-vous que le dossier d'enregistrement existe
            Directory.CreateDirectory(outputDirectory);

            // Générer un chemin de fichier unique
            string outputFilePath = GenerateUniqueFilePath();

            waveIn = new WaveInEvent
            {
                DeviceNumber = comboBoxDevices.SelectedIndex, // Utilisez le périphérique sélectionné
                WaveFormat = new WaveFormat(44100, 1)
            };
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;
            writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);

            waveIn.StartRecording();
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            statusLabel.Content = "Enregistrement en cours...";
        }

        private string GenerateUniqueFilePath()
        {
            int fileNumber = 1;
            string filePath;
            do
            {
                filePath = Path.Combine(outputDirectory, $"enregistrement{fileNumber}.wav");
                fileNumber++;
            } while (File.Exists(filePath));

            return filePath;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            // Arrêtez d'abord l'enregistrement
            waveIn.StopRecording();

            // Ensuite, disposez les ressources de l'enregistrement
            DisposeRecordingResources();

            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            statusLabel.Content = "Enregistrement terminé.";
        }

        private void OnDataAvailable(object sender, WaveInEventArgs args)
        {
            writer.Write(args.Buffer, 0, args.BytesRecorded);
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs args)
        {
            // Disposez les ressources ici si l'enregistrement s'arrête inopinément
            DisposeRecordingResources();

            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            statusLabel.Content = "Enregistrement arrêté inopinément.";
        }

        private void DisposeRecordingResources()
        {
            writer?.Dispose();
            writer = null;
            waveIn?.Dispose();
            waveIn = null;
        }
    }
}
