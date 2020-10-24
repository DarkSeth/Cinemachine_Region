using UnityEditor;
using UnityEngine;

namespace ActionCode.Cinemachine.Editor
{
    /// <summary>
    /// Region overlay window.
    /// Draws a Window containing a Region information over the scene view.
    /// </summary>
    public class RegionOverlaySceneWindow
    {
        /// <summary>
        /// The window title.
        /// </summary>
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

            if (regionsEditor.HasRegions())
            {
                var hasSelectedRegion = regionsEditor.selectedRegion != null;
                if (hasSelectedRegion)
                {
                    DrawRegionFields();
                    GUILayout.Space(10F);
                    DrawRegionWorldPositions();
                }
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
            regionsEditor.selectedRegion.name = EditorGUILayout.TextField(regionsEditor.selectedRegion.name);
            regionsEditor.selectedRegion.area = EditorGUILayout.RectField("Area", regionsEditor.selectedRegion.area);
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
            regionsEditor.selectedRegion.Top = EditorGUILayout.FloatField("Top", regionsEditor.selectedRegion.Top);
            regionsEditor.selectedRegion.Bottom = EditorGUILayout.FloatField("Bottom", regionsEditor.selectedRegion.Bottom);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            regionsEditor.selectedRegion.Left = EditorGUILayout.FloatField("Left", regionsEditor.selectedRegion.Left);
            regionsEditor.selectedRegion.Right = EditorGUILayout.FloatField("Right", regionsEditor.selectedRegion.Right);
            EditorGUILayout.EndHorizontal();

            // Resets the value.
            EditorGUIUtility.labelWidth = 0F;
        }
    }
}
