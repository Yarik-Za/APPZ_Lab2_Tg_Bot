namespace GUI_Bot
{
    public partial class MainForm : Form
    {
        BotService? bot = null;
        public MainForm()
        {
            InitializeComponent();
            cbBotRun.Checked = true;
            LoadBot();
        }

        private void onRunClick(object sender, EventArgs e)
        {
            LoadBot();
        }

        private void LoadBot()
        {
            if (cbBotRun.Checked)
            {
                bot = new BotService();
                _ = bot.StartAsync();
            }
            else
            {
                bot?.Cancel();
                bot = null;
            }
        }
    }
}
