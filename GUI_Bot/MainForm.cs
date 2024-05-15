namespace GUI_Bot
{
    public partial class MainForm : Form
    {
        BotService? bot=null;
        public MainForm()
        {
            InitializeComponent();
        }

        private void onRunClick(object sender, EventArgs e)
        {
            if (cbBotRun.Checked)
            { bot = new BotService();
                _ = bot.StartAsync();
            }
            else { bot?.Cancel();
                bot = null;
            }

        }
    }
}
