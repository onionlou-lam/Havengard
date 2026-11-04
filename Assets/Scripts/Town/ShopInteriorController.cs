using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Havengard.Core;
using Havengard.Interactions;

namespace Havengard.Town
{
    /// <summary>
    /// Manages shop interior scenes. Auto-walks player to counter and starts the vendor dialogue.
    /// Dialogue and shop UI are handled by VendorNPC / DialogueManager / VendorShopUI.
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
        [Tooltip("The vendor NPC at the counter")]
        private VendorNPC vendor;

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
        private bool sequenceComplete;

        private void OnEnable()
        {
            PlayerInputLock.LockChanged += HandleLockChanged;
        }

        private void OnDisable()
        {
            PlayerInputLock.LockChanged -= HandleLockChanged;
        }

        private void Start()
        {
            StartCoroutine(ShopIntroSequence());
        }

        private IEnumerator ShopIntroSequence()
        {
            // Wait for scene to fully load
            yield return new WaitForSeconds(startDelay);

            SetupPlayer();

            if (playerInstance == null)
            {
                Debug.LogError("[ShopInteriorController] Failed to setup player!");
                yield break;
            }

            // Disable player input during the walk
            if (playerController != null)
                playerController.enabled = false;

            playerInstance.transform.position = playerSpawnPoint.position;

            yield return StartCoroutine(WalkToCounter());

            if (vendor != null)
                vendor.Interact();
            else
                Debug.LogWarning("[ShopInteriorController] No vendor assigned!");

            sequenceComplete = true;
        }

        private void SetupPlayer()
        {
            playerInstance = GameObject.FindGameObjectWithTag("Player");

            if (playerInstance == null && playerPrefab != null)
                playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);

            if (playerInstance != null)
            {
                playerAgent = playerInstance.GetComponent<NavMeshAgent>();
                playerController = playerInstance.GetComponent<PlayerController2D>();
            }
        }

        private IEnumerator WalkToCounter()
        {
            if (playerAgent == null || counterPosition == null)
                yield break;

            playerAgent.SetDestination(counterPosition.position);
            playerAgent.isStopped = false;

            while (Vector3.Distance(playerInstance.transform.position, counterPosition.position) > arrivalThreshold)
                yield return null;

            playerAgent.isStopped = true;
            playerAgent.velocity = Vector3.zero;
        }

        /// <summary>Gives control back once dialogue and shop UI have both released the input lock.</summary>
        private void HandleLockChanged(bool locked)
        {
            if (locked || !sequenceComplete) return;

            if (playerController != null)
                playerController.enabled = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
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

            if (playerSpawnPoint != null && counterPosition != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(playerSpawnPoint.position, counterPosition.position);
            }
        }
#endif
    }
}