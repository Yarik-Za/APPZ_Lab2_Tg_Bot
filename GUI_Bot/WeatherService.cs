using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace GUI_Bot
{
    internal class WeatherService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://api.oceandrivers.com/v1.0/getWeatherDisplay";

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetWeatherInfo(string location)
        {
            try
            {
                string apiUrl = $"{ApiBaseUrl}?location={location}";

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    WeatherData weatherData = await response.Content.ReadFromJsonAsync<WeatherData>();
                    if (weatherData != null)
                    {
                        return FormatWeatherInfo(weatherData);
                    }
                }

                return "Не удалось получить информацию о погоде для указанного местоположения.";
            }
            catch (Exception ex)
            {
                return $"Ошибка при запросе погоды: {ex.Message}";
            }
        }

        private string FormatWeatherInfo(WeatherData weatherData)
        {
            // Форматирование информации о погоде для отправки пользователю
            return $"Погода в {weatherData.location.name} ({weatherData.location.country}):\n" +
                   $"Температура: {weatherData.data.temperature} °C\n" +
                   $"Влажность: {weatherData.data.humidity} %\n" +
                   $"Скорость ветра: {weatherData.data.wind_speed} м/с";
        }
    }

    public class WeatherData
    {
        public Location location { get; set; }
        public WeatherDetails data { get; set; }
    }

    public class Location
    {
        public string name { get; set; }
        public string country { get; set; }
    }

    public class WeatherDetails
    {
        public float temperature { get; set; }
        public float humidity { get; set; }
        public float wind_speed { get; set; }
    }
}
