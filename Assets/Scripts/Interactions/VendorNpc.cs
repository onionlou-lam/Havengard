using Havengard.Core;
using Havengard.Items;
using Havengard.Vendors;
using Havengard.Vendors.UI;
using Havengard.Dialogue;
using UnityEngine;

namespace Havengard.Interactions
{
    /// <summary>
    /// NPC that talks first, then opens a shop if the player picked the "open_shop" choice.
    /// The catalog decides what kind of vendor this is (sales, item upgrades, tower upgrades...).
    /// </summary>
    public class VendorNPC : NPCInteractable
    {
        [Header("Vendor")]
        [SerializeField] private string shopTitle = "Shop";
        [Tooltip("Yarn speaker name, used to fetch the same portrait the dialogue shows.")]
        [SerializeField] private string speakerName;
        [SerializeField] private VendorCatalog catalog;
        [SerializeField] private VendorShopUI shopUI;
        [SerializeField] private string openShopEventId = "open_shop";

        private bool openShopAfterDialogue;

        private void Start()
        {
            if (shopUI == null) shopUI = FindObjectOfType<VendorShopUI>();
        }

        public override bool CanInteract()
        {
            return base.CanInteract() && !PlayerInputLock.IsLocked;
        }

        public override void RegisterDialogueFunctions(IDialogueFunctionBinder binder)
        {
            base.RegisterDialogueFunctions(binder);
            binder.Bind(openShopEventId, () => openShopAfterDialogue = true);
        }

        public override void OnDialogueEnded(IDialogueVariables variables)
        {
            base.OnDialogueEnded(variables);

            if (!openShopAfterDialogue) return;
            openShopAfterDialogue = false;
            OpenShop();
        }

        protected virtual void OpenShop()
        {
            if (shopUI == null || catalog == null)
            {
                Debug.LogWarning("[VendorNpc] Missing shop UI or catalog on " + name, this);
                return;
            }

            Sprite portrait = null;
            HavengardDialoguePresenter presenter = FindFirstObjectByType<HavengardDialoguePresenter>();
            if (presenter != null && !string.IsNullOrEmpty(speakerName))
                portrait = presenter.GetPortrait(speakerName);

            shopUI.Open(shopTitle, catalog, BuildContext(), null, portrait);
        }

        protected virtual VendorContext BuildContext()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            ItemInventory inventory = player != null ? player.GetComponentInChildren<ItemInventory>() : null;
            if (inventory == null) inventory = FindObjectOfType<ItemInventory>();
            return new VendorContext(inventory);
        }
    }
}