using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cables.Renderers
{
    public class WholeCableRenderer : CableRenderer
    {
        [Header("Whole Cable Renderer")]
        [SerializeField] private LineRenderer lineRenderer;

        #region CableRenderer
        
        protected override void OnInitialised()
        {
            base.OnInitialised();
            
            InitialiseLineRenderer(lineRenderer);
        }

        protected override void UpdateLineRenderers()
        {
            UpdateLineRendererLerp(lineRenderer, GetPoints());
        }
        
        #endregion

        private List<Vector3> GetPoints()
        {
            var points = new List<Vector3>();

            for (var segmentIndex = 0; segmentIndex < Segments.Count; segmentIndex++)
            {
                var segment = Segments[segmentIndex];
                
                var segmentPoints = segment.points;

                points.AddRange(SetZPositions(segmentIndex, segmentPoints));
            }

            points.Add(Segments.Last().Node.Position);

            return points;
        }

        // TODO: Have tested and it doesn't work, for the same reason it always hasn't.
        // TODO: Needs to be tested. Need to get XYNodes working again first.
        // Sets Z positions according to a triangle wave.
        private IEnumerable<Vector3> SetZPositions(int segmentIndex, List<Vector2> points)
        {
            var segment = Segments[segmentIndex];

            var pointsBetweenNodes = segment.pointsBetweenNodes;

            var evenSegment = segmentIndex % 2 == 0;
            
            var points3D = new List<Vector3>();

            for (var pointIndex = 0; pointIndex < points.Count; pointIndex++)
            {
                var point = points[pointIndex];
            
                var t = (float) pointIndex / pointsBetweenNodes;
            
                float zPos = Mathf.Abs(1 * t - 1) - 1;
            
                if (!evenSegment)
                    zPos *= -1;
                
                points3D.Add(new Vector3(point.x, point.y, zPos));
            }

            return points3D;
        }
    }
}