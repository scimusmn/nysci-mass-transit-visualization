using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Clipper2Lib;
using SMM.JsonTypes;


namespace SMM
{
    public class NeighborhoodsManager : MonoBehaviour
    {
        public struct Neighborhood
        {
            private string name;
            private int population;
            private int year;
            private PathD path;
            private GameObject gameObject;
            private LineRenderer lineRenderer;


            public string Name { readonly get => name; set => name = value; }
            public int Population { readonly get => population; set => population = value; }
            public int Year { readonly get => year; set => year = value; }
            public PathD Path { readonly get => path; set => path = value; }
            public GameObject GameObject { readonly get => gameObject; set => gameObject = value; }
            public LineRenderer LineRenderer { readonly get => lineRenderer; set => lineRenderer = value; }


            public Neighborhood(string name, int population, int year, PathD path, GameObject gameObject, LineRenderer lineRenderer)
            {
                this.name = name;
                this.population = population;
                this.year = year;
                this.path = path;
                this.gameObject = gameObject;
                this.lineRenderer = lineRenderer;
            }
        }


        [SerializeField]
        private TextAsset geoJson = null; // TODO : update to final data in editor once received from SMM
        [SerializeField]
        private double westExtentMap = -74.257159;
        [SerializeField]
        private double eastExtentMap = -73.699215;
        [SerializeField]
        private double northExtentMap = -73.699215;
        [SerializeField]
        private double southExtentMap = 40.496010;
        [SerializeField]
        private TextAsset neighborhoodsCSV = null; // TODO : update to final data in editor once received from SMM
        [SerializeField]
        private GameObject lineRendererPrefab = null;
        [SerializeField]
        private float neighborhoodsLineWidth = 0.05f;
        [SerializeField]
        private Color neighborhoodsColor = Color.white;
        [SerializeField]
        private GameObject map = null;


        [NonSerialized]
        private Dictionary<string, Neighborhood> neighborhoods = new Dictionary<string, Neighborhood>();
        [NonSerialized]
        private double mapWidth = 0;
        [NonSerialized]
        private double mapHeight = 0;


        [NonSerialized]
        private const float PositionZ = -0.01f;
        [NonSerialized]
        private const int ColumnsCSV = 6;


        public Dictionary<string, Neighborhood> Neighborhoods { get => neighborhoods; set => neighborhoods = value; }


        protected void OnEnable()
        {
            mapWidth = map.transform.localScale.x;
            mapHeight = map.transform.localScale.y;
            Debug.Log("map width: " + mapWidth + " " + "map height: " + mapHeight);
            SetupNeighborhoods();
        }

        protected void OnDisable()
        {
            foreach (var neighborhood in neighborhoods.Values)
            {
                Destroy(neighborhood.GameObject);
            }
            neighborhoods.Clear();
        }


        private void SetupNeighborhoods()
        {
            GeoJson geojson = JsonConvert.DeserializeObject<GeoJson>(geoJson.text);
            var features = geojson.Features;
            foreach (var feature in features)
            {
                var jsonCoordinates = feature.Geometry.Coordinates[0][0];
                int coordinatesCount = jsonCoordinates.Count * 2; // vector2 per coordinate
                double[] coordinates = new double[coordinatesCount];
                for (int i = 0, j = 0; i < jsonCoordinates.Count; i++, j += 2)
                {
                    double xCoord = jsonCoordinates[i][1]; // GeoJSON orders the coordinates as "longitude, latitude"
                    double yCoord = jsonCoordinates[i][0];
                    xCoord = MathUtils.Remap(xCoord, westExtentMap, eastExtentMap, (-mapWidth / 2), mapWidth / 2);
                    yCoord = MathUtils.Remap(yCoord, southExtentMap, northExtentMap, (-mapHeight / 2), mapHeight / 2);
                    coordinates[j] = xCoord;
                    coordinates[j + 1] = yCoord;
                }
                neighborhoods.Add(
                    feature.Properties.NtaName,
                    new Neighborhood(feature.Properties.NtaName, 0, 0, Clipper.MakePath(coordinates), null, null));
            }

            string[] data = neighborhoodsCSV.text.Split(new string[] { ",", "\n" }, StringSplitOptions.None);
            for (int i = ColumnsCSV - 1; i < (data.Length - ColumnsCSV); i += ColumnsCSV)
            {
                string ntaName = data[i + 5];
                // TODO : remove once done fixing coordinates range
                if (ntaName == "Greenpoint")
                {
                    if (neighborhoods.TryGetValue(ntaName, out var neighborhood))
                    {
                        neighborhood.Population = int.Parse(data[i + 6]);
                        CreateNeighborhoodObject(ref neighborhood);
                        return;
                    }
                }
                // TODO : add back in once done testing what is happening with coordinates
                /* if (neighborhoods.TryGetValue(ntaName, out var neighborhood))
                {
                    neighborhood.Population = int.Parse(data[i + 6]);
                    CreateNeighborhoodObject(ref neighborhood);
                } */
            }
            // TODO : how to check that every neighborhood in dictonary has a population number?
        }

        private void CreateNeighborhoodObject(ref Neighborhood neighborhood)
        {
            var (gameObject, lineRenderer) = PathUtils.PathDToLineRenderer(
                neighborhood.Path, lineRendererPrefab, map.transform,
                neighborhood.Name, neighborhoodsColor, neighborhoodsLineWidth, PositionZ);
            neighborhood.GameObject = gameObject;
            neighborhood.LineRenderer = lineRenderer;
        }
    }
}
