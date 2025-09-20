using UnityEngine;
using System.Collections.Generic;
using Havengard.Items;

namespace Havengard.Heroes
{
    [CreateAssetMenu(menuName = "Havengard/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemData> allItems;

        private Dictionary<string, ItemData> lookup;

        private void OnEnable()
        {
            lookup = new Dictionary<string, ItemData>();
            foreach (var item in allItems)
            {
                if (item != null && !lookup.ContainsKey(item.itemName))
                    lookup[item.itemName] = item;
            }
        }

        public List<ItemData> GetItemsByNames(string[] names)
        {
            List<ItemData> items = new();
            foreach (var name in names)
            {
                if (lookup.TryGetValue(name, out var item))
                    items.Add(item);
            }
            return items;
        }
    }
}
