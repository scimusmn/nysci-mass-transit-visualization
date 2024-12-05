using System;
using System.Collections.Generic;
using UnityEngine;
using SMM.Input;
using Clipper2Lib;

namespace SMM
{
    // TODO : rename to 'manager' since handling more than generating lines now?
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
        private NeighborhoodsManager neighborhoodsManager = null;
        [SerializeField]
        private Camera mainCamera = null;
        [SerializeField]
        private GameObject map = null;
        [SerializeField]
        private GameObject lineRendererPrefab = null;
        [SerializeField]
        private float transitLineWidth = 0.1f;
        [SerializeField]
        private float systemIsochroneLineWidth = 0.05f;
        [SerializeField]
        private Color systemIsochroneColor = Color.white;


        [NonSerialized]
        private readonly List<TransitLine> transitLines = new List<TransitLine>();
        [NonSerialized]
        private (PathD path, LineRenderer lineRenderer, GameObject gameObject) systemIsochrone = new();
        [NonSerialized]
        private int totalPopulationServed = 0;


        [NonSerialized]
        private const float PositionZ = -0.015f;
        [NonSerialized]
        private const float SystemIsochronePositionZ = -0.01f;


        protected void OnEnable()
        {
            // TODO : listen for input from tabletop instead of mouse
            inputController.Place += OnPlace;

            // TODO : instantiate transit lines from loaded file instead of placeholder lines
            SetupTransitLine(Clipper.MakePath(new double[] { -1.5, 1, 0, 1, 1.5, 1 }), new Color(0.953f, 0.435f, 0.318f, 1.0f));
            SetupTransitLine(Clipper.MakePath(new double[] { -1.5, -1, 0, -1, 1.5, -1 }), new Color(0.337f, 0.722f, 0.914f, 1.0f));
            SetupSystemIsochrone();
            CalculatePopulationServed();
        }

        protected void OnDisable()
        {
            inputController.Place -= OnPlace;
            foreach (var transitLine in transitLines)
            {
                Destroy(transitLine.GameObject);
            }
            transitLines.Clear();

            Destroy(systemIsochrone.gameObject);
        }


        private void SetupTransitLine(PathD path, Color color)
        {
            var lineRenderer = PathUtils.PathDToLineRenderer(
                path, lineRendererPrefab, map.transform,
                name, color, transitLineWidth, PositionZ);
            transitLines.Add(new TransitLine(lineRenderer.gameObject, lineRenderer));
        }

        private void SetupSystemIsochrone()
        {
            // TODO : replace with real data. This placeholder has no correlatation to the system on screen currently.
            var path = Clipper.MakePath(new double[] { -4, -4.1, -4, 0.1, 1, 0.1, 1, -4.1, -4, -4.1 });
            var lineRenderer = PathUtils.PathDToLineRenderer(
                path,
                lineRendererPrefab, map.transform, "Transit System Isochrone",
                systemIsochroneColor, systemIsochroneLineWidth, SystemIsochronePositionZ);
            systemIsochrone.path = path;
            systemIsochrone.lineRenderer = lineRenderer;
            systemIsochrone.gameObject = lineRenderer.gameObject;
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

            CalculateSystemIsochrone();
            CalculatePopulationServed();
        }

        private void CalculateSystemIsochrone()
        {
            // TODO : update system isochrone as new stops are added
        }

        private void CalculatePopulationServed()
        {
            totalPopulationServed = 0;
            foreach (var neighborhood in neighborhoodsManager.Neighborhoods)
            {
                double neighborhoodArea = Math.Abs(Clipper.Area(neighborhood.Line.path)); // area could be negative depending on path's winding orientation
                PathsD intersection = Clipper.Intersect(new PathsD() { neighborhood.Line.path }, new PathsD() { systemIsochrone.path }, FillRule.NonZero);
                if (intersection.Count == 0) { continue; }

                double intersectionArea = Clipper.Area(intersection);
                double percentageServed = intersectionArea / neighborhoodArea;
                totalPopulationServed += (int)Math.Round(neighborhood.Population * percentageServed, MidpointRounding.AwayFromZero);
            }
            // TODO : replace log with updating UI
            Debug.Log("total population served: " + totalPopulationServed);
        }
    }
}
