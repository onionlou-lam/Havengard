#if UNITY_EDITOR
using Havengard.Units;
using Havengard.Waves.UI.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Havengard.Waves.Editor
{
    /// <summary>
    /// Complete Wave System Setup Wizard
    /// Sets up everything you need for a functioning wave system
    /// </summary>
    public class WaveSystemSetupWizard : EditorWindow
    {
        private int spawnZoneCount = 2;
        private SpawnZoneLayout layoutType = SpawnZoneLayout.LeftRight;
        private bool createLightingController = true;
        private bool createPreWavePhase = true;
        private bool createWavePreviewPanel = true;
        private bool createDefaultTarget = true;
        private Vector3 defaultTargetPosition = Vector3.zero;

        private enum SpawnZoneLayout
        {
            LeftRight,
            Ring4Sides,
            Grid2x2,
            Custom
        }

        [MenuItem("Window/Wave System/Setup Wizard", false, 0)]
        public static void ShowWindow()
        {
            WaveSystemSetupWizard window = GetWindow<WaveSystemSetupWizard>("Wave System Setup");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Wave System Setup Wizard", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("This wizard will set up a complete wave system for your test level.", MessageType.Info);
            GUILayout.Space(10);

            // Spawn Zones
            GUILayout.Label("Spawn Zones", EditorStyles.boldLabel);
            layoutType = (SpawnZoneLayout)EditorGUILayout.EnumPopup("Layout Type", layoutType);

            if (layoutType == SpawnZoneLayout.Custom)
            {
                spawnZoneCount = EditorGUILayout.IntSlider("Zone Count", spawnZoneCount, 1, 8);
            }
            GUILayout.Space(10);

            // Optional Components
            GUILayout.Label("Optional Components", EditorStyles.boldLabel);
            createLightingController = EditorGUILayout.Toggle("Lighting Controller", createLightingController);
            createPreWavePhase = EditorGUILayout.Toggle("Pre-Wave Phase UI", createPreWavePhase);
            createWavePreviewPanel = EditorGUILayout.Toggle("Wave Preview Panel", createWavePreviewPanel);
            createDefaultTarget = EditorGUILayout.Toggle("Default Target (Gate)", createDefaultTarget);

            if (createDefaultTarget)
            {
                EditorGUI.indentLevel++;
                defaultTargetPosition = EditorGUILayout.Vector3Field("Target Position", defaultTargetPosition);
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(20);

            // Setup button
            if (GUILayout.Button("Setup Wave System", GUILayout.Height(40)))
            {
                SetupWaveSystem();
            }

            GUILayout.Space(10);

            // Quick actions
            GUILayout.Label("Quick Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Wave Definition Template"))
            {
                CreateWaveDefinitionTemplate();
            }

            if (GUILayout.Button("Create Wave Set Template"))
            {
                CreateWaveSetTemplate();
            }

            if (GUILayout.Button("Create Wave Audio Config"))
            {
                CreateWaveAudioConfig();
            }
        }

        private void SetupWaveSystem()
        {
            // Create WaveManager
            WaveManager waveManager = FindFirstObjectByType<WaveManager>();
            GameObject managerObj;

            if (waveManager == null)
            {
                managerObj = new GameObject("WaveManager");
                waveManager = managerObj.AddComponent<WaveManager>();
                Debug.Log("✓ Created WaveManager");
            }
            else
            {
                managerObj = waveManager.gameObject;
                Debug.Log("✓ Found existing WaveManager");
            }

            // Create spawn zones
            GameObject zonesParent = new GameObject("SpawnZones");
            zonesParent.transform.SetParent(managerObj.transform);

            Transform[] spawnZoneTransforms = null;

            switch (layoutType)
            {
                case SpawnZoneLayout.LeftRight:
                    spawnZoneTransforms = CreateLeftRightZones(zonesParent.transform);
                    break;
                case SpawnZoneLayout.Ring4Sides:
                    spawnZoneTransforms = CreateRingZones(zonesParent.transform);
                    break;
                case SpawnZoneLayout.Grid2x2:
                    spawnZoneTransforms = CreateGridZones(zonesParent.transform);
                    break;
                case SpawnZoneLayout.Custom:
                    spawnZoneTransforms = CreateCustomZones(zonesParent.transform, spawnZoneCount);
                    break;
            }

            // Wire up spawn zones
            var spawnZonesField = waveManager.GetType().GetField("spawnZones", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            spawnZonesField?.SetValue(waveManager, spawnZoneTransforms);
            Debug.Log($"✓ Created {spawnZoneTransforms.Length} spawn zones");

            // Create Lighting Controller
            if (createLightingController)
            {
                GameObject lightingObj = new GameObject("WaveLightingController");
                lightingObj.transform.SetParent(managerObj.transform);
                WaveLightingController lightingController = lightingObj.AddComponent<WaveLightingController>();

                // Try to find global light
                Light2D globalLight = FindFirstObjectByType<Light2D>();
                if (globalLight != null && globalLight.lightType == Light2D.LightType.Global)
                {
                    lightingController.GetType()
                        .GetField("globalLight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(lightingController, globalLight);
                    Debug.Log("✓ Created Lighting Controller (Global Light found)");
                }
                else
                {
                    Debug.Log("✓ Created Lighting Controller (Please assign Global Light manually)");
                }

                // Wire to WaveManager
                waveManager.GetType()
                    .GetField("lightingController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(waveManager, lightingController);
            }

            // Create Pre-Wave Phase
            if (createPreWavePhase)
            {
                GameObject phaseObj = new GameObject("PreWavePhase");
                phaseObj.transform.SetParent(managerObj.transform);
                PreWavePhase preWavePhase = phaseObj.AddComponent<PreWavePhase>();

                waveManager.GetType()
                    .GetField("preWavePhase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(waveManager, preWavePhase);

                Debug.Log("✓ Created Pre-Wave Phase");
            }

            // Create Wave Preview Panel
            if (createWavePreviewPanel)
            {
                WavePreviewPanelGenerator.CreateWavePreviewPanel();
                Debug.Log("✓ Created Wave Preview Panel");
            }

            // Create Default Target (Gate)
            if (createDefaultTarget)
            {
                GameObject gateObj = new GameObject("Gate_DefaultTarget");
                gateObj.transform.position = defaultTargetPosition;

                // Add Health component
                var health = gateObj.AddComponent<Havengard.Core.HealthSystem.Health>();
                health.SetFaction(Havengard.Units.Faction.Ally);
                health.SetStartingMaxHealth(1000);

                // Add DefaultTarget component
                var defaultTarget = gateObj.AddComponent<DefaultTarget>();
                defaultTarget.GetType()
                    .GetField("targetPriority", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(defaultTarget, 1);

                // Add visual placeholder
                SpriteRenderer sr = gateObj.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.5f, 0.5f, 0.8f, 0.8f);

                // Add collider for targeting
                BoxCollider2D collider = gateObj.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(2, 3);

                Debug.Log("✓ Created Default Target (Gate)");
            }

            EditorUtility.SetDirty(waveManager);
            Selection.activeGameObject = managerObj;

            EditorUtility.DisplayDialog("Wave System Setup Complete!",
                $"Wave system has been set up successfully!\n\n" +
                $"Next steps:\n" +
                $"1. Create Wave Definitions (Assets/Create/Havengard/Waves/Wave Definition)\n" +
                $"2. Create a Wave Set and assign your waves\n" +
                $"3. Assign the Wave Set to the WaveManager\n" +
                $"4. Configure audio clips in Wave Audio Config\n" +
                $"5. Set up your NavMesh for enemy pathfinding",
                "OK");

            Debug.Log("========================================");
            Debug.Log("WAVE SYSTEM SETUP COMPLETE!");
            Debug.Log("========================================");
        }

        private Transform[] CreateLeftRightZones(Transform parent)
        {
            GameObject left = CreateSpawnZone("SpawnZone_Left", new Vector3(-10f, 0f, 0f), parent);
            SetSpawnDirection(left, Vector3.right);

            GameObject right = CreateSpawnZone("SpawnZone_Right", new Vector3(10f, 0f, 0f), parent);
            SetSpawnDirection(right, Vector3.left);

            return new Transform[] { left.transform, right.transform };
        }

        private Transform[] CreateRingZones(Transform parent)
        {
            float radius = 15f;

            GameObject top = CreateSpawnZone("SpawnZone_Top", new Vector3(0f, radius, 0f), parent);
            SetSpawnDirection(top, Vector3.down);

            GameObject bottom = CreateSpawnZone("SpawnZone_Bottom", new Vector3(0f, -radius, 0f), parent);
            SetSpawnDirection(bottom, Vector3.up);

            GameObject left = CreateSpawnZone("SpawnZone_Left", new Vector3(-radius, 0f, 0f), parent);
            SetSpawnDirection(left, Vector3.right);

            GameObject right = CreateSpawnZone("SpawnZone_Right", new Vector3(radius, 0f, 0f), parent);
            SetSpawnDirection(right, Vector3.left);

            return new Transform[] { top.transform, bottom.transform, left.transform, right.transform };
        }

        private Transform[] CreateGridZones(Transform parent)
        {
            float spacing = 8f;

            GameObject tl = CreateSpawnZone("SpawnZone_TopLeft", new Vector3(-spacing, spacing, 0f), parent);
            GameObject tr = CreateSpawnZone("SpawnZone_TopRight", new Vector3(spacing, spacing, 0f), parent);
            GameObject bl = CreateSpawnZone("SpawnZone_BottomLeft", new Vector3(-spacing, -spacing, 0f), parent);
            GameObject br = CreateSpawnZone("SpawnZone_BottomRight", new Vector3(spacing, -spacing, 0f), parent);

            return new Transform[] { tl.transform, tr.transform, bl.transform, br.transform };
        }

        private Transform[] CreateCustomZones(Transform parent, int count)
        {
            Transform[] zones = new Transform[count];
            float angleStep = 360f / count;
            float radius = 12f;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

                GameObject zone = CreateSpawnZone($"SpawnZone_{i + 1}", position, parent);

                // Point toward center
                Vector3 directionToCenter = -position.normalized;
                SetSpawnDirection(zone, directionToCenter);

                zones[i] = zone.transform;
            }

            return zones;
        }

        private GameObject CreateSpawnZone(string name, Vector3 position, Transform parent)
        {
            GameObject zoneObj = new GameObject(name);
            zoneObj.transform.SetParent(parent, false);
            zoneObj.transform.position = position;

            SpawnZoneVisualizer visualizer = zoneObj.AddComponent<SpawnZoneVisualizer>();

            return zoneObj;
        }

        private void SetSpawnDirection(GameObject zone, Vector3 direction)
        {
            var visualizer = zone.GetComponent<SpawnZoneVisualizer>();
            if (visualizer != null)
            {
                visualizer.GetType()
                    .GetField("spawnDirection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(visualizer, direction);
            }
        }

        private void CreateWaveDefinitionTemplate()
        {
            // This would normally create a ScriptableObject, but we'll just show a message
            EditorUtility.DisplayDialog("Create Wave Definition",
                "To create a Wave Definition:\n\n" +
                "1. Right-click in Project window\n" +
                "2. Create → Havengard → Waves → Wave Definition\n" +
                "3. Configure wave settings and spawn groups",
                "OK");
        }

        private void CreateWaveSetTemplate()
        {
            EditorUtility.DisplayDialog("Create Wave Set",
                "To create a Wave Set:\n\n" +
                "1. Right-click in Project window\n" +
                "2. Create → Havengard → Waves → Wave Set\n" +
                "3. Assign your Wave Definitions to the array",
                "OK");
        }

        private void CreateWaveAudioConfig()
        {
            EditorUtility.DisplayDialog("Create Wave Audio Config",
                "To create Wave Audio Config:\n\n" +
                "1. Right-click in Project window\n" +
                "2. Create → Havengard → Waves → Wave Audio Config\n" +
                "3. Assign your audio clips",
                "OK");
        }
    }
}
#endif