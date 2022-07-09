using System.Linq;
using Cables.Renderers;
using UnityEngine;

namespace Cables
{
    public class CableCollider : MonoBehaviour
    {
        [SerializeField] private CableRenderer cableRenderer;
        [SerializeField] private EdgeCollider2D edgeCollider;
        [SerializeField] private int segmentsToSkipWhenInProgress;

        private void OnEnable()
        {
            cableRenderer.initialised.AddListener(OnInitialised);
            cableRenderer.pointsUpdated.AddListener(OnPointsUpdated);
        }

        private void OnDisable()
        {
            cableRenderer.pointsUpdated.RemoveListener(OnInitialised);
            cableRenderer.pointsUpdated.RemoveListener(OnPointsUpdated);
        }

        private void OnInitialised()
        {
            edgeCollider.edgeRadius = cableRenderer.Cable.cableWidth / 2;
        }

        private void OnPointsUpdated()
        {
            var cableInProgress = cableRenderer.Cable.state == CableController.CableState.InProgress;
            
            var segmentsToSkip = cableInProgress ? segmentsToSkipWhenInProgress : 0;
            
            if (cableRenderer.Segments.Count <= segmentsToSkip + 1)
            {
                edgeCollider.enabled = false;

                return;
            }

            var segments = cableRenderer.Segments
                .TakeWhile((_, i) => i < cableRenderer.Segments.Count - segmentsToSkip);
            
            var lastPoint = segments.Last().Node.Position;
            
            var points = segments
                .SelectMany(segment => segment.points)
                .Append(lastPoint)
                .Select(p => transform.InverseTransformPoint(p))
                .Select(p => (Vector2)p)
                .ToList();

            edgeCollider.enabled = true;

            edgeCollider.SetPoints(points);
        }
    }
}