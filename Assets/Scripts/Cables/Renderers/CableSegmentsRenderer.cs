using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cables.Renderers
{
    public class CableSegmentsRenderer : CableRenderer
    {
        [Header("Cable Segments Renderer")]
        [SerializeField] private GameObject cableSegmentPrefab;
        [SerializeField] private Transform cableSegmentParent;

        private readonly Dictionary<CableSegment, LineRenderer> lineRenderers = new Dictionary<CableSegment, LineRenderer>();
        
        #region CableRenderer
        
        protected override void OnInitialised()
        {
            base.OnInitialised();
        
            foreach (var segment in cableSegmentsController.Segments)
            {
                OnSegmentCreated(segment);
            }
        
            // TODO: Unlisten
            cableSegmentsController.cableSegmentCreated.AddListener(OnSegmentCreated);
            cableSegmentsController.cableSegmentDestroyed.AddListener(OnSegmentDestroyed);
        }

        protected override void UpdateLineRenderers()
        {
            foreach (var segment in lineRenderers.Keys)
            {
                UpdateLineRendererLerp(lineRenderers[segment], GetTargetPoints(segment));
            }
        }

        #endregion
        
        private void OnSegmentCreated(CableSegment segment)
        {
            CreateCableSegmentRenderer(segment);
        }
        
        private void OnSegmentDestroyed(CableSegment segment)
        {
            DestroyCableSegmentRenderer(segment);
        }
        
        private void CreateCableSegmentRenderer(CableSegment segment)
        {
            var lineRendererObject = Instantiate(cableSegmentPrefab, cableSegmentParent);
            var lineRenderer = lineRendererObject.GetComponent<LineRenderer>();

            lineRenderer.sortingLayerName = "Cable";
            InitialiseLineRenderer(lineRenderer);
            
            lineRenderers.Add(segment, lineRenderer);
            
            UpdateLineRendererInstant(lineRenderers[segment], GetTargetPoints(segment));
        }
        
        private void DestroyCableSegmentRenderer(CableSegment segment)
        {
            Destroy(lineRenderers[segment].gameObject);
        
            lineRenderers.Remove(segment);
        }

        private List<Vector3> GetTargetPoints(CableSegment segment)
        {
            var points = segment.points;
            
            points.Add(segment.Node.Position);

            // TODO: ToList needs optimising, runs slow when lots of nodes.
            var points3D = SetZPositions(segment, points).ToList();
            
            return points3D;
        }

        private IEnumerable<Vector3> SetZPositions(CableSegment segment, List<Vector2> points)
        {
            var segmentIndex = Segments.IndexOf(segment);
            
            var zPos = segmentIndex % 2 == 0 ? -1 : 1;
            
            return points.Select(p => new Vector3(p.x, p.y, zPos));
        }
    }
}