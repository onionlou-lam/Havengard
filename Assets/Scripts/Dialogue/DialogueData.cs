using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Dialogue
{
    [CreateAssetMenu(menuName = "Havengard/Dialogue/Dialogue Data", fileName = "NewDialogue")]
    public class DialogueData : ScriptableObject
    {
        public string speakerName;
        public Sprite portrait;
        public List<DialogueNode> nodes = new List<DialogueNode>();
    }
}