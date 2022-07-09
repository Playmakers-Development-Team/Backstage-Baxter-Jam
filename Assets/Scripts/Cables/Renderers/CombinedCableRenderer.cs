using System.Collections.Generic;
using System.Linq;
using Cables.Pipes;
using UnityEngine;

namespace Cables.Renderers
{
    public class CombinedCableRenderer : CableRenderer
    {
        [Header("Combined Cable Renderer")]
        [SerializeField] private GameObject lineRendererPrefab;
        [SerializeField] private Transform lineRendererParent;

        private readonly List<CableMultiSegment> multiSegments = new List<CableMultiSegment>();
        private readonly Dictionary<CableMultiSegment, LineRenderer> lineRenderers =
            new Dictionary<CableMultiSegment, LineRenderer>();
        private readonly Dictionary<CableSegment, CableMultiSegment> segmentMultiSegments =
            new Dictionary<CableSegment, CableMultiSegment>();

        private class CableMultiSegment
        {
            public readonly List<CableSegment> Segments = new List<CableSegment>();
        }
        
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
            foreach (var multiSegment in lineRenderers.Keys)
            {
                UpdateLineRendererLerp(lineRenderers[multiSegment], GetTargetPoints(multiSegment));
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
            var segmentIndex = AddSegmentToMultiSegment(segment);

            var multiSegment = segmentMultiSegments[segment];

            if (segment.Node is PipeNode)
            {
                SplitMultiSegment(multiSegment, segmentIndex);
            }

            UpdateMultiSegmentRenderer(multiSegment);
        }

        private int AddSegmentToMultiSegment(CableSegment segment)
        {
            CableMultiSegment multiSegment = null;
            CableSegment nextSegment = null;

            if (multiSegments.Count == 0)
            {
                // If no multisegments exist, create one
                multiSegment = CreateCableMultiSegment(0);
            }
            else
            {
                // Find an existing multisegment to add the segment to
                nextSegment = Segments.Find(s => s.PreviousNode == segment.Node);

                multiSegment = nextSegment == null
                    ? multiSegments.Last()
                    : segmentMultiSegments[nextSegment];
            }

            // Add the segment to that multisegment in the right position
            var nextSegmentIndex =
                nextSegment == null ? multiSegment.Segments.Count : multiSegment.Segments.IndexOf(nextSegment);

            multiSegment.Segments.Insert(nextSegmentIndex, segment);

            segmentMultiSegments.Add(segment, multiSegment);
            
            return nextSegmentIndex;
        }

        private void SplitMultiSegment(CableMultiSegment multiSegment, int splitAfterIndex)
        {
            // Get all the segments following the added segment in the multisegment
            var segmentsToMove = multiSegment.Segments.Skip(splitAfterIndex + 1).ToList();

            // Create a new multisegment
            var multiSegmentIndex = multiSegments.IndexOf(multiSegment);
            var newMultiSegment = CreateCableMultiSegment(multiSegmentIndex + 1);

            foreach (var segmentToMove in segmentsToMove)
            {
                multiSegment.Segments.Remove(segmentToMove);

                newMultiSegment.Segments.Add(segmentToMove);
                
                segmentMultiSegments[segmentToMove] = newMultiSegment;
            }

            UpdateMultiSegmentRenderer(multiSegment);
        }

        private void UpdateMultiSegmentRenderer(CableMultiSegment multiSegment)
        {
            UpdateLineRendererInstant(lineRenderers[multiSegment], GetTargetPoints(multiSegment));
        }

        private CableMultiSegment CreateCableMultiSegment(int index)
        {
            // Create new multiSegment
            var lineRendererObject = Instantiate(lineRendererPrefab, lineRendererParent);
            var lineRenderer = lineRendererObject.GetComponent<LineRenderer>();

            InitialiseLineRenderer(lineRenderer);

            var newMultiSegment = new CableMultiSegment();

            lineRenderers.Add(newMultiSegment, lineRenderer);

            multiSegments.Insert(index, newMultiSegment);

            return newMultiSegment;
        }

        private void DestroyCableSegmentRenderer(CableSegment segment)
        {
            var multiSegment = segmentMultiSegments[segment];

            // TODO: At the moment there's no way to delete a PipeNode, so this hasn't been tested
            if (segment.Node is PipeNode)
            {
                var multiSegmentIndex = multiSegments.IndexOf(multiSegment);
                
                if (multiSegmentIndex < multiSegments.Count - 1)
                {
                    var nextMultiSegment = multiSegments[multiSegmentIndex + 1];

                    multiSegment.Segments.AddRange(nextMultiSegment.Segments);

                    // Also need to update all the references in segmentMultiSegments
                    foreach (var nextMultiSegmentSegment in nextMultiSegment.Segments)
                    {
                        segmentMultiSegments[nextMultiSegmentSegment] = multiSegment;
                    }

                    Destroy(lineRenderers[nextMultiSegment].gameObject);

                    lineRenderers.Remove(nextMultiSegment);

                    multiSegments.Remove(nextMultiSegment);
                }
            }

            multiSegment.Segments.Remove(segment);
            
            segmentMultiSegments.Remove(segment);

            if (multiSegment.Segments.Count == 0)
            {
                Destroy(lineRenderers[multiSegment].gameObject);

                lineRenderers.Remove(multiSegment);
                
                multiSegments.Remove(multiSegment);
            }
        }

        private List<Vector3> GetTargetPoints(CableMultiSegment multiSegment)
        {
            var points = new List<Vector2>();

            foreach (var segment in multiSegment.Segments)
            {
                points.AddRange(segment.points);
            }
            
            points.Add(multiSegment.Segments.Last().Node.Position);

            // TODO: ToList needs optimising, runs slow when lots of nodes.
            var points3D = SetZPositions(multiSegment, points).ToList();
            
            return points3D;
        }

        private IEnumerable<Vector3> SetZPositions(CableMultiSegment multiSegment, List<Vector2> points)
        {
            var multiSegmentIndex = multiSegments.IndexOf(multiSegment);
            
            var zPos = multiSegmentIndex % 2 == 0 ? -1 : 1;
            
            return points.Select(p => new Vector3(p.x, p.y, zPos));
        }
    }
}