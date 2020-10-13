using UnityEditor;
using UnityEngine;

namespace ActionCode.Cinemachine.Editor
{
    public class RegionOverlaySceneWindow
    {
        public GUIContent Title { get; private set; }

        private readonly int Id;
        private readonly CinemachineRegionsConfinerEditor regionsEditor;
        private readonly Vector2 ScenePanelSize = new Vector2(200F, 140F);
        private readonly Vector2 ScenePanelPadding = new Vector2(10F, 30F);

        public RegionOverlaySceneWindow(CinemachineRegionsConfinerEditor regionsEditor, string title = "Current Region")
        {
            this.regionsEditor = regionsEditor;
            Id = typeof(RegionOverlaySceneWindow).GetHashCode();
            Title = EditorGUIUtility.TrTextContent(title);
        }

        public void DisplayWindow()
        {
            var position = GetWindowsPosition();
            GUILayout.Window(Id, position, DisplayRegionContent, Title);
        }

        private Rect GetWindowsPosition()
        {
            return new Rect(ScenePanelPadding, ScenePanelSize);
        }

        private void DisplayRegionContent(int windowID)
        {
            Handles.BeginGUI();
            GUILayout.BeginVertical();
            GUILayout.Space(10F);

            var hasRegion = regionsEditor.SelectedRegion != null;
            if (hasRegion)
            {
                DrawRegionFields();
                GUILayout.Space(10F);
                DrawRegionWorldPositions();
            }
            else
            {
                DrawCreateRegionButton();
            }

            GUILayout.EndVertical();
            Handles.EndGUI();
        }

        private void DrawRegionFields()
        {
            regionsEditor.SelectedRegion.name = EditorGUILayout.TextField(regionsEditor.SelectedRegion.name);
            regionsEditor.SelectedRegion.area = EditorGUILayout.RectField("Area", regionsEditor.SelectedRegion.area);
        }

        private void DrawCreateRegionButton()
        {
            const string msg = "No Region found.";
            EditorGUILayout.HelpBox(msg, MessageType.Info);

            var createRegion = GUILayout.Button("Create Region");
            if (createRegion) regionsEditor.CreateFirstRegion();
        }

        private void DrawRegionWorldPositions()
        {
            EditorGUILayout.LabelField("World Positions");

            EditorGUIUtility.labelWidth = 50F;

            EditorGUILayout.BeginHorizontal();
            regionsEditor.SelectedRegion.Top = EditorGUILayout.FloatField("Top", regionsEditor.SelectedRegion.Top);
            regionsEditor.SelectedRegion.Bottom = EditorGUILayout.FloatField("Bottom", regionsEditor.SelectedRegion.Bottom);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            regionsEditor.SelectedRegion.Left = EditorGUILayout.FloatField("Left", regionsEditor.SelectedRegion.Left);
            regionsEditor.SelectedRegion.Right = EditorGUILayout.FloatField("Right", regionsEditor.SelectedRegion.Right);
            EditorGUILayout.EndHorizontal();

            // Resets the value.
            EditorGUIUtility.labelWidth = 0F;
        }
    }
}
