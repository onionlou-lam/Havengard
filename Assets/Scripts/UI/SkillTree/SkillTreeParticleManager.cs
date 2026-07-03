using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Havengard.UI
{
    /// <summary>
    /// Manages particle effects for the skill tree, spawning them outside the canvas hierarchy
    /// so they render on top of UI elements.
    /// </summary>
    public class SkillTreeParticleManager : MonoBehaviour
    {
        [Header("Particle Prefabs")]
        [SerializeField] private ParticleSystem clickParticlePrefab;
        [SerializeField] private ParticleSystem hoverParticlePrefab;
        [SerializeField] private ParticleSystem unlockParticlePrefab;
        [SerializeField] private ParticleSystem pulseParticlePrefab;

        [Header("Settings")]
        [SerializeField] private int poolSize = 10;
        [SerializeField] private Camera uiCamera;

        private Dictionary<ParticleSystem, Queue<ParticleSystem>> particlePools;
        private Transform particleContainer;

        //-----------------------------------------------------

        private void Awake()
        {
            Debug.Log("=== SkillTreeParticleManager Awake ===");

            // Create container in world space
            GameObject container = new GameObject("SkillTreeParticles");
            particleContainer = container.transform;
            particleContainer.position = Vector3.zero;

            // Initialize pools
            particlePools = new Dictionary<ParticleSystem, Queue<ParticleSystem>>();

            // Check and initialize each prefab
            if (clickParticlePrefab != null)
            {
                Debug.Log($"✅ Click particle prefab found: {clickParticlePrefab.name}");
                InitializePool(clickParticlePrefab);
            }
            else
            {
                Debug.LogError("❌ Click particle prefab is NOT assigned!");
            }

            if (hoverParticlePrefab != null)
            {
                Debug.Log($"✅ Hover particle prefab found: {hoverParticlePrefab.name}");
                InitializePool(hoverParticlePrefab);
            }
            else
            {
                Debug.LogError("❌ Hover particle prefab is NOT assigned!");
            }

            if (unlockParticlePrefab != null)
            {
                Debug.Log($"✅ Unlock particle prefab found: {unlockParticlePrefab.name}");
                InitializePool(unlockParticlePrefab);
            }
            else
            {
                Debug.LogError("❌ Unlock particle prefab is NOT assigned!");
            }

            if (pulseParticlePrefab != null)
            {
                Debug.Log($"✅ Pulse particle prefab found: {pulseParticlePrefab.name}");
                InitializePool(pulseParticlePrefab);
            }
            else
            {
                Debug.LogError("❌ Pulse particle prefab is NOT assigned!");
            }

            // Auto-find camera
            if (uiCamera == null)
            {
                uiCamera = Camera.main;
                if (uiCamera != null)
                {
                    Debug.Log($"✅ UI Camera auto-assigned: {uiCamera.name}");
                }
                else
                {
                    Debug.LogError("❌ No Main Camera found!");
                }
            }
            else
            {
                Debug.Log($"✅ UI Camera assigned: {uiCamera.name}");
            }

            Debug.Log($"=== Pool initialized with {particlePools.Count} particle types ===");
        }

        //-----------------------------------------------------

        private void InitializePool(ParticleSystem prefab)
        {
            Queue<ParticleSystem> pool = new Queue<ParticleSystem>();

            for (int i = 0; i < poolSize; i++)
            {
                ParticleSystem ps = Instantiate(prefab, particleContainer);
                ps.gameObject.SetActive(false);
                pool.Enqueue(ps);
            }

            particlePools[prefab] = pool;
            Debug.Log($"[ParticleManager] Initialized pool for {prefab.name} with {poolSize} instances");
        }

        //-----------------------------------------------------

        private void Update()
        {
            // Test particle spawn with P key
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("=== MANUAL PARTICLE TEST ===");

                if (clickParticlePrefab != null && particlePools.ContainsKey(clickParticlePrefab))
                {
                    if (particlePools[clickParticlePrefab].Count > 0)
                    {
                        ParticleSystem ps = particlePools[clickParticlePrefab].Dequeue();

                        // Spawn in front of camera
                        Vector3 testPos = uiCamera.transform.position + uiCamera.transform.forward * 10f;
                        ps.transform.position = testPos;
                        ps.gameObject.SetActive(true);
                        ps.Clear();
                        ps.Play();

                        Debug.Log($"✅ Test particle at {testPos}, Active: {ps.gameObject.activeSelf}, Playing: {ps.isPlaying}");

                        StartCoroutine(ReturnToPoolAfterPlay(ps, clickParticlePrefab));
                    }
                    else
                    {
                        Debug.LogError("Pool is empty!");
                    }
                }
                else
                {
                    Debug.LogError("Click particle prefab not assigned or pool not initialized!");
                }
            }
        }

        //-----------------------------------------------------

        public void PlayParticleAtUI(ParticleSystem prefab, RectTransform uiElement)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[ParticleManager] Prefab is null!");
                return;
            }

            if (uiElement == null)
            {
                Debug.LogWarning("[ParticleManager] UI Element is null!");
                return;
            }

            if (!particlePools.ContainsKey(prefab))
            {
                Debug.LogError($"[ParticleManager] No pool for prefab: {prefab.name}");
                return;
            }

            if (particlePools[prefab].Count == 0)
            {
                Debug.LogWarning($"[ParticleManager] Pool empty for {prefab.name}");
                return;
            }

            ParticleSystem particle = GetFromPool(prefab);
            if (particle == null)
            {
                Debug.LogError("[ParticleManager] Failed to get particle from pool!");
                return;
            }

            // Get world position
            Vector3 worldPos = GetWorldPositionOfUI(uiElement);
            particle.transform.position = worldPos;

            // Configure renderer
            var renderer = particle.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = "UI";
                renderer.sortingOrder = 1000;
            }

            // Activate and play
            particle.gameObject.SetActive(true);
            particle.Clear();
            particle.Play();

            Debug.Log($"✅ [ParticleManager] Playing {particle.name} at {worldPos}, Active: {particle.gameObject.activeSelf}, Playing: {particle.isPlaying}");

            // Return to pool
            StartCoroutine(ReturnToPoolAfterPlay(particle, prefab));
        }

        //-----------------------------------------------------

        private Vector3 GetWorldPositionOfUI(RectTransform uiElement)
        {
            if (uiCamera == null)
            {
                Debug.LogError("[ParticleManager] UI Camera not assigned!");
                return Vector3.zero;
            }

            // Get the canvas
            Canvas canvas = uiElement.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError($"[ParticleManager] No canvas found for {uiElement.name}");
                return Vector3.zero;
            }

            Vector3 worldPos;

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // Get screen position from RectTransform
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, uiElement.position);

                // Convert to world position at canvas distance
                worldPos = canvas.worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, canvas.planeDistance - 0.5f));

                Debug.Log($"[ParticleManager] UI: {uiElement.name}, Screen: {screenPos}, World: {worldPos}, Canvas Plane: {canvas.planeDistance}");
            }
            else
            {
                Debug.LogWarning("[ParticleManager] Canvas is not Screen Space - Camera!");
                worldPos = uiElement.position;
            }

            return worldPos;
        }

        //-----------------------------------------------------

        private ParticleSystem GetFromPool(ParticleSystem prefab)
        {
            if (!particlePools.ContainsKey(prefab) || particlePools[prefab].Count == 0)
                return null;

            return particlePools[prefab].Dequeue();
        }

        //-----------------------------------------------------

        private IEnumerator ReturnToPoolAfterPlay(ParticleSystem ps, ParticleSystem prefab)
        {
            yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);

            ps.Stop();
            ps.gameObject.SetActive(false);

            if (particlePools.ContainsKey(prefab))
                particlePools[prefab].Enqueue(ps);
        }

        //-----------------------------------------------------

        public ParticleSystem PlayContinuousParticle(ParticleSystem prefab, RectTransform uiElement)
        {
            if (prefab == null || uiElement == null || !particlePools.ContainsKey(prefab))
                return null;

            ParticleSystem ps = GetFromPool(prefab);
            if (ps == null)
                return null;

            Vector3 worldPos = GetWorldPositionOfUI(uiElement);
            ps.transform.position = worldPos;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = "UI";
                renderer.sortingOrder = 1000;
            }

            ps.gameObject.SetActive(true);
            ps.Clear();
            ps.Play();

            return ps;
        }

        //-----------------------------------------------------

        public void StopContinuousParticle(ParticleSystem ps, ParticleSystem prefab)
        {
            if (ps == null || prefab == null)
                return;

            ps.Stop();
            ps.gameObject.SetActive(false);

            if (particlePools.ContainsKey(prefab))
                particlePools[prefab].Enqueue(ps);
        }
    }
}