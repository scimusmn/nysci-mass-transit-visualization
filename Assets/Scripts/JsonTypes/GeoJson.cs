using System.Collections.Generic;
using Newtonsoft.Json;

namespace SMM.JsonTypes
{
    public class GeoJson
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("features")]
        public List<GeoJsonFeature> Features { get; set; }
    }

    public class GeoJsonFeature
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("properties")]
        public GeoJsonProperties Properties { get; set; }
        [JsonProperty("geometry")]
        public GeoJsonGeometry Geometry { get; set; }
    }

    public class GeoJsonProperties
    {
        [JsonProperty("shapeArea")]
        public float ShapeArea { get; set; }
        [JsonProperty("ntaName")]
        public string NtaName { get; set; }
        [JsonProperty("cdtaName")]
        public string CdtaName { get; set; }
        [JsonProperty("shapeLeng")]
        public float ShapeLeng { get; set; }
        [JsonProperty("boroName")]
        public string BoroName { get; set; }
        [JsonProperty("ntaType")]
        public int NtaType { get; set; }
        [JsonProperty("nta2020")]
        public string Nta2020 { get; set; }
        [JsonProperty("boroCode")]
        public int BoroCode { get; set; }
        [JsonProperty("countyFips")]
        public string CountyFips { get; set; }
        [JsonProperty("ntaAbbrev")]
        public string NtaAbbrev { get; set; }
        [JsonProperty("cdta2020")]
        public string Cdta2020 { get; set; }
    }

    public class GeoJsonGeometry
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("coordinates")]
        public List<List<List<List<double>>>> Coordinates { get; set; }
    }
}
