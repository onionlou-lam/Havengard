using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Progression;

public class EXPBarHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EXPSystem expSystem;
    [SerializeField] private Image expFillImage;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text levelText;

    [Header("Animation")]
    [SerializeField] private float fillLerpSpeed = 8f;

    private float displayedFill = 0f;

    private void Awake()
    {
        if (expSystem == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                expSystem = player.GetComponent<EXPSystem>();
                Debug.Log($"[EXPBarHud] Found ExpSystem on {player.name}: {(expSystem ? "OK" : "NULL")}");
            }
            else
            {
                Debug.LogWarning("[EXPBarHud] No Player found for ExpSystem lookup.");
            }
        }
    }

    private void OnEnable()
    {
        SnapToCurrent();
    }

    private void Update()
    {
        if (expSystem == null || expFillImage == null)
            return;

        int current = expSystem.CurrentExp;
        int required = Mathf.Max(1, expSystem.ExpToNextLevel);

        float targetFill = Mathf.Clamp01((float)current / required);
        displayedFill = Mathf.Lerp(displayedFill, targetFill, Time.deltaTime * fillLerpSpeed);
        expFillImage.fillAmount = displayedFill;

        if (expText != null)
            expText.text = $"{current} / {required}";

        if (levelText != null)
            levelText.text = $"Lv {expSystem.CurrentLevel}";
    }

    private void SnapToCurrent()
    {
        if (expSystem == null || expFillImage == null)
            return;

        int current = expSystem.CurrentExp;
        int required = Mathf.Max(1, expSystem.ExpToNextLevel);

        displayedFill = Mathf.Clamp01((float)current / required);
        expFillImage.fillAmount = displayedFill;

        if (expText != null)
            expText.text = $"{current} / {required}";

        if (levelText != null)
            levelText.text = $"Lv {expSystem.CurrentLevel}";
    }
}
