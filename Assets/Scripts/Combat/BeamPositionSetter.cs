using UnityEngine;

namespace MyCombat
{
    internal sealed class BeamPositionSetter : MonoBehaviour
    {
        private LineRenderer _lineRenderer;

        public void Set(Vector3 startPosition, Vector3 endPosition)
        {
            if (_lineRenderer == null)
                return;

            _lineRenderer.SetPosition(0, startPosition);
            _lineRenderer.SetPosition(1, endPosition);
        }

        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }
    }
}
