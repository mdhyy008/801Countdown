namespace _801CountdownApp1
{
    partial class Form4
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel[] stylePanels;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.ClientSize = new System.Drawing.Size(620, 242);
            this.Name = "Form4";
            this.Text = "样式选择";
            int cols = 4;
            int total = 7;
            int cellW = 140;
            int cellH = 96;
            int gap = 10;
            int marginX = (this.ClientSize.Width - cols * cellW - (cols - 1) * gap) / 2;
            int marginY = (this.ClientSize.Height - 2 * cellH - gap) / 2;
            this.stylePanels = new System.Windows.Forms.Panel[total];
            for (int i = 0; i < total; i++)
            {
                this.stylePanels[i] = new System.Windows.Forms.Panel();
                this.stylePanels[i].Size = new System.Drawing.Size(cellW, cellH);
                this.stylePanels[i].Location = new System.Drawing.Point(
                    marginX + (i % cols) * (cellW + gap),
                    marginY + (i / cols) * (cellH + gap));
                this.stylePanels[i].BackColor = System.Drawing.Color.White;
                this.stylePanels[i].Cursor = System.Windows.Forms.Cursors.Hand;
                this.Controls.Add(this.stylePanels[i]);
            }
        }

        #endregion
    }
}
