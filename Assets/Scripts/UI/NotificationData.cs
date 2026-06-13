using UnityEngine;

namespace Havengard.UI.Notifications
{
    /// <summary>
    /// Data for displaying a notification
    /// </summary>
    public class NotificationData
    {
        public string message;
        public Sprite icon;
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        public Color textColor = Color.white;
        public float duration = 2f;
        public NotificationType type = NotificationType.Info;

        public NotificationData(string message)
        {
            this.message = message;
        }

        public NotificationData(string message, NotificationType type)
        {
            this.message = message;
            this.type = type;
            ApplyTypeDefaults();
        }

        private void ApplyTypeDefaults()
        {
            switch (type)
            {
                case NotificationType.Success:
                    backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);
                    break;
                case NotificationType.Warning:
                    backgroundColor = new Color(0.9f, 0.7f, 0.2f, 0.9f);
                    break;
                case NotificationType.Error:
                    backgroundColor = new Color(0.9f, 0.2f, 0.2f, 0.9f);
                    break;
                case NotificationType.Info:
                    backgroundColor = new Color(0.2f, 0.5f, 0.9f, 0.9f);
                    break;
            }
        }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}