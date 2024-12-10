using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;

namespace SMM
{
    public class AngledLineRenderer
    {
        private enum DirectionNames
        {
            Up = 0,
            Left,
            Down,
            Right,
            DiagonalQuad1,
            DiagonalQuad2,
            DiagonalQuad3,
            DiagonalQuad4,
        }

        private readonly Vector2[] availableDirections =
        {
            Vector2.up,
            Vector2.left,
            Vector2.down,
            Vector2.right,
            Vector2.one,
            new Vector2(-1, 1),
            -Vector2.one,
            new Vector2(1, -1),
        };

        private readonly LineRenderer lineRenderer;
        private readonly PathD path;
        private readonly float cornerRadius;
        private readonly float smoothingLength;
        private readonly int smoothingSections;
        private readonly List<CubicBezierCurve> curves = new List<CubicBezierCurve>(); // TODO : delete if not using


        private const float PositionZ = -0.015f;
        private const float StopPositionZ = PositionZ - 0.01f;


        public List<CubicBezierCurve> Curves { get => curves; }


        public AngledLineRenderer(LineRenderer lineRenderer, PathD path, float cornerRadius, float smoothingLength, int smoothingSections)
        {
            this.lineRenderer = lineRenderer;
            this.path = path;
            this.cornerRadius = cornerRadius;
            this.smoothingLength = smoothingLength;
            this.smoothingSections = smoothingSections;
            UpdateCurve(); // TODO : setup seperate function to smooth initial curve? then do not update curves for this section of route?
        }

        // TODO : handle. will need to pass in original path points not use the smoothed line renderer points
        // TODO : create so supports 'movement' in the future?
        public Vector2 UpdateLine(Vector2 mousePosition, int closestPositionIndex)
        {
            var closestPoint = closestPositionIndex == 0 ? path[closestPositionIndex] : path[^1];
            var finalPosition = MapToClosestDirection(PathUtils.ConvertPointDToVector3(closestPoint, PositionZ), mousePosition);
            Debug.Log("closestPositionIndex: " + closestPositionIndex);
            if (closestPositionIndex == 0)
            {
                path.Insert(closestPositionIndex, new PointD(finalPosition.x, finalPosition.y));
            }
            else
            {
                path.Add(new PointD(finalPosition.x, finalPosition.y));
            }
            UpdateCurve(); // TODO : only recalculate curves on non-permanent routes?
            return finalPosition; // TODO : update this with new point after smooth
        }


        private Vector2 MapToClosestDirection(Vector2 startPoint, Vector2 mousePosition)
        {
            var currentDirection = mousePosition - startPoint;

            var smallestAngle = float.MaxValue;
            var direction = currentDirection;
            for (int i = 0; i < availableDirections.Length; i++)
            {
                var currentAngle = Vector2.Angle(currentDirection, availableDirections[i]);
                if (currentAngle < smallestAngle)
                {
                    direction = availableDirections[i];
                    smallestAngle = currentAngle;
                }
            }

            var mappedDirection = (Vector2)Vector3.Project(currentDirection, direction); // TODO : do this math ourselves with dot product?
            return startPoint + mappedDirection;
        }

        // TODO : re-name
        private void UpdateCurve()
        {
            curves.Clear();
            List<Vector3> newPoints = new List<Vector3>
            {
                PathUtils.ConvertPointDToVector3(path[0], PositionZ),
            };
            for (int i = 1; i < path.Count - 1; i++)
            {
                var lastPosition = PathUtils.ConvertPointDToVector3(path[i - 1], PositionZ);
                Debug.Log("Last Position: " + lastPosition);
                var position = PathUtils.ConvertPointDToVector3(path[i], PositionZ);
                Debug.Log("Position: " + position);
                var nextPosition = PathUtils.ConvertPointDToVector3(path[i + 1], PositionZ);
                Debug.Log("next position: " + nextPosition);

                var lastDirection = (position - lastPosition).normalized;
                var nextDirection = (nextPosition - position).normalized;
                // TODO - figure out how to set how far along these lines we want to set the points.
                // TODO - remove cornerRadius if do not use
                var point0 = lastPosition; // (position - lastDirection) * cornerRadius;
                var point3 = nextPosition; // (position + nextDirection) * cornerRadius; TODO

                var startTangent = (lastDirection + nextDirection) * smoothingLength;
                var endTangent = (lastDirection + nextDirection) * -smoothingLength;
                Vector3[] points = { point0, position + startTangent, position + endTangent, point3 };
                var curve = new CubicBezierCurve(points);
                curves.Add(curve);
                Vector3[] segments = curve.GetSegments(smoothingSections);
                newPoints.AddRange(segments);
                // newPoints.Add(position);
            }
            newPoints.Add(PathUtils.ConvertPointDToVector3(path[^1], PositionZ));

            lineRenderer.positionCount = newPoints.Count;
            lineRenderer.SetPositions(newPoints.ToArray());
        }
    }
}
