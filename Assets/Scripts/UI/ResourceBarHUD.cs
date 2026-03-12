// Assets/Scripts/UI/ResourceBarHUD.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Core.Character;

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
        UpdateBar(resource != null ? resource.CurrentResource : 0, 
                  resource != null ? resource.MaxResource : 0);
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

    // CHANGED: Added parameters to match Action<int, int> delegate signature
    private void UpdateBar(int currentResource, int maxResource)
    {
        if (fillImage == null) return;

        float normalized = maxResource > 0f ? (float)currentResource / maxResource : 0f;
        fillImage.fillAmount = normalized;

        if (valueText != null)
        {
            valueText.text = $"{currentResource}/{maxResource}";
        }
    }
}
