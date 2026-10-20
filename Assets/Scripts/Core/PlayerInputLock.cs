using System;
using UnityEngine;

namespace Havengard.Core
{
    /// <summary>Counter-based lock so overlapping UIs (dialogue + shop) don't unlock each other early.</summary>
    public static class PlayerInputLock
    {
        private static int count;

        public static bool IsLocked { get { return count > 0; } }
        public static event Action<bool> LockChanged;

        public static void Acquire()
        {
            count++;
            if (count == 1 && LockChanged != null) LockChanged(true);
        }

        public static void Release()
        {
            if (count == 0) return;
            count--;
            if (count == 0 && LockChanged != null) LockChanged(false);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            count = 0;
            LockChanged = null;
        }
    }
}