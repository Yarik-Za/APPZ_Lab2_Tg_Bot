using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace GUI_Bot
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
                    WeatherDataLite weatherData = JsonConvert.DeserializeObject<WeatherDataLite>(jsonContent);

                    if (weatherData != null)
                    {
                        return FormatWeatherInfo(weatherData);
                    }
                }

                return "Не удалось получить информацию о погоде для указанного города.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при запросе погоды: {ex.Message}");
                return $"Ошибка при запросе погоды";
            }
        }

        private string FormatWeatherInfo(WeatherDataLite weatherData)
        {
            if (weatherData == null)
            {
                return "Данные о погоде недоступны.";
            }

            return $"Погода в {weatherData.Name} ({weatherData.Sys.Country}):\n" +
                   $"🌡️Температура: {weatherData.Main.Temp} °C\n" +
                   $"📉Мінімальна температура: {weatherData.Main.TempMin} °C\n" +
                   $"📈Максимальна температура: {weatherData.Main.TempMax} °C\n" +
                   $"💦Вологість повітря: {weatherData.Main.Humidity} %\n" +
                   $"💨Швидкість вітру: {weatherData.Wind.Speed} м/с" +
                   $"Погодні умови: {weatherData.Weather.MainDescription} ({weatherData.Weather.Description}) м/с";
        }
    }

    public class WeatherDataLite
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
        public WeatherInfo Weather { get; set; }
    }

    public class WeatherInfo
    {
        [JsonProperty("main")]
        public string MainDescription { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

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
