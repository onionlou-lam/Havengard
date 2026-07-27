namespace Havengard.Interactions
{
    /// <summary>
    /// Interface for any interactable object in the game.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Gets the interaction prompt text (e.g., "Enter Inn", "Talk to NPC")
        /// </summary>
        string GetInteractionPrompt();

        /// <summary>
        /// Gets the interaction key/button hint (e.g., "E", "A")
        /// </summary>
        string GetInteractionKey();

        /// <summary>
        /// Called when the player interacts with this object
        /// </summary>
        void Interact();

        /// <summary>
        /// Whether this object can currently be interacted with
        /// </summary>
        bool CanInteract();

        /// <summary>
        /// Transform position for the tooltip (usually above the object)
        /// </summary>
        UnityEngine.Transform GetTooltipTransform();
    }
}