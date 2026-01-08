using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Statuses
{
    /// <summary>
    /// Static helper for stacking/refreshing attached status VFX on a host.
    /// This is NOT a MonoBehaviour, and should NOT be attached to GameObjects.
    /// </summary>
    public static class StatusVFXStack
    {
        // Key: (host instance id, effect instance id)
        private struct Key
        {
            public int hostId;
            public int effectId;

            public Key(int hostId, int effectId)
            {
                this.hostId = hostId;
                this.effectId = effectId;
            }
        }

        private class StackEntry
        {
            public readonly List<GameObject> instances = new List<GameObject>();
        }

        private static readonly Dictionary<Key, StackEntry> stacks = new Dictionary<Key, StackEntry>();

        /// <summary>
        /// Adds (or refreshes) a stack VFX instance, up to maxStacks.
        /// Returns current stack count after the operation.
        /// </summary>
        public static int AddStack(MonoBehaviour host, int effectInstanceId, GameObject vfxPrefab, float duration, int maxStacks)
        {
            if (host == null) return 0;
            if (vfxPrefab == null) return GetCount(host, effectInstanceId);

            maxStacks = Mathf.Max(1, maxStacks);

            var key = new Key(host.GetInstanceID(), effectInstanceId);

            if (!stacks.TryGetValue(key, out var entry))
            {
                entry = new StackEntry();
                stacks[key] = entry;
            }

            // Clean dead refs
            for (int i = entry.instances.Count - 1; i >= 0; i--)
            {
                if (entry.instances[i] == null) entry.instances.RemoveAt(i);
            }

            // If already at max, just refresh duration by re-destroy scheduling (spawn a new one is optional)
            if (entry.instances.Count >= maxStacks)
            {
                // Optional: refresh by restarting the last one’s lifetime (simple approach: do nothing here)
                return entry.instances.Count;
            }

            var fx = Object.Instantiate(vfxPrefab, host.transform.position, Quaternion.identity, host.transform);

            if (duration > 0f)
                Object.Destroy(fx, duration);

            entry.instances.Add(fx);
            return entry.instances.Count;
        }

        public static int GetCount(MonoBehaviour host, int effectInstanceId)
        {
            if (host == null) return 0;
            var key = new Key(host.GetInstanceID(), effectInstanceId);

            if (!stacks.TryGetValue(key, out var entry)) return 0;

            // Clean dead refs
            for (int i = entry.instances.Count - 1; i >= 0; i--)
            {
                if (entry.instances[i] == null) entry.instances.RemoveAt(i);
            }

            return entry.instances.Count;
        }

        /// <summary>
        /// Clears all VFX stacks for this host+effect.
        /// </summary>
        public static void Clear(MonoBehaviour host, int effectInstanceId)
        {
            if (host == null) return;

            var key = new Key(host.GetInstanceID(), effectInstanceId);

            if (!stacks.TryGetValue(key, out var entry)) return;

            foreach (var fx in entry.instances)
            {
                if (fx != null) Object.Destroy(fx);
            }

            entry.instances.Clear();
            stacks.Remove(key);
        }
    }
}
