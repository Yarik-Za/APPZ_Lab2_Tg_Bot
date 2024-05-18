using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        private long waitingForCityChatId; // Чат, в котором ожидается ввод названия города
        
        #region CityName
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
        #endregion

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

            var chatId = message.Chat.Id;

            // Only process text messages
            if (message.Text is not { } messageText)
                return;


            Debug.WriteLine($"Received a '{messageText}' message in chat {chatId} from {message.Chat.FirstName}.");


            // Check if the message is a command
            if (messageText.StartsWith("/"))
            {
                // Handle different commands
                if (messageText.StartsWith("/start"))
                {
                    // Logic for handling /start command
                    await HandleStartCommandAsync(botClient, chatId, cancellationToken);
                }
                else if (messageText.StartsWith("/weather"))
                {
                    // Logic for handling /weather command
                    await HandleWeatherCommandAsync(botClient, chatId, messageText, cancellationToken);
                }
                else if (messageText.StartsWith("/help")) { await HandleHelpCommandAsync(botClient, chatId, cancellationToken: cancellationToken); }



                // Add more commands as needed




                // Return after handling the command
                return;
            }
            // If the message is not a command, handle other types of messages here

            // For example, if it's not a command and not handled, you can respond with a default message
            await botClient.SendTextMessageAsync(chatId, "Неизвестная команда. Попробуйте другую команду.", cancellationToken: cancellationToken);
        }










        //else
        //{
        //    // Respond with keyboard options
        //    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        //    {
        //        new KeyboardButton[] { "Отримати прогноз", "Змінити місто" }
        //    })
        //    {
        //        ResizeKeyboard = true
        //    };

        //    await botClient.SendTextMessageAsync(
        //        chatId: chatId,
        //        text: "Оберіть дію",
        //        replyMarkup: replyKeyboardMarkup,
        //        cancellationToken: cancellationToken);
        //}


        private async Task HandleStartCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            // Handle the /start command
            await botClient.SendTextMessageAsync
                (chatId,
                "Привіт!\nЯ бот для отримання погоди.\n" +
                "Для цього використовую OpenWeatherMap API.\nВведіть команду <code>/weather Місто</code>" +
                "\nПриклад: /weather London" +
                "\n<i>(Місто обов'язково латинецею)</i>, щоб дізнатися погоду в певному місті.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        // Написать метод
        // для работы
        // с командой
        // messageText.StartsWith
        // ("Отримати прогноз")


        private async Task HandleWeatherCommandAsync(ITelegramBotClient botClient, long chatId, string messageText, CancellationToken cancellationToken)
        {
            if (CityName == " ")
                try
                {
                    cityName = messageText.Replace("/weather", "").Trim();
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }

            if (!string.IsNullOrWhiteSpace(cityName))
            {
                // Call WeatherService to get weather information for the specified city
                await HandleWeatherRequest(botClient, chatId, cityName);
                cityName = " ";
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Ви ще не встановили місто для отримання прогнозу." +
                    "Прочитайте інструкцію \\help з використання команди \\weather, або встановіть кнопкою внизу", cancellationToken: cancellationToken);

               

                //SetCity(messageText);
            }
           
        }

        private async Task HandleHelpCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken) {
           await botClient.SendTextMessageAsync
                (chatId,
                "Для отримання погоди ведіть команду <code>/weather Місто</code>" +
                "\nПриклад: /weather London" +
                "\n<i>(Місто обов'язково латинецею)</i>, щоб дізнатися погоду в певному місті."+
                "Кнопка Отримати прогноз використовується для швидкого отримання, при умові що Ви раніше вводили своє місто"+
                "Кнопка Змінити місто - змінює місто для пошуку погоди",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        // метод для отправки запроса погоды
        private async Task HandleWeatherRequest(ITelegramBotClient botClient, long chatId, string cityName)
        {
            var ws = new WeatherService(new HttpClient(), "78e036e5241dc4d28d98d543e9a6db04");
            var weatherInfo = await ws.GetWeatherInfo(cityName);

            // Отправьте погодную информацию обратно пользователю
            await botClient.SendTextMessageAsync(chatId, weatherInfo, cancellationToken: cts.Token);

            
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
