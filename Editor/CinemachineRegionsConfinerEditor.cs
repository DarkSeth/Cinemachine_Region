using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace ActionCode.Cinemachine.Editor
{
    /// <summary>
    /// Custom editor for <see cref="CinemachineRegionsConfiner"/>.
    /// </summary>
    [CustomEditor(typeof(CinemachineRegionsConfiner))]
    public class CinemachineRegionsConfinerEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Current selected region. 
        /// </summary>
        public Region selectedRegion;

        private readonly Color REGIONS_COLOR = new Color(0f, 1f, 0f, 0.4f);

        private CinemachineRegionsConfiner confiner;
        private RegionOverlaySceneWindow overlayWindow;

        private GUIStyle sceneLabelStyle;
        private BoxBoundsHandle currentRegionHandle;

        private void OnEnable()
        {
            confiner = (CinemachineRegionsConfiner)target;
            overlayWindow = new RegionOverlaySceneWindow(this);

            currentRegionHandle = new BoxBoundsHandle
            {
                axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y
            };

            selectedRegion = HasRegions() ? confiner.regionsData.First : null;

            SaveData();
            InitializeGUIStyles();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!confiner.HasRegionsData())
            {
                DrawCreateRegionsDataButton();
            }
        }

        private void OnSceneGUI()
        {
            if (confiner.HasRegions())
            {
                DrawRegions();
                var hasCurrentRegion = selectedRegion != null;
                if (hasCurrentRegion)
                {
                    HandleCurrentRegion();
                    DrawCurrentRegionCreateButtons();
                    DrawCurrentRegionDeleteButton();
                }
            }

            overlayWindow.DisplayWindow();
        }

        internal void CreateFirstRegion()
        {
            if (!confiner.HasRegionsData()) return;

            Rect area;
            var camera = Camera.main;
            if (camera)
            {
                const float SKIN = 0.8f;
                Vector2 bottomLeftPos = camera.ViewportToWorldPoint(Vector2.zero);
                Vector2 topRightPos = camera.ViewportToWorldPoint(Vector2.one);

                // Expand
                bottomLeftPos -= Vector2.one * SKIN;
                topRightPos += Vector2.one * SKIN;

                var size = topRightPos - bottomLeftPos;
                area = new Rect(bottomLeftPos, size);
            }
            else area = new Rect(-20, -10, 40, 20);

            confiner.regionsData.Create(area);
            selectedRegion = confiner.regionsData.First;
        }

        internal bool HasRegions()
        {
            return confiner.HasRegions();
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
                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.Refresh();

                confiner.regionsData = AssetDatabase.LoadAssetAtPath<RegionsData>(path);
                CreateFirstRegion();
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
                if (selectRegion)
                {
                    SaveData();
                    selectedRegion = region;
                }
            }

            Handles.color = lastHandlesColor;
        }

        private void HandleCurrentRegion()
        {
            currentRegionHandle.center = selectedRegion.area.center;
            currentRegionHandle.size = selectedRegion.area.size;

            EditorGUI.BeginChangeCheck();
            currentRegionHandle.DrawHandle();
            var hasChanges = EditorGUI.EndChangeCheck();

            if (hasChanges)
            {
                // This order is important
                selectedRegion.area.size = currentRegionHandle.size;
                selectedRegion.area.center = currentRegionHandle.center;
                //SaveRegionsData();
            }
        }

        private void DrawCurrentRegionDeleteButton()
        {
            var size = Vector2.one * 2F;
            var position = selectedRegion.TopRightPos - size;
            var deleteButtonDown = HandlesButton.CrossButton(position, size, 0F);

            if (deleteButtonDown) DeleteRegion();
        }

        private void DrawCurrentRegionCreateButtons()
        {
            const float SKIN = 1.5F;
            var size = Vector2.one * 2F;

            var rightPos = selectedRegion.CenterRightPos + Vector2.right * SKIN;
            var leftPos = selectedRegion.CenterLeftPos + Vector2.left * SKIN;
            var topPos = selectedRegion.TopPos + Vector2.up * SKIN;
            var bottomPos = selectedRegion.BottomPos + Vector2.down * SKIN;

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
                CreateRegion(Vector2.right, selectedRegion.area.width);
            }
            else if (leftButtonDown)
            {
                CreateRegion(Vector2.left, selectedRegion.area.width);
            }
            else if (topButtonDown)
            {
                CreateRegion(Vector2.up, selectedRegion.area.height);
            }
            else if (bottomButtonDown)
            {
                CreateRegion(Vector2.down, selectedRegion.area.height);
            }
        }

        private void CreateRegion(Vector2 direction, float distance)
        {
            SaveData();
            var area = new Rect(selectedRegion.area);
            area.position += direction * distance;
            confiner.regionsData.Create(area);
            selectedRegion = confiner.regionsData.Last;
        }

        private void DeleteRegion()
        {
            SaveData();
            confiner.regionsData.Delete(selectedRegion);
            selectedRegion = null;
        }

        private void SaveData()
        {
            Undo.RecordObject(this, "Modify Cinemachine Regions Confiner Editor data");
            if (confiner.HasRegionsData())
            {
                Undo.RecordObject(confiner.regionsData, "Modify Regions data");
            }
        }
    }
}
