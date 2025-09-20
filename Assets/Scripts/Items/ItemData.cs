using UnityEngine;

namespace Havengard.Items
{
    [CreateAssetMenu(menuName = "Havengard/Item")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public Sprite icon;
        [TextArea] public string description;

        [Header("Stats")]
        public int attackBonus;
        public int defenseBonus;
        public int healthBonus;
        public int resourceBonus;
    }
}
