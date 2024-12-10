using Clipper2Lib;
using UnityEngine;

namespace SMM
{
    public static class PathUtils
    {
        public static (GameObject, LineRenderer) PathDToLineRenderer(PathD path, GameObject prefab, Transform parent,
            string name, Color lineColor, float lineWidth, float positionZ)
        {
            var gameObject = Object.Instantiate(prefab, parent, true);
            gameObject.name = name;
            if (gameObject.TryGetComponent(out LineRenderer lineRenderer))
            {
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                int pointsCount = path.Count;
                lineRenderer.positionCount = pointsCount;
                var positions = new Vector3[pointsCount];
                for (int i = 0; i < pointsCount; i++)
                {
                    PointD position = path[i];
                    positions[i] = new Vector3((float)position.x, (float)position.y, positionZ);
                }
                lineRenderer.SetPositions(positions);
            }
            return (gameObject, lineRenderer);
        }

        public static Vector3 ConvertPointDToVector3(PointD point, float positionZ)
        {
            return new Vector3((float)point.x, (float)point.y, positionZ);
        }
    }
}
