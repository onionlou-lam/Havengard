using UnityEngine;
using Havengard.Resources;

namespace Havengard.Heroes
{
    public class RecruitmentSystem : MonoBehaviour
    {
        [SerializeField] private HeroData[] recruitableHeroes;
        [SerializeField] private int recruitCost = 50;
        [SerializeField] private Transform recruitParent; // container for allies in Phase 2

        public bool RecruitHero(int index)
        {
            if (index < 0 || index >= recruitableHeroes.Length) return false;
            if (!GoldSystem.Instance.SpendGold(recruitCost)) return false;

            HeroData data = recruitableHeroes[index];
            var heroGO = new GameObject(data.heroName);
            var hero = heroGO.AddComponent<HeroInstance>();
            hero.Init(data);

            heroGO.transform.SetParent(recruitParent);

            return true;
        }
    }
}
