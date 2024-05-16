using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;


namespace GUI_Bot
{
    internal class BotService
    {
        TelegramBotClient botClient = new TelegramBotClient("6919475386:AAH5YtigtvZ1XXf_3x_CNGVc_B5WJUbpyAE");
        CancellationTokenSource cts = new();
        WeatherService ws = new WeatherService(new System.Net.Http.HttpClient(), "78e036e5241dc4d28d98d543e9a6db04");

        private string CityName = null;

        public string cityName
        {
            get { return CityName; }
            set
            {
                // Проверяем, что value состоит только из букв и пробелов
                if (IsValidCityName(value))
                {
                    CityName = value;
                }
                else
                {
                    throw new ArgumentException("Invalid city name. Only letters and spaces are allowed.");
                }
            }
        }

        private bool IsValidCityName(string input)
        {
            // Используем регулярное выражение для проверки наличия только букв и пробелов
            return Regex.IsMatch(input, @"^[a-zA-Z\s]+$");
        }

        public void SetCity(string newCity)
        {
            cityName = newCity; // Устанавливаем новое значение через сеттер
        }

        public string GetCity()
        {
            return cityName; // Возвращаем текущее значение через геттер
        }

        public BotService()
        {

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = botClient.GetMeAsync();

            // Debug.WriteLine($"Start listening for @{me.Username}");
        }

        public void Cancel()
        {
            // Send cancellation request to stop bot
            cts.Cancel();
        }

        public async Task StartAsync()
        {
            var me = await botClient.GetMeAsync();

            Debug.WriteLine($"Start listening for @{me.Username}");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message

            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            Debug.WriteLine($"Received a '{messageText}' message in chat {chatId} from {message.Chat.FirstName}.");

            if ((messageText.StartsWith("/weather") || messageText.StartsWith("Отримати прогноз")))
            {
                try { cityName = messageText.Replace("/weather", "").Trim(); 
                }
                catch (Exception ex){ Debug.WriteLine(ex.Message); }
               
                if (!string.IsNullOrWhiteSpace(cityName))
                {
                    string weatherInfo = await ws.GetWeatherInfo(cityName);
                    await botClient.SendTextMessageAsync(chatId, weatherInfo, cancellationToken: cancellationToken);
                    cityName = null;
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Ви ще не встановили місто для отримання прогнозу.\nВведіть місто:", cancellationToken: cancellationToken);
                    //SetCity(messageText);
                }
            }
            else
            {
                // Respond with keyboard options
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    new KeyboardButton[] { "Отримати прогноз", "Змінити місто" }
                })
                {
                    ResizeKeyboard = true
                };

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Оберіть дію",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }


            //// Echo received message text
            //Message sentMessage = await botClient.SendTextMessageAsync(
            //    chatId: chatId,
            //    text: "You said:\n" + messageText,
            //    cancellationToken: cancellationToken);
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Debug.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

    }
}
