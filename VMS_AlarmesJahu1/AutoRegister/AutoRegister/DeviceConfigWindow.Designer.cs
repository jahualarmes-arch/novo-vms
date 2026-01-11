namespace AutoRegister
{
    partial class DeviceConfigWindow
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_registerip = new System.Windows.Forms.TextBox();
            this.textBox_registerport = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_deviceid = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_get = new System.Windows.Forms.Button();
            this.button_set = new System.Windows.Forms.Button();
            this.textBox_ip = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_port = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_username = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_login = new System.Windows.Forms.Button();
            this.button_logout = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox_register = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "AutoRegister IP(注册IP):";
            // 
            // textBox_registerip
            // 
            this.textBox_registerip.Location = new System.Drawing.Point(179, 56);
            this.textBox_registerip.Name = "textBox_registerip";
            this.textBox_registerip.Size = new System.Drawing.Size(121, 21);
            this.textBox_registerip.TabIndex = 1;
            // 
            // textBox_registerport
            // 
            this.textBox_registerport.Location = new System.Drawing.Point(179, 84);
            this.textBox_registerport.Name = "textBox_registerport";
            this.textBox_registerport.Size = new System.Drawing.Size(121, 21);
            this.textBox_registerport.TabIndex = 3;
            this.textBox_registerport.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_registerport_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "AutoRegister Port(注册端口):";
            // 
            // textBox_deviceid
            // 
            this.textBox_deviceid.Location = new System.Drawing.Point(179, 113);
            this.textBox_deviceid.Name = "textBox_deviceid";
            this.textBox_deviceid.Size = new System.Drawing.Size(121, 21);
            this.textBox_deviceid.TabIndex = 5;
            this.textBox_deviceid.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_deviceid_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(54, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Device ID(设备序号):";
            // 
            // button_get
            // 
            this.button_get.Location = new System.Drawing.Point(56, 144);
            this.button_get.Name = "button_get";
            this.button_get.Size = new System.Drawing.Size(93, 23);
            this.button_get.TabIndex = 6;
            this.button_get.Text = "Get(获取)";
            this.button_get.UseVisualStyleBackColor = true;
            this.button_get.Click += new System.EventHandler(this.button_get_Click);
            // 
            // button_set
            // 
            this.button_set.Location = new System.Drawing.Point(160, 144);
            this.button_set.Name = "button_set";
            this.button_set.Size = new System.Drawing.Size(86, 23);
            this.button_set.TabIndex = 7;
            this.button_set.Text = "Set(设置)";
            this.button_set.UseVisualStyleBackColor = true;
            this.button_set.Click += new System.EventHandler(this.button_set_Click);
            // 
            // textBox_ip
            // 
            this.textBox_ip.Location = new System.Drawing.Point(179, 20);
            this.textBox_ip.Name = "textBox_ip";
            this.textBox_ip.Size = new System.Drawing.Size(121, 21);
            this.textBox_ip.TabIndex = 9;
            this.textBox_ip.Text = "172.23.12.17";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(60, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "Device IP(设备IP):";
            // 
            // textBox_port
            // 
            this.textBox_port.Location = new System.Drawing.Point(179, 47);
            this.textBox_port.Name = "textBox_port";
            this.textBox_port.Size = new System.Drawing.Size(121, 21);
            this.textBox_port.TabIndex = 11;
            this.textBox_port.Text = "37777";
            this.textBox_port.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_port_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(36, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(137, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "Device Port(设备端口):";
            // 
            // textBox_username
            // 
            this.textBox_username.Location = new System.Drawing.Point(179, 74);
            this.textBox_username.Name = "textBox_username";
            this.textBox_username.Size = new System.Drawing.Size(121, 21);
            this.textBox_username.TabIndex = 13;
            this.textBox_username.Text = "admin";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(66, 77);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(107, 12);
            this.label6.TabIndex = 12;
            this.label6.Text = "UserName(用户名):";
            // 
            // textBox_password
            // 
            this.textBox_password.Location = new System.Drawing.Point(180, 101);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(121, 21);
            this.textBox_password.TabIndex = 15;
            this.textBox_password.Text = "admin123";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(78, 104);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(95, 12);
            this.label7.TabIndex = 14;
            this.label7.Text = "Password(密码):";
            // 
            // button_login
            // 
            this.button_login.Location = new System.Drawing.Point(56, 135);
            this.button_login.Name = "button_login";
            this.button_login.Size = new System.Drawing.Size(93, 23);
            this.button_login.TabIndex = 16;
            this.button_login.Text = "Login(登录)";
            this.button_login.UseVisualStyleBackColor = true;
            this.button_login.Click += new System.EventHandler(this.button_login_Click);
            // 
            // button_logout
            // 
            this.button_logout.Location = new System.Drawing.Point(160, 135);
            this.button_logout.Name = "button_logout";
            this.button_logout.Size = new System.Drawing.Size(86, 23);
            this.button_logout.TabIndex = 17;
            this.button_logout.Text = "Logout(登出)";
            this.button_logout.UseVisualStyleBackColor = true;
            this.button_logout.Click += new System.EventHandler(this.button_logout_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_ip);
            this.groupBox1.Controls.Add(this.button_logout);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.button_login);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBox_password);
            this.groupBox1.Controls.Add(this.textBox_port);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBox_username);
            this.groupBox1.Location = new System.Drawing.Point(10, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(307, 164);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Device Login(设备登录)";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox_register);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBox_registerip);
            this.groupBox2.Controls.Add(this.button_set);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.button_get);
            this.groupBox2.Controls.Add(this.textBox_registerport);
            this.groupBox2.Controls.Add(this.textBox_deviceid);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(10, 173);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(307, 175);
            this.groupBox2.TabIndex = 19;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Config(配置)";
            // 
            // checkBox_register
            // 
            this.checkBox_register.AutoSize = true;
            this.checkBox_register.Location = new System.Drawing.Point(38, 26);
            this.checkBox_register.Name = "checkBox_register";
            this.checkBox_register.Size = new System.Drawing.Size(96, 16);
            this.checkBox_register.TabIndex = 8;
            this.checkBox_register.Text = "Enable(启用)";
            this.checkBox_register.UseVisualStyleBackColor = true;
            // 
            // DeviceConfigWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(326, 352);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeviceConfigWindow";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DeviceConfig(设备配置)";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_registerip;
        private System.Windows.Forms.TextBox textBox_registerport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_deviceid;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_get;
        private System.Windows.Forms.Button button_set;
        private System.Windows.Forms.TextBox textBox_ip;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_port;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_username;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_login;
        private System.Windows.Forms.Button button_logout;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox_register;
    }
}