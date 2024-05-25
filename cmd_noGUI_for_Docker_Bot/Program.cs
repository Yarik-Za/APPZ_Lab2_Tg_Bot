using cmd_noGUI_for_Docker_Bot;

namespace APPZ_Lab2_Zaychenko_622ï
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            BotService botService = new BotService();
            await botService.StartAsync();

            Console.WriteLine("Bot is running. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}




       
       

