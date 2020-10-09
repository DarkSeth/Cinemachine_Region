using UnityEngine;
using Cinemachine;
using Cinemachine.Utility;

namespace ActionCode.Cinemachine
{
    [ExecuteAlways]
    public class CinemachineRegionsConfiner : CinemachineExtension
    {
        [Tooltip("The regions which the camera is to be contained.")]
        public RegionsData regionsData;
        [Range(0F, 10F)]
        [Tooltip("How gradually to return the camera to the bounding volume if it goes beyond the borders.")]
        public float damping = 0F;

        public Region CurrentRegion { get; private set; }

        private Vector3 previousDisplacement;

        public bool HasRegions()
        {
            return regionsData != null;
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state, float deltaTime)
        {
            var isValidStage = stage == CinemachineCore.Stage.Finalize || stage == CinemachineCore.Stage.Body;
            if (!HasRegions() || !isValidStage) return;

            UpdateCurrentRegionBound(vcam.Follow);
            if (CurrentRegion == null) return;

            var isValidState = damping > 0 && deltaTime >= 0 &&
                VirtualCamera.PreviousStateIsValid;
            var displacement = state.Lens.Orthographic ?
                ConfineScreenEdges(ref state) :
                ConfinePoint(state.CorrectedPosition);

            if (isValidState)
            {
                Vector3 delta = displacement - previousDisplacement;
                delta = Damper.Damp(delta, damping, deltaTime);
                displacement = previousDisplacement + delta;
            }

            previousDisplacement = displacement;
            state.PositionCorrection += displacement;
        }

        private void UpdateCurrentRegionBound(Transform target)
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

        // Camera must be orthographic
        private Vector3 ConfineScreenEdges(ref CameraState state)
        {
            Quaternion rot = Quaternion.Inverse(state.CorrectedOrientation);
            float dy = state.Lens.OrthographicSize;
            float dx = dy * state.Lens.Aspect;
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
    }
}
