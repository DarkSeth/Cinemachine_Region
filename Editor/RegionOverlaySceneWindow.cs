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

        private bool showBounds = true;

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
                DrawRegionBounds();
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

        private void DrawRegionBounds()
        {
            showBounds = EditorGUILayout.Foldout(showBounds, "Bounds Positions", true);
            if (!showBounds) return;

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.Vector2Field("Top Left", regionsEditor.SelectedRegion.TopLeftPos);
            EditorGUILayout.Vector2Field("Top Right", regionsEditor.SelectedRegion.TopRightPos);

            EditorGUILayout.Vector2Field("Center", regionsEditor.SelectedRegion.CenterPos);

            EditorGUILayout.Vector2Field("Bottom Left", regionsEditor.SelectedRegion.BottomLeftPos);
            EditorGUILayout.Vector2Field("Bottom Right", regionsEditor.SelectedRegion.BottomRightPos);

            EditorGUI.EndDisabledGroup();
        }
    }
}
