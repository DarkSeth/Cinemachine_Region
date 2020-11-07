using UnityEngine;
using UnityEditor;

namespace ActionCode.Cinemachine.Editor
{
    [CustomPropertyDrawer(typeof(RegionsDataPropertyAttribute))]
    public sealed class RegionsDataPropertyDrawer : PropertyDrawer
    {
        private bool hasRegionData;
        private RegionsData regionsData;
        private SerializedProperty property;

        private readonly GUIContent CREATE_REGION_SCENE = new GUIContent("Create Scene Region");
        private readonly GUIContent CREATE_REGION_DATA_ASSET = new GUIContent("Create new Region Data asset");

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            this.property = property;

            LoadRegionsData();
            hasRegionData = regionsData != null;

            DrawRegionPropertyField(ref rect, label);
            DrawCogPopupButton(rect);
        }

        private void DrawRegionPropertyField(ref Rect rect, GUIContent label)
        {
            const float width = 2F;
            var iconSize = rect.height + 4F;
            rect.width -= iconSize + width;
            EditorGUI.PropertyField(rect, property, label);
            rect.x += rect.width + width; rect.width = iconSize;

            if (!hasRegionData)
            {
                DrawNoRegionsDataAssetWarningMessage();
            }
            else if (regionsData.IsEmpty())
            {
                DrawNoSceneRegionsWarningMessage();
            }
        }

        private void DrawCogPopupButton(Rect rect)
        {
            var wasButtonDown = GUI.Button(rect, EditorGUIUtility.IconContent("_Popup"), GUI.skin.label);
            if (!wasButtonDown) return;

            var iconMenu = new GenericMenu();

            iconMenu.AddItem(CREATE_REGION_DATA_ASSET, false, CreateRegionsDataAsset);
            if (hasRegionData) iconMenu.AddItem(CREATE_REGION_SCENE, false, CreateSceneRegion);
            else iconMenu.AddDisabledItem(CREATE_REGION_SCENE);

            iconMenu.ShowAsContext();
        }

        private void CreateRegionsDataAsset()
        {
            var data = ScriptableObject.CreateInstance<RegionsData>();
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scenePath = scene != null ? System.IO.Path.GetDirectoryName(scene.path) : "Assets/Scenes";
            var name = $"{scene.name}-Regions";
            var path = EditorUtility.SaveFilePanelInProject("New Regions Data", name, "asset", string.Empty, scenePath);
            var isValidPath = !string.IsNullOrEmpty(path);
            if (isValidPath)
            {
                property.objectReferenceValue = data;
                property.serializedObject.ApplyModifiedProperties();

                CreateSceneRegion();

                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.Refresh();
            }
        }

        private void CreateSceneRegion()
        {
            var area = CinemachineRegionsConfiner.GetMainCameraFrameArea();
            var isAreaTooSmall = area.size.sqrMagnitude < 125F;
            if (isAreaTooSmall)
            {
                area = new Rect(area.position, new Vector2(10F, 5F));
            }

            // Expanding area
            const float EXPAND_FACTOR = 2.5F;
            area.min -= Vector2.one * EXPAND_FACTOR;
            area.max += Vector2.one * EXPAND_FACTOR;

            if (regionsData == null)
            {
                LoadRegionsData();
            }

            regionsData.Create(area);
        }

        private void DrawNoRegionsDataAssetWarningMessage()
        {
            const string msg = "A Regions Data asset is required. Create one using the Cog button above.";
            EditorGUILayout.HelpBox(msg, MessageType.Warning);
        }

        private void DrawNoSceneRegionsWarningMessage()
        {
            const string msg = "No Scene Regions were found. Create one using the Cog button above.";
            EditorGUILayout.HelpBox(msg, MessageType.Warning);
        }

        private void LoadRegionsData()
        {
            regionsData = property.objectReferenceValue as RegionsData;
        }
    }
}