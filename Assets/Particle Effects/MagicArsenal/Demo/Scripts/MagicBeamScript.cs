using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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

        [Header("Adjustable Variables")]
        public float beamEndOffset = 1f;
        public float textureScrollSpeed = 8f;
        public float textureLengthScale = 3;

        [Header("Beam Width/Charge Scaling")]
        [Tooltip("When externally controlled, charge will lerp the line width between these values")]
        public float minBeamWidth = 0.05f;
        public float maxBeamWidth = 0.4f;
        [Tooltip("When externally controlled, scale start/end effect transforms between these values")]
        public float minStartScale = 0.2f;
        public float maxStartScale = 1.0f;

        [Header("2D Support")]
        [Tooltip("Enable for 2D games - uses screen to world point instead of raycasting")]
        public bool use2DMode = true;
        [Tooltip("Maximum beam distance in world units")]
        public float maxBeamDistance = 100f;
        [Tooltip("Layers that block the beam visually (leave as Nothing for infinite beam)")]
        public LayerMask beamBlockingLayers = 0;

        [Header("Particle Rotation")]
        [Tooltip("Force particle systems to align with beam direction (for arrow/projectile particles)")]
        public bool rotateParticles = true;

        [Header("Put Sliders here (Optional)")]
        public Slider endOffSetSlider;
        public Slider scrollSpeedSlider;

        [Header("Put UI Text object here to show beam name")]
        public Text textBeamName;

        private bool isFiringBeam = false;
        private bool isInitialized = false;

        // When set to true, this component will ignore its built-in mouse handling and accept external Activate/Deactivate/UpdateDirectionToPoint calls.
        [HideInInspector] public bool externalControl = false;
        private float currentChargePercent = 0f;
        private Vector3 originalStartScale = Vector3.one;
        private Vector3 originalEndScale = Vector3.one;

        // Cache particle systems for rotation
        private ParticleSystem[] startParticleSystems;
        private ParticleSystem[] endParticleSystems;

        // Use Awake instead of Start for immediate initialization
        void Awake()
        {
            transform = gameObject.transform;
            EnsureInitialized();
        }

        void Start()
        {
            if (textBeamName)
                textBeamName.text = beamLineRendererPrefab[(int)currentBeam].name;
            if (endOffSetSlider)
                endOffSetSlider.value = beamEndOffset;
            if (scrollSpeedSlider)
                scrollSpeedSlider.value = textureScrollSpeed;
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
            // Clean up existing objects if any
            if (beamStart != null) Destroy(beamStart);
            if (beamEnd != null) Destroy(beamEnd);
            if (beam != null) Destroy(beam);

            beamStart = Instantiate(beamStartPrefab[(int)currentBeam], Vector3.zero, Quaternion.identity, transform);
            beamEnd = Instantiate(beamEndPrefab[(int)currentBeam], Vector3.zero, Quaternion.identity, transform);
            beam = Instantiate(beamLineRendererPrefab[(int)currentBeam], Vector3.zero, Quaternion.identity, transform);
            line = beam.GetComponent<LineRenderer>();
            
            // IMPORTANT: Store the original scales from the prefabs
            if (beamStart != null)
                originalStartScale = beamStart.transform.localScale;
            if (beamEnd != null)
                originalEndScale = beamEnd.transform.localScale;
            
            // Cache particle systems for rotation
            if (beamStart != null)
                startParticleSystems = beamStart.GetComponentsInChildren<ParticleSystem>();
            if (beamEnd != null)
                endParticleSystems = beamEnd.GetComponentsInChildren<ParticleSystem>();
            
            // Ensure proper rendering in 2D
            if (use2DMode && line != null)
            {
                line.useWorldSpace = true;
                
                // Set sorting layer for 2D visibility
                line.sortingLayerName = "Characters";
                line.sortingOrder = 5;
                
                // Force alignment for 2D
                line.alignment = LineAlignment.TransformZ;
                
                // Ensure the line has a reasonable width
                line.startWidth = 0.2f;
                line.endWidth = 0.2f;
                
                // Make sure positions are set (even if zero initially)
                line.positionCount = 2;
                line.SetPosition(0, Vector3.zero);
                line.SetPosition(1, Vector3.forward);
                
                Debug.Log($"LineRenderer created - Sorting Layer: {line.sortingLayerName}, Order: {line.sortingOrder}, Width: {line.startWidth}, Material: {line.sharedMaterial?.name}, Positions: {line.positionCount}");
            }
            
            // Also set sorting for particle renderers if they exist
            SetParticleRendererSorting(beamStart);
            SetParticleRendererSorting(beamEnd);
            
            // Configure particle systems for directional emission
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
                renderer.sortingLayerName = "Characters";
                renderer.sortingOrder = 100;
            }
        }

        /// <summary>
        /// Configure particle systems to emit in the direction of the beam
        /// </summary>
        void ConfigureParticleSystemsForDirection()
        {
            if (!rotateParticles) return;

            // Configure beamStart particles to emit forward
            ConfigureParticleDirection(startParticleSystems, true);
            
            // Configure beamEnd particles (if needed)
            ConfigureParticleDirection(endParticleSystems, false);
        }

        void ConfigureParticleDirection(ParticleSystem[] systems, bool isStartEffect)
        {
            if (systems == null) return;

            foreach (var ps in systems)
            {
                if (ps == null) continue;

                var main = ps.main;
                
                // Use local space so particles inherit parent rotation
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                
                var shape = ps.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 0f; // Tight cone for arrow-like emission
                shape.radius = 0.1f;
                
                if (isStartEffect)
                {
                    // Start particles emit forward (in local Z+ direction for 2D)
                    shape.rotation = new Vector3(0, 90, 0); // Emit along local right (for 2D facing right)
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
                    beamStart.SetActive(true);
                    beamEnd.SetActive(true);
                    beam.SetActive(true);
                }
                if (Input.GetMouseButtonUp(0))
                {
                    isFiringBeam = false;
                    beamStart.SetActive(false);
                    beamEnd.SetActive(false);
                    beam.SetActive(false);
                }

                if (isFiringBeam)
                {
                    Vector3 targetPoint = GetTargetPoint();
                    Vector3 dir = targetPoint - transform.position;
                    ShootBeamInDir(transform.position, dir);
                }

                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    currentBeam = (BeamType)(((int)currentBeam + 1) % beamLineRendererPrefab.Length);
                    UpdateBeam();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    currentBeam = (BeamType)(((int)currentBeam - 1 + beamLineRendererPrefab.Length) % beamLineRendererPrefab.Length);
                    UpdateBeam();
                }
            }

            // Scroll texture even when externally controlled if active
            if (line != null && line.gameObject.activeSelf)
            {
                textureScrollOffset -= Time.deltaTime * textureScrollSpeed;
                if (textureScrollOffset < 0f)
                    textureScrollOffset += 1f;
                line.sharedMaterial.mainTextureOffset = new Vector2(textureScrollOffset, 0);
            }
        }

        Vector3 GetTargetPoint()
        {
            if (use2DMode)
            {
                // 2D Mode: Convert mouse to world point
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = transform.position.z; // Keep same Z as beam
                return mouseWorld;
            }
            else
            {
                // 3D Mode: Raycast
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit))
                {
                    return hit.point;
                }
                else
                {
                    return ray.origin + ray.direction * maxBeamDistance;
                }
            }
        }

        void UpdateBeam()
        {
            if (textBeamName)
                textBeamName.text = beamLineRendererPrefab[(int)currentBeam].name;
            Destroy(beamStart);
            Destroy(beamEnd);
            Destroy(beam);
            CreateBeamObjects();
        }

        void ShootBeamInDir(Vector3 start, Vector3 dir)
        {
            if (line == null)
            {
                Debug.LogError("Line is null in ShootBeamInDir!");
                return;
            }

            // Ensure Z is consistent in 2D
            if (use2DMode)
            {
                start.z = 0f;
            }

            line.SetPosition(0, start);
            beamStart.transform.position = start;

            Vector3 end = Vector3.zero;
            
            if (use2DMode)
            {
                // 2D: Calculate end point based on direction and max distance
                Vector2 start2D = start;
                Vector2 dir2D = new Vector2(dir.x, dir.y).normalized;
                
                // Only raycast if we have blocking layers set
                if (beamBlockingLayers.value != 0)
                {
                    RaycastHit2D hit2D = Physics2D.Raycast(start2D, dir2D, maxBeamDistance, beamBlockingLayers);
                    if (hit2D.collider != null)
                    {
                        end = hit2D.point - (dir2D * beamEndOffset);
                        end.z = 0f;
                    }
                    else
                    {
                        end = start + (dir.normalized * maxBeamDistance);
                        end.z = 0f;
                    }
                }
                else
                {
                    // No blocking layers - beam goes full distance
                    end = start + (dir.normalized * maxBeamDistance);
                    end.z = 0f;
                }
            }
            else
            {
                // 3D: Original behavior
                RaycastHit hit;
                if (Physics.Raycast(start, dir, out hit))
                    end = hit.point - (dir.normalized * beamEndOffset);
                else
                    end = transform.position + (dir.normalized * maxBeamDistance);
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
            line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
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
            
            Debug.Log($"Beam Activated - Start: {beamStart != null}, End: {beamEnd != null}, Beam: {beam != null}, Line: {line != null}");
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
                float w = Mathf.Lerp(minBeamWidth, maxBeamWidth, currentChargePercent);
                line.startWidth = w;
                line.endWidth = w;
            }
            if (beamStart != null)
            {
                float s = Mathf.Lerp(minStartScale, maxStartScale, currentChargePercent);
                // Multiply by original scale to respect prefab size
                beamStart.transform.localScale = originalStartScale * s;
            }
            if (beamEnd != null)
            {
                float s = Mathf.Lerp(minStartScale, maxStartScale, currentChargePercent);
                // Multiply by original scale to respect prefab size
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
    }
}