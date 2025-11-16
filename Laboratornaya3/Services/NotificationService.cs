using System.Windows;
using Infrastructure.Services;

namespace Laboratornaya3.Services
{
    public class NotificationService : INotificationService
    {
        public void ShowInfo(string message, string title = null)
        {
            MessageBox.Show(message, title ?? "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string message, string title = null)
        {
            MessageBox.Show(message, title ?? "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public bool Confirm(string message, string title = null)
        {
            var result = MessageBox.Show(message, title ?? "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
