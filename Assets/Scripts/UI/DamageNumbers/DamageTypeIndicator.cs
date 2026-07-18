using UnityEngine;
using TMPro;
using Havengard.Combat;

namespace Havengard.UI
{
    /// <summary>
    /// Visual indicator for damage types on abilities
    /// </summary>
    public class DamageTypeIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private UnityEngine.UI.Image typeIcon;

        [Header("Colors")]
        [SerializeField] private Color physicalColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color fireColor = new Color(1f, 0.3f, 0f);
        [SerializeField] private Color frostColor = new Color(0.3f, 0.8f, 1f);
        [SerializeField] private Color lightningColor = new Color(1f, 1f, 0.3f);
        [SerializeField] private Color holyColor = new Color(1f, 0.9f, 0.3f);

        public void SetDamageType(DamageType type, bool canHeal = false)
        {
            Color color = GetColorForType(type);
            string text = canHeal && type == DamageType.Holy ? "Holy (Heal)" : type.ToString();

            if (typeText != null)
            {
                typeText.text = text;
                typeText.color = color;
            }

            if (typeIcon != null)
            {
                typeIcon.color = color;
            }
        }

        private Color GetColorForType(DamageType type)
        {
            return type switch
            {
                DamageType.Physical => physicalColor,
                DamageType.Fire => fireColor,
                DamageType.Frost => frostColor,
                DamageType.Lightning => lightningColor,
                DamageType.Holy => holyColor,
                _ => Color.white
            };
        }
    }
}