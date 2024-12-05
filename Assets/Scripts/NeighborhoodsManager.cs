using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;
using System;

namespace SMM
{
    public class NeighborhoodsManager : MonoBehaviour
    {
        public struct Neighborhood
        {
            private string name;
            private int population;
            private GameObject gameObject;
            private (PathD path, LineRenderer lineRenderer) polygon;


            public string Name { get => name; set => name = value; }
            public int Population { get => population; set => population = value; }
            public GameObject GameObject { get => gameObject; set => gameObject = value; }
            public (PathD path, LineRenderer lineRenderer) Line { get => polygon; set => polygon = value; }


            public Neighborhood(string name, int population, GameObject gameObject, (PathD path, LineRenderer lineRenderer) line)
            {
                this.name = name;
                this.population = population;
                this.gameObject = gameObject;
                this.polygon = line;
            }
        }


        [SerializeField]
        private GameObject lineRendererPrefab = null;
        [SerializeField]
        private float neighborhoodsLineWidth = 0.05f;
        [SerializeField]
        private Color neighborhoodsColor = Color.white;
        [SerializeField]
        private GameObject map = null;


        [NonSerialized]
        private List<Neighborhood> neighborhoods = new List<Neighborhood>();


        [NonSerialized]
        private const float PositionZ = -0.01f;


        public List<Neighborhood> Neighborhoods { get => neighborhoods; set => neighborhoods = value; }


        protected void OnEnable()
        {
            // TODO : load in "New York City Population By Neighborhood Tabulation Areas"
            // TODO : load in "2020 Neighborhood Tabulation Areas (NTAs)"
            // TODO : setup neighborhoods with data loaded in

            SetupNeighborhoods();
        }

        protected void OnDisable()
        {
            foreach (var neighborhood in neighborhoods)
            {
                Destroy(neighborhood.GameObject);
            }
            neighborhoods.Clear();
        }


        private void SetupNeighborhoods()
        {
            // TODO : remove, placeholder neighborhoods
            SetupNeighborhood("Greenpoint", 37821, Clipper.MakePath(new double[] { -5, -2, -5, -4.7, -3, -4.7, -3, -2, -5, -2 }));
            SetupNeighborhood("Williamsburg", 31878, Clipper.MakePath(new double[] { 3, 4.7, 3, 3.1, 5, 3.1, 5, 4.7, 3, 4.7 }));
        }

        private void SetupNeighborhood(string name, int population, PathD path)
        {
            var lineRenderer = PathUtils.PathDToLineRenderer(
                path, lineRendererPrefab, map.transform,
                name, neighborhoodsColor, neighborhoodsLineWidth, PositionZ);
            var neighborhood = new Neighborhood(name, population, lineRenderer.gameObject, (path, lineRenderer));
            neighborhoods.Add(neighborhood);
        }
    }
}
