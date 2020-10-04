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

        public Region this[int i]
        {
            get { return regions[i]; }
            set { regions[i] = value; }
        }
    }
}
