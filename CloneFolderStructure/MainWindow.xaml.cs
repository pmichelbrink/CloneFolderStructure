using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace CloneFolderStructure
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double _fileCount = 0;
        string _logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CloneFolderStructure.log");
        private double _folderCount = 0;
        CancellationTokenSource _tokenSource = new CancellationTokenSource();

        CancellationToken _cancelToken;
        public MainWindow()
        {
            InitializeComponent();

            txtFilecount.Dispatcher.BeginInvoke((Action)(() => txtFilecount.Text = _fileCount.ToString()));
            txtFoldercount.Dispatcher.BeginInvoke((Action)(() => txtFoldercount.Text = _folderCount.ToString()));

            _cancelToken = _tokenSource.Token;
        }

        private async void btnClone_Click(object sender, RoutedEventArgs e)
        {
            txtHistory.Text = string.Empty;

            File.WriteAllText(_logPath, $"Clone started at {DateTime.Now}" + Environment.NewLine);

            DirectoryInfo sourceFolder = new DirectoryInfo(txtSource.Text);
            DirectoryInfo destFolder = new DirectoryInfo(txtDest.Text);

            var task = Task.Run(() =>
            {
                ClonePath(sourceFolder, destFolder);

                string finishedMsg = $"Clone finished at {DateTime.Now}";
                File.AppendAllText(_logPath, finishedMsg);
                MessageBox.Show(finishedMsg);
            }, _tokenSource.Token);

            try
            {
                await task;
            }
            catch (OperationCanceledException ex)
            {
                MessageBox.Show("Cloning canceled.");
                _tokenSource = new CancellationTokenSource();
                _cancelToken = _tokenSource.Token;
            }
        }
        private void ClonePath(DirectoryInfo sourceFolder, DirectoryInfo destFolder)
        {
            try
            {
                if (!sourceFolder.Exists || !destFolder.Exists)
                    return;

                if (_cancelToken.IsCancellationRequested)
                {
                    _cancelToken.ThrowIfCancellationRequested();
                    return;
                }

                ++_folderCount;
                txtFoldercount.Dispatcher.BeginInvoke((Action)(() => txtFoldercount.Text = _folderCount.ToString()));

                txtHistory.Dispatcher.BeginInvoke((Action)(() => txtHistory.Text = $"Cloning {sourceFolder.FullName}{Environment.NewLine}{txtHistory.Text}"));

                sourceFolder.GetFiles().ToList().ForEach(file =>
                {
                    ++_fileCount;
                    txtFilecount.Dispatcher.BeginInvoke((Action)(() => txtFilecount.Text = _fileCount.ToString()));

                    string text = $"File {_fileCount} written at {DateTime.Now}";
                    File.WriteAllText(Path.Combine(destFolder.FullName, Path.ChangeExtension(file.Name, ".txt")), text);
                });

                sourceFolder.GetDirectories().ToList().ForEach(dir =>
                {
                    DirectoryInfo newDir = Directory.CreateDirectory(Path.Combine(destFolder.FullName, dir.Name));
                    ClonePath(dir, newDir);
                });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                File.AppendAllText(_logPath, $"Error at {DateTime.Now}: {ex.Message} {ex.InnerException}" + Environment.NewLine);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _tokenSource.Cancel();
        }

        private void btnSourceBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                //Title = Properties.Resources.SystmFolderLocation,
                //InitialDirectory = currentSystmFolderPath
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                txtSource.Text = dialog.FileName;
        }

        private void btnDestBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                //Title = Properties.Resources.SystmFolderLocation,
                //InitialDirectory = currentSystmFolderPath
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                txtDest.Text = dialog.FileName;
        }
    }
}
