﻿using UnityEngine;
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

        public Bounds region;

        /// <summary>
        /// The number of regions.
        /// </summary>
        public int Count => regions.Count;

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
