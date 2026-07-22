using UnityEngine;
using UnityEngine.AI;

namespace Havengard.Town
{
    /// <summary>
    /// Quick test script - click anywhere to make player navigate there.
    /// Attach to player temporarily to test NavMesh.
    /// </summary>
    public class NavMeshQuickTest : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Camera mainCamera;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            mainCamera = Camera.main;

            if (agent == null)
            {
                Debug.LogError("[NavMeshQuickTest] No NavMeshAgent found!");
                enabled = false;
                return;
            }

            Debug.Log($"[NavMeshQuickTest] Agent on NavMesh: {agent.isOnNavMesh}");
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;

                if (agent.SetDestination(mousePos))
                {
                    Debug.Log($"[NavMeshQuickTest] Moving to: {mousePos}");
                }
                else
                {
                    Debug.LogWarning($"[NavMeshQuickTest] Cannot reach: {mousePos}");
                }
            }
        }
    }
}