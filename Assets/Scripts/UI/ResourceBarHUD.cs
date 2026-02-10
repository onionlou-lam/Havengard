// Assets/Scripts/UI/ResourceBarHUD.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Character;

public class ResourceBarHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResourceSystem resource;  // Player ResourceSystem
    [SerializeField] private Image fillImage;          // Fill image for mana bar
    [SerializeField] private TMP_Text valueText;       // Optional "current/max" text

    private void Awake()
    {
        // Auto-find player resource if not set
        if (resource == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                resource = player.GetComponent<ResourceSystem>();
        }
    }

    private void OnEnable()
    {
        Hook();
        UpdateBar();
    }

    private void OnDisable()
    {
        Unhook();
    }

    private void Hook()
    {
        if (resource == null) return;
        resource.OnResourceChanged += UpdateBar;
    }

    private void Unhook()
    {
        if (resource == null) return;
        resource.OnResourceChanged -= UpdateBar;
    }

    private void UpdateBar()
    {
        if (resource == null || fillImage == null) return;

        float normalized = resource.MaxResource > 0f ? (float)resource.CurrentResource / resource.MaxResource : 0f;
        fillImage.fillAmount = normalized;

        if (valueText != null)
        {
            int cur = Mathf.FloorToInt(resource.CurrentResource);
            int max = Mathf.FloorToInt(resource.MaxResource);
            valueText.text = $"{cur}/{max}";
        }
    }
}
