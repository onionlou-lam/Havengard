using UnityEngine;
//using Havengard.UI.SkillTree;
using System.Collections; // ADD THIS

namespace Havengard.UI
{
    /// <summary>
    /// Improved diagnostic script that finds inactive objects
    /// </summary>
    public class SkillTreeDebugHelper : MonoBehaviour
    {
        private void Start()
        {
            // Wait a frame to let everything initialize
            StartCoroutine(DiagnosticAfterFrame());
        }

        private IEnumerator DiagnosticAfterFrame()
        {
            yield return null; // Wait one frame

            Debug.Log("===== SKILL TREE DIAGNOSTIC (Enhanced) =====");
            
            // Method 1: Find all components (including on inactive objects)
            // FIXED: Use UnityEngine.Resources explicitly
            var allSkillTreeUIs = UnityEngine.Resources.FindObjectsOfTypeAll<SkillTreeUI>();
            Debug.Log($"Found {allSkillTreeUIs.Length} SkillTreeUI components (including inactive)");
            
            foreach (var ui in allSkillTreeUIs)
            {
                if (ui.gameObject.scene.name != null) // Filter out prefabs in Project
                {
                    Debug.Log($"  - SkillTreeUI on: {GetGameObjectPath(ui.gameObject)} (Active: {ui.gameObject.activeInHierarchy})");
                }
            }
            
            var allTooltips = UnityEngine.Resources.FindObjectsOfTypeAll<SkillTreeTooltip>();
            Debug.Log($"Found {allTooltips.Length} SkillTreeTooltip components (including inactive)");
            
            foreach (var tooltip in allTooltips)
            {
                if (tooltip.gameObject.scene.name != null) // Filter out prefabs
                {
                    Debug.Log($"  - SkillTreeTooltip on: {GetGameObjectPath(tooltip.gameObject)} (Active: {tooltip.gameObject.activeInHierarchy})");
                }
            }
            
            // Method 2: Search Canvas hierarchy
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            Debug.Log($"\nSearching {allCanvases.Length} Canvas objects:");
            
            foreach (Canvas canvas in allCanvases)
            {
                Debug.Log($"  Canvas: {canvas.name}");
                
                // Search for SkillTreePanel
                Transform skillTreePanel = canvas.transform.Find("SkillTreePanel");
                if (skillTreePanel != null)
                {
                    Debug.Log($"    ? Found SkillTreePanel (Active: {skillTreePanel.gameObject.activeInHierarchy})");
                    var ui = skillTreePanel.GetComponent<SkillTreeUI>();
                    Debug.Log($"      - Has SkillTreeUI: {ui != null}");
                }
                
                // Search for SkillTreeTooltipPanel
                Transform tooltipPanel = canvas.transform.Find("SkillTreeTooltipPanel");
                if (tooltipPanel != null)
                {
                    Debug.Log($"    ? Found SkillTreeTooltipPanel (Active: {tooltipPanel.gameObject.activeInHierarchy})");
                    var tooltip = tooltipPanel.GetComponent<SkillTreeTooltip>();
                    Debug.Log($"      - Has SkillTreeTooltip: {tooltip != null}");
                }
            }
            
            Debug.Log("===== END ENHANCED DIAGNOSTIC =====");
        }
        
        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }
    }
}