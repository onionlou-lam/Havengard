using Havengard.Core.Heroes;
using UnityEngine;
using Yarn.Unity;

namespace Havengard.Dialogue
{
    /// <summary>Finds the hero the player is controlling and supplies their dialogue lines to Yarn.</summary>
    public static class HeroDialogue
    {
        private static HeroData explicitHero;
        private static HeroInstance cachedInstance;

        /// <summary>The active hero. Uses SetCurrent if called, otherwise the HeroInstance on the Player-tagged object.</summary>
        public static HeroData Current
        {
            get
            {
                if (explicitHero != null) return explicitHero;

                if (cachedInstance == null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        cachedInstance = player.GetComponentInChildren<HeroInstance>();
                        if (cachedInstance == null) cachedInstance = player.GetComponentInParent<HeroInstance>();
                    }
                }

                return cachedInstance != null ? cachedInstance.Data : null;
            }
        }

        /// <summary>Call this when a character is chosen, if the player object doesn't carry a HeroInstance.</summary>
        public static void SetCurrent(HeroData hero) { explicitHero = hero; }

        private static bool TryResolve(string key, out string text)
        {
            text = null;

            HeroData hero = Current;
            HeroDialogueProfile profile = hero != null ? hero.dialogueProfile : null;
            if (profile == null) return false;

            if (profile.TryGet(key, out text)) return true;

            // Fall back to the hero's default for the beat: "enchanter.active_reply" -> "active_reply".
            int dot = key.LastIndexOf('.');
            return dot >= 0 && dot < key.Length - 1 && profile.TryGet(key.Substring(dot + 1), out text);
        }

        [YarnFunction("hero_line")]
        public static string HeroLine(string key, string fallback)
        {
            string text;
            return TryResolve(key, out text) ? text : fallback;
        }

        /// <summary>True if the hero's profile supplies this line. Use it to make a reply optional.</summary>
        [YarnFunction("hero_has_line")]
        public static bool HeroHasLine(string key)
        {
            string text;
            return TryResolve(key, out text);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            explicitHero = null;
            cachedInstance = null;
        }
    }
}