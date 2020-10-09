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
                var hasCurrentRegion = currentRegion != null;
                if (hasCurrentRegion)
                {
                    HandleCurrentRegion();
                    DrawCurrentRegionCreateButtons();
                    DrawCurrentRegionDeleteButton();
                }
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
                var area = new Rect(-20, -10, 40, 20);
                data.Create(area);

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

        private void DrawCurrentRegionDeleteButton()
        {
            var size = Vector2.one * 2F;
            var position = currentRegion.TopRightPos - size;
            var deleteButtonDown = HandlesButton.CrossButton(position, size, 0F);

            if (deleteButtonDown) DeleteRegion();
        }

        private void DrawCurrentRegionCreateButtons()
        {
            const float SKIN = 1.5F;
            var size = Vector2.one * 2F;

            var rightPos = currentRegion.CenterRightPos + Vector2.right * SKIN;
            var leftPos = currentRegion.CenterLeftPos + Vector2.left * SKIN;
            var topPos = currentRegion.TopPos + Vector2.up * SKIN;
            var bottomPos = currentRegion.BottomPos + Vector2.down * SKIN;

            var isRightButtonAvailable = !confiner.regionsData.Contains(rightPos);
            var isLeftButtonAvailable = !confiner.regionsData.Contains(leftPos);
            var isTopButtonAvailable = !confiner.regionsData.Contains(topPos);
            var isBottomButtonAvailable = !confiner.regionsData.Contains(bottomPos);

            var rightButtonDown =
                isRightButtonAvailable &&
                HandlesButton.ArrowButton(rightPos, size, 0F);
            var leftButtonDown =
                isLeftButtonAvailable &&
                HandlesButton.ArrowButton(leftPos, size, 180F);
            var topButtonDown =
                isTopButtonAvailable &&
                HandlesButton.ArrowButton(topPos, size, 90F);
            var bottomButtonDown =
                isBottomButtonAvailable &&
                HandlesButton.ArrowButton(bottomPos, size, 270F);

            if (rightButtonDown)
            {
                CreateRegion(Vector2.right, currentRegion.area.width);
            }
            else if (leftButtonDown)
            {
                CreateRegion(Vector2.left, currentRegion.area.width);
            }
            else if (topButtonDown)
            {
                CreateRegion(Vector2.up, currentRegion.area.height);
            }
            else if (bottomButtonDown)
            {
                CreateRegion(Vector2.down, currentRegion.area.height);
            }
        }

        private void CreateRegion(Vector2 direction, float distance)
        {
            var area = new Rect(currentRegion.area);
            area.position += direction * distance;
            confiner.regionsData.Create(area);
            currentRegion = confiner.regionsData.Last;
        }

        private void DeleteRegion()
        {
            confiner.regionsData.Delete(currentRegion);
            currentRegion = null;
        }
    }
}
