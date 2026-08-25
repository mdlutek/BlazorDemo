using System.Text.Json.Serialization;

namespace BlazorDemoApp.Models
{
    // Modele API
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
