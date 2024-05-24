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
            try
            {

                if (update.Type == UpdateType.Message)
                {

                    // Only process Message updates: https://core.telegram.org/bots/api#message

                    if (update.Message is not { } message)
                        return;

                    var chatId = message.Chat.Id;

                    // Only process text messages
                    if (message.Text is not { } messageText)
                        return;

                    Debug.WriteLine($"Received a '{messageText}' message in chat {chatId} from {message.Chat.FirstName}.");

                    // Получаем текущее состояние пользователя
                    var userState = GetUserState(chatId);

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
                            if (string.IsNullOrEmpty(cityName))
                                // Logic for handling weather CITY command
                                SetCityFromCommand(messageText);

                            if (!string.IsNullOrEmpty(cityName))
                                // Logic for handling /weather command
                                await HandleWeatherCommandAsync(botClient, chatId, cancellationToken);
                        }
                        else if (messageText.StartsWith("/help")) { await HandleHelpCommandAsync(botClient, chatId, cancellationToken: cancellationToken); }
                        else if (messageText.StartsWith("/changecity")) { await HandleChangeCityCommandAsync(botClient, chatId, cancellationToken); }
                        else
                        {
                            // For example, if it's not a command and not handled, you can respond with a default message
                            await botClient.SendTextMessageAsync(chatId, "Невідома команда. Спробуйте іншу", cancellationToken: cancellationToken);
                        }

                        // Return after handling the command
                        return;
                    }
                    // If the message is not a command, handle other types of messages here
                    else if (messageText.StartsWith("Змінити місто"))
                    {
                        await HandleChangeCityCommandAsync(botClient, chatId, cancellationToken);
                    }
                    else if (userState == UserState.WaitingForCity)
                    {
                        // Если пользователь ожидает ввода города, обрабатываем его сообщение как новый город
                        string newCity = message.Text;
                        cityName = newCity;
                        // Устанавливаем состояние пользователя как "не ожидает ввода города"
                        SetUserState(chatId, UserState.None);
                        // Отправляем сообщение об успешном изменении города
                        await botClient.SendTextMessageAsync(chatId, $"Ваше місто змінене на {cityName} успішно.");
                    }
                    else if (messageText.StartsWith("Отримати прогноз"))
                    {
                        if (!string.IsNullOrWhiteSpace(cityName))
                        {
                            // Logic for handling /weather command
                            await HandleWeatherCommandAsync(botClient, chatId, cancellationToken);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, $"Місто не встановлене.");
                        }
                    }
                }

                // Проверяем, является ли обновление CallbackQuery
                if (update.Type == UpdateType.CallbackQuery)
                {
                    // Получаем данные о нажатой кнопке
                    var callbackQuery = update.CallbackQuery;

                    // Проверяем, является ли нажатая кнопка "Отмена"
                    if (callbackQuery.Data == "cancel_change_city")
                    {
                        // Выполняем действия для отмены изменения города
                        var chatId = callbackQuery.Message.Chat.Id;
                        await botClient.SendTextMessageAsync(chatId, "Зміна міста скасована.");
                    }

                }
            }

            catch (ApiRequestException apiEx) when (apiEx.ErrorCode == 403)
            {
                // Игнорируем ошибку блокировки пользователем
                Debug.WriteLine("Bot was blocked by the user. Ignoring this error and continuing...");
            }
            catch (Exception ex)
            {
                // Обработка всех других исключений
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }

        #region for_Waited_message
        // Перечисление для представления состояний пользователя
        private enum UserState
        {
            None, // Не ожидает ввода города
            WaitingForCity // Ожидает ввода города
        }

        // Словарь для хранения состояний пользователей
        private readonly Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();

        private UserState GetUserState(long chatId)
        {
            // Получаем состояние пользователя из хранилища
            if (userStates.TryGetValue(chatId, out var state))
            {
                return state;
            }
            else
            {
                // Если состояние не найдено, возвращаем "не ожидает ввода города" по умолчанию
                return UserState.None;
            }
        }
        private void SetUserState(long chatId, UserState state)
        {
            userStates[chatId] = state;
        }


        private async Task HandleChangeCityCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            // Отправляем сообщение с просьбой ввести новый город
            //await botClient.SendTextMessageAsync(chatId, "Ви змінюєте місто, введіть його:", cancellationToken: cancellationToken);

            // Создаем кнопку "Отмена"
            var cancelButton = InlineKeyboardButton.WithCallbackData("Відмінити", "cancel_change_city");

            // Создаем клавиатуру с кнопкой "Отмена"
            var keyboard = new InlineKeyboardMarkup(new[] { new[] { cancelButton } });

            // Отправляем сообщение с клавиатурой
            await botClient.SendTextMessageAsync(chatId, "Ви змінюєте місто, введіть його в наступному повідомленні." +
                "\n\nНатисніть кнопку \"Відмінити\", щоб скасувати зміну міста.", replyMarkup: keyboard, cancellationToken: cancellationToken);


            // Устанавливаем состояние пользователя как "ожидает ввода города"
            SetUserState(chatId, UserState.WaitingForCity);
        }
        #endregion


        private async Task HandleStartCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            string start_txt = "Привіт!\nЯ бот для отримання погоди.\n" +
                "Для цього використовую OpenWeatherMap API.\nВведіть команду <code>/weather Місто</code>" +
                "\nПриклад: /weather London" +
                "\n<i>(Місто обов'язково латинецею)</i>, щоб дізнатися погоду в певному місті.";

            // Respond with keyboard options
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { "Отримати прогноз", "Змінити місто" }
            })
            {
                ResizeKeyboard = true
            };

            // Handle the /start command
            await botClient.SendTextMessageAsync
                (chatId,
                start_txt,
                parseMode: ParseMode.Html,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        private async Task HandleWeatherCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(cityName))
            {
                // Call WeatherService to get weather information for the specified city
                await HandleWeatherRequest(botClient, chatId, cityName);
                //cityName = " ";
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Ви ще не встановили місто для отримання прогнозу.\n" +
                    "Прочитайте інструкцію /help з використання команди /weather, або встановіть кнопкою внизу", parseMode: ParseMode.Html, cancellationToken: cancellationToken);
            }
        }

        private void SetCityFromCommand(string messageText)
        {
            try
            {
                cityName = messageText.Replace("/weather", "").Trim();
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            return;
        }

        private async Task HandleHelpCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync
                 (chatId,
                 "Для отримання погоди ведіть команду <code>/weather Місто</code>" +
                 "\nПриклад: /weather London" +
                 "\n<i>(Місто обов'язково латинецею)</i>, щоб дізнатися погоду в певному місті." +
                 "\nКнопка <b>Отримати прогноз</b> використовується для швидкого отримання погоди, при умові що Ви раніше вводили своє місто." +
                 "\nКнопка <b>Змінити місто</b> - змінює місто для пошуку погоди",
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

            // Игнорируем ошибку 403, когда бот заблокирован пользователем
            if (exception is ApiRequestException apiEx && apiEx.ErrorCode == 403)
            {
                Debug.WriteLine("Bot was blocked by the user. Ignoring this error and continuing...");
                return Task.CompletedTask;
            }

            Debug.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task SendTextMessageAsyncNoBlock(long ch_id, string text, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(ch_id, text, cancellationToken: cancellationToken);
            }
            catch
            {

            }


        }

    }
}
