using System;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Quests
{
    public enum QuestProgress { None, Accepted, InProgress, ReadyToClaim, Complete }

    /// <summary>Quests the player has unlocked and can send heroes on. Same singleton pattern as GoldSystem.</summary>
    public class ExpeditionLog : MonoBehaviour
    {
        public static ExpeditionLog Instance { get; private set; }

        public event Action<QuestDefinition> QuestAdded;
        public event Action<QuestDefinition, QuestProgress> ProgressChanged;

        [Header("Debug")]
        [SerializeField] private QuestDefinition debugQuest;

        private readonly List<QuestDefinition> available = new List<QuestDefinition>();
        private readonly Dictionary<string, QuestProgress> progress = new Dictionary<string, QuestProgress>();

        public IReadOnlyList<QuestDefinition> Available { get { return available; } }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool Contains(QuestDefinition quest)
        {
            return quest != null && available.Exists(q => q.questId == quest.questId);
        }

        /// <summary>Returns false if the quest is null or already on the list.</summary>
        public bool TryAdd(QuestDefinition quest)
        {
            if (quest == null || Contains(quest)) return false;

            available.Add(quest);
            progress[quest.questId] = QuestProgress.Accepted;
            if (QuestAdded != null) QuestAdded(quest);
            return true;
        }

        public bool Remove(QuestDefinition quest)
        {
            if (quest == null) return false;

            progress.Remove(quest.questId);
            return available.RemoveAll(q => q.questId == quest.questId) > 0;
        }

        public QuestProgress GetProgress(string questId)
        {
            QuestProgress value;
            return questId != null && progress.TryGetValue(questId, out value) ? value : QuestProgress.None;
        }

        /// <summary>Called by the expedition flow: InProgress when a hero is sent, Complete when they return.</summary>
        public void SetProgress(QuestDefinition quest, QuestProgress value)
        {
            if (quest == null || !Contains(quest)) return;

            progress[quest.questId] = value;
            if (ProgressChanged != null) ProgressChanged(quest, value);
        }

        [ContextMenu("Debug: Set In Progress")]
        private void DebugSetInProgress() { SetProgress(debugQuest, QuestProgress.InProgress); }

        [ContextMenu("Debug: Set Complete")]
        private void DebugSetComplete() { SetProgress(debugQuest, QuestProgress.Complete); }
    }
}