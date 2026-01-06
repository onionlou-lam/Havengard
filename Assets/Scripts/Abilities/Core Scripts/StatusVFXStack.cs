using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Statuses
{
    /// <summary>
    /// Simple helper that manages stacking/refreshing VFX objects on a target.
    /// StatusEffectInstance can call into this so VFX doesn't duplicate weirdly.
    /// </summary>
    [DisallowMultipleComponent]
    public class StatusVFXStack : MonoBehaviour
    {
        // Key = VFX prefab instance ID (or prefab reference), Value = spawned instances
        private readonly Dictionary<GameObject, List<GameObject>> spawned = new Dictionary<GameObject, List<GameObject>>();

        /// <summary>
        /// Spawns a new VFX instance as a child, and tracks it as a "stack".
        /// </summary>
        public GameObject AddStack(GameObject vfxPrefab, float lifetime)
        {
            if (vfxPrefab == null) return null;

            if (!spawned.TryGetValue(vfxPrefab, out var list))
            {
                list = new List<GameObject>();
                spawned[vfxPrefab] = list;
            }

            var inst = Instantiate(vfxPrefab, transform.position, Quaternion.identity, transform);
            list.Add(inst);

            if (lifetime > 0f)
                Destroy(inst, lifetime);

            return inst;
        }

        /// <summary>
        /// Clears all VFX stacks for a prefab (or all if prefab is null).
        /// </summary>
        public void Clear(GameObject vfxPrefab = null)
        {
            if (vfxPrefab == null)
            {
                foreach (var kv in spawned)
                {
                    foreach (var go in kv.Value)
                        if (go != null) Destroy(go);
                }
                spawned.Clear();
                return;
            }

            if (!spawned.TryGetValue(vfxPrefab, out var list)) return;

            foreach (var go in list)
                if (go != null) Destroy(go);

            spawned.Remove(vfxPrefab);
        }
    }
}
