namespace AutoRegister
{
    partial class AutoRegisterDemo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_config = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.treeView_devicelist = new System.Windows.Forms.TreeView();
            this.pictureBox_play = new System.Windows.Forms.PictureBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBox_port = new System.Windows.Forms.TextBox();
            this.textBox_ip = new System.Windows.Forms.TextBox();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_start = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button_stoptalk = new System.Windows.Forms.Button();
            this.button_talk = new System.Windows.Forms.Button();
            this.button_capture = new System.Windows.Forms.Button();
            this.button_stoprealplay = new System.Windows.Forms.Button();
            this.button_realplay = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.button_logout = new System.Windows.Forms.Button();
            this.button_clear = new System.Windows.Forms.Button();
            this.button_exportdevicelist = new System.Windows.Forms.Button();
            this.button_importdevicelist = new System.Windows.Forms.Button();
            this.button_deletedevice = new System.Windows.Forms.Button();
            this.button_modifydevice = new System.Windows.Forms.Button();
            this.button_adddevice = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.pictureBox_image = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_play)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_image)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_config);
            this.groupBox1.Location = new System.Drawing.Point(6, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(223, 53);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Operate(操作)";
            // 
            // button_config
            // 
            this.button_config.Location = new System.Drawing.Point(9, 20);
            this.button_config.Name = "button_config";
            this.button_config.Size = new System.Drawing.Size(207, 23);
            this.button_config.TabIndex = 0;
            this.button_config.Text = "Config Device(配置设备)";
            this.button_config.UseVisualStyleBackColor = true;
            this.button_config.Click += new System.EventHandler(this.button_config_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.treeView_devicelist);
            this.groupBox2.Location = new System.Drawing.Point(7, 63);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(223, 455);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "DeviceList(设备列表)";
            // 
            // treeView_devicelist
            // 
            this.treeView_devicelist.Location = new System.Drawing.Point(6, 20);
            this.treeView_devicelist.Name = "treeView_devicelist";
            this.treeView_devicelist.Size = new System.Drawing.Size(210, 429);
            this.treeView_devicelist.TabIndex = 0;
            this.treeView_devicelist.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_devicelist_MouseDown);
            // 
            // pictureBox_play
            // 
            this.pictureBox_play.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.pictureBox_play.Location = new System.Drawing.Point(6, 13);
            this.pictureBox_play.Name = "pictureBox_play";
            this.pictureBox_play.Size = new System.Drawing.Size(287, 234);
            this.pictureBox_play.TabIndex = 2;
            this.pictureBox_play.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBox_port);
            this.groupBox3.Controls.Add(this.textBox_ip);
            this.groupBox3.Controls.Add(this.button_stop);
            this.groupBox3.Controls.Add(this.button_start);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(541, 5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(233, 110);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Listen Device(监听设备)";
            // 
            // textBox_port
            // 
            this.textBox_port.Location = new System.Drawing.Point(125, 46);
            this.textBox_port.Name = "textBox_port";
            this.textBox_port.Size = new System.Drawing.Size(92, 21);
            this.textBox_port.TabIndex = 5;
            this.textBox_port.Text = "9500";
            this.textBox_port.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_port_KeyPress);
            // 
            // textBox_ip
            // 
            this.textBox_ip.Location = new System.Drawing.Point(125, 17);
            this.textBox_ip.Name = "textBox_ip";
            this.textBox_ip.Size = new System.Drawing.Size(92, 21);
            this.textBox_ip.TabIndex = 4;
            this.textBox_ip.Text = "10.34.3.119";
            // 
            // button_stop
            // 
            this.button_stop.Location = new System.Drawing.Point(127, 76);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(80, 23);
            this.button_stop.TabIndex = 7;
            this.button_stop.Text = "Stop(停止)";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_start
            // 
            this.button_start.Location = new System.Drawing.Point(26, 76);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(80, 23);
            this.button_start.TabIndex = 6;
            this.button_start.Text = "Start(开始)";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "Port(监听端口):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "Listen IP(监听IP):";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button_stoptalk);
            this.groupBox5.Controls.Add(this.button_talk);
            this.groupBox5.Controls.Add(this.button_capture);
            this.groupBox5.Controls.Add(this.button_stoprealplay);
            this.groupBox5.Controls.Add(this.button_realplay);
            this.groupBox5.Location = new System.Drawing.Point(541, 347);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(225, 171);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Basic Operation(基本操作)";
            // 
            // button_stoptalk
            // 
            this.button_stoptalk.Location = new System.Drawing.Point(33, 109);
            this.button_stoptalk.Name = "button_stoptalk";
            this.button_stoptalk.Size = new System.Drawing.Size(170, 23);
            this.button_stoptalk.TabIndex = 4;
            this.button_stoptalk.Text = "StopTalk(停止对讲)";
            this.button_stoptalk.UseVisualStyleBackColor = true;
            this.button_stoptalk.Click += new System.EventHandler(this.StopTalk);
            // 
            // button_talk
            // 
            this.button_talk.Location = new System.Drawing.Point(33, 80);
            this.button_talk.Name = "button_talk";
            this.button_talk.Size = new System.Drawing.Size(170, 23);
            this.button_talk.TabIndex = 3;
            this.button_talk.Text = "Talk(对讲)";
            this.button_talk.UseVisualStyleBackColor = true;
            this.button_talk.Click += new System.EventHandler(this.Talk);
            // 
            // button_capture
            // 
            this.button_capture.Location = new System.Drawing.Point(33, 138);
            this.button_capture.Name = "button_capture";
            this.button_capture.Size = new System.Drawing.Size(170, 23);
            this.button_capture.TabIndex = 2;
            this.button_capture.Text = "Capture(抓图)";
            this.button_capture.UseVisualStyleBackColor = true;
            this.button_capture.Click += new System.EventHandler(this.CapturePictrue);
            // 
            // button_stoprealplay
            // 
            this.button_stoprealplay.Location = new System.Drawing.Point(33, 51);
            this.button_stoprealplay.Name = "button_stoprealplay";
            this.button_stoprealplay.Size = new System.Drawing.Size(170, 23);
            this.button_stoprealplay.TabIndex = 1;
            this.button_stoprealplay.Text = "StopPlay(停止监视)";
            this.button_stoprealplay.UseVisualStyleBackColor = true;
            this.button_stoprealplay.Click += new System.EventHandler(this.StopRealPlay);
            // 
            // button_realplay
            // 
            this.button_realplay.Location = new System.Drawing.Point(33, 22);
            this.button_realplay.Name = "button_realplay";
            this.button_realplay.Size = new System.Drawing.Size(170, 23);
            this.button_realplay.TabIndex = 0;
            this.button_realplay.Text = "RealPlay(监视)";
            this.button_realplay.UseVisualStyleBackColor = true;
            this.button_realplay.Click += new System.EventHandler(this.RealPlay);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.button_logout);
            this.groupBox6.Controls.Add(this.button_clear);
            this.groupBox6.Controls.Add(this.button_exportdevicelist);
            this.groupBox6.Controls.Add(this.button_importdevicelist);
            this.groupBox6.Controls.Add(this.button_deletedevice);
            this.groupBox6.Controls.Add(this.button_modifydevice);
            this.groupBox6.Controls.Add(this.button_adddevice);
            this.groupBox6.Location = new System.Drawing.Point(541, 119);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(230, 224);
            this.groupBox6.TabIndex = 6;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "DeviceManager";
            // 
            // button_logout
            // 
            this.button_logout.Location = new System.Drawing.Point(33, 194);
            this.button_logout.Name = "button_logout";
            this.button_logout.Size = new System.Drawing.Size(170, 23);
            this.button_logout.TabIndex = 6;
            this.button_logout.Text = "Logout(设备登出)";
            this.button_logout.UseVisualStyleBackColor = true;
            this.button_logout.Click += new System.EventHandler(this.button_logout_Click);
            // 
            // button_clear
            // 
            this.button_clear.Location = new System.Drawing.Point(33, 165);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(170, 23);
            this.button_clear.TabIndex = 5;
            this.button_clear.Text = "Clear Devices(清空设备)";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // button_exportdevicelist
            // 
            this.button_exportdevicelist.Location = new System.Drawing.Point(33, 136);
            this.button_exportdevicelist.Name = "button_exportdevicelist";
            this.button_exportdevicelist.Size = new System.Drawing.Size(170, 23);
            this.button_exportdevicelist.TabIndex = 4;
            this.button_exportdevicelist.Text = "Export Devices(导出设备)";
            this.button_exportdevicelist.UseVisualStyleBackColor = true;
            this.button_exportdevicelist.Click += new System.EventHandler(this.button_exportdevicelist_Click);
            // 
            // button_importdevicelist
            // 
            this.button_importdevicelist.Location = new System.Drawing.Point(33, 107);
            this.button_importdevicelist.Name = "button_importdevicelist";
            this.button_importdevicelist.Size = new System.Drawing.Size(170, 23);
            this.button_importdevicelist.TabIndex = 3;
            this.button_importdevicelist.Text = "Import Devices(导入设备)";
            this.button_importdevicelist.UseVisualStyleBackColor = true;
            this.button_importdevicelist.Click += new System.EventHandler(this.button_importdevicelist_Click);
            // 
            // button_deletedevice
            // 
            this.button_deletedevice.Location = new System.Drawing.Point(33, 78);
            this.button_deletedevice.Name = "button_deletedevice";
            this.button_deletedevice.Size = new System.Drawing.Size(170, 23);
            this.button_deletedevice.TabIndex = 2;
            this.button_deletedevice.Text = "Delete Device(删除设备)";
            this.button_deletedevice.UseVisualStyleBackColor = true;
            this.button_deletedevice.Click += new System.EventHandler(this.DeleteDevice);
            // 
            // button_modifydevice
            // 
            this.button_modifydevice.Location = new System.Drawing.Point(33, 49);
            this.button_modifydevice.Name = "button_modifydevice";
            this.button_modifydevice.Size = new System.Drawing.Size(170, 23);
            this.button_modifydevice.TabIndex = 1;
            this.button_modifydevice.Text = "Modify Device(修改设备)";
            this.button_modifydevice.UseVisualStyleBackColor = true;
            this.button_modifydevice.Click += new System.EventHandler(this.ModifyDevive);
            // 
            // button_adddevice
            // 
            this.button_adddevice.Location = new System.Drawing.Point(33, 20);
            this.button_adddevice.Name = "button_adddevice";
            this.button_adddevice.Size = new System.Drawing.Size(170, 23);
            this.button_adddevice.TabIndex = 0;
            this.button_adddevice.Text = "Add Device(增加设备)";
            this.button_adddevice.UseVisualStyleBackColor = true;
            this.button_adddevice.Click += new System.EventHandler(this.AddDevice);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.pictureBox_play);
            this.groupBox4.Location = new System.Drawing.Point(236, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(299, 254);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Preview(预览)";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.pictureBox_image);
            this.groupBox7.Location = new System.Drawing.Point(236, 264);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(299, 254);
            this.groupBox7.TabIndex = 8;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Image(图片)";
            // 
            // pictureBox_image
            // 
            this.pictureBox_image.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.pictureBox_image.Location = new System.Drawing.Point(7, 16);
            this.pictureBox_image.Name = "pictureBox_image";
            this.pictureBox_image.Size = new System.Drawing.Size(286, 232);
            this.pictureBox_image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_image.TabIndex = 0;
            this.pictureBox_image.TabStop = false;
            // 
            // AutoRegisterDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(776, 519);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "AutoRegisterDemo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoRegister(主动注册)";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_play)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_image)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox pictureBox_play;
        private System.Windows.Forms.TreeView treeView_devicelist;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBox_port;
        private System.Windows.Forms.TextBox textBox_ip;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_config;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button button_stoprealplay;
        private System.Windows.Forms.Button button_realplay;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button button_exportdevicelist;
        private System.Windows.Forms.Button button_importdevicelist;
        private System.Windows.Forms.Button button_deletedevice;
        private System.Windows.Forms.Button button_modifydevice;
        private System.Windows.Forms.Button button_adddevice;
        private System.Windows.Forms.Button button_capture;
        private System.Windows.Forms.Button button_stoptalk;
        private System.Windows.Forms.Button button_talk;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.PictureBox pictureBox_image;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.Button button_logout;
    }
}

