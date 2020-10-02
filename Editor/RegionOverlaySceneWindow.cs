using UnityEditor;
using UnityEngine;

namespace ActionCode.Cinemachine.Editor
{
    public class RegionOverlaySceneWindow
    {
        public GUIContent Title { get; private set; }

        public bool IsAbleToEdit { get; set; } = true;

        private readonly int Id;
        private readonly Vector2 ScenePanelSize = new Vector2(200F, 140F);
        private readonly Vector2 ScenePanelPadding = new Vector2(22F, 22F);

        public RegionOverlaySceneWindow(string title = "Regions")
        {
            Id = typeof(RegionOverlaySceneWindow).GetHashCode();
            Title = EditorGUIUtility.TrTextContent(title);
        }

        public void DisplayWindow()
        {
            var position = GetWindowsPosition();
            GUILayout.Window(Id, position, DisplayWindowContent, Title);
        }

        private Rect GetWindowsPosition()
        {
            var sceneSize = SceneView.currentDrawingSceneView.position.size;
            return new Rect(
                ScenePanelPadding.x,
                sceneSize.y - ScenePanelSize.y - ScenePanelPadding.y,
                ScenePanelSize.x, ScenePanelSize.y);
        }

        private void DisplayWindowContent(int windowID)
        {
            GUILayout.Space(15F);
            GUILayout.BeginVertical();

            EditorGUI.BeginDisabledGroup(!IsAbleToEdit);
            DisplayFields();
            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();
        }

        private void DisplayFields()
        {
            EditorGUILayout.TextField("Name", "");
            EditorGUILayout.RectField("Area", default);
        }
    }
}
