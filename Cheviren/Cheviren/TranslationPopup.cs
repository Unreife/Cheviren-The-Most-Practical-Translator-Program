using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cheviren
{
    public class TranslationPopup : Form
    {
        private Label label;
        private System.Windows.Forms.Timer autoCloseTimer;
        private int autoCloseMs;

        public TranslationPopup(string text, int autoCloseMs = 3000)
        {
            this.autoCloseMs = autoCloseMs;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.LightYellow;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            label = new Label
            {
                Text = text,
                AutoSize = true,
                MaximumSize = new Size(400, 0),
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10)
            };
            Controls.Add(label);

            // Mouse enter/leave eventleri
            label.MouseEnter += (s, e) => autoCloseTimer?.Stop();
            label.MouseLeave += async (s, e) =>
            {
                await Task.Delay(200);
                if (!this.IsDisposed && this.Visible)
                    this.Close();
            };
            this.MouseEnter += (s, e) => autoCloseTimer?.Stop();
            this.MouseLeave += async (s, e) =>
            {
                await Task.Delay(200);
                if (!this.IsDisposed && this.Visible)
                    this.Close();
            };

            // Otomatik kapanma için timer
            autoCloseTimer = new System.Windows.Forms.Timer();
            autoCloseTimer.Interval = autoCloseMs;
            autoCloseTimer.Tick += (s, e) =>
            {
                autoCloseTimer.Stop();
                if (!this.IsDisposed && this.Visible)
                    this.Close();
            };
            autoCloseTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            autoCloseTimer?.Stop();
            autoCloseTimer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}