using System;
using System.Collections.Generic;
using UnityEngine;
using SMM.Input;

namespace SMM
{
    public class TransitLineGenerator : MonoBehaviour
    {
        private class TransitLine
        {
            private GameObject gameObject;
            private LineRenderer lineRenderer;


            public GameObject GameObject { get => gameObject; set => gameObject = value; }
            public LineRenderer LineRenderer { get => lineRenderer; set => lineRenderer = value; }


            public TransitLine(GameObject gameObject, LineRenderer lineRenderer)
            {
                this.gameObject = gameObject;
                this.lineRenderer = lineRenderer;
            }
        }


        [SerializeField]
        private InputController inputController = null;
        [SerializeField]
        private Camera mainCamera = null;
        [SerializeField]
        private GameObject map = null;
        [SerializeField]
        private List<Vector2> positionsRedLine = new List<Vector2>(); // TODO : remove eventually, for testing
        [SerializeField]
        private List<Vector2> positionsBlueLine = new List<Vector2>(); // TODO : remove eventually, for testing
        [SerializeField]
        private GameObject transitLinePrefab = null;


        [NonSerialized]
        private readonly List<TransitLine> transitLines = new List<TransitLine>();


        [NonSerialized]
        private const float PositionZ = -0.01f;


        protected void OnEnable()
        {
            // TODO : listen for input from tabletop instead of mouse
            inputController.Place += OnPlace;

            // TODO : instantiate transit lines from loaded file instead of placeholder red & blue lines
            SetupTransitLine(positionsRedLine, new Color(0.953f, 0.435f, 0.318f, 1.0f));
            SetupTransitLine(positionsBlueLine, new Color(0.337f, 0.722f, 0.914f, 1.0f));
        }

        protected void OnDisable()
        {
            inputController.Place -= OnPlace;
            foreach (var transitLine in transitLines)
            {
                Destroy(transitLine.GameObject);
            }
            transitLines.Clear();
        }


        private void SetupTransitLine(List<Vector2> points, Color color)
        {
            var transitLine = Instantiate(transitLinePrefab, map.transform, true);
            if (transitLine.TryGetComponent(out LineRenderer lineRenderer))
            {
                int pointsCount = points.Count;
                lineRenderer.positionCount = pointsCount;
                var positions = new Vector3[pointsCount];
                for (int i = 0; i < pointsCount; i++)
                {
                    var position = points[i];
                    positions[i] = new Vector3(position.x, position.y, PositionZ);
                }
                lineRenderer.SetPositions(positions);
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
            transitLines.Add(new TransitLine(transitLine, lineRenderer));
        }

        private void OnPlace()
        {
            var mousePosition = mainCamera.ScreenToWorldPoint(inputController.MousePosition);
            mousePosition.z = PositionZ;

            int finalPositionIndex = -1;
            TransitLine closestLine = null;
            float closestSqrMagnitude = float.PositiveInfinity;

            foreach (var transitLine in transitLines)
            {
                var lineRenderer = transitLine.LineRenderer;
                var positionCount = lineRenderer.positionCount;
                var startOfLinePosition = lineRenderer.GetPosition(0);
                var endOfLinePosition = lineRenderer.GetPosition(positionCount - 1);
                var sqrMagnitudeStart = (startOfLinePosition - mousePosition).sqrMagnitude;
                var sqrMagnitudeEnd = (endOfLinePosition - mousePosition).sqrMagnitude;

                float sqrManitude = float.PositiveInfinity;
                int positionIndex = -1;
                if (sqrMagnitudeStart < sqrMagnitudeEnd)
                {
                    sqrManitude = sqrMagnitudeStart;
                    positionIndex = 0;
                }
                else
                {
                    sqrManitude = sqrMagnitudeEnd;
                    positionIndex = positionCount;
                }


                if (sqrManitude < closestSqrMagnitude)
                {
                    closestLine = transitLine;
                    closestSqrMagnitude = sqrManitude;
                    finalPositionIndex = positionIndex;
                }
            }

            if (closestLine != null)
            {
                var lineRenderer = closestLine.LineRenderer;
                int positionCount = lineRenderer.positionCount;
                Vector3[] positions = new Vector3[positionCount];
                lineRenderer.GetPositions(positions);
                Vector3[] newPositions = new Vector3[positionCount + 1];
                newPositions[finalPositionIndex] = mousePosition;
                int copyToIndex = finalPositionIndex == 0 ? 1 : 0;
                Array.Copy(positions, 0, newPositions, copyToIndex, positionCount);
                lineRenderer.positionCount++;
                lineRenderer.SetPositions(newPositions);
            }
        }
    }
}
