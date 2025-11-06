using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using System;
using Laboratornaya3.Views;

namespace Laboratornaya3
{
    public partial class App : Application
    {
        private MediaPlayer startupMusicPlayer; // Для заставки (4 секунды)
        private MediaPlayer backgroundMusicPlayer; // Для фоновой музыки

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Запускаем музыку заставки
            PlayStartupMusic();

            // Запускаем фоновую музыку (после заставки)
            Task.Run(async () =>
            {
                await Task.Delay(4000); // Ждем окончания заставки
                Dispatcher.Invoke(() => PlayBackgroundMusic());
            });

            // Показываем заставку
            var splashScreen = new SplashScreenWindow();
            splashScreen.Show();

            Task.Run(async () =>
            {
                await Task.Delay(3000);

                Dispatcher.Invoke(() =>
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    splashScreen.Close();
                });
            });
        }

        private void PlayStartupMusic()
        {
            try
            {
                startupMusicPlayer = new MediaPlayer();
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string audioPath = System.IO.Path.Combine(basePath, "Audio", "startup.mp3");

                startupMusicPlayer.Open(new Uri(audioPath));
                startupMusicPlayer.Volume = 0.13;
                startupMusicPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка стартовой музыки: {ex.Message}");
            }
        }

        private void PlayBackgroundMusic()
        {
            try
            {
                backgroundMusicPlayer = new MediaPlayer();
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string audioPath = System.IO.Path.Combine(basePath, "Audio", "background.mp3"); 

                backgroundMusicPlayer.Open(new Uri(audioPath));
                backgroundMusicPlayer.Volume = 0.05; 
                backgroundMusicPlayer.MediaEnded += BackgroundMusic_MediaEnded; // Зацикливание
                backgroundMusicPlayer.Play();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка фоновой музыки: {ex.Message}");
            }
        }

        // Зацикливание фоновой музыки
        private void BackgroundMusic_MediaEnded(object sender, EventArgs e)
        {
            backgroundMusicPlayer.Position = TimeSpan.Zero;
            backgroundMusicPlayer.Play();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Останавливаем всю музыку при выходе
            startupMusicPlayer?.Stop();
            startupMusicPlayer?.Close();
            backgroundMusicPlayer?.Stop();
            backgroundMusicPlayer?.Close();
            base.OnExit(e);
        }
    }
}