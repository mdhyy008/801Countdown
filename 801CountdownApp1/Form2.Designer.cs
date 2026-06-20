namespace _801CountdownApp1
{
    partial class Form2
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxBasic = new System.Windows.Forms.GroupBox();
            this.basicLayout = new System.Windows.Forms.TableLayoutPanel();
            this.labelTotalTime = new System.Windows.Forms.Label();
            this.textBoxTotalTime = new System.Windows.Forms.TextBox();
            this.comboBoxUnit = new System.Windows.Forms.ComboBox();
            this.labelOpacity = new System.Windows.Forms.Label();
            this.trackBarOpacity = new System.Windows.Forms.TrackBar();
            this.checkBoxPPTAuto = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.alertLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBoxSound = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.groupBoxAppearance = new System.Windows.Forms.GroupBox();
            this.appearanceLayout = new System.Windows.Forms.TableLayoutPanel();
            this.buttonFont = new System.Windows.Forms.Button();
            this.labelForeColor = new System.Windows.Forms.Label();
            this.textBoxForeColor = new System.Windows.Forms.TextBox();
            this.buttonColorSelect = new System.Windows.Forms.Button();
            this.labelBackColor = new System.Windows.Forms.Label();
            this.textBoxBackColor = new System.Windows.Forms.TextBox();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.trackBarFontSize = new System.Windows.Forms.TrackBar();
            this.labelFontSizeVal = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.remoteLayout = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.button3 = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.mainLayout.SuspendLayout();
            this.groupBoxBasic.SuspendLayout();
            this.basicLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarOpacity)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.alertLayout.SuspendLayout();
            this.groupBoxAppearance.SuspendLayout();
            this.appearanceLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFontSize)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.remoteLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // mainLayout
            //
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.groupBoxBasic, 0, 0);
            this.mainLayout.Controls.Add(this.groupBox2, 0, 1);
            this.mainLayout.Controls.Add(this.groupBoxAppearance, 0, 2);
            this.mainLayout.Controls.Add(this.groupBox1, 0, 3);
            this.mainLayout.Controls.Add(this.buttonPanel, 0, 4);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.Padding = new System.Windows.Forms.Padding(14, 12, 14, 12);
            this.mainLayout.RowCount = 5;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            //
            // groupBoxBasic
            // 
            this.groupBoxBasic.AutoSize = true;
            this.groupBoxBasic.Controls.Add(this.basicLayout);
            this.groupBoxBasic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxBasic.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxBasic.Location = new System.Drawing.Point(14, 12);
            this.groupBoxBasic.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.groupBoxBasic.Name = "groupBoxBasic";
            this.groupBoxBasic.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxBasic.Size = new System.Drawing.Size(612, 172);
            this.groupBoxBasic.TabIndex = 0;
            this.groupBoxBasic.TabStop = false;
            this.groupBoxBasic.Text = "基本设置";
            // 
            // basicLayout
            // 
            this.basicLayout.AutoSize = true;
            this.basicLayout.ColumnCount = 7;
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.basicLayout.Controls.Add(this.labelTotalTime, 0, 0);
            this.basicLayout.Controls.Add(this.textBoxTotalTime, 1, 0);
            this.basicLayout.Controls.Add(this.comboBoxUnit, 2, 0);
            this.basicLayout.Controls.Add(this.labelOpacity, 0, 1);
            this.basicLayout.Controls.Add(this.trackBarOpacity, 1, 1);
            this.basicLayout.Controls.Add(this.checkBoxPPTAuto, 0, 2);
            this.basicLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.basicLayout.Location = new System.Drawing.Point(8, 32);
            this.basicLayout.Name = "basicLayout";
            this.basicLayout.Padding = new System.Windows.Forms.Padding(4, 8, 8, 4);
            this.basicLayout.RowCount = 4;
            this.basicLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.basicLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.basicLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.basicLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.basicLayout.Size = new System.Drawing.Size(596, 132);
            this.basicLayout.TabIndex = 0;
            // 
            // labelTotalTime
            // 
            this.labelTotalTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTotalTime.AutoSize = true;
            this.labelTotalTime.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.labelTotalTime.Location = new System.Drawing.Point(7, 15);
            this.labelTotalTime.Name = "labelTotalTime";
            this.labelTotalTime.Size = new System.Drawing.Size(84, 25);
            this.labelTotalTime.TabIndex = 0;
            this.labelTotalTime.Text = "总时间：";
            // 
            // textBoxTotalTime
            // 
            this.textBoxTotalTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxTotalTime.Location = new System.Drawing.Point(98, 12);
            this.textBoxTotalTime.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxTotalTime.MaxLength = 5;
            this.textBoxTotalTime.Name = "textBoxTotalTime";
            this.textBoxTotalTime.Size = new System.Drawing.Size(90, 31);
            this.textBoxTotalTime.TabIndex = 1;
            this.textBoxTotalTime.Text = "20";
            // 
            // comboBoxUnit
            // 
            this.comboBoxUnit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBoxUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxUnit.Items.AddRange(new object[] {
            "分",
            "秒"});
            this.comboBoxUnit.Location = new System.Drawing.Point(196, 14);
            this.comboBoxUnit.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxUnit.Name = "comboBoxUnit";
            this.comboBoxUnit.Size = new System.Drawing.Size(70, 33);
            this.comboBoxUnit.TabIndex = 2;
            // 
            // labelOpacity
            // 
            this.labelOpacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelOpacity.AutoSize = true;
            this.labelOpacity.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.labelOpacity.Location = new System.Drawing.Point(7, 56);
            this.labelOpacity.Name = "labelOpacity";
            this.labelOpacity.Size = new System.Drawing.Size(84, 25);
            this.labelOpacity.TabIndex = 3;
            this.labelOpacity.Text = "透明度：";
            // 
            // trackBarOpacity
            // 
            this.trackBarOpacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.trackBarOpacity.AutoSize = false;
            this.basicLayout.SetColumnSpan(this.trackBarOpacity, 4);
            this.trackBarOpacity.Location = new System.Drawing.Point(98, 51);
            this.trackBarOpacity.Margin = new System.Windows.Forms.Padding(4);
            this.trackBarOpacity.Maximum = 250;
            this.trackBarOpacity.Minimum = 100;
            this.trackBarOpacity.Name = "trackBarOpacity";
            this.trackBarOpacity.Size = new System.Drawing.Size(340, 36);
            this.trackBarOpacity.TabIndex = 4;
            this.trackBarOpacity.TickFrequency = 10;
            this.trackBarOpacity.Value = 250;
            // 
            // checkBoxPPTAuto
            // 
            this.checkBoxPPTAuto.AutoSize = true;
            this.basicLayout.SetColumnSpan(this.checkBoxPPTAuto, 7);
            this.checkBoxPPTAuto.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.checkBoxPPTAuto.Location = new System.Drawing.Point(8, 95);
            this.checkBoxPPTAuto.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxPPTAuto.Name = "checkBoxPPTAuto";
            this.checkBoxPPTAuto.Size = new System.Drawing.Size(217, 29);
            this.checkBoxPPTAuto.TabIndex = 5;
            this.checkBoxPPTAuto.Text = "PPT全屏时：自动计时";
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.alertLayout);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox2.Location = new System.Drawing.Point(14, 192);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(8);
            this.groupBox2.Size = new System.Drawing.Size(612, 85);
            this.groupBox2.TabIndex = 100;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "计时提醒设置";
            // 
            // alertLayout
            // 
            this.alertLayout.AutoSize = true;
            this.alertLayout.Controls.Add(this.checkBoxSound);
            this.alertLayout.Controls.Add(this.checkBox2);
            this.alertLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alertLayout.Location = new System.Drawing.Point(8, 32);
            this.alertLayout.Name = "alertLayout";
            this.alertLayout.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.alertLayout.Size = new System.Drawing.Size(596, 45);
            this.alertLayout.TabIndex = 0;
            // 
            // checkBoxSound
            // 
            this.checkBoxSound.AutoSize = true;
            this.checkBoxSound.Location = new System.Drawing.Point(12, 8);
            this.checkBoxSound.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxSound.Name = "checkBoxSound";
            this.checkBoxSound.Size = new System.Drawing.Size(92, 29);
            this.checkBoxSound.TabIndex = 11;
            this.checkBoxSound.Text = "提示音";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(112, 8);
            this.checkBox2.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(110, 29);
            this.checkBox2.TabIndex = 13;
            this.checkBox2.Text = "闪烁三次";
            // 
            // groupBoxAppearance
            // 
            this.groupBoxAppearance.AutoSize = true;
            this.groupBoxAppearance.Controls.Add(this.appearanceLayout);
            this.groupBoxAppearance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxAppearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxAppearance.Location = new System.Drawing.Point(14, 285);
            this.groupBoxAppearance.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.groupBoxAppearance.Name = "groupBoxAppearance";
            this.groupBoxAppearance.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxAppearance.Size = new System.Drawing.Size(612, 138);
            this.groupBoxAppearance.TabIndex = 1;
            this.groupBoxAppearance.TabStop = false;
            this.groupBoxAppearance.Text = "外观设置";
            // 
            // appearanceLayout
            // 
            this.appearanceLayout.AutoSize = true;
            this.appearanceLayout.ColumnCount = 5;
            this.appearanceLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.appearanceLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.appearanceLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.appearanceLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.appearanceLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.appearanceLayout.Controls.Add(this.buttonFont, 0, 0);
            this.appearanceLayout.Controls.Add(this.labelForeColor, 1, 0);
            this.appearanceLayout.Controls.Add(this.textBoxForeColor, 2, 0);
            this.appearanceLayout.Controls.Add(this.buttonColorSelect, 0, 1);
            this.appearanceLayout.Controls.Add(this.labelBackColor, 1, 1);
            this.appearanceLayout.Controls.Add(this.textBoxBackColor, 2, 1);
            this.appearanceLayout.Controls.Add(this.labelFontSize, 0, 2);
            this.appearanceLayout.Controls.Add(this.trackBarFontSize, 1, 2);
            this.appearanceLayout.Controls.Add(this.labelFontSizeVal, 4, 2);
            this.appearanceLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appearanceLayout.Location = new System.Drawing.Point(8, 32);
            this.appearanceLayout.Name = "appearanceLayout";
            this.appearanceLayout.Padding = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.appearanceLayout.RowCount = 3;
            this.appearanceLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.appearanceLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.appearanceLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.appearanceLayout.Size = new System.Drawing.Size(596, 98);
            this.appearanceLayout.TabIndex = 0;
            // 
            // buttonFont
            // 
            this.buttonFont.AutoSize = true;
            this.buttonFont.Location = new System.Drawing.Point(8, 10);
            this.buttonFont.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFont.Name = "buttonFont";
            this.buttonFont.Size = new System.Drawing.Size(130, 35);
            this.buttonFont.TabIndex = 0;
            this.buttonFont.Text = "字体样式选择";
            // 
            // labelForeColor
            // 
            this.labelForeColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelForeColor.AutoSize = true;
            this.labelForeColor.Location = new System.Drawing.Point(146, 15);
            this.labelForeColor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelForeColor.Name = "labelForeColor";
            this.labelForeColor.Size = new System.Drawing.Size(84, 25);
            this.labelForeColor.TabIndex = 21;
            this.labelForeColor.Text = "字体色：";
            // 
            // textBoxForeColor
            // 
            this.textBoxForeColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxForeColor.Location = new System.Drawing.Point(238, 12);
            this.textBoxForeColor.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxForeColor.Name = "textBoxForeColor";
            this.textBoxForeColor.Size = new System.Drawing.Size(90, 31);
            this.textBoxForeColor.TabIndex = 22;
            this.textBoxForeColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelFontSize
            //
            this.labelFontSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.labelFontSize.Location = new System.Drawing.Point(8, 104);
            this.labelFontSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(102, 25);
            this.labelFontSize.TabIndex = 31;
            this.labelFontSize.Text = "字体大小：";
            //
            // trackBarFontSize
            //
            this.trackBarFontSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.trackBarFontSize.AutoSize = false;
            this.appearanceLayout.SetColumnSpan(this.trackBarFontSize, 3);
            this.trackBarFontSize.Location = new System.Drawing.Point(136, 100);
            this.trackBarFontSize.Margin = new System.Windows.Forms.Padding(4);
            this.trackBarFontSize.Maximum = 72;
            this.trackBarFontSize.Minimum = 8;
            this.trackBarFontSize.Name = "trackBarFontSize";
            this.trackBarFontSize.Size = new System.Drawing.Size(340, 36);
            this.trackBarFontSize.TabIndex = 32;
            this.trackBarFontSize.TickFrequency = 4;
            this.trackBarFontSize.Value = 16;
            //
            // labelFontSizeVal
            //
            this.labelFontSizeVal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelFontSizeVal.AutoSize = true;
            this.labelFontSizeVal.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.labelFontSizeVal.Location = new System.Drawing.Point(480, 104);
            this.labelFontSizeVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelFontSizeVal.Name = "labelFontSizeVal";
            this.labelFontSizeVal.Size = new System.Drawing.Size(50, 25);
            this.labelFontSizeVal.TabIndex = 35;
            this.labelFontSizeVal.Text = "16";
            this.labelFontSizeVal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // buttonColorSelect
            // 
            this.buttonColorSelect.AutoSize = true;
            this.buttonColorSelect.Location = new System.Drawing.Point(8, 53);
            this.buttonColorSelect.Margin = new System.Windows.Forms.Padding(4);
            this.buttonColorSelect.Name = "buttonColorSelect";
            this.buttonColorSelect.Size = new System.Drawing.Size(114, 35);
            this.buttonColorSelect.TabIndex = 20;
            this.buttonColorSelect.Text = "配色选择";
            // 
            // labelBackColor
            // 
            this.labelBackColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelBackColor.AutoSize = true;
            this.labelBackColor.Location = new System.Drawing.Point(146, 58);
            this.labelBackColor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBackColor.Name = "labelBackColor";
            this.labelBackColor.Size = new System.Drawing.Size(84, 25);
            this.labelBackColor.TabIndex = 23;
            this.labelBackColor.Text = "背景色：";
            // 
            // textBoxBackColor
            // 
            this.textBoxBackColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxBackColor.Location = new System.Drawing.Point(238, 55);
            this.textBoxBackColor.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxBackColor.Name = "textBoxBackColor";
            this.textBoxBackColor.Size = new System.Drawing.Size(90, 31);
            this.textBoxBackColor.TabIndex = 24;
            this.textBoxBackColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.remoteLayout);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(14, 431);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8);
            this.groupBox1.Size = new System.Drawing.Size(612, 324);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "远控配置";
            // 
            // remoteLayout
            // 
            this.remoteLayout.AutoSize = true;
            this.remoteLayout.ColumnCount = 4;
            this.remoteLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.remoteLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.remoteLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.remoteLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.remoteLayout.Controls.Add(this.label1, 0, 0);
            this.remoteLayout.Controls.Add(this.textBoxIP, 1, 0);
            this.remoteLayout.Controls.Add(this.checkBox4, 0, 1);
            this.remoteLayout.Controls.Add(this.checkBox3, 1, 1);
            this.remoteLayout.Controls.Add(this.linkLabel1, 0, 2);
            this.remoteLayout.Controls.Add(this.label2, 0, 3);
            this.remoteLayout.Controls.Add(this.label3, 3, 3);
            this.remoteLayout.Controls.Add(this.pictureBox1, 0, 4);
            this.remoteLayout.Controls.Add(this.pictureBox2, 3, 4);
            this.linkLabelOfficial = new System.Windows.Forms.LinkLabel();
            this.remoteLayout.Controls.Add(this.linkLabelOfficial, 3, 5);
            this.remoteLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.remoteLayout.Location = new System.Drawing.Point(8, 32);
            this.remoteLayout.Name = "remoteLayout";
            this.remoteLayout.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);
            this.remoteLayout.RowCount = 6;
            this.remoteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteLayout.Size = new System.Drawing.Size(596, 284);
            this.remoteLayout.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 13);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 25);
            this.label1.TabIndex = 11;
            this.label1.Text = "本设备IP地址";
            // 
            // textBoxIP
            // 
            this.textBoxIP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.remoteLayout.SetColumnSpan(this.textBoxIP, 2);
            this.textBoxIP.Location = new System.Drawing.Point(180, 10);
            this.textBoxIP.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.ReadOnly = true;
            this.textBoxIP.Size = new System.Drawing.Size(180, 31);
            this.textBoxIP.TabIndex = 18;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(8, 49);
            this.checkBox4.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(164, 29);
            this.checkBox4.TabIndex = 8;
            this.checkBox4.Text = "开始/暂停/重置";
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.remoteLayout.SetColumnSpan(this.checkBox3, 2);
            this.checkBox3.Location = new System.Drawing.Point(180, 49);
            this.checkBox3.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(110, 29);
            this.checkBox3.TabIndex = 9;
            this.checkBox3.Text = "设定时间";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.remoteLayout.SetColumnSpan(this.linkLabel1, 3);
            this.linkLabel1.Location = new System.Drawing.Point(8, 82);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(174, 25);
            this.linkLabel1.TabIndex = 13;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "浏览器网络访问地址";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 107);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(156, 25);
            this.label2.TabIndex = 14;
            this.label2.Text = "网络控制器二维码";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(448, 107);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(138, 25);
            this.label3.TabIndex = 16;
            this.label3.Text = "微信打赏二维码";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.remoteLayout.SetColumnSpan(this.pictureBox1, 2);
            this.pictureBox1.Location = new System.Drawing.Point(8, 136);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(140, 140);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox2.Image = global::_801CountdownApp1.Properties.Resources.zsm;
            this.pictureBox2.Location = new System.Drawing.Point(448, 136);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(140, 140);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 17;
            this.pictureBox2.TabStop = false;
            // 
            // linkLabelOfficial
            //
            this.linkLabelOfficial.AutoSize = true;
            this.linkLabelOfficial.Dock = System.Windows.Forms.DockStyle.Right;
            this.linkLabelOfficial.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.linkLabelOfficial.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelOfficial.LinkArea = new System.Windows.Forms.LinkArea(0, 4);
            this.linkLabelOfficial.Margin = new System.Windows.Forms.Padding(4, 0, 4, 8);
            this.linkLabelOfficial.Name = "linkLabelOfficial";
            this.linkLabelOfficial.Padding = new System.Windows.Forms.Padding(0, 4, 8, 0);
            this.linkLabelOfficial.Size = new System.Drawing.Size(140, 33);
            this.linkLabelOfficial.TabIndex = 102;
            this.linkLabelOfficial.TabStop = true;
            this.linkLabelOfficial.Text = "官方网站";
            this.linkLabelOfficial.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.linkLabelOfficial.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOfficial_LinkClicked);
            //
            // buttonPanel
            //
            this.buttonPanel.AutoSize = true;
            this.buttonPanel.Controls.Add(this.button3);
            this.buttonPanel.Controls.Add(this.buttonSave);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonPanel.Location = new System.Drawing.Point(14, 763);
            this.buttonPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.buttonPanel.Size = new System.Drawing.Size(612, 73);
            this.buttonPanel.TabIndex = 101;
            // 
            // button3
            // 
            this.button3.AutoSize = true;
            this.button3.Location = new System.Drawing.Point(528, 10);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(80, 45);
            this.button3.TabIndex = 4;
            this.button3.Text = "取消";
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.AutoSize = true;
            this.buttonSave.Location = new System.Drawing.Point(420, 10);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(100, 45);
            this.buttonSave.TabIndex = 99;
            this.buttonSave.Text = "保存配置";
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 500);
            this.Controls.Add(this.mainLayout);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "设置";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.groupBoxBasic.ResumeLayout(false);
            this.groupBoxBasic.PerformLayout();
            this.basicLayout.ResumeLayout(false);
            this.basicLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFontSize)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.alertLayout.ResumeLayout(false);
            this.alertLayout.PerformLayout();
            this.groupBoxAppearance.ResumeLayout(false);
            this.groupBoxAppearance.PerformLayout();
            this.appearanceLayout.ResumeLayout(false);
            this.appearanceLayout.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.remoteLayout.ResumeLayout(false);
            this.remoteLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.buttonPanel.ResumeLayout(false);
            this.buttonPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.GroupBox groupBoxBasic;
        private System.Windows.Forms.TableLayoutPanel basicLayout;
        private System.Windows.Forms.Label labelTotalTime;
        private System.Windows.Forms.TextBox textBoxTotalTime;
        private System.Windows.Forms.ComboBox comboBoxUnit;
        private System.Windows.Forms.Label labelOpacity;
        private System.Windows.Forms.TrackBar trackBarOpacity;
        private System.Windows.Forms.CheckBox checkBoxPPTAuto;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.FlowLayoutPanel alertLayout;
        private System.Windows.Forms.CheckBox checkBoxSound;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.GroupBox groupBoxAppearance;
        private System.Windows.Forms.TableLayoutPanel appearanceLayout;
        private System.Windows.Forms.Button buttonFont;
        private System.Windows.Forms.Label labelForeColor;
        private System.Windows.Forms.TextBox textBoxForeColor;
        private System.Windows.Forms.Label labelBackColor;
        private System.Windows.Forms.TextBox textBoxBackColor;
        private System.Windows.Forms.Button buttonColorSelect;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel remoteLayout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.LinkLabel linkLabelOfficial;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.TrackBar trackBarFontSize;
        private System.Windows.Forms.Label labelFontSizeVal;
    }
}
