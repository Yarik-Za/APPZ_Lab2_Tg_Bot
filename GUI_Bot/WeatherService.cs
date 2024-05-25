using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace cmd_noGUI_for_Docker_Bot
{
    internal class WeatherService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://api.openweathermap.org/data/2.5/weather";
        private readonly string _apiKey; // Ваш API ключ OpenWeatherMap

        public WeatherService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public async Task<string> GetWeatherInfo(string cityName)
        {
            try
            {
                // Формируем URL для запроса погоды по названию города
                string apiUrl = $"{ApiBaseUrl}?q={cityName}&appid={_apiKey}&units=metric";

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Читаем содержимое ответа как строку JSON
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    // Десериализуем JSON в объект WeatherDataLite
                    WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(jsonContent);

                    if (weatherData != null)
                    {
                        return FormatWeatherInfo(weatherData);
                    }
                }

                return "Не вдалось отримати інформацію про погоду для вказоного міста.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка при запиті погоди: {ex.Message}");
                return $"Помилка при запиті погоди";
            }
        }

        private string FormatWeatherInfo(WeatherData weatherData)
        {
            if (weatherData == null)
            {
                return "Данні про погоду недоступні.";
            }

            return $"Погода в {weatherData.Name} ({weatherData.Sys.Country}):\n" +
                   $"🌡️Температура: {weatherData.Main.Temp} °C\n" +
                   $"📉Мінімальна температура: {weatherData.Main.TempMin} °C\n" +
                   $"📈Максимальна температура: {weatherData.Main.TempMax} °C\n" +
                   $"💦Вологість повітря: {weatherData.Main.Humidity} %\n" +
                   $"💨Швидкість вітру: {weatherData.Wind.Speed} м/с\n" +
                   $"Погодні умови: {weatherData.Weather[0].MainDescription} ({weatherData.Weather[0].Description})";
        }
    }

    public class WeatherData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sys")]
        public SysInfo Sys { get; set; }

        [JsonProperty("main")]
        public MainInfo Main { get; set; }

        [JsonProperty("wind")]
        public WindInfo Wind { get; set; }

        [JsonProperty("weather")]
        public List<WeatherInfo> Weather { get; set; }
    }

    public class WeatherInfo
    {
        [JsonProperty("main")]
        public string MainDescription { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
    }

    public class SysInfo
    {
        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class MainInfo
    {
        [JsonProperty("temp")]
        public float Temp { get; set; }

        [JsonProperty("temp_min")]
        public float TempMin { get; set; }

        [JsonProperty("temp_max")]
        public float TempMax { get; set; }

        [JsonProperty("humidity")]
        public float Humidity { get; set; }
    }

    public class WindInfo
    {
        [JsonProperty("speed")]
        public float Speed { get; set; }
    }

}
