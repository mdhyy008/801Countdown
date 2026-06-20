namespace _801CountdownApp1
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel[] colorPanels;

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
            this.ClientSize = new System.Drawing.Size(420, 460);
            this.Name = "Form3";
            this.Text = "配色选择";
            int cols = 5;
            int total = 30;
            this.colorPanels = new System.Windows.Forms.Panel[total];
            for (int i = 0; i < total; i++)
            {
                this.colorPanels[i] = new System.Windows.Forms.Panel();
                this.colorPanels[i].Size = new System.Drawing.Size(60, 60);
                this.colorPanels[i].Location = new System.Drawing.Point(30 + (i % cols) * 70, 30 + (i / cols) * 70);
                this.colorPanels[i].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                this.Controls.Add(this.colorPanels[i]);
            }
        }

        #endregion
    }
}