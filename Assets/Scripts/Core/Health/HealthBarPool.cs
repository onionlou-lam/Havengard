using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Core.HealthSystem
{
    public class HealthBarPool : MonoBehaviour
    {
        public static HealthBarPool Instance { get; private set; }

        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private int initialSize = 30;

        private readonly Queue<HealthBar> pool = new();

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < Mathf.Max(1, initialSize); i++)
                CreateNew();
        }

        private HealthBar CreateNew()
        {
            var go = Instantiate(healthBarPrefab, transform);
            go.SetActive(false);
            var hb = go.GetComponent<HealthBar>();
            pool.Enqueue(hb);
            return hb;
        }

        public HealthBar Get()
        {
            if (pool.Count == 0) CreateNew();
            var hb = pool.Dequeue();
            hb.gameObject.SetActive(true);
            return hb;
        }

        public void Return(HealthBar hb)
        {
            if (!hb) return;
            hb.gameObject.SetActive(false);
            hb.transform.SetParent(transform, false);
            pool.Enqueue(hb);
        }
    }
}
