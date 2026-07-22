using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Havengard.Town
{
    /// <summary>
    /// Manages shop interior scenes. Auto-walks player to counter and starts dialogue.
    /// </summary>
    public class ShopInteriorController : MonoBehaviour
    {
        [Header("Scene Setup")]
        [SerializeField]
        [Tooltip("Transform where the player spawns")]
        private Transform playerSpawnPoint;

        [SerializeField]
        [Tooltip("Transform where the player should walk to (counter position)")]
        private Transform counterPosition;

        [SerializeField]
        [Tooltip("Reference to the player GameObject")]
        private GameObject playerPrefab;

        [Header("NPC Setup")]
        [SerializeField]
        [Tooltip("The shopkeeper NPC")]
        private ShopkeeperNPC shopkeeper;

        [Header("Timing")]
        [SerializeField]
        [Tooltip("Delay before starting the walk sequence")]
        private float startDelay = 0.5f;

        [SerializeField]
        [Tooltip("Distance threshold to consider player has reached counter")]
        private float arrivalThreshold = 0.5f;

        private GameObject playerInstance;
        private NavMeshAgent playerAgent;
        private PlayerController2D playerController;
        private bool sequenceComplete = false;

        private void Start()
        {
            StartCoroutine(ShopIntroSequence());
        }

        private IEnumerator ShopIntroSequence()
        {
            // Wait for scene to fully load
            yield return new WaitForSeconds(startDelay);

            // Find or spawn player
            SetupPlayer();

            if (playerInstance == null)
            {
                Debug.LogError("[ShopInteriorController] Failed to setup player!");
                yield break;
            }

            // Disable player input during sequence
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // Position player at spawn point
            playerInstance.transform.position = playerSpawnPoint.position;

            // Walk to counter
            yield return StartCoroutine(WalkToCounter());

            // Start dialogue
            if (shopkeeper != null)
            {
                shopkeeper.StartGreeting();
            }
            else
            {
                Debug.LogWarning("[ShopInteriorController] No shopkeeper assigned!");
            }

            sequenceComplete = true;
        }

        private void SetupPlayer()
        {
            // Try to find existing player in scene
            playerInstance = GameObject.FindGameObjectWithTag("Player");

            // If no player exists, instantiate from prefab
            if (playerInstance == null && playerPrefab != null)
            {
                playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            }

            if (playerInstance != null)
            {
                playerAgent = playerInstance.GetComponent<NavMeshAgent>();
                playerController = playerInstance.GetComponent<PlayerController2D>();
            }
        }

        private IEnumerator WalkToCounter()
        {
            if (playerAgent == null || counterPosition == null)
            {
                yield break;
            }

            // Set destination
            playerAgent.SetDestination(counterPosition.position);
            playerAgent.isStopped = false;

            // Wait until player reaches counter
            while (Vector3.Distance(playerInstance.transform.position, counterPosition.position) > arrivalThreshold)
            {
                yield return null;
            }

            // Stop at counter
            playerAgent.isStopped = true;
            playerAgent.velocity = Vector3.zero;
        }

        /// <summary>
        /// Call this when dialogue completes and shop UI opens
        /// </summary>
        public void OnShopUIOpened()
        {
            // Keep player control disabled while in shop UI
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        /// <summary>
        /// Call this when player exits the shop
        /// </summary>
        public void OnShopExit()
        {
            // Re-enable player control
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Visualize spawn and counter positions
            if (playerSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerSpawnPoint.position, 0.3f);
                UnityEditor.Handles.Label(playerSpawnPoint.position + Vector3.up * 0.5f, "Spawn");
            }

            if (counterPosition != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(counterPosition.position, arrivalThreshold);
                UnityEditor.Handles.Label(counterPosition.position + Vector3.up * 0.5f, "Counter");
            }

            // Draw path line
            if (playerSpawnPoint != null && counterPosition != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(playerSpawnPoint.position, counterPosition.position);
            }
        }
#endif
    }
}