using System.Text.Json.Serialization;

namespace BlazorDemoApp.Models
{
    // ==========================================
    // MODELE DANYCH Z API
    // ==========================================

    public class AirQualityApiResponse
    {
        [JsonPropertyName("current")]
        public CurrentAirQuality? Current { get; set; }
    }

    public class CurrentAirQuality
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("european_aqi")]
        public double? EuropeanAqi { get; set; }

        [JsonPropertyName("pm10")]
        public double? Pm10 { get; set; }

        [JsonPropertyName("pm2_5")]
        public double? Pm25 { get; set; }

        [JsonPropertyName("carbon_monoxide")]
        public double? CarbonMonoxide { get; set; }

        [JsonPropertyName("nitrogen_dioxide")]
        public double? NitrogenDioxide { get; set; }

        [JsonPropertyName("sulphur_dioxide")]
        public double? SulphurDioxide { get; set; }

        [JsonPropertyName("ozone")]
        public double? Ozone { get; set; }

        [JsonPropertyName("uv_index")]
        public double? UvIndex { get; set; }

        [JsonPropertyName("alder_pollen")]
        public double? AlderPollen { get; set; }

        [JsonPropertyName("birch_pollen")]
        public double? BirchPollen { get; set; }

        [JsonPropertyName("grass_pollen")]
        public double? GrassPollen { get; set; }

        [JsonPropertyName("ragweed_pollen")]
        public double? RagweedPollen { get; set; }
    }

    public class GeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<GeocodingResult>? Results { get; set; }
    }

    public class GeocodingResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("admin1")]
        public string? Admin1 { get; set; }
    }

    public class WeatherApiResponse
    {
        [JsonPropertyName("current")]
        public CurrentWeather? Current { get; set; }

        [JsonPropertyName("hourly")]
        public HourlyWeather? Hourly { get; set; }

        [JsonPropertyName("daily")]
        public DailyWeather? Daily { get; set; }
    }

    public class CurrentWeather
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("temperature_2m")]
        public double Temperature { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public int Humidity { get; set; }

        [JsonPropertyName("apparent_temperature")]
        public double ApparentTemperature { get; set; }

        [JsonPropertyName("precipitation")]
        public double Precipitation { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("is_day")]
        public int IsDay { get; set; } = 1;
    }

    public class DailyWeather
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("weather_code")]
        public List<int> WeatherCode { get; set; } = new();

        [JsonPropertyName("temperature_2m_max")]
        public List<double> TemperatureMax { get; set; } = new();

        [JsonPropertyName("temperature_2m_min")]
        public List<double> TemperatureMin { get; set; } = new();
    }

    public class HourlyWeather
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("temperature_2m")]
        public List<double> Temperature { get; set; } = new();

        [JsonPropertyName("relative_humidity_2m")]
        public List<int> Humidity { get; set; } = new();

        [JsonPropertyName("surface_pressure")]
        public List<double> SurfacePressure { get; set; } = new();

        [JsonPropertyName("weather_code")]
        public List<int> WeatherCode { get; set; } = new();

        [JsonPropertyName("is_day")]
        public List<int> IsDay { get; set; } = new();
    }
}
