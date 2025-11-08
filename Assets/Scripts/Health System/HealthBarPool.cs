using System.Collections.Generic;
using UnityEngine;

namespace Havengard.HealthSystem
{
    /// <summary>
    /// Object pool for NPC HealthBars to reduce instantiation overhead.
    /// </summary>
    public class HealthBarPool : MonoBehaviour
    {
        public static HealthBarPool Instance { get; private set; }

        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private int initialSize = 30;

        private readonly Queue<HealthBar> pool = new Queue<HealthBar>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Prewarm
            for (int i = 0; i < initialSize; i++)
                CreateNew();
        }

        private HealthBar CreateNew()
        {
            var obj = Instantiate(healthBarPrefab, transform);
            obj.gameObject.SetActive(false);
            var hb = obj.GetComponent<HealthBar>();
            pool.Enqueue(hb);
            return hb;
        }

        public HealthBar Get()
        {
            if (pool.Count == 0)
                CreateNew();

            var hb = pool.Dequeue();
            hb.gameObject.SetActive(true);
            return hb;
        }

        public void Return(HealthBar hb)
        {
            if (hb == null) return;
            hb.gameObject.SetActive(false);
            hb.transform.SetParent(transform, false);
            pool.Enqueue(hb);
        }
    }
}
