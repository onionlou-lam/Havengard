using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Havengard.Abilities;

namespace Havengard.UI
{
    /// <summary>
    /// Draws connection lines between skill tree nodes and their prerequisites.
    /// Supports different line styles for main abilities and sub-skills.
    /// </summary>
    public class SkillTreeConnectionRenderer : MonoBehaviour
    {
        [Header("Main Line Settings")]
        [SerializeField] private Color mainLineColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        [SerializeField] private float mainLineWidth = 2f;
        [SerializeField] private Material mainLineMaterial;

        [Header("Sub-Skill Line Settings")]
        [SerializeField] private Color subSkillLineColor = new Color(0.3f, 0.7f, 1f, 0.8f);
        [SerializeField] private float subSkillLineWidth = 1.5f;
        [SerializeField] private Material subSkillLineMaterial;

        private List<GameObject> lineObjects = new List<GameObject>();

        /// <summary>
        /// Draw all prerequisite connections
        /// </summary>
        public void DrawConnections(List<SkillTreeNodeUI> mainNodes, List<SubSkillNodeUI> subSkillNodes, 
                                    ClassAbility[] classAbilities)
        {
            ClearLines();

            if (mainNodes == null || classAbilities == null)
                return;

            // Draw prerequisite connections for main abilities
            for (int i = 0; i < mainNodes.Count; i++)
            {
                ClassAbility classAbility = classAbilities[i];

                if (!classAbility.HasPrerequisites())
                    continue;

                foreach (int prereqIndex in classAbility.prerequisiteIndices)
                {
                    if (prereqIndex < 0 || prereqIndex >= mainNodes.Count)
                        continue;

                    SkillTreeNodeUI fromNode = mainNodes[prereqIndex];
                    SkillTreeNodeUI toNode = mainNodes[i];

                    DrawLine(fromNode.RectTransform.anchoredPosition,
                            toNode.RectTransform.anchoredPosition,
                            false);
                }
            }

            // Draw parent-to-sub-skill connections
            if (subSkillNodes != null)
            {
                foreach (SubSkillNodeUI subNode in subSkillNodes)
                {
                    int parentIndex = subNode.ParentAbilityIndex;
                    if (parentIndex < 0 || parentIndex >= mainNodes.Count)
                        continue;

                    SkillTreeNodeUI parentNode = mainNodes[parentIndex];

                    DrawLine(parentNode.RectTransform.anchoredPosition,
                            subNode.RectTransform.anchoredPosition,
                            true); // Use sub-skill line style
                }
            }
        }

        /// <summary>
        /// Draw a line with specified style
        /// </summary>
        private void DrawLine(Vector2 startPos, Vector2 endPos, bool isSubSkillLine)
        {
            GameObject lineObj = new GameObject(isSubSkillLine ? "SubSkill Connection Line" : "Connection Line");
            lineObj.transform.SetParent(transform, false);

            Image lineImage = lineObj.AddComponent<Image>();
            
            // Apply style based on line type
            if (isSubSkillLine)
            {
                lineImage.color = subSkillLineColor;
                if (subSkillLineMaterial != null)
                    lineImage.material = subSkillLineMaterial;
            }
            else
            {
                lineImage.color = mainLineColor;
                if (mainLineMaterial != null)
                    lineImage.material = mainLineMaterial;
            }

            RectTransform rectTransform = lineObj.GetComponent<RectTransform>();

            // Calculate position and rotation
            Vector2 direction = endPos - startPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Position line at midpoint
            rectTransform.anchoredPosition = startPos + direction * 0.5f;
            rectTransform.sizeDelta = new Vector2(distance, isSubSkillLine ? subSkillLineWidth : mainLineWidth);
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