using UnityEngine;
using UnityEngine.AI;

namespace Havengard.Units
{
    /// <summary>
    /// Temporary debug script to diagnose animator issues.
    /// Remove after fixing animation.
    /// </summary>
    public class AnimatorDebug : MonoBehaviour
    {
        private Animator animator;
        private NavMeshAgent agent;
        
        private void Start()
        {
            animator = GetComponentInChildren<Animator>();
            agent = GetComponent<NavMeshAgent>();
            
            if (animator == null)
                Debug.LogError($"[{name}] NO ANIMATOR FOUND!");
            else
                Debug.Log($"[{name}] Animator found: {animator.runtimeAnimatorController?.name}");
                
            if (agent == null)
                Debug.LogError($"[{name}] NO NAVMESHAGENT FOUND!");
        }
        
        private void Update()
        {
            if (animator != null && agent != null)
            {
                float speed = agent.enabled && agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
                
                Debug.Log($"[{name}] Speed={speed:F2}, AnimSpeed={animator.GetFloat("Speed"):F2}, " +
                         $"IsStopped={agent.isStopped}, OnNavMesh={agent.isOnNavMesh}");
                
                // List all parameters
                foreach (var param in animator.parameters)
                {
                    Debug.Log($"  Param: {param.name} ({param.type})");
                }
            }
        }
    }
}