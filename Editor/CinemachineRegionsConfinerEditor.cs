using UnityEditor;
using UnityEngine;

namespace ActionCode.Cinemachine.Editor
{
    [CustomEditor(typeof(CinemachineRegionsConfiner))]
    public class CinemachineRegionsConfinerEditor : UnityEditor.Editor
    {
        private CinemachineRegionsConfiner confiner;

        private GUIStyle sceneLabelStyle;

        private readonly Color regionsColor = new Color(0f, 1f, 0f, 0.4f);

        private void OnEnable()
        {
            confiner = (CinemachineRegionsConfiner)target;

            InitializeGUIStyles();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawCreateRegionsDataButton();
        }

        private void OnSceneGUI()
        {
            DrawRegions();
        }

        private void InitializeGUIStyles()
        {
            sceneLabelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerLeft,
                fontSize = 16
            };
            sceneLabelStyle.normal.textColor = regionsColor;
        }

        private void DrawCreateRegionsDataButton()
        {
            if (confiner.HasRegions()) return;

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

                confiner.regions = AssetDatabase.LoadAssetAtPath<RegionsData>(path);
            }
        }

        private void DrawRegions()
        {
            if (!confiner.HasRegions()) return;

            Handles.color = regionsColor;
            foreach (var region in confiner.regions.regions)
            {
                Vector2 topLeft = region.area.position + Vector2.up * region.area.height;
                Vector2 bottomRight = region.area.position + Vector2.right * region.area.width;

                Handles.DrawLine(region.area.position, topLeft);
                Handles.DrawLine(topLeft, region.area.max);
                Handles.DrawLine(region.area.max, bottomRight);
                Handles.DrawLine(region.area.position, bottomRight);

                Handles.Label(topLeft, region.name, sceneLabelStyle);
            }
        }
    }
}
