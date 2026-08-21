using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Reusable configuration for beam visual properties.
    /// Create variants like "FastRapidFire", "ThickLaser", "ThinArrow" etc.
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Abilities/Beam Config")]
    public class BeamConfig : ScriptableObject
    {
        [Header("Visual Settings")]
        [Tooltip("How far before hit point the beam visual ends")]
        public float beamEndOffset = 0.5f;
        
        [Tooltip("Speed of texture scrolling (higher = faster movement)")]
        public float textureScrollSpeed = 20f;
        
        [Tooltip("Texture tiling scale (affects texture repeat)")]
        public float textureLengthScale = 2f;

        [Header("Beam Width")]
        [Tooltip("Minimum beam width at 0% charge")]
        public float minBeamWidth = 0.1f;
        
        [Tooltip("Maximum beam width at 100% charge")]
        public float maxBeamWidth = 0.4f;

        [Header("Particle Scaling")]
        [Tooltip("Min scale for start/end particles at 0% charge")]
        public float minParticleScale = 0.5f;
        
        [Tooltip("Max scale for start/end particles at 100% charge")]
        public float maxParticleScale = 1.5f;

        [Header("Prefab Scale Multipliers")]
        [Tooltip("Independent scale multiplier for Beam Start prefab (default 1.0). Reduce if start effect is too large.")]
        [Range(0.1f, 5f)]
        public float beamStartPrefabScale = 1.0f;
        
        [Tooltip("Independent scale multiplier for Beam End prefab (default 1.0). Reduce if end effect is too large.")]
        [Range(0.1f, 5f)]
        public float beamEndPrefabScale = 1.0f;

        [Header("Particle Behavior")]
        [Tooltip("Force particles to align with beam direction (for arrows/projectiles)")]
        public bool rotateParticles = true;
        
        [Tooltip("If true, particles use Local simulation space (inherit rotation)")]
        public bool useLocalParticleSpace = true;

        [Header("Particle Stretch (For Arrow Effects)")]
        [Tooltip("Enable stretched billboard rendering for arrow/projectile effects. Disable for normal explosions/impacts.")]
        public bool useStretchedBillboard = true;
        
        [Tooltip("Apply stretch only to beam START particles (recommended). If false, applies to all particles.")]
        public bool stretchOnlyStartParticles = true;
        
        [Tooltip("How much particles stretch based on velocity (0 = no stretch, higher = more stretch)")]
        [Range(0f, 3f)]
        public float particleSpeedScale = 0.3f;
        
        [Tooltip("Base particle length multiplier")]
        [Range(0f, 2f)]
        public float particleLengthScale = 0.5f;

        [Header("Particle Shape")]
        [Tooltip("Emission cone angle (lower = tighter, more focused stream)")]
        [Range(0f, 45f)]
        public float emissionConeAngle = 3f;
        
        [Tooltip("Spawn radius for particles (lower = spawn closer to origin)")]
        [Range(0f, 1f)]
        public float emissionRadius = 0.05f;

        [Header("2D Rendering")]
        [Tooltip("Sorting layer for LineRenderer")]
        public string sortingLayer = "Characters";
        
        [Tooltip("Sorting order for LineRenderer")]
        public int sortingOrder = 5;
        
        [Tooltip("Sorting order for particle effects")]
        public int particleSortingOrder = 100;

        [Header("Distance")]
        [Tooltip("Maximum beam distance in world units")]
        public float maxBeamDistance = 15f;
        
        [Tooltip("Layers that block the beam visually")]
        public LayerMask beamBlockingLayers = 0;
    }
}