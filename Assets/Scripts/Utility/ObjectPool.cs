using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Utility
{
    /// <summary>
    /// Simple, reusable object pool for GameObjects.
    /// Use ObjectPool.Get() to fetch an instance and ObjectPool.Return() to release it.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;

        private readonly Queue<GameObject> pool = new Queue<GameObject>();
        private Transform parentContainer;

        private void Awake()
        {
            parentContainer = new GameObject($"{prefab.name}_Pool").transform;
            parentContainer.SetParent(transform);
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = CreateNewObject();
                pool.Enqueue(obj);
            }
        }

        private GameObject CreateNewObject()
        {
            GameObject obj = Instantiate(prefab, parentContainer);
            obj.SetActive(false);
            return obj;
        }

        /// <summary>
        /// Get an instance from the pool.
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj = pool.Count > 0 ? pool.Dequeue() : CreateNewObject();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Return an instance back to the pool.
        /// </summary>
        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(parentContainer);
            pool.Enqueue(obj);
        }
    }
}
