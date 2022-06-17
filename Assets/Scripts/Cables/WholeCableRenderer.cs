﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cables
{
    public class WholeCableRenderer : CableRenderer
    {
        private enum ZPosFunctionID { None, Triangle, Slope }

        [SerializeField] private ZPosFunctionID zPosFunctionID;
        
        [Header("Experimental Features")]
        // TODO: Move to CableRenderer and implement on CableSegmentsRenderer.
        [SerializeField] private bool hangUnsupportedCables;

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();

            SetLineWidth(lineRenderer);
        }
        
        private void Update()
        {
            DrawLine();
        }

        protected override void OnInitialised()
        {
            base.OnInitialised();
            
            lineRenderer.material.mainTexture = cableSprite.texture; 
        }

        private void DrawLine()
        {
            var points = new List<Vector2>();

            // Connections between nodes
            for (var nodeIndex = 0; nodeIndex < Nodes.Count - 1; nodeIndex++)
            {
                if (SegmentIsSupported(nodeIndex) || !hangUnsupportedCables)
                {
                    points.AddRange(PointsBetweenPositions(Nodes[nodeIndex], Nodes[nodeIndex + 1]));
                }
                else
                {
                    var pointsBetweenPositions = PointsBetweenPositions(Nodes[nodeIndex], Nodes[nodeIndex + 1], CurveFunctions.CurveFunction.Catenary, 20);
                    
                    // Duplicate points to prevent tearing
                    if (nodeIndex > 0)
                    {
                        points.Add(pointsBetweenPositions.First());
                        points.Add(pointsBetweenPositions.First());
                    }

                    points.AddRange(pointsBetweenPositions);
                }
            }
            
            points.Add(Nodes.Last().transform.position);

            var points3D = GetZPosFunction(zPosFunctionID)(points).ToList();
            
            lineRenderer.positionCount = points3D.Count;
            lineRenderer.SetPositions(points3D.ToArray());
        }

        // TODO: Convert to a pure static function and move to base class
        // For every pair of points, use the slope to set the Z position
        private IEnumerable<Vector3> SetZPositionsWithSlope(List<Vector2> points)
        {
            var points3D = new List<Vector3>();
            
            for (var pointIndex = 0; pointIndex < points.Count - 1; pointIndex++)
            {
                var point = points[pointIndex];
                var nextPoint = points[pointIndex + 1];

                var node = Nodes[Mathf.FloorToInt(pointIndex / (float) pointsBetweenNodes)];

                OrientationUtil.OrientedVector2 vector = new OrientationUtil.OrientedVector2(nextPoint - point);
            
                float vectorSlope;

                // Prevent division by zero
                var nodeOrientation = NodeOrientation(node).Inverse();
                
                if (vector.X(nodeOrientation) == 0)
                {
                    vectorSlope = 20;
                }
                else
                {
                    vectorSlope = vector.Y(nodeOrientation) / vector.X(nodeOrientation);
                }

                points3D.Add(new Vector3(point.x, point.y, (Sigmoid(vectorSlope) - 0.5f) * 30));
            }

            return points3D;
        }

        private IEnumerable<Vector3> SetZPositionsWithTriangleWave(List<Vector2> points)
        {
            var points3D = new List<Vector3>();
            
            for (var pointIndex = 0; pointIndex < points.Count - 1; pointIndex++)
            {
                var point = points[pointIndex];

                var indexInSegment = pointIndex % pointsBetweenNodes;

                var evenSegment = Mathf.FloorToInt(pointIndex / (float)pointsBetweenNodes) == 0;

                var t = (float) indexInSegment / pointsBetweenNodes;

                float zPos = 0;

                if (evenSegment)
                {
                    zPos = Mathf.Abs(2 * t - 1) - 1;
                }
                else
                {
                    zPos = -1 * (Mathf.Abs(2 * t - 1) - 1);
                }
                
                points3D.Add(new Vector3(point.x, point.y, zPos));
            }

            return points3D;
        }

        private Func<List<Vector2>, IEnumerable<Vector3>> GetZPosFunction(ZPosFunctionID zPosFunctionID)
        {
            return zPosFunctionID switch
            {
                ZPosFunctionID.None => points => points.Select(p => (Vector3)p),
                ZPosFunctionID.Triangle => SetZPositionsWithTriangleWave,
                ZPosFunctionID.Slope => SetZPositionsWithSlope,
                _ => throw new ArgumentOutOfRangeException(nameof(zPosFunctionID), zPosFunctionID, null)
            };
        }

        private static float Sigmoid(double value) { return 1.0f / (1.0f + (float) Math.Exp(-value)); }

        private bool SegmentIsSupported(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= Nodes.Count - 1) throw new IndexOutOfRangeException();
            
            var node = Nodes[nodeIndex];
            var nextNode = Nodes[nodeIndex + 1];

            if (node.PolyCollider == null || nextNode.PolyCollider == null) return false;
            
            if (node.PolyCollider != nextNode.PolyCollider) return false;
            
            if (!CyclicPointsAreAdjacent(node.VertexIndex, nextNode.VertexIndex, node.PolyCollider.points.Length)) return false;
            
            if (!VectorPointsUp(node.ZAxisNormal.normalized + nextNode.ZAxisNormal.normalized)) return false;

            return true;
        }

        private static bool CyclicPointsAreAdjacent(int indexA, int indexB, int cycleLength)
        {
            var absDiff = Mathf.Abs(indexA - indexB);

            return absDiff == 1 || absDiff == cycleLength - 1;
        }

        private static bool VectorPointsUp(Vector2 vector)
        {
            return Vector2.Dot(vector, Vector2.up) > 0;
        }
    }
}