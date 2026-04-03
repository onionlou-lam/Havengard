using UnityEngine;
using System.Collections.Generic;

namespace Havengard.Abilities
{
    /// <summary>
    /// Object pool for projectiles to avoid instantiation overhead
    /// </summary>
    public class ProjectilePool : MonoBehaviour
    {
        private static ProjectilePool instance;
        public static ProjectilePool Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("ProjectilePool");
                    instance = obj.AddComponent<ProjectilePool>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new Queue<GameObject>();
            }

            GameObject obj;
            if (pools[prefab].Count > 0)
            {
                obj = pools[prefab].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
            else
            {
                obj = Instantiate(prefab, position, rotation);
            }

            return obj;
        }

        public void ReturnToPool(GameObject prefab, GameObject obj)
        {
            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new Queue<GameObject>();
            }

            obj.SetActive(false);
            pools[prefab].Enqueue(obj);
        }
    }
}