using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Havengard.Abilities; // Import for BeamConfig

namespace MagicArsenal
{
    public enum BeamType
    {
        Type1,
        Type2,
        Type3
    }

    public class MagicBeamScript : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject[] beamLineRendererPrefab;
        public GameObject[] beamStartPrefab;
        public GameObject[] beamEndPrefab;

        private BeamType currentBeam = BeamType.Type1;
        private GameObject beamStart;
        private GameObject beamEnd;
        private GameObject beam;
        private LineRenderer line;
        private new Transform transform;
        private float textureScrollOffset;

        [Header("Configuration")]
        [Tooltip("Optional: Use a BeamConfig asset to override settings below")]
        public BeamConfig beamConfig;

        [Header("Adjustable Variables (Overridden by BeamConfig if set)")]
        public float beamEndOffset = 1f;
        public float textureScrollSpeed = 8f;
        public float textureLengthScale = 3;

        [Header("Beam Width/Charge Scaling")]
        public float minBeamWidth = 0.05f;
        public float maxBeamWidth = 0.4f;
        public float minStartScale = 0.2f;
        public float maxStartScale = 1.0f;

        [Header("Prefab Scale Multipliers (Overridden by BeamConfig if set)")]
        [Tooltip("Independent scale multiplier for Beam Start prefab (default 1.0). Reduce if start effect is too large.")]
        [Range(0.1f, 5f)]
        public float beamStartPrefabScale = 1.0f;
        
        [Tooltip("Independent scale multiplier for Beam End prefab (default 1.0). Reduce if end effect is too large.")]
        [Range(0.1f, 5f)]
        public float beamEndPrefabScale = 1.0f;

        [Header("2D Support")]
        public bool use2DMode = true;
        public float maxBeamDistance = 100f;
        public LayerMask beamBlockingLayers = 0;
        public bool rotateParticles = true;

        [Header("Dynamic Particle Lifetime")]
        [Tooltip("Adjust particle lifetime dynamically based on beam distance to prevent particles going through walls")]
        public bool dynamicParticleLifetime = true;
        [Tooltip("Particle speed in units per second (used to calculate lifetime)")]
        public float particleSpeed = 10f;

        [Header("Put Sliders here (Optional)")]
        public Slider endOffSetSlider;
        public Slider scrollSpeedSlider;

        [Header("Put UI Text object here to show beam name")]
        public Text textBeamName;

        private bool isFiringBeam = false;
        private bool isInitialized = false;

        [HideInInspector] public bool externalControl = false;
        private float currentChargePercent = 0f;
        private Vector3 originalStartScale = Vector3.one;
        private Vector3 originalEndScale = Vector3.one;

        private ParticleSystem[] startParticleSystems;
        private ParticleSystem[] endParticleSystems;

        // Cached settings (from BeamConfig or inspector)
        private float _beamEndOffset;
        private float _textureScrollSpeed;
        private float _textureLengthScale;
        private float _minBeamWidth;
        private float _maxBeamWidth;
        private float _minStartScale;
        private float _maxStartScale;
        private float _beamStartPrefabScale;
        private float _beamEndPrefabScale;
        private float _maxBeamDistance;
        private LayerMask _beamBlockingLayers;
        private bool _rotateParticles;
        private string _sortingLayer;
        private int _sortingOrder;
        private int _particleSortingOrder;

        private float currentBeamDistance = 0f;

        void Awake()
        {
            transform = gameObject.transform;
            ApplyBeamConfig();
            EnsureInitialized();
        }

        /// <summary>
        /// Apply BeamConfig if set, otherwise use inspector values
        /// </summary>
        void ApplyBeamConfig()
        {
            if (beamConfig != null)
            {
                _beamEndOffset = beamConfig.beamEndOffset;
                _textureScrollSpeed = beamConfig.textureScrollSpeed;
                _textureLengthScale = beamConfig.textureLengthScale;
                _minBeamWidth = beamConfig.minBeamWidth;
                _maxBeamWidth = beamConfig.maxBeamWidth;
                _minStartScale = beamConfig.minParticleScale;
                _maxStartScale = beamConfig.maxParticleScale;
                _beamStartPrefabScale = beamConfig.beamStartPrefabScale;
                _beamEndPrefabScale = beamConfig.beamEndPrefabScale;
                _maxBeamDistance = beamConfig.maxBeamDistance;
                _beamBlockingLayers = beamConfig.beamBlockingLayers;
                _rotateParticles = beamConfig.rotateParticles;
                _sortingLayer = beamConfig.sortingLayer;
                _sortingOrder = beamConfig.sortingOrder;
                _particleSortingOrder = beamConfig.particleSortingOrder;

                Debug.Log($"[MagicBeamScript] Applied BeamConfig: {beamConfig.name} - StartScale: {_beamStartPrefabScale}, EndScale: {_beamEndPrefabScale}");
            }
            else
            {
                // Use inspector values
                _beamEndOffset = beamEndOffset;
                _textureScrollSpeed = textureScrollSpeed;
                _textureLengthScale = textureLengthScale;
                _minBeamWidth = minBeamWidth;
                _maxBeamWidth = maxBeamWidth;
                _minStartScale = minStartScale;
                _maxStartScale = maxStartScale;
                _beamStartPrefabScale = beamStartPrefabScale;
                _beamEndPrefabScale = beamEndPrefabScale;
                _maxBeamDistance = maxBeamDistance;
                _beamBlockingLayers = beamBlockingLayers;
                _rotateParticles = rotateParticles;
                _sortingLayer = "Characters";
                _sortingOrder = 5;
                _particleSortingOrder = 100;
            }
        }

        void Start()
        {
            if (textBeamName)
                textBeamName.text = beamLineRendererPrefab[(int)currentBeam].name;
            if (endOffSetSlider)
                endOffSetSlider.value = _beamEndOffset;
            if (scrollSpeedSlider)
                scrollSpeedSlider.value = _textureScrollSpeed;
        }

        void EnsureInitialized()
        {
            if (isInitialized) return;
            
            if (beamLineRendererPrefab == null || beamLineRendererPrefab.Length == 0 ||
                beamStartPrefab == null || beamStartPrefab.Length == 0 ||
                beamEndPrefab == null || beamEndPrefab.Length == 0)
            {
                Debug.LogError("MagicBeamScript: Missing prefab references!");
                return;
            }
            
            CreateBeamObjects();
            isInitialized = true;
        }

        void CreateBeamObjects()
        {
            if (beamStart != null) Destroy(beamStart);
            if (beamEnd != null) Destroy(beamEnd);
            if (beam != null) Destroy(beam);

            beamStart = Instantiate(beamStartPrefab[(int)currentBeam], Vector3.zero, Quaternion.identity, transform);
            beamEnd = Instantiate(beamEndPrefab[(int)currentBeam], Vector3.zero, Quaternion.identity, transform);
            beam = Instantiate(beamLineRendererPrefab[(int)currentBeam], Vector3.zero, Quaternion.identity, transform);
            line = beam.GetComponent<LineRenderer>();
            
            // Store original scales from prefabs
            if (beamStart != null)
                originalStartScale = beamStart.transform.localScale;
            if (beamEnd != null)
                originalEndScale = beamEnd.transform.localScale;
            
            // Apply independent prefab scale multipliers immediately
            if (beamStart != null)
            {
                beamStart.transform.localScale = originalStartScale * _beamStartPrefabScale;
                // Update stored original to include the base multiplier
                originalStartScale = beamStart.transform.localScale;
                Debug.Log($"[MagicBeamScript] Applied Start Prefab Scale: {_beamStartPrefabScale} -> Final Scale: {originalStartScale}");
            }
            if (beamEnd != null)
            {
                beamEnd.transform.localScale = originalEndScale * _beamEndPrefabScale;
                // Update stored original to include the base multiplier
                originalEndScale = beamEnd.transform.localScale;
                Debug.Log($"[MagicBeamScript] Applied End Prefab Scale: {_beamEndPrefabScale} -> Final Scale: {originalEndScale}");
            }
            
            if (beamStart != null)
                startParticleSystems = beamStart.GetComponentsInChildren<ParticleSystem>();
            if (beamEnd != null)
                endParticleSystems = beamEnd.GetComponentsInChildren<ParticleSystem>();
            
            if (use2DMode && line != null)
            {
                line.useWorldSpace = true;
                line.sortingLayerName = _sortingLayer;
                line.sortingOrder = _sortingOrder;
                line.alignment = LineAlignment.TransformZ;
                line.startWidth = _minBeamWidth;
                line.endWidth = _minBeamWidth;
                line.positionCount = 2;
                line.SetPosition(0, Vector3.zero);
                line.SetPosition(1, Vector3.forward);
            }
            
            SetParticleRendererSorting(beamStart);
            SetParticleRendererSorting(beamEnd);
            ConfigureParticleSystemsForDirection();
            
            beamStart.SetActive(false);
            beamEnd.SetActive(false);
            beam.SetActive(false);
        }

        void SetParticleRendererSorting(GameObject particleObj)
        {
            if (!use2DMode || particleObj == null) return;
            
            ParticleSystemRenderer[] renderers = particleObj.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.sortingLayerName = _sortingLayer;
                renderer.sortingOrder = _particleSortingOrder;
            }
        }

        void ConfigureParticleSystemsForDirection()
        {
            if (!_rotateParticles) return;

            ConfigureParticleDirection(startParticleSystems, true);
            ConfigureParticleDirection(endParticleSystems, false);
        }

        void ConfigureParticleDirection(ParticleSystem[] systems, bool isStartEffect)
        {
            if (systems == null) return;

            foreach (var ps in systems)
            {
                if (ps == null) continue;

                var main = ps.main;
                
                if (beamConfig != null && beamConfig.useLocalParticleSpace)
                {
                    main.simulationSpace = ParticleSystemSimulationSpace.Local;
                }
                
                var shape = ps.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 0f;
                shape.radius = 0.1f;
                
                if (isStartEffect)
                {
                    shape.rotation = new Vector3(0, 90, 0);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!externalControl)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    Application.Quit();

                if (Input.GetMouseButtonDown(0))
                {
                    isFiringBeam = true;
                    if (beamStart) beamStart.SetActive(true);
                    if (beamEnd) beamEnd.SetActive(true);
                    if (beam) beam.SetActive(true);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    isFiringBeam = false;
                    if (beamStart) beamStart.SetActive(false);
                    if (beamEnd) beamEnd.SetActive(false);
                    if (beam) beam.SetActive(false);
                }
            }

            if (isFiringBeam)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                if (use2DMode)
                {
                    mousePos.z = 0f;
                }

                Vector3 start = transform.position;
                Vector3 dir = mousePos - start;
                ShootBeamInDir(start, dir);
            }

            if (line != null && isFiringBeam)
            {
                textureScrollOffset += Time.deltaTime * _textureScrollSpeed;
                if (line.sharedMaterial != null)
                {
                    line.sharedMaterial.mainTextureOffset = new Vector2(textureScrollOffset, 0);
                }
            }
        }

        void ShootBeamInDir(Vector3 start, Vector3 dir)
        {
            if (line == null) return;

            line.SetPosition(0, start);
            beamStart.transform.position = start;

            Vector3 end = Vector3.zero;

            if (use2DMode)
            {
                RaycastHit2D hit = Physics2D.Raycast(start, dir, _maxBeamDistance, _beamBlockingLayers);
                if (hit.collider != null)
                {
                    end = hit.point - (Vector2)(dir.normalized * _beamEndOffset);
                    end.z = 0f;
                }
                else
                {
                    end = start + (dir.normalized * _maxBeamDistance);
                    end.z = 0f;
                }
            }
            else
            {
                // 3D: Original behavior
                RaycastHit hit;
                if (Physics.Raycast(start, dir, out hit))
                    end = hit.point - (dir.normalized * _beamEndOffset);
                else
                    end = transform.position + (dir.normalized * _maxBeamDistance);
            }

            beamEnd.transform.position = end;
            line.SetPosition(1, end);

            // In 2D, we don't need full 3D LookAt - just rotate around Z axis
            if (use2DMode)
            {
                // Calculate angle for 2D rotation
                Vector2 direction = (end - start).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                
                beamStart.transform.rotation = Quaternion.Euler(0, 0, angle);
                beamEnd.transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
            }
            else
            {
                beamStart.transform.LookAt(beamEnd.transform.position);
                beamEnd.transform.LookAt(beamStart.transform.position);
            }

            float distance = Vector3.Distance(start, end);
            currentBeamDistance = distance;
            line.sharedMaterial.mainTextureScale = new Vector2(distance / _textureLengthScale, 1);

            // Dynamically adjust particle lifetime based on distance
            if (dynamicParticleLifetime)
            {
                UpdateParticleLifetimes(distance);
            }
        }

        /// <summary>
        /// Dynamically adjust particle lifetimes based on beam distance
        /// </summary>
        void UpdateParticleLifetimes(float beamDistance)
        {
            if (startParticleSystems == null) return;

            // Calculate how long particles should live to reach the end of the beam
            // Lifetime = Distance / Speed
            float calculatedLifetime = beamDistance / particleSpeed;
            
            // Clamp to reasonable values
            calculatedLifetime = Mathf.Clamp(calculatedLifetime, 0.1f, 2f);

            foreach (var ps in startParticleSystems)
            {
                if (ps == null) continue;

                // Skip orb/glow effects (they don't travel along the beam)
                if (ps.name.ToLower().Contains("orb") || 
                    ps.name.ToLower().Contains("glow") || 
                    ps.name.ToLower().Contains("swirl") ||
                    ps.name.ToLower().Contains("aura"))
                {
                    continue;
                }

                var main = ps.main;
                main.startLifetime = calculatedLifetime;
            }
        }

        // -- Public API for external channel/ability controllers --

        public void Activate()
        {
            // Ensure objects are created before activating
            EnsureInitialized();
            
            isFiringBeam = true;
            if (beamStart) beamStart.SetActive(true);
            if (beamEnd) beamEnd.SetActive(true);
            if (beam) beam.SetActive(true);
        }

        public void Deactivate()
        {
            isFiringBeam = false;
            if (beamStart) beamStart.SetActive(false);
            if (beamEnd) beamEnd.SetActive(false);
            if (beam) beam.SetActive(false);
        }

        public void SetCharge(float percent)
        {
            currentChargePercent = Mathf.Clamp01(percent);
            if (line != null)
            {
                float w = Mathf.Lerp(_minBeamWidth, _maxBeamWidth, currentChargePercent);
                line.startWidth = w;
                line.endWidth = w;
            }
            if (beamStart != null)
            {
                float s = Mathf.Lerp(_minStartScale, _maxStartScale, currentChargePercent);
                beamStart.transform.localScale = originalStartScale * s;
            }
            if (beamEnd != null)
            {
                float s = Mathf.Lerp(_minStartScale, _maxStartScale, currentChargePercent);
                beamEnd.transform.localScale = originalEndScale * s;
            }
        }

        public void UpdateDirectionToPoint(Vector3 point)
        {
            if (line == null || !isFiringBeam) return;
            Vector3 start = transform.position;
            Vector3 dir = point - start;
            ShootBeamInDir(start, dir);
        }

        /// <summary>
        /// Apply a BeamConfig at runtime (useful for ability systems)
        /// </summary>
        public void ApplyConfig(BeamConfig config)
        {
            beamConfig = config;
            ApplyBeamConfig();
        }

        public void ChangeBeamPrefab(int newBeamIndex)
        {
            if (newBeamIndex >= 0 && newBeamIndex < beamLineRendererPrefab.Length)
            {
                currentBeam = (BeamType)newBeamIndex;
                isInitialized = false;
                EnsureInitialized();
                
                if (textBeamName)
                    textBeamName.text = beamLineRendererPrefab[newBeamIndex].name;
            }
        }

        public void ChangeOffsetSliderValue()
        {
            _beamEndOffset = endOffSetSlider.value;
        }

        public void ChangeScrollSliderValue()
        {
            _textureScrollSpeed = scrollSpeedSlider.value;
        }
    }
}