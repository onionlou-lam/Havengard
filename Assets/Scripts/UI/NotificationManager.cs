using UnityEngine;
using System.Collections.Generic;

namespace Havengard.UI.Notifications
{
    /// <summary>
    /// Singleton manager for displaying notifications
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        [Header("Prefab")]
        [SerializeField] private GameObject notificationPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Transform notificationParent;
        [SerializeField] private Vector2 spawnPosition = new Vector2(0f, -100f);
        [SerializeField] private float verticalSpacing = 80f;

        [Header("Queue Settings")]
        [SerializeField] private int maxSimultaneousNotifications = 5;

        private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
        private List<NotificationUI> activeNotifications = new List<NotificationUI>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Auto-find canvas if not assigned
            if (notificationParent == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                    notificationParent = canvas.transform;
            }
        }

        /// <summary>
        /// Show a simple text notification
        /// </summary>
        public void Show(string message)
        {
            Show(new NotificationData(message));
        }

        /// <summary>
        /// Show a notification with type (success, warning, error)
        /// </summary>
        public void Show(string message, NotificationType type)
        {
            Show(new NotificationData(message, type));
        }

        /// <summary>
        /// Show a notification with full customization
        /// </summary>
        public void Show(NotificationData data)
        {
            if (notificationPrefab == null)
            {
                Debug.LogError("[NotificationManager] Notification prefab not assigned!");
                return;
            }

            if (notificationParent == null)
            {
                Debug.LogError("[NotificationManager] Notification parent not assigned!");
                return;
            }

            // Check if we're at max capacity
            if (activeNotifications.Count >= maxSimultaneousNotifications)
            {
                // Queue for later
                notificationQueue.Enqueue(data);
                return;
            }

            SpawnNotification(data);
        }

        /// <summary>
        /// Spawn a notification GameObject
        /// </summary>
        private void SpawnNotification(NotificationData data)
        {
            // Instantiate
            GameObject notificationObj = Instantiate(notificationPrefab, notificationParent);
            NotificationUI notification = notificationObj.GetComponent<NotificationUI>();

            if (notification == null)
            {
                Debug.LogError("[NotificationManager] Notification prefab missing NotificationUI component!");
                Destroy(notificationObj);
                return;
            }

            // Position at spawn point
            RectTransform rectTransform = notificationObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = spawnPosition;
            }

            // Track active notifications
            activeNotifications.Add(notification);

            // Show notification
            notification.Show(data);

            // Clean up when destroyed
            StartCoroutine(CleanupNotification(notification, data.duration + 1f));
        }

        /// <summary>
        /// Clean up notification after it's done
        /// </summary>
        private System.Collections.IEnumerator CleanupNotification(NotificationUI notification, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            activeNotifications.Remove(notification);

            // Process queue if any
            if (notificationQueue.Count > 0)
            {
                NotificationData nextData = notificationQueue.Dequeue();
                SpawnNotification(nextData);
            }
        }

        /// <summary>
        /// Clear all active notifications
        /// </summary>
        public void ClearAll()
        {
            foreach (var notification in activeNotifications)
            {
                if (notification != null)
                    Destroy(notification.gameObject);
            }

            activeNotifications.Clear();
            notificationQueue.Clear();
        }
    }
}