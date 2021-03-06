using System;
using UnityEngine;

namespace ActionCode.Cinemachine
{
    /// <summary>
    /// Serializable class to hold Region data.
    /// </summary>
    [Serializable]
    public class Region : IEquatable<Region>
    {
        /// <summary>
        /// The region name.
        /// </summary>
        public string name;

        /// <summary>
        /// The region area.
        /// </summary>
        public Rect area;

        /// <summary>
        /// Creates a new region with the given name and area.
        /// </summary>
        /// <param name="name">The region name.</param>
        /// <param name="area">The region area.</param>
        public Region(string name, Rect area)
        {
            this.name = name;
            this.area = area;
        }

        /// <summary>
        /// The closest point on the region area.
        /// </summary>
        /// <param name="point">Arbitrary point.</param>
        /// <returns>The point on the area inside the region.</returns>
        public Vector3 ClosestPoint(Vector3 point)
        {
            //TODO: optimize this function in the future (remove Bound).
            var bounds = new Bounds(area.center, area.size);
            return bounds.ClosestPoint(point);
        }

        /// <summary>
        /// Returns true if the given point is inside this region area.
        /// </summary>
        /// <param name="point">Point to test.</param>
        /// <returns>True if the point lies within the area.</returns>
        public bool Contains(Vector2 point)
        {
            return area.Contains(point);
        }

        public bool Contains(Rect area)
        {
            return
                Contains(area.min) &&
                Contains(area.max);
        }

        public bool Equals(Region other)
        {
            return area.Equals(other.area);
        }

        public Vector2 TopLeftPos => BottomLeftPos + Vector2.up * area.height;

        public Vector2 TopPos => CenterPos + Vector2.up * area.height * 0.5F;

        public Vector2 TopRightPos => area.max;

        public Vector2 CenterLeftPos => BottomLeftPos + Vector2.up * area.height * 0.5F;

        public Vector2 CenterPos => area.center;

        public Vector2 CenterRightPos => TopRightPos + Vector2.down * area.height * 0.5F;

        public Vector2 BottomRightPos => BottomLeftPos + Vector2.right * area.width;

        public Vector2 BottomPos => CenterPos + Vector2.down * area.height * 0.5F;

        public Vector2 BottomLeftPos => area.min;

        /// <summary>
        /// Vertical maximum position.
        /// </summary>
        public float Top
        {
            get { return area.yMax; }
            set { area.yMax = value; }
        }

        /// <summary>
        /// Vertical minimum position.
        /// </summary>
        public float Bottom
        {
            get { return area.yMin; }
            set { area.yMin = value; }
        }


        /// <summary>
        /// Horizontal minimum position.
        /// </summary>
        public float Left
        {
            get { return area.xMin; }
            set { area.xMin = value; }
        }

        /// <summary>
        /// Horizontal maximum position.
        /// </summary>
        public float Right
        {
            get { return area.xMax; }
            set { area.xMax = value; }
        }
    }
}
