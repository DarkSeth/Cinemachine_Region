using UnityEngine;

namespace ActionCode.Cinemachine
{
    public struct RegionTransition
    {
        private Vector3 start;
        private Vector3 end;
        private Vector3 direction;
        private float step;

        public bool IsTransition { get; private set; }

        public void Start(Vector3 nextPosition)
        {
            start = Camera.main.transform.position;
            end = nextPosition;
            direction = (end - start).normalized;
            step = 0F;
            IsTransition = true;
        }

        public void Update(ref Vector3 displacement, float speed, float deltaTime)
        {
            step += speed * deltaTime;

            var horizontalTransition = Mathf.Abs(direction.x) > 0.01F;
            var verticalTransition = Mathf.Abs(direction.y) > 0.01F;

            if (horizontalTransition) displacement.x *= step;
            if (verticalTransition) displacement.y *= step;

            IsTransition = step < 1F;
            if (!IsTransition) End();
        }

        public void End()
        {
            IsTransition = false;
            step = 0F;
        }

        public void Draw()
        {
            Debug.DrawLine(start, end, Color.red);
        }
    }
}