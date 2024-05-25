namespace cmd_noGUI_for_Docker_Bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            BotService bot = new BotService();
            await bot.StartAsync();

            Console.WriteLine("Bot is running. Press Ctrl+C to exit.");
            try
            {
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (TaskCanceledException)
            {
                // This exception is expected on cancellation.
            }
        }
    }
}
