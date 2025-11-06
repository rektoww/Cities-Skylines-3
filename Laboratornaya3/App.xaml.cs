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

                // Путь к папке проекта + Audio (3 уровня вверх из bin/Debug/)
                string projectPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
                string audioPath = System.IO.Path.Combine(projectPath, "Audio", "startup.mp3");

                startupMusicPlayer.Open(new Uri(audioPath));
                startupMusicPlayer.Volume = 0.8;
                startupMusicPlayer.Play();
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        private void PlayBackgroundMusic()
        {
            try
            {
                backgroundMusicPlayer = new MediaPlayer();

                // Путь к папке проекта + Audio (3 уровня вверх из bin/Debug/)
                string projectPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
                string audioPath = System.IO.Path.Combine(projectPath, "Audio", "background.mp3");

                backgroundMusicPlayer.Open(new Uri(audioPath));
                backgroundMusicPlayer.Volume = 0.3;
                backgroundMusicPlayer.MediaEnded += BackgroundMusic_MediaEnded;
                backgroundMusicPlayer.Play();
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

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