using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetSDKCS;
using System.Runtime.InteropServices;

namespace AutoRegister
{
    public partial class DeviceConfigWindow : Form
    {
        const string CFG_CMD_DVRIP = "DVRIP";
        const int m_Waite = 3000;
        NET_DEVICEINFO_Ex m_DeviceInfo = new NET_DEVICEINFO_Ex();
        IntPtr m_LoginID = IntPtr.Zero;

        public DeviceConfigWindow()
        {
            InitializeComponent();
            this.Load += new EventHandler(DeviceConfigWindow_Load);
        }

        void DeviceConfigWindow_Load(object sender, EventArgs e)
        {
            this.checkBox_register.Enabled = false;
            this.button_logout.Enabled = false;
            this.button_get.Enabled = false;
            this.button_set.Enabled = false;
            this.textBox_registerip.Enabled = false;
            this.textBox_registerport.Enabled = false;
            this.textBox_deviceid.Enabled = false;
        }

        private void button_login_Click(object sender, EventArgs e)
        {
            string ip;
            ushort port;
            string name;
            string password;
            if (textBox_ip.Text == null || textBox_ip.Text == "")
            {
                MessageBox.Show("Please input device ip(请输入设备IP地址)");
                return;
            }
            ip = textBox_ip.Text.Trim();
            if (textBox_port.Text == null || textBox_port.Text == "")
            {
                MessageBox.Show("Please input device port(请输入设备端口号)");
                return;
            }
            try
            {
                port = Convert.ToUInt16(textBox_port.Text.Trim());
            }
            catch
            {
                MessageBox.Show("The device port is error,the value must be 1-65535(设备端口错误，值为1-65535)");
                return;
            }
            if (textBox_username.Text == null || textBox_username.Text == "")
            {
                MessageBox.Show("Please input username(请输入用户名)");
                return;
            }
            name = textBox_username.Text.Trim();
            password = textBox_password.Text;
            m_LoginID = NETClient.Login(ip, port, name, password, EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
            if (IntPtr.Zero == m_LoginID)
            {
                MessageBox.Show(NETClient.GetLastError());
                return;
            }
            this.checkBox_register.Enabled = true;
            this.button_login.Enabled = false;
            this.button_logout.Enabled = true;
            this.button_get.Enabled = true;
            this.button_set.Enabled = true;
            this.textBox_registerip.Enabled = true;
            this.textBox_registerport.Enabled = true;
            this.textBox_deviceid.Enabled = true;
        }

        private void button_logout_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != m_LoginID)
            {
                bool ret = NETClient.Logout(m_LoginID);
                if (!ret)
                {
                    MessageBox.Show(NETClient.GetLastError());
                    return;
                }
                m_LoginID = IntPtr.Zero;
                this.checkBox_register.Checked = false;
                this.checkBox_register.Enabled = false;
                this.button_login.Enabled = true;
                this.button_logout.Enabled = false;
                this.button_get.Enabled = false;
                this.button_set.Enabled = false;
                this.textBox_registerip.Enabled = false;
                this.textBox_registerport.Enabled = false;
                this.textBox_deviceid.Enabled = false;
                this.textBox_registerip.Text = "";
                this.textBox_registerport.Text = "";
                this.textBox_deviceid.Text = "";
            }
        }

        private void button_get_Click(object sender, EventArgs e)
        {
            CFG_DVRIP_INFO info = new CFG_DVRIP_INFO();
            object obj = info;
            bool ret = NETClient.GetNewDevConfig(m_LoginID, -1, CFG_CMD_DVRIP, ref obj, typeof(CFG_DVRIP_INFO), m_Waite);
            if (!ret)
            {
                MessageBox.Show(NETClient.GetLastError());
                return;
            }
            info = (CFG_DVRIP_INFO)obj;
            textBox_deviceid.Text = info.stuRegisters[0].szDeviceID;
            textBox_registerip.Text = info.stuRegisters[0].stuServers[0].szAddress;
            textBox_registerport.Text = info.stuRegisters[0].stuServers[0].nPort.ToString();
            checkBox_register.Checked = info.stuRegisters[0].bEnable;
        }

        private void button_set_Click(object sender, EventArgs e)
        {
            if (textBox_registerip.Text == null || textBox_registerip.Text == "")
            {
                MessageBox.Show("Please input register IP(请输入注册IP地址)");
                return;
            }
            if (textBox_deviceid.Text == null || textBox_deviceid.Text == "")
            {
                MessageBox.Show("Please input device ID(请输入设备ID)");
                return;
            }
            if (textBox_registerport.Text == null || textBox_registerport.Text == "")
            {
                MessageBox.Show("Please input register Port(请输入注册端口号)");
                return;
            }
            ushort port;
            try
            {
                port = Convert.ToUInt16(textBox_registerport.Text.Trim());
            }
            catch
            {
                MessageBox.Show("The register port is error,the value must be 1-65535(注册端口号错误，值为1-65535)");
                return;
            }
            CFG_DVRIP_INFO info = new CFG_DVRIP_INFO();
            object getobj = info;
            bool ret = NETClient.GetNewDevConfig(m_LoginID, -1, CFG_CMD_DVRIP, ref getobj, typeof(CFG_DVRIP_INFO), m_Waite);
            if (!ret)
            {
                MessageBox.Show(NETClient.GetLastError());
                return;
            }
            info = (CFG_DVRIP_INFO)getobj;
            info.nRegistersNum = 1;
            info.stuRegisters[0].szDeviceID = textBox_deviceid.Text.Trim();
            info.stuRegisters[0].stuServers[0].szAddress = textBox_registerip.Text.Trim();
            info.stuRegisters[0].stuServers[0].nPort = port;
            info.stuRegisters[0].bEnable = this.checkBox_register.Checked;
            info.stuRegisters[0].nServersNum = 1;
            object obj = info;
            ret = NETClient.SetNewDevConfig(m_LoginID, -1, CFG_CMD_DVRIP, obj, typeof(CFG_DVRIP_INFO), m_Waite);
            if (!ret)
            {
                MessageBox.Show(NETClient.GetLastError());
                return;
            }
            else
            {
                MessageBox.Show("Set successfully(设置成功)");
                return;
            }     
        }

        private void textBox_port_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox_registerport_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox_deviceid_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Encoding.Default.GetBytes(((TextBox)sender).Text).Length > 253 && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }
    }
}
