using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace APPZ_Lab2_Zaychenko_622п
{
    internal class BotService
    {
        TelegramBotClient botClient = new TelegramBotClient("6919475386:AAH5YtigtvZ1XXf_3x_CNGVc_B5WJUbpyAE");

        public BotService() {
            botClient.StartReceiving(onUpdate, onError);
        }

        private Task onError(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask; 
        }

        private async Task onUpdate(ITelegramBotClient client, Update update, CancellationToken token)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {message?.Chat.FirstName}.");

            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said:\n" + messageText
                //cancellationToken: cancellationToken
               );
        }
    }
}
