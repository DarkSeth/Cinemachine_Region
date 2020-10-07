using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace ActionCode.Cinemachine.Editor
{
    [CustomEditor(typeof(CinemachineRegionsConfiner))]
    public class CinemachineRegionsConfinerEditor : UnityEditor.Editor
    {
        private readonly Color REGIONS_COLOR = new Color(0f, 1f, 0f, 0.4f);

        private CinemachineRegionsConfiner confiner;
        private RegionOverlaySceneWindow overlayWindow;

        private GUIStyle sceneLabelStyle;
        private BoxBoundsHandle currentRegionHandle;

        private Region currentRegion;

        private void OnEnable()
        {
            confiner = (CinemachineRegionsConfiner)target;
            overlayWindow = new RegionOverlaySceneWindow();

            currentRegionHandle = new BoxBoundsHandle
            {
                axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y
            };

            if (confiner.HasRegions())
            {
                currentRegion = confiner.regionsData.First;
            }

            InitializeGUIStyles();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!confiner.HasRegions())
            {
                DrawCreateRegionsDataButton();
            }
        }

        private void OnSceneGUI()
        {
            if (confiner.HasRegions())
            {
                DrawRegions();
                HandleCurrentRegion();
                overlayWindow.DisplayWindow(ref currentRegion);
            }
        }

        private void InitializeGUIStyles()
        {
            sceneLabelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerLeft,
                fontSize = 16
            };
            sceneLabelStyle.normal.textColor = REGIONS_COLOR;
        }

        private void DrawCreateRegionsDataButton()
        {
            if (GUILayout.Button("Create New Regions"))
            {
                CreateRegionsData();
            }
        }

        private void CreateRegionsData()
        {
            var data = CreateInstance<RegionsData>();
            var path = EditorUtility.SaveFilePanelInProject(
                "New Regions Data", "NewRegionsData", "asset", string.Empty, "/Scenes");
            var isValid = path.Length > 0;
            if (isValid)
            {
                var region = new Region("Region #0", new Rect(-20, -10, 40, 20));
                data.regions.Add(region);

                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.Refresh();

                confiner.regionsData = AssetDatabase.LoadAssetAtPath<RegionsData>(path);
            }
        }

        private void DrawRegions()
        {
            var lastHandlesColor = Handles.color;
            Handles.color = REGIONS_COLOR;

            foreach (var region in confiner.regionsData.regions)
            {
                Handles.Label(region.TopLeftPos, region.name, sceneLabelStyle);

                var selectRegion = HandlesButton.RectButton(region.area);
                if (selectRegion) currentRegion = region;
            }

            Handles.color = lastHandlesColor;
        }

        private void HandleCurrentRegion()
        {
            if (currentRegion == null) return;

            currentRegionHandle.center = currentRegion.area.center;
            currentRegionHandle.size = currentRegion.area.size;

            EditorGUI.BeginChangeCheck();
            currentRegionHandle.DrawHandle();
            var hasChanges = EditorGUI.EndChangeCheck();

            if (hasChanges)
            {
                // This order is important
                currentRegion.area.size = currentRegionHandle.size;
                currentRegion.area.center = currentRegionHandle.center;
            }
        }
    }
}
