using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Havengard.Abilities;

namespace Havengard.UI.SkillTree
{
    /// <summary>
    /// Draws connection lines between skill tree nodes and their prerequisites
    /// </summary>
    public class SkillTreeConnectionRenderer : MonoBehaviour
    {
        [Header("Line Settings")]
        [SerializeField] private Color lineColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        [SerializeField] private float lineWidth = 2f;
        [SerializeField] private Material lineMaterial; // Optional: UI/Default material

        private List<GameObject> lineObjects = new List<GameObject>();

        /// <summary>
        /// Draw all prerequisite connections
        /// </summary>
        public void DrawConnections(List<SkillTreeNodeUI> nodes, ClassAbility[] classAbilities)
        {
            ClearLines();

            if (nodes == null || classAbilities == null)
                return;

            for (int i = 0; i < nodes.Count; i++)
            {
                ClassAbility classAbility = classAbilities[i];

                if (!classAbility.HasPrerequisites())
                    continue;

                // Draw line from this node to each prerequisite
                foreach (int prereqIndex in classAbility.prerequisiteIndices)
                {
                    if (prereqIndex < 0 || prereqIndex >= nodes.Count)
                        continue;

                    SkillTreeNodeUI fromNode = nodes[prereqIndex]; // Prerequisite node
                    SkillTreeNodeUI toNode = nodes[i]; // Current node

                    DrawLine(fromNode.RectTransform.anchoredPosition, toNode.RectTransform.anchoredPosition);
                }
            }
        }

        private void DrawLine(Vector2 startPos, Vector2 endPos)
        {
            // Create line object
            GameObject lineObj = new GameObject("Connection Line");
            lineObj.transform.SetParent(transform, false);

            Image lineImage = lineObj.AddComponent<Image>();
            lineImage.color = lineColor;

            if (lineMaterial != null)
                lineImage.material = lineMaterial;

            RectTransform rectTransform = lineObj.GetComponent<RectTransform>();

            // Calculate position and rotation
            Vector2 direction = endPos - startPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Position line at midpoint
            rectTransform.anchoredPosition = startPos + direction * 0.5f;
            rectTransform.sizeDelta = new Vector2(distance, lineWidth);
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            lineObjects.Add(lineObj);
        }

        private void ClearLines()
        {
            foreach (GameObject lineObj in lineObjects)
            {
                if (lineObj != null)
                    Destroy(lineObj);
            }
            lineObjects.Clear();
        }
    }
}