using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetSDKCS;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace AutoRegister
{

    public partial class AutoRegisterDemo : Form
    {
        private IntPtr m_ListenID = IntPtr.Zero;
        private fServiceCallBack m_ServiceCallBack;
        private fDisConnectCallBack m_DisConnectCallBack;
        private TreeNode m_CurrentNode;
        private IntPtr m_RealPlayID = IntPtr.Zero;
        private string m_CurrentPlayedDeviceID = "";
        private object queueLock = new object();
        private Queue<DEVICE_INFO> m_DeviceQueue = new Queue<DEVICE_INFO>();
        private Thread m_LoginThread;
        private IntPtr m_TalkID = IntPtr.Zero;
        private fSnapRevCallBack m_SnapRevCallBack;
        private const int SampleRate = 8000;
        private const int AudioBit = 16;
        private const int PacketPeriod = 25;
        private const int SendAudio = 0;
        private const int ReviceAudio = 1;
        private fAudioDataCallBack m_AudioDataCallBack;
        private IntPtr m_LoginID = IntPtr.Zero;
        private string m_CurrentTalkDeviceID = "";
        
        public AutoRegisterDemo()
        {
            InitializeComponent();
            this.Load += new EventHandler(AutoRegisterDemo_Load);
        }

        void AutoRegisterDemo_Load(object sender, EventArgs e)
        {
            try
            {
                m_DisConnectCallBack = new fDisConnectCallBack(DisConnectCallBack);
                NETClient.Init(m_DisConnectCallBack, IntPtr.Zero, null);
                m_ServiceCallBack = new fServiceCallBack(ServiceCallBack);
                m_SnapRevCallBack = new fSnapRevCallBack(SnapRevCallBack);
                NETClient.SetSnapRevCallBack(m_SnapRevCallBack, IntPtr.Zero);
                m_AudioDataCallBack = new fAudioDataCallBack(AudioDataCallBack);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Process.GetCurrentProcess().Kill();
            }
            ThreadStart ts = new ThreadStart(DeviceLogin);
            m_LoginThread = new Thread(ts);
            m_LoginThread.IsBackground = true;
            m_LoginThread.Start();
            DateTime now = DateTime.Now;
            this.button_logout.Enabled = false;
            this.button_stop.Enabled = false;
            this.button_modifydevice.Enabled = false;
            this.button_deletedevice.Enabled = false;
            this.button_realplay.Enabled = false;
            this.button_stoprealplay.Enabled = false;
            this.button_talk.Enabled = false;
            this.button_stoptalk.Enabled = false;
            this.button_capture.Enabled = false;
            this.treeView_devicelist.Nodes.Add("Root(根目录)");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            NETClient.Cleanup();
        }

        private void DisConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            string ip = Marshal.PtrToStringAnsi(pchDVRIP);
            this.Invoke(new Action<string, int>(DisconnectUpdateUI), ip, nDVRPort);//如果有对讲业务这里必须要用同步，断线回调体出来登录句柄会被SDK清空,此时就无法停止对讲
        }

        private void DisconnectUpdateUI(string ip, int port)
        {
            foreach (TreeNode node in treeView_devicelist.Nodes[0].Nodes)
            {
                DEVICE_INFO item = (DEVICE_INFO)node.Tag;
                if (item.IP == ip && item.Port == port)
                {
                    if (m_CurrentNode != null && m_CurrentNode.Parent != null && m_CurrentNode.Parent.Parent == null)//deivce node(设备节点)
                    {
                        DEVICE_INFO info = (DEVICE_INFO)m_CurrentNode.Tag;
                        if (info.ID == item.ID)
                        {
                            this.button_talk.Enabled = false;
                            this.button_stoptalk.Enabled = false;
                            this.button_logout.Enabled = false;
                            this.button_modifydevice.Enabled = true;
                            this.button_deletedevice.Enabled = true;
                        }
                    }
                    if (m_CurrentNode != null && m_CurrentNode.Parent != null && m_CurrentNode.Parent.Parent != null)// channel node(通道节点)
                    {
                        DEVICE_INFO info = (DEVICE_INFO)m_CurrentNode.Parent.Tag;
                        if (info.ID == item.ID)
                        {
                            m_CurrentNode = m_CurrentNode.Parent;
                            this.treeView_devicelist.SelectedNode = m_CurrentNode;
                            this.button_capture.Enabled = false;
                            this.button_realplay.Enabled = false;
                            this.button_stoprealplay.Enabled = false;
                            this.button_modifydevice.Enabled = true;
                            this.button_deletedevice.Enabled = true;
                        }
                    }
                    if (item.ID == m_CurrentPlayedDeviceID )
                    {
                        m_CurrentPlayedDeviceID = "";
                        NETClient.StopRealPlay(m_RealPlayID);
                        m_RealPlayID = IntPtr.Zero;
                        if (pictureBox_play.Image != null)
                        {
                            pictureBox_play.Image.Dispose();
                        }
                        pictureBox_play.Refresh();
                    }
                    if (item.ID == m_CurrentTalkDeviceID)
                    {
                        NETClient.RecordStop(item.LoginID);
                        m_LoginID = IntPtr.Zero;
                        NETClient.StopTalk(m_TalkID);
                        m_TalkID = IntPtr.Zero;
                    }
                    NETClient.Logout(item.LoginID);
                    item.LoginID = IntPtr.Zero;
                    node.Text = item.ID;
                    node.ForeColor = Color.Black;
                    node.Nodes.Clear();
                    break;
                }
            }
        }

        private void button_config_Click(object sender, EventArgs e)
        {
            DeviceConfigWindow deviceConfigWindow = new DeviceConfigWindow();
            deviceConfigWindow.ShowDialog();
            deviceConfigWindow.Dispose();
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            if (textBox_ip.Text == null || textBox_ip.Text == "")
            {
                MessageBox.Show("Please input listen IP(请输入监听IP)");
                return;
            }
            if (textBox_port.Text == null || textBox_port.Text == "")
            {
                MessageBox.Show("Please input listen Port(请输入监听端口)");
                return;
            }
            ushort port;
            try
            {
                port = Convert.ToUInt16(textBox_port.Text.Trim());
            }
            catch
            {
                MessageBox.Show("Listen port is error,the value must be 1-65535(监听端口错误,值是1-65535)");
                return;
            }
            m_ListenID = NETClient.ListenServer(textBox_ip.Text.Trim(), port, 1000, m_ServiceCallBack, IntPtr.Zero);
            if (IntPtr.Zero == m_ListenID)
            {
                MessageBox.Show(NETClient.GetLastError());
                return;
            }
            this.button_start.Enabled = false;
            this.button_stop.Enabled = true;
            this.button_modifydevice.Enabled = false;
            this.button_deletedevice.Enabled = false;
            this.button_stop.Focus();
        }

        private int ServiceCallBack(IntPtr lHandle, IntPtr pIp, ushort wPort, int lCommand, IntPtr pParam, uint dwParamLen, IntPtr dwUserData)
        {
            EM_LISTEN_TYPE type = (EM_LISTEN_TYPE)lCommand;
            string ip = Marshal.PtrToStringAnsi(pIp);
            string id = "";
            if (dwParamLen > 0)
            {
                id = Marshal.PtrToStringAnsi(pParam);
            }
            this.BeginInvoke(new Action<string, ushort, EM_LISTEN_TYPE, string>(UpdateDevice), ip, wPort, type, id);
            return 0;
        }

        private void UpdateDevice(string ip, ushort port, EM_LISTEN_TYPE type, string id)
        {
            switch (type)
            {
                case EM_LISTEN_TYPE.NET_DVR_DISCONNECT:
                    {
                        TreeNode[] nodes = treeView_devicelist.Nodes[0].Nodes.Find(id, false);
                        if (nodes.Count() > 0)
                        {
                            DEVICE_INFO info = (DEVICE_INFO)nodes[0].Tag;
                            if (info.LoginID != IntPtr.Zero)
                            {
                                info.LoginID = IntPtr.Zero;
                            }
                            nodes[0].ForeColor = Color.Black;
                            nodes[0].Nodes.Clear();
                        }
                    }
                    break;
                case EM_LISTEN_TYPE.NET_DVR_SERIAL_RETURN:
                    {
                        TreeNode[] nodes = treeView_devicelist.Nodes[0].Nodes.Find(id, false);
                        if (nodes.Count() > 0)
                        {
                            DEVICE_INFO info = (DEVICE_INFO)nodes[0].Tag;
                            info.IP = ip;
                            info.Port = port;
                            lock (queueLock)
                            {
                                m_DeviceQueue.Enqueue(info);
                            }
                        }
                    }
                    break;
            }
        }

        private void DeviceLogin()
        {
            while (true)
            {
                bool res = false;
                lock (queueLock)
                {
                    if (m_DeviceQueue.Count > 0)
                    {
                        res = true;
                    }
                }
                if (res)
                {
                    DEVICE_INFO item;
                    lock (queueLock)
                    {
                        item = m_DeviceQueue.Dequeue();
                    }
                    if (item == null)
                    {
                        continue;
                    }
                    NET_DEVICEINFO_Ex device = new NET_DEVICEINFO_Ex();
                    IntPtr pParam = Marshal.StringToHGlobalAnsi(item.ID);
                    IntPtr loginID = NETClient.Login(item.IP, item.Port, item.UserName, item.Password, EM_LOGIN_SPAC_CAP_TYPE.SERVER_CONN, pParam, ref device);
                    if (loginID != IntPtr.Zero)
                    {
                        item.LoginID = loginID;
                        item.ChannelNumber = device.nChanNum;
                        this.BeginInvoke(new Action(() => {
                            if (m_CurrentNode != null && m_CurrentNode.Parent != null && m_CurrentNode.Parent.Parent == null)
                            {
                                DEVICE_INFO info = (DEVICE_INFO)m_CurrentNode.Tag;
                                if (info != null)
                                {
                                    if (info.ID == item.ID)
                                    {
                                        this.button_modifydevice.Enabled = false;
                                        this.button_logout.Enabled = true;
                                        if (m_TalkID == IntPtr.Zero)
                                        {
                                            this.button_talk.Enabled = true;
                                            this.button_stoptalk.Enabled = false;
                                        }
                                        else
                                        {
                                            this.button_talk.Enabled = false;
                                            this.button_stoptalk.Enabled = true;
                                        }
                                    }
                                }
                            }
                            TreeNode[] nodes = treeView_devicelist.Nodes[0].Nodes.Find(item.ID, false);
                            if (nodes.Count() > 0)
                            {
                                nodes[0].Text += "|" + item.IP;
                                nodes[0].ForeColor = Color.Green;
                                for (int i = 0; i < item.ChannelNumber; i++)
                                {
                                    nodes[0].Nodes.Add(string.Format("Channel(通道)-{0}", i + 1));
                                }
                            }
                        }));
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            if (m_ListenID != IntPtr.Zero)
            {
                bool ret = NETClient.StopListenServer(m_ListenID);
                if (!ret)
                {
                    MessageBox.Show(NETClient.GetLastError());
                    return;
                }
                m_ListenID = IntPtr.Zero;
                this.button_start.Enabled = true;
                this.button_stop.Enabled = false;
                this.button_modifydevice.Enabled = false;
                this.button_deletedevice.Enabled = false;
                this.button_start.Focus();
            }
        }

        private void textBox_port_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void treeView_devicelist_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left || e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                m_CurrentNode = treeView_devicelist.GetNodeAt(new Point(e.X, e.Y));
                treeView_devicelist.SelectedNode = m_CurrentNode;
                if (m_CurrentNode == null)
                {
                    this.button_modifydevice.Enabled = false;
                    this.button_deletedevice.Enabled = false;
                    this.button_realplay.Enabled = false;
                    this.button_stoprealplay.Enabled = false;
                    this.button_talk.Enabled = false;
                    this.button_stoptalk.Enabled = false;
                    this.button_capture.Enabled = false;
                    this.button_logout.Enabled = false;
                    return;
                }
                if (m_CurrentNode.Parent == null)
                {
                    this.button_modifydevice.Enabled = false;
                    this.button_deletedevice.Enabled = false;
                    this.button_realplay.Enabled = false;
                    this.button_stoprealplay.Enabled = false;
                    this.button_talk.Enabled = false;
                    this.button_stoptalk.Enabled = false;
                    this.button_capture.Enabled = false;
                    this.button_logout.Enabled = false;
                }
                else if (m_CurrentNode.Parent != null && m_CurrentNode.Parent.Parent == null)
                {
                    if (((DEVICE_INFO)m_CurrentNode.Tag).LoginID != IntPtr.Zero)
                    {
                        this.button_modifydevice.Enabled = false;
                        this.button_logout.Enabled = true;
                        if (m_TalkID == IntPtr.Zero)
                        {
                            this.button_talk.Enabled = true;
                            this.button_stoptalk.Enabled = false;
                        }
                        else
                        {
                            this.button_talk.Enabled = false;
                            this.button_stoptalk.Enabled = true;
                        }
                    }
                    else
                    {
                        this.button_talk.Enabled = false;
                        this.button_stoptalk.Enabled = false;
                        this.button_modifydevice.Enabled = true;
                        this.button_logout.Enabled = false;
                    }
                    this.button_deletedevice.Enabled = true;
                    this.button_realplay.Enabled = false;
                    this.button_stoprealplay.Enabled = false;
                    this.button_capture.Enabled = false;
                }
                else
                {
                    this.button_modifydevice.Enabled = false;
                    this.button_deletedevice.Enabled = false;
                    this.button_logout.Enabled = false;
                    if (m_RealPlayID == IntPtr.Zero)
                    {
                        this.button_realplay.Enabled = true;
                        this.button_stoprealplay.Enabled = false;
                    }
                    else
                    {
                        this.button_realplay.Enabled = false;
                        this.button_stoprealplay.Enabled = true;
                    }
                    this.button_capture.Enabled = true;
                    this.button_talk.Enabled = false;
                    this.button_stoptalk.Enabled = false;
                }
            }
        }

        private void AddDevice(object sender, EventArgs e)
        {
            if (treeView_devicelist.Nodes[0].Nodes.Count == 100)
            {
                MessageBox.Show("Can't add device,the device has reached 100(不能增加设备,设备已达到100个)");
                return;
            }
            DeviceWindow addDeviceWindow = new DeviceWindow(this.treeView_devicelist, null);
            var ret = addDeviceWindow.ShowDialog();
            if (ret == System.Windows.Forms.DialogResult.OK)
            {
                TreeNode node = new TreeNode();
                node.Name = addDeviceWindow.DeviceInfo.ID;
                node.Text = addDeviceWindow.DeviceInfo.ID;
                node.Tag = addDeviceWindow.DeviceInfo;
                this.treeView_devicelist.Nodes[0].Nodes.Add(node);
            }
            this.treeView_devicelist.Focus();
        }

        private void ModifyDevive(object sender, EventArgs e)
        {
            if (m_CurrentNode == null)
            {
                return;
            }
            DEVICE_INFO info = (DEVICE_INFO)m_CurrentNode.Tag;
            DeviceWindow editDeviceWindow = new DeviceWindow(this.treeView_devicelist, m_CurrentNode);
            var ret = editDeviceWindow.ShowDialog();
            if (ret == System.Windows.Forms.DialogResult.OK)
            {
                if (info.LoginID == IntPtr.Zero)
                {
                    m_CurrentNode.Text = editDeviceWindow.DeviceInfo.ID;
                    m_CurrentNode.Name = editDeviceWindow.DeviceInfo.ID;
                }
                else
                {
                    m_CurrentNode.Text = editDeviceWindow.DeviceInfo.ID + "|" + info.IP;
                    m_CurrentNode.Name = editDeviceWindow.DeviceInfo.ID;
                }
            }
            this.treeView_devicelist.Focus();
            this.treeView_devicelist.SelectedNode = m_CurrentNode;
        }

        private void DeleteDevice(object sender, EventArgs e)
        {
            if (m_CurrentNode == null)
            {
                return;
            }
            DEVICE_INFO info = (DEVICE_INFO)m_CurrentNode.Tag;
            if (m_TalkID != IntPtr.Zero && info.ID == m_CurrentTalkDeviceID)
            {
                NETClient.RecordStop(info.LoginID);
                m_LoginID = IntPtr.Zero;
                NETClient.StopTalk(m_TalkID);
                m_TalkID = IntPtr.Zero;
                this.button_stoptalk.Enabled = false;
                m_CurrentTalkDeviceID = "";
            }
            if (m_RealPlayID != IntPtr.Zero && info.ID  == m_CurrentPlayedDeviceID)
            {
                NETClient.StopRealPlay(m_RealPlayID);
                this.button_stoprealplay.Enabled = false;
                m_RealPlayID = IntPtr.Zero;
                m_CurrentPlayedDeviceID = "";
            }
            if (info.LoginID != IntPtr.Zero)
            {
                NETClient.Logout(info.LoginID);
                this.button_logout.Enabled = false;
            }
            this.treeView_devicelist.Nodes[0].Nodes.Remove(m_CurrentNode);
            m_CurrentNode = null;
            pictureBox_play.Refresh();
            this.treeView_devicelist.Focus();
            this.button_talk.Enabled = false;
            this.button_realplay.Enabled = false;
            this.button_modifydevice.Enabled = false;
            this.button_deletedevice.Enabled = false;
            this.treeView_devicelist.SelectedNode = null;
        }

        private void RealPlay(object sender, EventArgs e)
        {
            TreeNode node = m_CurrentNode.Parent;
            if (node == null)
            {
                MessageBox.Show("This device is offline(设备已离线)");
                return;
            }
            DEVICE_INFO info = (DEVICE_INFO)node.Tag;
            if (m_RealPlayID != IntPtr.Zero)
            {
                NETClient.StopRealPlay(m_RealPlayID);
                m_RealPlayID = IntPtr.Zero;
            }
            IntPtr playID = NETClient.RealPlay(info.LoginID, m_CurrentNode.Index, pictureBox_play.Handle, EM_RealPlayType.Realplay);
            if (playID == IntPtr.Zero)
            {
                MessageBox.Show(NETClient.GetLastError());
                return;
            }
            m_CurrentPlayedDeviceID = info.ID;
            m_RealPlayID = playID;
            this.treeView_devicelist.Focus();
            this.button_realplay.Enabled = false;
            this.button_stoprealplay.Enabled = true;
        }

        private void StopRealPlay(object sender, EventArgs e)
        {
            this.treeView_devicelist.Focus();
            TreeNode node = m_CurrentNode.Parent;
            if (node == null)
            {
                MessageBox.Show("This device is offline(设备已离线)");
                return;
            }
            if (m_RealPlayID != IntPtr.Zero)
            {
                NETClient.StopRealPlay(m_RealPlayID);
                m_RealPlayID = IntPtr.Zero;
                pictureBox_play.Refresh();
                this.button_realplay.Enabled = true;
                this.button_stoprealplay.Enabled = false;
                m_CurrentPlayedDeviceID = "";
            }
        }

        private void Talk(object sender, EventArgs e)
        {
            this.treeView_devicelist.Focus();
            TreeNode node = m_CurrentNode;
            if (node == null)
            {
                MessageBox.Show("This device is offline(设备已离线)");
                return;
            }
            string id = "";
            DEVICE_INFO info = (DEVICE_INFO)node.Tag;
            m_LoginID = info.LoginID;
            id = info.ID;

            IntPtr talkEncodePointer = IntPtr.Zero;
            IntPtr talkSpeakPointer = IntPtr.Zero;
            IntPtr talkTransferPointer = IntPtr.Zero;
            IntPtr channelPointer = IntPtr.Zero;

            NET_DEV_TALKDECODE_INFO talkCodeInfo = new NET_DEV_TALKDECODE_INFO();
            talkCodeInfo.encodeType = EM_TALK_CODING_TYPE.PCM;
            talkCodeInfo.dwSampleRate = SampleRate;
            talkCodeInfo.nAudioBit = AudioBit;
            talkCodeInfo.nPacketPeriod = PacketPeriod;
            talkCodeInfo.reserved = new byte[60];

            talkEncodePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NET_DEV_TALKDECODE_INFO)));
            Marshal.StructureToPtr(talkCodeInfo, talkEncodePointer, true);
            // set talk encode type 设置对讲编码类型
            NETClient.SetDeviceMode(m_LoginID, EM_USEDEV_MODE.TALK_ENCODE_TYPE, talkEncodePointer);

            NET_SPEAK_PARAM speak = new NET_SPEAK_PARAM();
            speak.dwSize = (uint)Marshal.SizeOf(typeof(NET_SPEAK_PARAM));
            speak.nMode = 0;
            speak.bEnableWait = false;
            speak.nSpeakerChannel = 0;
            talkSpeakPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NET_SPEAK_PARAM)));
            Marshal.StructureToPtr(speak, talkSpeakPointer, true);
            //set talk speak mode 设置对讲模式
            NETClient.SetDeviceMode(m_LoginID, EM_USEDEV_MODE.TALK_SPEAK_PARAM, talkSpeakPointer);

            NET_TALK_TRANSFER_PARAM transfer = new NET_TALK_TRANSFER_PARAM();
            transfer.dwSize = (uint)Marshal.SizeOf(typeof(NET_TALK_TRANSFER_PARAM));
            transfer.bTransfer = false;
            talkTransferPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NET_TALK_TRANSFER_PARAM)));
            Marshal.StructureToPtr(transfer, talkTransferPointer, true);
            //set talk transfer mode 设置对讲是否转发模式
            NETClient.SetDeviceMode(m_LoginID, EM_USEDEV_MODE.TALK_TRANSFER_MODE, talkTransferPointer);

            m_TalkID = NETClient.StartTalk(m_LoginID, m_AudioDataCallBack, IntPtr.Zero);
            Marshal.FreeHGlobal(talkEncodePointer);
            Marshal.FreeHGlobal(talkSpeakPointer);
            Marshal.FreeHGlobal(talkTransferPointer);
            Marshal.FreeHGlobal(channelPointer);
            if (IntPtr.Zero == m_TalkID)
            {
                MessageBox.Show(this, NETClient.GetLastError());
                return;
            }
            bool ret = NETClient.RecordStart(m_LoginID);
            if (!ret)
            {
                NETClient.StopTalk(m_TalkID);
                m_TalkID = IntPtr.Zero;
                MessageBox.Show(this, NETClient.GetLastError());
                return;
            }
            m_CurrentTalkDeviceID = id;
            this.button_talk.Enabled = false;
            this.button_stoptalk.Enabled = true;
        }

        private void StopTalk(object sender, EventArgs e)
        {
            this.treeView_devicelist.Focus();
            TreeNode node = m_CurrentNode.Parent;
            if (node == null)
            {
                MessageBox.Show("This device is offline(设备已离线)");
                return;
            }
            NETClient.RecordStop(m_LoginID);
            m_LoginID = IntPtr.Zero;
            NETClient.StopTalk(m_TalkID);
            m_TalkID = IntPtr.Zero;
            this.button_talk.Enabled = true;
            this.button_stoptalk.Enabled = false;
        }

        private void AudioDataCallBack(IntPtr lTalkHandle, IntPtr pDataBuf, uint dwBufSize, byte byAudioFlag, IntPtr dwUser)
        {
            if (lTalkHandle == m_TalkID)
            {
                if (SendAudio == byAudioFlag)
                {
                    //send talk data 发送语音数据
                    NETClient.TalkSendData(lTalkHandle, pDataBuf, dwBufSize);
                }
                else if (ReviceAudio == byAudioFlag)
                {
                    //here call netsdk decode audio,or can send data to other user.这里调用netsdk解码语音数据，或也可以把语音数据发送给另外的用户
                    try
                    {
                        NETClient.AudioDec(pDataBuf, dwBufSize);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }


        private void CapturePictrue(object sender, EventArgs e)
        {
            this.treeView_devicelist.Focus();
            TreeNode node = m_CurrentNode.Parent;
            if (node == null)
            {
                MessageBox.Show("This device is offline(设备已离线)");
                return;
            }
            DEVICE_INFO info = (DEVICE_INFO)node.Tag;
            NET_SNAP_PARAMS asyncSnap = new NET_SNAP_PARAMS();
            asyncSnap.Channel = (uint)m_CurrentNode.Index;
            asyncSnap.Quality = 6;
            asyncSnap.ImageSize = 2;
            asyncSnap.mode = 0;
            asyncSnap.InterSnap = 0;
            bool ret = NETClient.SnapPictureEx(info.LoginID, asyncSnap, IntPtr.Zero);
            if (!ret)
            {
                MessageBox.Show(this, NETClient.GetLastError());
                return;
            }
        }

        private void SnapRevCallBack(IntPtr lLoginID, IntPtr pBuf, uint RevLen, uint EncodeType, uint CmdSerial, IntPtr dwUser)
        {
            if (EncodeType == 10) //.jpg
            {
                byte[] data = new byte[RevLen];
                Marshal.Copy(pBuf, data, 0, (int)RevLen);
                using (MemoryStream stream = new MemoryStream(data))
                {
                    try // add try catch for catch exception when the stream is not image format,and the stream is from device.
                    {
                        Image faceImage = Image.FromStream(stream);
                        pictureBox_image.Image = faceImage;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        private void button_exportdevicelist_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "|*.csv";
            var ret = saveFileDialog.ShowDialog();
            if (ret == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    List<DEVICE_INFO> list = new List<DEVICE_INFO>();
                    foreach (TreeNode node in this.treeView_devicelist.Nodes[0].Nodes)
                    {
                        list.Add((DEVICE_INFO)node.Tag);
                    }
                    CSVHelper helper = new CSVHelper();
                    helper.WriteToCSVFile(list, saveFileDialog.FileName, false);
                    MessageBox.Show("Save successfully(保存成功)!");
                }
                catch
                {
                    saveFileDialog.Dispose();
                }
            }
            saveFileDialog.Dispose();
        }

        private void button_importdevicelist_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV(*.csv)|*.csv";
            var ret = openFileDialog.ShowDialog();
            if (ret == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    CSVHelper helper = new CSVHelper();
                    List<DEVICE_INFO> ls = helper.ReadCSV(openFileDialog.FileName);
                    foreach (var item in ls)
                    {
                        bool isExists = false;
                        TreeNode[] nodes = this.treeView_devicelist.Nodes[0].Nodes.Find(item.ID, false);
                        if (nodes.Count() > 0)
                        {
                            isExists = true;
                        }
                        if (!isExists)
                        {
                            if (treeView_devicelist.Nodes[0].Nodes.Count < 100)
                            {
                                TreeNode node = new TreeNode();
                                node.Tag = item;
                                node.Text = item.ID;
                                node.Name = item.ID;
                                treeView_devicelist.Nodes[0].Nodes.Add(node);
                            }
                        }
                    }
                }
                catch
                {
                    openFileDialog.Dispose();
                }
            }
            openFileDialog.Dispose();
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            Task task = new Task(() =>
            {
                if (m_RealPlayID != IntPtr.Zero)
                {
                    NETClient.StopRealPlay(m_RealPlayID);
                    m_RealPlayID = IntPtr.Zero;
                    this.BeginInvoke(new Action(() => 
                    {
                        pictureBox_play.Refresh();
                    }));
                }
                if (m_TalkID != IntPtr.Zero)
                {
                    NETClient.RecordStop(m_LoginID);
                    m_LoginID = IntPtr.Zero;
                    NETClient.StopTalk(m_TalkID);
                    m_TalkID = IntPtr.Zero;
                }
                for (int i = 0; i < treeView_devicelist.Nodes[0].Nodes.Count; i++)
                {
                    if (((DEVICE_INFO)treeView_devicelist.Nodes[0].Nodes[i].Tag).LoginID != IntPtr.Zero)
                    {
                        NETClient.Logout(((DEVICE_INFO)treeView_devicelist.Nodes[0].Nodes[i].Tag).LoginID);
                    }
                    this.Invoke(new Action(() =>
                    {
                        treeView_devicelist.Nodes[0].Nodes.Remove(treeView_devicelist.Nodes[0].Nodes[i]);
                    }));
                    i--;
                }
                this.BeginInvoke(new Action(() => 
                {
                    if (pictureBox_image.Image != null)
                    {
                        pictureBox_image.Image.Dispose();
                        pictureBox_image.Image = null;
                    }
                    pictureBox_image.Refresh();
                    this.button_capture.Enabled = false;
                    this.button_deletedevice.Enabled = false;
                    this.button_modifydevice.Enabled = false;
                    this.button_realplay.Enabled = false;
                    this.button_stoprealplay.Enabled = false;
                    this.button_talk.Enabled = false;
                    this.button_stoptalk.Enabled = false;
                    this.button_logout.Enabled = false;
                    m_CurrentNode = null;
                }));
            });
            task.Start();
        }

        private void button_logout_Click(object sender, EventArgs e)
        {
            TreeNode node = m_CurrentNode;
            DEVICE_INFO info = (DEVICE_INFO)node.Tag;
            if (info.ID == m_CurrentPlayedDeviceID)
            {
                this.button_realplay.Enabled = false;
                this.button_stoprealplay.Enabled = false;
                m_CurrentPlayedDeviceID = "";
                NETClient.StopRealPlay(m_RealPlayID);
                m_RealPlayID = IntPtr.Zero;
                if (pictureBox_play.Image != null)
                {
                    pictureBox_play.Image.Dispose();
                }
                pictureBox_play.Refresh();
            }
            if (info.ID == m_CurrentTalkDeviceID)
            {
                NETClient.RecordStop(m_LoginID);
                m_LoginID = IntPtr.Zero;
                NETClient.StopTalk(m_TalkID);
                m_TalkID = IntPtr.Zero;
            }
            NETClient.Logout(info.LoginID);
            info.LoginID = IntPtr.Zero;
            node.Text = info.ID;
            node.ForeColor = Color.Black;
            node.Nodes.Clear();
            this.button_logout.Enabled = false;
            this.button_talk.Enabled = false;
            this.button_stoptalk.Enabled = false;
            this.button_modifydevice.Enabled = true;
            this.treeView_devicelist.SelectedNode = m_CurrentNode;
            this.treeView_devicelist.Focus();
        }

    }

    public class DEVICE_INFO
    {
        public string ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public ushort Port { get; set; }
        public IntPtr LoginID { get; set; }
        public int ChannelNumber { get; set; }
    }
}
