using MagicArsenal;
using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Adapter that wraps MagicBeamScript from the asset pack.
    /// Provides a clean interface for the ability system and applies BeamConfig settings.
    /// Optimized for 2D top-down games with 3D particle effects.
    /// </summary>
    [RequireComponent(typeof(MagicBeamScript))]
    public class BeamVisualAdapter : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Visual configuration for this beam")]
        [SerializeField] private BeamConfig beamConfig;

        [Header("2D Top-Down Settings")]
        [Tooltip("For 2D games: prevents particles from rotating with beam, keeping them upright")]
        [SerializeField] private bool preventParticleFlip = true;

        private MagicBeamScript beamScript;
        private bool isInitialized = false;

        private void Awake()
        {
            beamScript = GetComponent<MagicBeamScript>();
            
            if (beamConfig != null)
            {
                ApplyConfiguration(beamConfig);
            }
        }

        /// <summary>
        /// Apply a BeamConfig to the underlying MagicBeamScript
        /// </summary>
        public void ApplyConfiguration(BeamConfig config)
        {
            if (config == null || beamScript == null)
            {
                Debug.LogWarning("[BeamVisualAdapter] Cannot apply null config or beamScript not found");
                return;
            }

            beamConfig = config;

            // Apply settings to MagicBeamScript
            beamScript.beamEndOffset = config.beamEndOffset;
            beamScript.textureScrollSpeed = config.textureScrollSpeed;
            beamScript.textureLengthScale = config.textureLengthScale;
            beamScript.minBeamWidth = config.minBeamWidth;
            beamScript.maxBeamWidth = config.maxBeamWidth;
            beamScript.minStartScale = config.minParticleScale;
            beamScript.maxStartScale = config.maxParticleScale;
            beamScript.maxBeamDistance = config.maxBeamDistance;
            beamScript.beamBlockingLayers = config.beamBlockingLayers;

            // Configure particles after beam creates its objects
            StartCoroutine(ConfigureParticlesDelayed());

            isInitialized = true;

            Debug.Log($"[BeamVisualAdapter] Applied config '{config.name}' to beam");
        }

        private System.Collections.IEnumerator ConfigureParticlesDelayed()
        {
            // Wait for MagicBeamScript to instantiate its particle objects
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame(); // Extra frame to ensure instantiation

            if (beamConfig != null && beamConfig.rotateParticles)
            {
                // Find start and end particle systems separately
                Transform beamStartTransform = transform.Find(beamScript.beamStartPrefab[0].name + "(Clone)");
                Transform beamEndTransform = transform.Find(beamScript.beamEndPrefab[0].name + "(Clone)");

                int configuredCount = 0;

                if (beamStartTransform != null)
                {
                    ParticleSystem[] startParticles = beamStartTransform.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var ps in startParticles)
                    {
                        ConfigureParticleSystemForDirection(ps, true);
                        configuredCount++;
                    }
                }

                if (beamEndTransform != null && !beamConfig.stretchOnlyStartParticles)
                {
                    ParticleSystem[] endParticles = beamEndTransform.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var ps in endParticles)
                    {
                        ConfigureParticleSystemForDirection(ps, false);
                        configuredCount++;
                    }
                }
                else if (beamEndTransform != null)
                {
                    // Still configure end particles but without stretch
                    ParticleSystem[] endParticles = beamEndTransform.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var ps in endParticles)
                    {
                        ConfigureParticleSystemBasic(ps);
                        configuredCount++;
                    }
                }

                Debug.Log($"[BeamVisualAdapter] Configured {configuredCount} particle systems for 2D top-down");
            }
        }

        private void ConfigureParticleSystemForDirection(ParticleSystem ps, bool isStartEffect)
        {
            if (ps == null) return;

            // Skip stretch/velocity configuration for orb/glow effects (keep them as normal billboards)
            bool isOrbEffect = ps.name.ToLower().Contains("orb") || 
                               ps.name.ToLower().Contains("glow") || 
                               ps.name.ToLower().Contains("swirl") ||
                               ps.name.ToLower().Contains("aura");

            if (isOrbEffect)
            {
                var main = ps.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;

                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                }

                Debug.Log($"[BeamVisualAdapter] Applied orb/glow configuration (no stretch) to: {ps.name}");
                return;
            }

            // Original stretch/velocity configuration for arrow particles
            var mainModule = ps.main;
            
            // Local space for 2D top-down
            mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;
            // CRITICAL: Use Hierarchy scaling so child particles inherit parent GameObject scale
            mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
            
            // No pre-rotation
            mainModule.startRotation3D = false;
            mainModule.startRotation = 0;
            
            // Reduce lifetime to prevent off-screen particles (only for start effect)
            if (isStartEffect)
            {
                mainModule.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
            }

            // Configure renderer
            var particleRenderer = ps.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
            {
                // Only apply stretch if enabled and appropriate for this particle system
                if (beamConfig.useStretchedBillboard && isStartEffect)
                {
                    particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                    particleRenderer.cameraVelocityScale = 0f;
                    particleRenderer.velocityScale = beamConfig.particleSpeedScale;
                    particleRenderer.lengthScale = beamConfig.particleLengthScale;
                    particleRenderer.normalDirection = 1f;
                    
                    Debug.Log($"[BeamVisualAdapter] Applied STRETCHED billboard to {ps.name}");
                }
                else
                {
                    // Keep as Billboard for impact/end effects
                    particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                    
                    Debug.Log($"[BeamVisualAdapter] Applied BILLBOARD to {ps.name}");
                }
            }

            // Configure emission shape for start effects
            if (isStartEffect)
            {
                var shape = ps.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = beamConfig.emissionConeAngle;
                shape.radius = beamConfig.emissionRadius;
                shape.radiusThickness = 1f;
                shape.arc = 360f;
                
                // Use NEGATIVE rotation to emit in correct direction for 2D
                shape.rotation = new Vector3(0, -90, 0);
                
                shape.position = Vector3.zero;
                shape.scale = Vector3.one;

                // Velocity along -X
                var velocityOverLifetime = ps.velocityOverLifetime;
                velocityOverLifetime.enabled = true;
                velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
                velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-10f);
                velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0f);
                velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f);
            }
        }

        /// <summary>
        /// Basic configuration for end/impact particles (no stretch, no velocity override)
        /// </summary>
        private void ConfigureParticleSystemBasic(ParticleSystem ps)
        {
            if (ps == null) return;

            var main = ps.main;
            
            // Local space for 2D top-down
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            // CRITICAL: Use Hierarchy scaling so child particles inherit parent GameObject scale
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            // Keep renderer as Billboard for impact effects
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                Debug.Log($"[BeamVisualAdapter] Applied basic BILLBOARD to {ps.name}");
            }
        }

        /// <summary>
        /// Enable/disable external control mode (disables demo input handling)
        /// </summary>
        public void EnableExternalControl(bool enable)
        {
            if (beamScript != null)
            {
                beamScript.externalControl = enable;
                Debug.Log($"[BeamVisualAdapter] External control set to: {enable}");
            }
        }

        /// <summary>
        /// Activate the beam visual
        /// </summary>
        public void Activate()
        {
            if (beamScript != null)
            {
                beamScript.Activate();
            }
        }

        /// <summary>
        /// Deactivate the beam visual
        /// </summary>
        public void Deactivate()
        {
            if (beamScript != null)
            {
                beamScript.Deactivate();
            }
        }

        /// <summary>
        /// Set charge percentage (affects beam width)
        /// </summary>
        public void SetCharge(float percent)
        {
            if (beamScript != null)
            {
                beamScript.SetCharge(percent);
            }
        }

        /// <summary>
        /// Update beam direction to point at a target
        /// </summary>
        public void UpdateDirection(Vector3 targetPoint)
        {
            if (beamScript != null)
            {
                beamScript.UpdateDirectionToPoint(targetPoint);
            }
        }

        /// <summary>
        /// Get the current BeamConfig
        /// </summary>
        public BeamConfig GetConfig()
        {
            return beamConfig;
        }
    }
}