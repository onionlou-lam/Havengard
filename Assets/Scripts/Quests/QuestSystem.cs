using UnityEngine;
using System.Collections.Generic;
using Havengard.Resources;
using Havengard.Core.Heroes;
namespace Havengard.Quests
{
    [System.Serializable]
    public class Quest
    {
        public string questName;
        public int durationDays;
        public int baseGoldReward;
        public int baseExpReward;
        public int baseCelestiumReward;
    }

    public class QuestSystem : MonoBehaviour
    {
        private List<(HeroInstance hero, Quest quest)> activeQuests = new();

        public void SendOnQuest(HeroInstance hero, Quest quest)
        {
            hero.StartQuest(quest.durationDays);
            activeQuests.Add((hero, quest));
        }

        public void ProgressDay()
        {
            for (int i = activeQuests.Count - 1; i >= 0; i--)
            {
                var (hero, quest) = activeQuests[i];
                hero.ProgressQuestDay();
                if (!hero.IsOnQuest) // quest completed
                {
                    DistributeRewards(hero, quest);
                    activeQuests.RemoveAt(i);
                }
            }
        }

        private void DistributeRewards(HeroInstance hero, Quest quest)
        {
            GoldSystem.Instance.AddGold(quest.baseGoldReward);
            CelestiumSystem.Instance.AddCelestium(quest.baseCelestiumReward);

            hero.ExpSystem.AddEXP(quest.baseExpReward);

            Debug.Log($"{hero.Data.heroName} completed quest {quest.questName}!");
        }
    }
}
