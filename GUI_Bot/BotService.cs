using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        {// Send cancellation request to stop bot
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

            // using Telegram.Bot.Types.ReplyMarkups;



            //        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            //        {
            //new KeyboardButton[] { "Help me", "Call me ☎️" },})
            //        {
            //            ResizeKeyboard = true
            //        };

            //        Message sentMessage = await botClient.SendTextMessageAsync(
            //            chatId: chatId,
            //            text: "Choose a response",
            //            replyMarkup: replyKeyboardMarkup,
            //            cancellationToken: cancellationToken);

            // using Telegram.Bot.Types.ReplyMarkups;

            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                // first row
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "1.1", callbackData: "11"),
                    InlineKeyboardButton.WithCallbackData(text: "1.2", callbackData: "12"),
                },
                // second row
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "2.1", callbackData: "21"),
                    InlineKeyboardButton.WithCallbackData(text: "2.2", callbackData: "22"),
                },
            });

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "A message with an inline keyboard markup",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);


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
