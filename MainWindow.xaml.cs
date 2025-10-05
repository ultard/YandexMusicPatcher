using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using YandexMusicPatcher.Constants;
using YandexMusicPatcher.Models;
using YandexMusicPatcher.Services;

namespace YandexMusicPatcher;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var config = ConfigService.LoadConfig();
                TxtYmPath.Text = string.IsNullOrWhiteSpace(config.YmExePath) || !File.Exists(config.YmExePath) ? Paths.YmExePath : config.YmExePath;
        ComboBoxReleaseChannel.SelectedIndex = (int)config.ReleaseChannel;
        CheckBoxKeepAsar.IsChecked = config.KeepAsarFile;
    }

    private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выберите Яндекс Музыка.exe",
            Filter = "Executables|*.exe"
        };

        if (dlg.ShowDialog() != true) return;

        ConfigService.UpdateField("YmExePath", dlg.FileName);
        TxtYmPath.Text = Path.GetFullPath(dlg.FileName);
    }

    private void ButtonBrowseClear_Click(object sender, RoutedEventArgs e)
    {
        ConfigService.UpdateField("YmExePath", string.Empty);
        TxtYmPath.Text = string.Empty;
    }
    
    private void ComboBoxReleaseChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxReleaseChannel.SelectedIndex == -1) return;
        ConfigService.UpdateField("ReleaseChannel", (ReleaseChannel)ComboBoxReleaseChannel.SelectedIndex);
    }
    
    private void CheckBoxKeepAsar_Changed(object sender, RoutedEventArgs e)
    {
        ConfigService.UpdateField("KeepAsarFile", CheckBoxKeepAsar.IsChecked ?? false);
    }
    
    private async void ButtonInstall_Click(object sender, RoutedEventArgs e)
    {
        var config = ConfigService.LoadConfig();
        
        ProgressText.Text = "Инициализация";
        ProgressBar.Value = 0;
        
        var ymExePath = string.IsNullOrEmpty(config.YmExePath) ? Paths.YmExePath : config.YmExePath;
        if (!File.Exists(ymExePath))
        {
            MessageBox.Show("Путь к Яндекс Музыка.exe не указан или файла не существует.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
        
        ProgressText.Text = "Получение метаданных";

        var metadata = await DownloadService.GetMetadata();
        if (metadata?.Assets == null || metadata.Assets.Length == 0)
        {
            MessageBox.Show("Не удалось получить метаданные для загрузки.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            ProgressBar.Value = 0;
            return;
        }
        
        ProgressText.Text = "Загрузка и распаковка патча";
        ProgressBar.Value += 1;
        
        if (!Directory.Exists(Paths.TmpPath))
        {
            Directory.CreateDirectory(Paths.TmpPath);
        }
        
        var asarFileName = string.Empty;
        switch (config.ReleaseChannel)
        {
            case ReleaseChannel.Full:
                asarFileName = "app.asar";
                break;
            case ReleaseChannel.DevTools:
                asarFileName = "appDevTools.asar";
                break;
        }

        var modedAsarPath = Path.Combine(Paths.TmpPath, asarFileName);
        var archivedAsarName = asarFileName + ".gz";
        var archivedAsarPath = modedAsarPath + ".gz";
        
        foreach (var asset in metadata.Assets)
        {
            if (asset.Name != archivedAsarName) continue;
            
            await DownloadService.DownloadFileAsync(asset.BrowserDownloadUrl, archivedAsarPath);
            DownloadService.DecompressGz(archivedAsarPath, modedAsarPath);
            File.Delete(archivedAsarPath);
            
            break;
        }
        
        ProgressText.Text = "Установка";
        ProgressBar.Value += 1;

        var success = PatchService.InstallPatch(modedAsarPath);
        if (!success)
        {
            MessageBox.Show("Не удалось установить патч.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            ProgressBar.Value = 0;
            return;
        }
        
        if (!config.KeepAsarFile && File.Exists(modedAsarPath))
        {
            File.Delete(modedAsarPath);
        }
        
        ProgressText.Text = "Готово";
        ProgressBar.Value += 1;
    }

    private void ButtonModRemove_Click(object sender, RoutedEventArgs e)
    {
        var config = ConfigService.LoadConfig();
        
        ProgressText.Text = "Поиск резервной копии";
        ProgressBar.Value = 0;
        
        var ymExePath = string.IsNullOrEmpty(config.YmExePath) ? Paths.YmExePath : config.YmExePath;
        if (!File.Exists(ymExePath))
        {
            MessageBox.Show("Путь к Яндекс Музыка.exe не указан или файла не существует.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
        
        if (!Directory.Exists(Paths.TmpPath))
        {
            MessageBox.Show("Папка с резервной копией не найдена.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var success = PatchService.UninstallPatch();
        if (!success)
        {
            MessageBox.Show("Не удалось удалить патч. Возможно, резервная копия не найдена.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
        
        ProgressText.Text = "Готово";
        ProgressBar.Value = 3;
    }
}