using UnityEditor;
using UnityEngine;

namespace ActionCode.Cinemachine.Editor
{
    public class RegionOverlaySceneWindow
    {
        public GUIContent Title { get; private set; }

        private readonly int Id;
        private readonly Vector2 ScenePanelSize = new Vector2(200F, 140F);
        private readonly Vector2 ScenePanelPadding = new Vector2(10F, 30F);

        private Region region;
        private bool showBounds = true;

        public RegionOverlaySceneWindow(string title = "Current Region")
        {
            Id = typeof(RegionOverlaySceneWindow).GetHashCode();
            Title = EditorGUIUtility.TrTextContent(title);
        }

        public void DisplayWindow(ref Region region)
        {
            this.region = region;
            var position = GetWindowsPosition();

            GUILayout.Window(Id, position, DisplayRegionContent, Title);
        }

        private Rect GetWindowsPosition()
        {
            var sceneSize = SceneView.currentDrawingSceneView.position.size;
            return new Rect(ScenePanelPadding, ScenePanelSize);
        }

        private void DisplayRegionContent(int windowID)
        {
            Handles.BeginGUI();
            GUILayout.BeginVertical();
            GUILayout.Space(10F);

            DrawRegionFields();
            DrawRegionBounds();

            GUILayout.EndVertical();
            Handles.EndGUI();
        }

        private void DrawRegionFields()
        {
            var hasRegion = region != null;

            if (hasRegion)
            {
                region.name = EditorGUILayout.TextField(region.name);
                region.area = EditorGUILayout.RectField("Area", region.area);
            }
            else
            {
                const string msg = "Select a Region to edit.";
                EditorGUILayout.HelpBox(msg, MessageType.Info);
            }
        }

        private void DrawRegionBounds()
        {
            showBounds = EditorGUILayout.Foldout(showBounds, "Bounds Positions", true);
            if (!showBounds) return;

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.Vector2Field("Top Left", region.TopLeftPos);
            EditorGUILayout.Vector2Field("Top Right", region.TopRightPos);

            EditorGUILayout.Vector2Field("Center", region.CenterPos);

            EditorGUILayout.Vector2Field("Bottom Left", region.BottomLeftPos);
            EditorGUILayout.Vector2Field("Bottom Right", region.BottomRightPos);

            EditorGUI.EndDisabledGroup();
        }
    }
}
