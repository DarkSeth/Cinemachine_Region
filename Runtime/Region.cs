using System;
using UnityEngine;

namespace ActionCode.Cinemachine
{
    /// <summary>
    /// Serializable struct to hold Region data.
    /// </summary>
    [Serializable]
    public struct Region
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
        /// The region unique Identifier.
        /// </summary>
        public Guid ID { get; private set; }

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
        /// Updates the <see cref="ID"/> property.
        /// </summary>
        public void UpdateID()
        {
            ID = Guid.NewGuid();
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
    }
}
