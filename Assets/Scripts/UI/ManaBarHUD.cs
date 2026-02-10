using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Character;   // where ResourceSystem lives

public class ManaBarHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResourceSystem resourceSystem;       // Player mana resource
    [SerializeField] private Image manaBarFill;                   // Fill image
    [SerializeField] private TextMeshProUGUI manaText;            // optional "xx / yy"

    [Header("Animation")]
    [SerializeField] private float fillLerpSpeed = 8f;

    private float displayedFill = 1f;

    private void Awake()
    {
        // Try auto-find from player if not assigned
        if (resourceSystem == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                resourceSystem = player.GetComponent<ResourceSystem>();
        }
    }

    private void OnEnable()
    {
        SnapToCurrent();
    }

    private void Update()
    {
        if (resourceSystem == null || manaBarFill == null)
            return;

        float max = Mathf.Max(1f, resourceSystem.MaxResource);
        float targetFill = Mathf.Clamp01(resourceSystem.CurrentResource / max);

        displayedFill = Mathf.Lerp(displayedFill, targetFill, Time.deltaTime * fillLerpSpeed);
        manaBarFill.fillAmount = displayedFill;

        UpdateManaText();
    }

    private void SnapToCurrent()
    {
        if (resourceSystem == null || manaBarFill == null) return;

        float max = Mathf.Max(1f, resourceSystem.MaxResource);
        displayedFill = Mathf.Clamp01(resourceSystem.CurrentResource / max);
        manaBarFill.fillAmount = displayedFill;
        UpdateManaText();
    }

    private void UpdateManaText()
    {
        if (manaText == null || resourceSystem == null) return;

        int current = Mathf.RoundToInt(resourceSystem.CurrentResource);
        int max = Mathf.RoundToInt(resourceSystem.MaxResource);
        manaText.text = $"{current} / {max}";
    }
}
