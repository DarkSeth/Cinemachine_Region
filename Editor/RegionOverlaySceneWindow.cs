using UnityEditor;
using UnityEngine;

namespace ActionCode.Cinemachine.Editor
{
    public class RegionOverlaySceneWindow
    {
        public GUIContent Title { get; private set; }

        private readonly int Id;
        private readonly Vector2 ScenePanelSize = new Vector2(200F, 120F);
        private readonly Vector2 ScenePanelPadding = new Vector2(22F, 22F);

        private Region region;

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
            return new Rect(
                ScenePanelPadding.x,
                sceneSize.y - ScenePanelSize.y - ScenePanelPadding.y,
                ScenePanelSize.x, ScenePanelSize.y);
        }

        private void DisplayRegionContent(int windowID)
        {
            GUILayout.Space(15F);

            GUILayout.BeginVertical();
            DisplayCurrentRegionFields();
            GUILayout.EndVertical();
        }

        private void DisplayCurrentRegionFields()
        {
            var hasRegion = region != null;

            if (hasRegion)
            {
                region.name = EditorGUILayout.TextField("Name", region.name);
                region.area = EditorGUILayout.RectField("Area", region.area);
            }
            else
            {
                const string msg = "Select a Region to edit.";
                EditorGUILayout.HelpBox(msg, MessageType.Info);
            }
        }
    }
}
