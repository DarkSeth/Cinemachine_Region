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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlID">The controller id.</param>
        /// <param name="area">The area to drawn.</param>
        /// <param name="collision">The area to check collision.</param>
        /// <param name="angle">The angle to draw.</param>
        /// <param name="eventType">The event type.</param>
        private delegate void DrawRectCapFunction(int controlID, Rect area, Rect collision, float angle, EventType eventType);


        /// <summary>
        /// Draws a rectangular button with the given area.
        /// </summary>
        /// <param name="area">Area to draw the button.</param>
        /// <param name="angle">The angle to draw the button.</param>
        /// <returns>True when the user clicks the button.</returns>
        public static bool RectButton(Rect area, float angle = 0F)
        {
            const float SKIN = 0.1F;
            var halfSize = new Vector2(area.width * SKIN, area.height * SKIN);
            var collision = area;
            collision.min += halfSize;
            collision.max -= halfSize;
            return RectButton(area, collision, angle);
        }

        /// <summary>
        /// Draws a rectangular button with the given area.
        /// </summary>
        /// <param name="area">Area to draw the button.</param>
        /// <param name="collision">The area to check collision with the button.</param>
        /// <param name="angle">The angle to draw the button.</param>
        /// <returns>True when the user clicks the button.</returns>
        public static bool RectButton(Rect area, Rect collision, float angle = 0F)
        {
            var id = GUIUtility.GetControlID(RectButtonHash, FocusType.Passive);
            return Do(id, area, collision, angle, DrawRectangleHandleCap);
        }

        private static bool Do(int id, Rect area, Rect collision, float angle, DrawRectCapFunction drawFunction)
        {
            var currentEvent = Event.current;
            var hasNearestControl = HandleUtility.nearestControl == id;
            var wasMouseLeftClick = currentEvent.button == 0;

            switch (currentEvent.GetTypeForControl(id))
            {
                case EventType.Layout:
                    if (GUI.enabled)
                    {
                        drawFunction(id, area, collision, angle, EventType.Layout);
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

                    drawFunction(id, area, collision, angle, EventType.Repaint);
                    Handles.color = origColor;
                    break;
            }

            return false;
        }

        private static void DrawRectangleHandleCap(int controlID, Rect area, Rect collision, float angle, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Layout:
                case EventType.MouseMove:
                    HandleUtility.AddControl(controlID, DistanceToRectangle(collision, angle));
                    break;
                case (EventType.Repaint):
                    UpdateRectangleHandlePointsCache(area, angle);
                    Handles.DrawPolyLine(RectangleHandlePointsCache);
                    break;
            }
        }

        private static float DistanceToRectangle(Rect collision, float angle)
        {
            UpdateRectangleHandlePointsCache(collision, angle);
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

        private static void UpdateRectangleHandlePointsCache(Rect area, float angle)
        {
            var topLeftPos = area.position + Vector2.up * area.height;
            var bottomRightPos = area.position + Vector2.right * area.width;

            RectangleHandlePointsCache[0] = area.min;
            RectangleHandlePointsCache[1] = bottomRightPos;
            RectangleHandlePointsCache[2] = area.max;
            RectangleHandlePointsCache[3] = topLeftPos;

            var applyRotation = Mathf.Abs(angle) > 0f;
            if (applyRotation)
            {
                for (int i = 0; i < 4; i++)
                {
                    RectangleHandlePointsCache[i] = RotateAroundPivot(RectangleHandlePointsCache[i], area.center, angle);
                }
            }

            RectangleHandlePointsCache[4] = RectangleHandlePointsCache[0];
        }

        private static Vector2 RotateAroundPivot(Vector2 point, Vector2 pivot, float angle)
        {
            const float TO_RADIANS = Mathf.PI / 180F;
            angle *= TO_RADIANS;
            Vector2 dir = point - pivot;
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);
            point.x = cosAngle * dir.x - sinAngle * dir.y + pivot.x;
            point.y = sinAngle * dir.x + cosAngle * dir.y + pivot.y;
            return point;
        }
    }
}