using UnityEngine;

namespace SMM
{
    public class CubicBezierCurve
    {
        private Vector3[] points;


        public Vector3[] Points { get => points; set => points = value; }


        public CubicBezierCurve()
        {
            points = new Vector3[4];
        }

        public CubicBezierCurve(Vector3[] points)
        {
            this.points = points;
        }

        public Vector3 StartPosition
        {
            get { return points[0]; }
        }

        public Vector3 EndPosition
        {
            get { return points[3]; }
        }

        public Vector3 GetSegment(float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusTime = 1 - t;
            return (oneMinusTime * oneMinusTime * oneMinusTime * points[0])
                + (3 * oneMinusTime * oneMinusTime * t * points[1])
                + (3 * oneMinusTime * t * t * points[2])
                + (t * t * t * points[3]);
        }

        public Vector3[] GetSegments(int subdivisions)
        {
            Vector3[] segments = new Vector3[subdivisions];
            float t;
            for (int i = 0; i < subdivisions; i++)
            {
                t = (float)i / subdivisions;
                segments[i] = GetSegment(t);
            }
            return segments;
        }
    }
}
