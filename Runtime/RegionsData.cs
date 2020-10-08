using UnityEngine;
using System.Collections.Generic;

namespace ActionCode.Cinemachine
{
    /// <summary>
    /// Serializable container to hold Regions data.
    /// </summary>
    public class RegionsData : ScriptableObject
    {
        /// <summary>
        /// A region list.
        /// </summary>
        public List<Region> regions = new List<Region>();

        /// <summary>
        /// The number of regions.
        /// </summary>
        public int Count => regions.Count;

        /// <summary>
        /// The first region if available. Returns null otherwise.
        /// </summary>
        public Region First
        {
            get
            {
                if (!IsEmpty()) return regions[0];
                return null;
            }
        }

        /// <summary>
        /// The last region if available. Returns null otherwise.
        /// </summary>
        public Region Last
        {
            get
            {
                if (!IsEmpty()) return regions[regions.Count - 1];
                return null;
            }
        }

        /// <summary>
        /// Whether this container has any region.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        /// Returns true if the given point is inside any region area.
        /// </summary>
        /// <param name="position">Point to test.</param>
        /// <returns>True if the point lies within any area.</returns>
        public bool Contains(Vector2 position)
        {
            foreach (var region in regions)
            {
                if (region.Contains(position)) return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a new Region using the given data.
        /// </summary>
        /// <param name="area">The new region area.</param>
        public void Create(Rect area)
        {
            var name = $"Region #{Count + 1}";
            var region = new Region(name, area);
            regions.Add(region);
        }

        public Region this[int i]
        {
            get { return regions[i]; }
            set { regions[i] = value; }
        }
    }
}
