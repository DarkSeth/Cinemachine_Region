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

        private GUIStyle sceneLabelStyle;
        private CinemachineRegionsConfiner confiner;
        private BoxBoundsHandle currentRegionHandle;
        private bool showSelectedRegionContent = true;

        private void OnEnable()
        {
            confiner = (CinemachineRegionsConfiner)target;
            currentRegionHandle = new BoxBoundsHandle
            {
                axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y
            };
            selectedRegion = confiner.ContainsRegions() ? confiner.regionsData.First : null;

            SaveData();
            InitializeGUIStyles();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            DrawExtraInspectorGUI();
        }

        private void OnSceneGUI()
        {
            if (confiner.ContainsRegions())
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
        }

        /// <summary>
        /// Whether has a selected region.
        /// </summary>
        /// <returns></returns>
        public bool HasSelectedRegion()
        {
            return confiner.ContainsRegions() && selectedRegion != null;
        }

        internal void DrawExtraInspectorGUI()
        {
            DrawCreationButtons();
            DrawSelectedRegionContent();
        }

        private void DrawCreationButtons()
        {
            if (!confiner.HasRegionsData())
            {
                DrawCreateRegionsDataButton();
            }
            else if (!confiner.ContainsRegions())
            {
                DrawCreateFirstRegionButton();
            }
        }

        private void DrawCreateRegionsDataButton()
        {
            const string msg = "Cinemachine Regions Confiner requires a Regions Data asset.";
            EditorGUILayout.HelpBox(msg, MessageType.Warning);

            if (GUILayout.Button("Create New Regions Data"))
            {
                CreateRegionsData();
                UpdateEditorGUI();
            }
        }

        private void DrawCreateFirstRegionButton()
        {
            const string msg = "No Regions were found.";
            EditorGUILayout.HelpBox(msg, MessageType.Warning);

            if (GUILayout.Button("Create First Region"))
            {
                CreateFirstRegion();
                UpdateEditorGUI();
            }
        }

        private void CreateFirstRegion()
        {
            var camera = Camera.main;
            var canCreateRegion = confiner.HasRegionsData() && camera;
            if (!canCreateRegion) return;

            Vector2 bottomLeftPos;
            Vector2 topRightPos;

            if (camera.orthographic)
            {
                bottomLeftPos = camera.ViewportToWorldPoint(Vector2.zero);
                topRightPos = camera.ViewportToWorldPoint(Vector2.one);
            }
            else
            {
                var distance = Mathf.Abs(camera.transform.position.z);
                bottomLeftPos = camera.ViewportToWorldPoint(new Vector3(0F, 0F, distance));
                topRightPos = camera.ViewportToWorldPoint(new Vector3(1F, 1F, distance));
            }

            var area = new Rect()
            {
                min = bottomLeftPos,
                max = topRightPos
            };

            confiner.regionsData.Create(area);
            selectedRegion = confiner.regionsData.First;
        }

        private void CreateRegionsData()
        {
            var data = CreateInstance<RegionsData>();
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scenePath = scene != null ? System.IO.Path.GetDirectoryName(scene.path) : "Assets/Scenes";
            var name = $"{scene.name}-Regions";
            var path = EditorUtility.SaveFilePanelInProject("New Regions Data", name, "asset", string.Empty, scenePath);
            var isValid = path.Length > 0;
            if (isValid)
            {
                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.Refresh();

                confiner.regionsData = AssetDatabase.LoadAssetAtPath<RegionsData>(path);
                CreateFirstRegion();
            }
        }

        private void DrawSelectedRegionContent()
        {
            if (!HasSelectedRegion()) return;

            showSelectedRegionContent = EditorGUILayout.Foldout(showSelectedRegionContent, "Selected Region", true, EditorStyles.foldoutHeader);

            if (showSelectedRegionContent)
            {
                EditorGUI.indentLevel++;

                DrawSelectedRegionFields();
                DrawSelectedRegionWorldPositions();
                EditorGUI.indentLevel--;
            }
        }

        private void DrawSelectedRegionFields()
        {
            selectedRegion.name = EditorGUILayout.TextField("Name", selectedRegion.name);
            selectedRegion.area = EditorGUILayout.RectField("Area", selectedRegion.area);
        }

        private void DrawSelectedRegionWorldPositions()
        {
            EditorGUILayout.LabelField("World Positions");

            EditorGUIUtility.labelWidth = 50F;

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
            sceneLabelStyle.normal.textColor = REGIONS_COLOR;
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
                    UpdateEditorGUI();
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
                UpdateEditorGUI();
                //SaveRegionsData();
            }
        }

        private void DrawCurrentRegionDeleteButton()
        {
            var size = Vector2.one * 2F;
            var position = selectedRegion.TopRightPos - size;
            var deleteButtonDown = HandlesButton.CrossButton(position, size, 0F);

            if (deleteButtonDown)
            {
                DeleteRegion();
                UpdateEditorGUI();
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
                UpdateEditorGUI();
            }
            else if (leftButtonDown)
            {
                CreateRegion(Vector2.left, selectedRegion.area.width);
                UpdateEditorGUI();
            }
            else if (topButtonDown)
            {
                CreateRegion(Vector2.up, selectedRegion.area.height);
                UpdateEditorGUI();
            }
            else if (bottomButtonDown)
            {
                CreateRegion(Vector2.down, selectedRegion.area.height);
                UpdateEditorGUI();
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

        private void UpdateEditorGUI()
        {
            EditorUtility.SetDirty(target);
        }
    }
}
