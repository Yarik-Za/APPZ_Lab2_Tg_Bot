namespace GUI_Bot
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cbBotRun = new CheckBox();
            SuspendLayout();
            // 
            // cbBotRun
            // 
            cbBotRun.AutoSize = true;
            cbBotRun.Checked = true;
            cbBotRun.CheckState = CheckState.Checked;
            cbBotRun.Location = new Point(12, 33);
            cbBotRun.Name = "cbBotRun";
            cbBotRun.Size = new Size(68, 19);
            cbBotRun.TabIndex = 0;
            cbBotRun.Text = "Run Bot";
            cbBotRun.UseVisualStyleBackColor = true;
            cbBotRun.CheckedChanged += onRunClick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(363, 271);
            Controls.Add(cbBotRun);
            Name = "MainForm";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox cbBotRun;
    }
}
