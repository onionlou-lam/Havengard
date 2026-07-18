using UnityEngine;
using Havengard.Core.Progression;

namespace Havengard.UI
{
    /// <summary>
    /// Data structure for a newly created character
    /// </summary>
    [System.Serializable]
    public class CharacterCreationData
    {
        public string characterName;
        public PlayerClass selectedClass;
        public bool isMale;

        // Static field to pass data between scenes
        public static CharacterCreationData currentCharacter;
    }
}