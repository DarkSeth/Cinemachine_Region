using UnityEditor;
using UnityEngine;

namespace ActionCode.Cinemachine.Editor
{
    /// <summary>
    /// Handles buttons drawings on the Scene window.
    /// <para>
    /// This class was created using <see cref="UnityEditorInternal.Button"/> and <see cref="Handles"/> as references.
    /// This was necessary since the original ones didn't support drawing rectangles with different width and height.
    /// </para>
    /// </summary>
    public static class HandlesButton
    {
        private static readonly int RectButtonHash = "RectButtonHash".GetHashCode();
        private static readonly Vector3[] RectangleHandlePointsCache = new Vector3[5];

        private delegate void DrawRectCapFunction(int controlID, Rect area, EventType eventType);

        /// <summary>
        /// Draws a rectangular button with the given area.
        /// </summary>
        /// <param name="area">Area to draw the button.</param>
        /// <returns>True when the user clicks the button.</returns>
        public static bool RectButton(Rect area)
        {
            int id = GUIUtility.GetControlID(RectButtonHash, FocusType.Passive);
            return Do(id, area, DrawRectangleHandleCap);
        }

        private static bool Do(int id, Rect area, DrawRectCapFunction drawFunction)
        {
            var currentEvent = Event.current;
            var hasNearestControl = HandleUtility.nearestControl == id;
            var wasMouseLeftClick = currentEvent.button == 0;

            switch (currentEvent.GetTypeForControl(id))
            {
                case EventType.Layout:
                    if (GUI.enabled)
                    {
                        drawFunction(id, area, EventType.Layout);
                    }
                    break;

                case EventType.MouseMove:
                    if (hasNearestControl && wasMouseLeftClick)
                    {
                        HandleUtility.Repaint();
                    }
                    break;

                case EventType.MouseDown:
                    if (hasNearestControl && wasMouseLeftClick)
                    {
                        // Grab mouse focus
                        GUIUtility.hotControl = id;
                        currentEvent.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && wasMouseLeftClick)
                    {
                        GUIUtility.hotControl = 0;
                        currentEvent.Use();
                        return hasNearestControl;
                    }
                    break;

                case EventType.Repaint:
                    Color origColor = Handles.color;
                    if (hasNearestControl && GUI.enabled && GUIUtility.hotControl == 0)
                    {
                        Handles.color = Handles.preselectionColor;
                    }

                    drawFunction(id, area, EventType.Repaint);
                    Handles.color = origColor;
                    break;
            }

            return false;
        }

        private static void DrawRectangleHandleCap(int controlID, Rect area, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Layout:
                case EventType.MouseMove:
                    // TODO: Create DistanceToRectangle
                    HandleUtility.AddControl(controlID, DistanceToRectangle(area));
                    break;
                case (EventType.Repaint):
                    UpdateRectangleHandlePointsCache(area);
                    Handles.DrawPolyLine(RectangleHandlePointsCache);
                    break;
            }
        }

        private static float DistanceToRectangle(Rect area)
        {
            UpdateRectangleHandlePointsCache(area);
            var points = new Vector3[RectangleHandlePointsCache.Length];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = HandleUtility.WorldToGUIPoint(RectangleHandlePointsCache[i]);
            }

            var mousePos = Event.current.mousePosition;
            var oddNodes = false;
            var j = 4;

            for (int i = 0; i < 5; i++)
            {
                if ((points[i].y > mousePos.y) != (points[j].y > mousePos.y))
                {
                    if (mousePos.x < (points[j].x - points[i].x) * (mousePos.y - points[i].y) / (points[j].y - points[i].y) + points[i].x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            if (!oddNodes)
            {
                // Distance to closest edge (not so fast)
                float closestDist = -1f;
                j = 1;
                for (int i = 0; i < 4; i++)
                {
                    var dist = HandleUtility.DistancePointToLineSegment(mousePos, points[i], points[j++]);
                    if (dist < closestDist || closestDist < 0)
                        closestDist = dist;
                }
                return closestDist;
            }
            return 0;
        }

        private static void UpdateRectangleHandlePointsCache(Rect area)
        {
            var topLeftPos = area.position + Vector2.up * area.height;
            var bottomRightPos = area.position + Vector2.right * area.width;

            RectangleHandlePointsCache[0] = area.min;
            RectangleHandlePointsCache[1] = bottomRightPos;
            RectangleHandlePointsCache[2] = area.max;
            RectangleHandlePointsCache[3] = topLeftPos;
            RectangleHandlePointsCache[4] = RectangleHandlePointsCache[0];
        }
    }
}