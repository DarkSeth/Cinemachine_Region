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

        //TODO get colors from Cinemachine Preferences
        private readonly Color REGIONS_AREA_COLOR = Color.green * 0.6F;
        private readonly Color REGIONS_DELETE_BUTTON_COLOR = Color.red * 0.9F;
        private readonly Color REGIONS_CREATE_BUTTON_COLOR = Color.green * 1.8F;

        private GUIStyle sceneLabelStyle;
        private CinemachineRegionsConfiner confiner;
        private BoxBoundsHandle currentRegionHandle;

        private void OnEnable()
        {
            confiner = (CinemachineRegionsConfiner)target;
            currentRegionHandle = new BoxBoundsHandle()
            {
                axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y
            };
            selectedRegion = confiner.ContainsRegions() ? confiner.regionsData.First : null;

            InitializeGUIStyles();
        }

        private void OnDisable()
        {
            SaveRegionsData();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            DrawSelectedRegionContent();
        }

        private void OnSceneGUI()
        {
            if (confiner.ContainsRegions())
            {
                var lastHandlesColor = Handles.color;

                DrawRegions();
                var hasCurrentRegion = selectedRegion != null;
                if (hasCurrentRegion)
                {
                    HandleCurrentRegion();
                    DrawCurrentRegionCreateButtons();
                    DrawCurrentRegionDeleteButton();
                }

                Handles.color = lastHandlesColor;
            }
        }

        /// <summary>
        /// Whether has a selected region.
        /// </summary>
        /// <returns></returns>
        public bool HasSelectedRegion()
        {
            return confiner.ContainsRegions() && selectedRegion != null;
        }

        private void DrawSelectedRegionContent()
        {
            if (!HasSelectedRegion()) return;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.PrefixLabel("Selected Region", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            DrawSelectedRegionFields();
            DrawSelectedRegionWorldPositions();
            var hasChanges = EditorGUI.EndChangeCheck();
            if (hasChanges)
            {
                SaveRegionsData();
                UpdateEditorGUI();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedRegionFields()
        {
            selectedRegion.name = EditorGUILayout.TextField("Name", selectedRegion.name);
            //selectedRegion.area = EditorGUILayout.RectField("Area", selectedRegion.area);
        }

        private void DrawSelectedRegionWorldPositions()
        {
            EditorGUILayout.LabelField("World Positions");

            EditorGUIUtility.labelWidth = 80F;

            EditorGUILayout.BeginHorizontal();
            selectedRegion.Top = EditorGUILayout.FloatField("Top", selectedRegion.Top);
            selectedRegion.Bottom = EditorGUILayout.FloatField("Bottom", selectedRegion.Bottom);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            selectedRegion.Left = EditorGUILayout.FloatField("Left", selectedRegion.Left);
            selectedRegion.Right = EditorGUILayout.FloatField("Right", selectedRegion.Right);
            EditorGUILayout.EndHorizontal();

            // Resets the value.
            EditorGUIUtility.labelWidth = 0F;
        }

        private void InitializeGUIStyles()
        {
            sceneLabelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerLeft,
                fontSize = 16
            };
            sceneLabelStyle.normal.textColor = REGIONS_AREA_COLOR * 2F;
        }

        private void DrawRegions()
        {
            Handles.color = REGIONS_AREA_COLOR;

            foreach (var region in confiner.regionsData.regions)
            {
                Handles.Label(region.TopLeftPos, region.name, sceneLabelStyle);

                var selectRegion = HandlesButton.RectButton(region.area);
                if (selectRegion)
                {
                    selectedRegion = region;
                    UpdateEditorGUI();
                }
            }
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

                UpdateEditorGUI();
            }
        }

        private void DrawCurrentRegionDeleteButton()
        {
            var size = Vector2.one * 2F;
            var position = selectedRegion.TopRightPos - size;
            Handles.color = REGIONS_DELETE_BUTTON_COLOR;
            var deleteButtonDown = HandlesButton.CrossButton(position, size, 0F);

            if (deleteButtonDown)
            {
                DeleteRegion();
            }
        }

        private void DrawCurrentRegionCreateButtons()
        {
            const float SKIN = 2F;
            var size = Vector2.one * 2F;

            var rightPos = selectedRegion.CenterRightPos + Vector2.right * SKIN;
            var leftPos = selectedRegion.CenterLeftPos + Vector2.left * SKIN;
            var topPos = selectedRegion.TopPos + Vector2.up * SKIN;
            var bottomPos = selectedRegion.BottomPos + Vector2.down * SKIN;

            var isRightButtonAvailable = !confiner.regionsData.Contains(rightPos);
            var isLeftButtonAvailable = !confiner.regionsData.Contains(leftPos);
            var isTopButtonAvailable = !confiner.regionsData.Contains(topPos);
            var isBottomButtonAvailable = !confiner.regionsData.Contains(bottomPos);

            Handles.color = REGIONS_CREATE_BUTTON_COLOR;

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
            var area = new Rect(selectedRegion.area);
            area.position += direction * distance;
            confiner.regionsData.Create(area);
            selectedRegion = confiner.regionsData.Last;

            SaveRegionsData();
            UpdateEditorGUI();
        }

        private void DeleteRegion()
        {
            confiner.regionsData.Delete(selectedRegion);
            selectedRegion = null;

            SaveRegionsData();
            UpdateEditorGUI();
        }

        private void SaveRegionsData()
        {
            if (!confiner.HasRegionsData()) return;
            EditorUtility.SetDirty(confiner.regionsData);
        }

        private void UpdateEditorGUI()
        {
            EditorUtility.SetDirty(target);
        }
    }
}
