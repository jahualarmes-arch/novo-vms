namespace AutoRegister
{
    partial class DeviceWindow
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
            this.textBox_deviceid = new System.Windows.Forms.TextBox();
            this.textBox_username = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button_ok = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Device ID(设备ID):";
            // 
            // textBox_deviceid
            // 
            this.textBox_deviceid.Location = new System.Drawing.Point(142, 21);
            this.textBox_deviceid.Name = "textBox_deviceid";
            this.textBox_deviceid.Size = new System.Drawing.Size(124, 21);
            this.textBox_deviceid.TabIndex = 2;
            this.textBox_deviceid.Text = "test";
            this.textBox_deviceid.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_deviceid_KeyPress);
            // 
            // textBox_username
            // 
            this.textBox_username.Location = new System.Drawing.Point(142, 48);
            this.textBox_username.Name = "textBox_username";
            this.textBox_username.Size = new System.Drawing.Size(124, 21);
            this.textBox_username.TabIndex = 9;
            this.textBox_username.Text = "admin";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 51);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(107, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "UserName(用户名):";
            // 
            // textBox_password
            // 
            this.textBox_password.Location = new System.Drawing.Point(142, 75);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(124, 21);
            this.textBox_password.TabIndex = 11;
            this.textBox_password.Text = "admin123";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(41, 75);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "Passwrod(密码):";
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(101, 108);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(97, 23);
            this.button_ok.TabIndex = 12;
            this.button_ok.Text = "ok";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // DeviceWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(301, 143);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_username);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_deviceid);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeviceWindow";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AddDevice";
            this.Click += new System.EventHandler(this.button_ok_Click);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_deviceid;
        private System.Windows.Forms.TextBox textBox_username;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button_ok;
    }
}