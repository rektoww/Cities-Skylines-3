namespace Infrastructure.Services
{
    public interface INotificationService
    {
        void ShowInfo(string message, string title = null);
        void ShowWarning(string message, string title = null);
        bool Confirm(string message, string title = null);
    }
}