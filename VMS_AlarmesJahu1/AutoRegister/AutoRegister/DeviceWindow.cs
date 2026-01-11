using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoRegister
{
    public partial class DeviceWindow : Form
    {
        private TreeView m_TreeView;
        private TreeNode m_Node;
        public DEVICE_INFO DeviceInfo { get; set; }
        private OPERATE_TYPE m_Type;
        public enum OPERATE_TYPE
        {
            Add,
            Modify,
        }
        public DeviceWindow(TreeView treeView, TreeNode node)
        {
            InitializeComponent();
            m_TreeView = treeView;
            m_Node = node;
            if (node != null)
            {
                DeviceInfo = (DEVICE_INFO)node.Tag;
            }
            else
            {
                DeviceInfo = new DEVICE_INFO();
            }
            this.Load += new EventHandler(DeviceWindow_Load);
        }

        void DeviceWindow_Load(object sender, EventArgs e)
        {
            if (DeviceInfo.ID != null)
            {
                this.textBox_deviceid.Text = DeviceInfo.ID;
                this.textBox_username.Text = DeviceInfo.UserName;
                this.textBox_password.Text = DeviceInfo.Password;
                this.button_ok.Text = "Modify(修改)";
                this.Text = "Modify Device(修改设备)";
                m_Type = OPERATE_TYPE.Modify;
            }
            else
            {
                this.button_ok.Text = "Add(增加)";
                this.Text = "Add Device(增加设备)";
                m_Type = OPERATE_TYPE.Add;
            }
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            if (textBox_deviceid.Text == null || textBox_deviceid.Text == "")
            {
                MessageBox.Show("Please input device id(请输入设备ID)");
                return;
            }
            if (textBox_username.Text == null || textBox_username.Text == "")
            {
                MessageBox.Show("Please input username(请输入用户名)");
                return;
            }
            if (m_Type == OPERATE_TYPE.Add)
            {
                DeviceInfo.ID = textBox_deviceid.Text.Trim();
                DeviceInfo.UserName = textBox_username.Text.Trim();
                DeviceInfo.Password = textBox_password.Text.Trim();
                bool isExists = false;
                TreeNode[] nodes = m_TreeView.Nodes[0].Nodes.Find(DeviceInfo.ID, false);
                if (nodes.Count() > 0)
                {
                    isExists = true;
                }
                if (!isExists)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("The Device ID is exists(设备已经存在)");
                }
            }
            else
            {
                if (m_Node != null)
                {
                    DEVICE_INFO info = (DEVICE_INFO)m_Node.Tag;
                    if (info.LoginID == IntPtr.Zero)
                    {
                        bool isExists = false;
                        if (DeviceInfo.ID == textBox_deviceid.Text.Trim())
                        {
                            DeviceInfo.ID = textBox_deviceid.Text.Trim();
                            DeviceInfo.UserName = textBox_username.Text.Trim();
                            DeviceInfo.Password = textBox_password.Text.Trim();
                            this.DialogResult = System.Windows.Forms.DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            TreeNode[] nodes = m_TreeView.Nodes[0].Nodes.Find(textBox_deviceid.Text.Trim(), false);
                            if (nodes.Count() > 0)
                            {
                                isExists = true;
                            }
                            if (!isExists)
                            {
                                DeviceInfo.ID = textBox_deviceid.Text.Trim();
                                DeviceInfo.UserName = textBox_username.Text.Trim();
                                DeviceInfo.Password = textBox_password.Text.Trim();
                                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("The Device ID is exists(设备已经存在)");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Device has logged in(设备已登录)");
                    }
                }
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
