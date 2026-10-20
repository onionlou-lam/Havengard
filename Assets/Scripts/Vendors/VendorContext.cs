using Havengard.Items;

namespace Havengard.Vendors
{
    /// <summary>What a catalog may need from the world. Add fields here (e.g. tower manager) as systems arrive.</summary>
    public class VendorContext
    {
        public ItemInventory PlayerInventory { get; private set; }

        public VendorContext(ItemInventory playerInventory)
        {
            PlayerInventory = playerInventory;
        }
    }
}