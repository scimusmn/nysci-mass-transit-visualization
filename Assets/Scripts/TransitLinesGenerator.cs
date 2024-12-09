using System;
using System.Collections.Generic;
using UnityEngine;
using SMM.Input;
using Clipper2Lib;

namespace SMM
{
    public class TransitLineGenerator : MonoBehaviour
    {
        private class Stop
        {
            private GameObject gameObject;
            private string name;

            public GameObject GameObject { get => gameObject; set => gameObject = value; }
            public string Name { get => name; set => name = value; }

            public Stop(GameObject gameObject, string name)
            {
                this.gameObject = gameObject;
                this.name = name;
            }
        }

        private class TransitLine
        {
            private GameObject gameObject;
            private LineRenderer lineRenderer;
            private List<Stop> stops;


            public GameObject GameObject { get => gameObject; set => gameObject = value; }
            public LineRenderer LineRenderer { get => lineRenderer; set => lineRenderer = value; }
            public List<Stop> Stops { get => stops; set => stops = value; }


            public TransitLine(GameObject gameObject, LineRenderer lineRenderer, List<Stop> stops)
            {
                this.gameObject = gameObject;
                this.lineRenderer = lineRenderer;
                this.stops = stops;
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
        private GameObject stopPrefab = null;
        [SerializeField]
        private float systemTravelShedLineWidth = 0.05f;
        [SerializeField]
        private Color systemTravelShedColor = Color.white;
#if UNITY_EDITOR
        [SerializeField]
        private bool showSystemTravelShedOutline = false;
#endif


        [NonSerialized]
        private readonly List<TransitLine> transitLines = new List<TransitLine>();
        [NonSerialized]
        private (PathD path, LineRenderer lineRenderer, GameObject gameObject) systemIsochrone = new();
        [NonSerialized]
        private int totalPopulationServed = 0;


        [NonSerialized]
        private const float PositionZ = -0.015f;
        [NonSerialized]
        private const float SystemTravelShedPositionZ = -0.01f;


        protected void OnEnable()
        {
            // TODO : listen for input from tabletop instead of mouse
            inputController.Place += OnPlace;

            // TODO : instantiate transit lines from loaded file instead of placeholder lines
            // TODO : create stops from loaded file instead of placeholder stops
            SetupTransitLine(
                Clipper.MakePath(new double[] { -1.5, 1, 0, 1, 1.5, 1 }),
                new Color(0.953f, 0.435f, 0.318f, 1.0f),
                new List<(string, Vector2)> { ("Stop 1", new Vector2(-1.5f, 1f)), ("Stop 2", new Vector2(-0.75f, 1f)), ("Stop 3", new Vector2(0f, 1f)), ("Stop 4", new Vector2(1.5f, 1f)) });
            SetupTransitLine(
                Clipper.MakePath(new double[] { -1.5, -1, 0, -1, 1.5, -1 }),
                new Color(0.337f, 0.722f, 0.914f, 1.0f),
                new List<(string, Vector2)> { ("Stop 5", new Vector2(-1.5f, -1f)), ("Stop 6", new Vector2(0f, -1f)), ("Stop 7", new Vector2(1.5f, -1f)) });
            SetupSystemTravelShed();
            CalculatePopulationServed();
        }

        protected void OnDisable()
        {
            inputController.Place -= OnPlace;
            foreach (var transitLine in transitLines)
            {
                foreach (var stop in transitLine.Stops)
                {
                    Destroy(stop.GameObject);
                }
                transitLine.Stops.Clear();
                Destroy(transitLine.GameObject);
            }
            transitLines.Clear();

            Destroy(systemIsochrone.gameObject);
        }


        private void SetupTransitLine(PathD path, Color color, List<(string, Vector2)> stops)
        {
            var (gameObject, lineRenderer) = PathUtils.PathDToLineRenderer(
                path, lineRendererPrefab, map.transform,
                name, color, transitLineWidth, PositionZ);
            transitLines.Add(new TransitLine(gameObject, lineRenderer, SetupStops(stops, gameObject.transform)));
        }

        private List<Stop> SetupStops(List<(string name, Vector2 location)> stops, Transform parent)
        {
            List<Stop> stopObjects = new List<Stop>(stops.Count);
            foreach (var stop in stops)
            {
                var gameObject = Instantiate(stopPrefab, parent, true);
                gameObject.name = stop.name;
                gameObject.transform.localPosition = new Vector3(stop.location.x, stop.location.y, PositionZ - 0.01f);
                stopObjects.Add(new Stop(gameObject, stop.name));
            }
            return stopObjects;
        }

        private void SetupSystemTravelShed()
        {
            // TODO : replace with real data. This placeholder has no correlatation to the system on screen currently.
            // Just places a large rectangle in middle of map for testing purposes.
            var path = Clipper.MakePath(new double[] { -4, -4.1, -4, 0.1, 1, 0.1, 1, -4.1, -4, -4.1 });
            systemIsochrone.path = path;
#if UNITY_EDITOR
            if (!showSystemTravelShedOutline) { return; }
            var (gameObject, lineRenderer) = PathUtils.PathDToLineRenderer(
                path,
                lineRendererPrefab, map.transform, "Transit System Isochrone",
                systemTravelShedColor, systemTravelShedLineWidth, SystemTravelShedPositionZ);
            systemIsochrone.lineRenderer = lineRenderer;
            systemIsochrone.gameObject = gameObject;
#endif
        }

        private void InsertNewStop(string name, Vector2 location, int index, TransitLine transitLine)
        {
            var gameObject = Instantiate(stopPrefab, transitLine.GameObject.transform, true);
            gameObject.name = name;
            gameObject.transform.localPosition = new Vector3(location.x, location.y, PositionZ - 0.01f);
            transitLine.Stops.Insert(index, new Stop(gameObject, name));
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
                InsertNewStop("New Stop", mousePosition, finalPositionIndex, closestLine); // TODO : give names to new stops
            }

            CalculateSystemTravelShed();
            CalculatePopulationServed();
        }

        private void CalculateSystemTravelShed()
        {
            // TODO : update total system travel shed as new stops are added
        }

        private void CalculatePopulationServed()
        {
            totalPopulationServed = 0;
            foreach (var neighborhood in neighborhoodsManager.Neighborhoods.Values)
            {
                double neighborhoodArea = Math.Abs(Clipper.Area(neighborhood.Path)); // area could be negative depending on path's winding orientation
                PathsD intersection = Clipper.Intersect(new PathsD() { neighborhood.Path }, new PathsD() { systemIsochrone.path }, FillRule.NonZero);
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
