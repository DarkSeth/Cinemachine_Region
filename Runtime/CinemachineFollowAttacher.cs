using Cinemachine;
using UnityEngine;

namespace ActionCode.Cinemachine
{
    /// <summary>
    /// Cinemachine Follow Attacher.
    /// <para>It will attach a Transform to be followed by the local VirtualCamera.</para>
    /// </summary>
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public sealed class CinemachineFollowAttacher : MonoBehaviour
    {
        [SerializeField, Tooltip("Virtual Camera to attach target Transform.")]
        private CinemachineVirtualCamera virtualCamera;
        [TagField, Tooltip("If set, it'll search and attach the first GameObject with this tag on Start function.")]
        public string tagOnStart = string.Empty;

        private void Reset()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        private void Start()
        {
            Attach(tagOnStart);
        }

        /// <summary>
        /// Attaches the given target GameObject to be followed by VirtualCamera.
        /// </summary>
        /// <param name="target">A GameObject to be followed.</param>
        public void Attach(GameObject target)
        {
            if (target) Attach(target.transform);
        }

        /// <summary>
        /// Attaches the given target to be followed by VirtualCamera.
        /// </summary>
        /// <param name="target">A Transform to be followed.</param>
        public void Attach(Transform target)
        {
            virtualCamera.Follow = target;
        }

        /// <summary>
        /// Attaches the first GameObject with the given tag to be followed by VirtualCamera.
        /// </summary>
        /// <param name="tag">A Tag to search for a GameObject.</param>
        public void Attach(string tag)
        {
            var invalidTag = tag.Length == 0;
            if (invalidTag) return;

            var target = GameObject.FindWithTag(tagOnStart);
            Attach(target);
        }
    }
}