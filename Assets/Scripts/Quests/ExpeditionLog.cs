using System;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Quests
{
    /// <summary>Quests the player has unlocked and can send heroes on. Same singleton pattern as GoldSystem.</summary>
    public class ExpeditionLog : MonoBehaviour
    {
        public static ExpeditionLog Instance { get; private set; }

        public event Action<QuestDefinition> QuestAdded;

        private readonly List<QuestDefinition> available = new List<QuestDefinition>();

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
            if (QuestAdded != null) QuestAdded(quest);
            return true;
        }

        public bool Remove(QuestDefinition quest)
        {
            return quest != null && available.RemoveAll(q => q.questId == quest.questId) > 0;
        }
    }
}