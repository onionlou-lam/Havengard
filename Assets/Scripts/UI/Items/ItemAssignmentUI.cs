using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;
using System.Collections.Generic;

namespace Havengard.UI
{
    /// <summary>
    /// UI for assigning items from cache to characters
    /// </summary>
    public class ItemAssignmentUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Transform characterListParent;
        [SerializeField] private GameObject characterButtonPrefab;

        [Header("Buttons")]
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button disenchantButton;

        private ItemInstance currentItem;
        private List<Button> characterButtons = new List<Button>();

        private void Start()
        {
            if (cancelButton != null)
                cancelButton.onClick.AddListener(Hide);

            if (disenchantButton != null)
                disenchantButton.onClick.AddListener(DisenchantCurrentItem);

            Hide();
        }

        public void Show(ItemInstance item)
        {
            currentItem = item;
            panel.SetActive(true);
            UpdateDisplay();
            PopulateCharacterList();
        }

        public void Hide()
        {
            panel.SetActive(false);
            currentItem = null;
        }

        private void UpdateDisplay()
        {
            if (currentItem == null) return;

            if (itemNameText != null)
                itemNameText.text = currentItem.itemData.itemName;

            if (itemIconImage != null)
                itemIconImage.sprite = currentItem.itemData.icon;

            if (descriptionText != null)
                descriptionText.text = currentItem.itemData.GetScaledDescription(currentItem.level);
        }

        private void PopulateCharacterList()
        {
            // Clear existing buttons
            foreach (var btn in characterButtons)
            {
                Destroy(btn.gameObject);
            }
            characterButtons.Clear();

            // Get all characters with inventories
            var characters = ItemManager.Instance?.GetAllCharactersWithInventories();
            if (characters == null) return;

            foreach (var character in characters)
            {
                CreateCharacterButton(character);
            }
        }

        private void CreateCharacterButton(GameObject character)
        {
            GameObject btnObj = Instantiate(characterButtonPrefab, characterListParent);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
                btnText.text = character.name;

            if (btn != null)
            {
                btn.onClick.AddListener(() => AssignToCharacter(character));
                characterButtons.Add(btn);
            }
        }

        private void AssignToCharacter(GameObject character)
        {
            if (currentItem == null) return;

            bool success = ItemManager.Instance?.AssignItemToCharacter(currentItem, character) ?? false;
            if (success)
            {
                Debug.Log($"[ItemAssignmentUI] Assigned {currentItem} to {character.name}");
                Hide();
            }
            else
            {
                Debug.LogWarning($"[ItemAssignmentUI] Failed to assign item to {character.name}");
            }
        }

        private void DisenchantCurrentItem()
        {
            if (currentItem == null) return;

            int celestium = ItemCache.Instance.DisenchantItem(currentItem);
            Debug.Log($"[ItemAssignmentUI] Disenchanted for {celestium} Celestium");
            Hide();
        }
    }
}