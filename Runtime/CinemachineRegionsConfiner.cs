using UnityEngine;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine.Events;

namespace ActionCode.Cinemachine
{
    /// <summary>
    /// Confiner extension for multiple regions.
    /// <para>The VirtualCamera will transition between them according.</para>
    /// </summary>
    [ExecuteAlways]
    public sealed class CinemachineRegionsConfiner : CinemachineExtension
    {
        [RegionsDataProperty]
        [Tooltip("The regions which the camera is to be contained.")]
        public RegionsData regionsData;
        [Min(MIN_TRANSITION_SPEED)]
        [SerializeField, Tooltip("Transition speed between regions.")]
        private float transitionSpeed = 0.6F;

        /// <summary>
        /// The current region that target is inside.
        /// </summary>
        public Region CurrentRegion { get; private set; }

        /// <summary>
        /// The last region that target was inside.
        /// </summary>
        public Region LastRegion { get; private set; }

        /// <summary>
        /// Whether a transition is in place.
        /// </summary>
        public bool IsTransition => transition.IsTransition;

        /// <summary>
        /// Transition speed between regions.
        /// </summary>
        public float TransitionSpeed
        {
            get => transitionSpeed;
            set => transitionSpeed = Mathf.Max(MIN_TRANSITION_SPEED, value);
        }

        /// <summary>
        /// Unity event fired when a transition between regions begins.
        /// <para>The first argument is the current Region and the second is the next one.</para>
        /// </summary>
        public UnityAction<Region, Region> OnRegionBeginChange;

        /// <summary>
        /// Unity event fired when a transition between regions has completed.
        /// <para>The first argument is the previous Region and the second is the current one.</para>
        /// </summary>
        public UnityAction<Region, Region> OnRegionChanged;

        private RegionTransition transition;
        private Region previousRegion;
        private Camera mainCamera;

        private const float MIN_TRANSITION_SPEED = 0.1F;

        /// <summary>
        /// Whether regions data is set.
        /// </summary>
        /// <returns></returns>
        public bool HasRegionsData()
        {
            return regionsData != null;
        }

        /// <summary>
        /// Whether regions data is not empty.
        /// </summary>
        /// <returns></returns>
        public bool ContainsRegions()
        {
            return HasRegionsData() && !regionsData.IsEmpty();
        }

        /// <summary>
        /// Returns the main camera frame area as a rectangle.
        /// </summary>
        /// <returns></returns>
        public static Rect GetMainCameraFrameArea()
        {
            var camera = Camera.main;
            if (camera == null) return default;

            Vector2 bottomLeftPos;
            Vector2 topRightPos;

            if (camera.orthographic)
            {
                bottomLeftPos = camera.ViewportToWorldPoint(Vector2.zero);
                topRightPos = camera.ViewportToWorldPoint(Vector2.one);
            }
            else
            {
                var distance = Mathf.Abs(camera.transform.position.z);
                bottomLeftPos = camera.ViewportToWorldPoint(new Vector3(0F, 0F, distance));
                topRightPos = camera.ViewportToWorldPoint(new Vector3(1F, 1F, distance));
            }

            return new Rect()
            {
                min = bottomLeftPos,
                max = topRightPos
            };
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state, float deltaTime)
        {
            var isValidStage = stage == CinemachineCore.Stage.Body;
            if (!HasRegionsData() || !isValidStage) return;

            previousRegion = CurrentRegion;
            UpdateCurrentRegion(vcam.Follow);

            if (CurrentRegion == null) return;

            CheckMainCamera();

            var displacement = ConfineScreenEdges(ref state);
            var isDifferentRegion = previousRegion != null &&
                CurrentRegion != previousRegion;
            var isValidState = deltaTime >= 0 &&
                VirtualCamera.PreviousStateIsValid;
            var startRegionTransition = isDifferentRegion &&
                isValidState &&
                Application.isPlaying;

            if (isDifferentRegion)
            {
                LastRegion = previousRegion;
            }

            if (startRegionTransition)
            {
                var nextPosition = state.CorrectedPosition + displacement;
                transition.Start(nextPosition);
                FireRegionBeginChangevent();
            }
            else if (IsTransition)
            {
                transition.Update(ref displacement, transitionSpeed, deltaTime);
                transition.Draw();

                var hasTransitionEnded = !transition.IsTransition;
                if (hasTransitionEnded)
                {
                    FireRegionChangedEvent();
                }
            }

            state.PositionCorrection += displacement;
        }

        private void UpdateCurrentRegion(Transform target)
        {
            if (regionsData.IsEmpty() || target == null)
            {
                CurrentRegion = default;
                return;
            }

            var selectedIndex = -1;
            for (int i = 0; i < regionsData.Count; i++)
            {
                if (regionsData[i].area.Contains(target.position))
                {
                    selectedIndex = i;
                    break;
                }
            }

            var isTargetOutsideRegions = selectedIndex < 0;
            if (isTargetOutsideRegions)
            {
                // Finds the closest region from target.
                var closestDistance = Mathf.Infinity;
                for (int i = 0; i < regionsData.Count; i++)
                {
                    var closestPosition = regionsData[i].ClosestPoint(target.position);
                    var distance = Vector3.Distance(target.position, closestPosition);
                    if (distance < closestDistance)
                    {
                        selectedIndex = i;
                        closestDistance = distance;
                    }
                }
            }

            CurrentRegion = regionsData[selectedIndex];
        }

        private Vector3 ConfineScreenEdges(ref CameraState state)
        {
            Quaternion rot = Quaternion.Inverse(state.CorrectedOrientation);
            float dy = mainCamera.orthographic ?
                mainCamera.orthographicSize :
                // vertical size for perspective = distance * Mathf.Tan(Field of View * 0.5F * Pi)
                Mathf.Abs(transform.position.z) * Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad * 0.5F);
            float dx = dy * mainCamera.aspect;// state.Lens.Aspect;
            Vector3 vx = (rot * Vector3.right) * dx;
            Vector3 vy = (rot * Vector3.up) * dy;

            Vector3 displacement = Vector3.zero;
            Vector3 camPos = state.CorrectedPosition;
            Vector3 lastD = Vector3.zero;
            const int kMaxIter = 12;
            for (int i = 0; i < kMaxIter; ++i)
            {
                Vector3 d = ConfinePoint((camPos - vy) - vx);
                if (d.AlmostZero())
                    d = ConfinePoint((camPos + vy) + vx);
                if (d.AlmostZero())
                    d = ConfinePoint((camPos - vy) + vx);
                if (d.AlmostZero())
                    d = ConfinePoint((camPos + vy) - vx);
                if (d.AlmostZero())
                    break;
                if ((d + lastD).AlmostZero())
                {
                    displacement += d * 0.5f;  // confiner too small: center it
                    break;
                }
                displacement += d;
                camPos += d;
                lastD = d;
            }
            return displacement;
        }

        private Vector3 ConfinePoint(Vector3 camPos)
        {
            if (CurrentRegion.Contains(camPos)) return Vector3.zero;

            var closest = CurrentRegion.ClosestPoint(camPos);
            closest.z = camPos.z;
            return closest - camPos;
        }

        private void FireRegionBeginChangevent()
        {
            if (OnRegionBeginChange != null)
            {
                OnRegionBeginChange.Invoke(LastRegion, CurrentRegion);
            }
        }

        private void FireRegionChangedEvent()
        {
            if (OnRegionChanged != null)
            {
                OnRegionChanged.Invoke(LastRegion, CurrentRegion);
            }
        }

        private void OnDrawGizmosSelected()
        {
            DrawCameraFrameArea();
        }

        private void DrawCameraFrameArea()
        {
            var frame = GetMainCameraFrameArea();
            var bottomRight = frame.min + Vector2.right * frame.width;
            var topLeft = frame.min + Vector2.up * frame.height;
            var gizmosColor = Gizmos.color;

            Gizmos.color = Color.white;

            Gizmos.DrawLine(frame.min, bottomRight);
            Gizmos.DrawLine(bottomRight, frame.max);
            Gizmos.DrawLine(frame.max, topLeft);
            Gizmos.DrawLine(topLeft, frame.min);

            Gizmos.color = gizmosColor;
        }

        private void CheckMainCamera()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
    }
}
