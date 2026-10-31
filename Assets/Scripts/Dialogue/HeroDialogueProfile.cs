using System;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>
    /// A hero's spoken lines. Keys are "npc.beat" for overrides (e.g. "enchanter.accept")
    /// or just "beat" for the hero's default (e.g. "accept").
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Dialogue/Hero Dialogue Profile")]
    public class HeroDialogueProfile : ScriptableObject
    {
        [Serializable]
        public struct Line
        {
            public string key;
            [TextArea(1, 3)] public string text;
        }

        [SerializeField] private List<Line> lines = new List<Line>();

        public bool TryGet(string key, out string text)
        {
            foreach (Line line in lines)
            {
                if (string.Equals(line.key, key, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(line.text))
                {
                    text = line.text;
                    return true;
                }
            }

            text = null;
            return false;
        }
    }
}