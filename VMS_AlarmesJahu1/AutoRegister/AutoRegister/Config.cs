using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace AutoRegister
{
    /// <summary>
    /// 
    /// 网络协议配置
    /// </summary>
    public struct CFG_DVRIP_INFO
    {
        /// <summary>
        /// 
        /// TCP服务端口,1025~65535
        /// </summary>
        public int nTcpPort;
        /// <summary>
        /// 
        /// SSL服务端口,1025~65535
        /// </summary>
        public int nSSLPort;
        /// <summary>
        /// 
        /// UDP服务端口,1025~65535
        /// </summary>
        public int nUDPPort;
        /// <summary>
        /// 
        /// 最大连接数
        /// </summary>
        public int nMaxConnections;
        /// <summary>
        /// 
        /// 组播使能
        /// </summary>
        public bool bMCASTEnable;
        /// <summary>
        /// 
        /// 组播端口号
        /// </summary>
        public int nMCASTPort;
        /// <summary>
        /// 
        /// 组播地址
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szMCASTAddress;
        /// <summary>
        /// 
        /// 主动注册配置个数
        /// </summary>
        public int nRegistersNum;
        /// <summary>
        /// 
        /// 主动注册配置
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public CFG_REGISTER_SERVER_INFO[] stuRegisters;
        /// <summary>
        /// 
        /// 带宽不足时码流策略
        /// </summary>
        public EM_STREAM_POLICY emStreamPolicy;
        public CFG_REGISTERSERVER_VEHICLE stuRegisterServerVehicle;	// 车载专用主动注册配置
    }

    // 车载专用主动注册配置
    public struct CFG_REGISTERSERVER_VEHICLE
    {
	    public bool				bEnable;						// 主动注册使能
	    public bool				bRepeatEnable;					// 是否发送相同坐标数据
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	    public string				szDeviceID;	// 子设备ID
	    public int					nSendInterval;					// 发送间隔, 单位：秒
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	    public string				szAddress;		// IP地址或网络名
	    public int					nPort;							// 端口号
	    public EM_CFG_SENDPOLICY	emSendPolicy;					// 上传策略
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	    public string				szTestAddress;	// 测试IP地址或网络名
	    public int					nTestPort;						// 测试端口号
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
	    public byte[]				byReserved;				// 保留字节
    }


    // 上传策略
    public enum EM_CFG_SENDPOLICY
    {
	    UNKNOWN = -1,
	    TIMING,								// 定时上报
	    EVENT,								// 事件触发上报
    }

    /// <summary>
    /// 
    /// 主动注册配置
    /// </summary>
    public struct CFG_REGISTER_SERVER_INFO
    {
        /// <summary>
        /// 
        /// 主动注册使能
        /// </summary>
        public bool bEnable;
        /// <summary>
        /// 
        /// 设备ID
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szDeviceID;
        /// <summary>
        /// 
        /// 服务器个数
        /// </summary>
        public int nServersNum;
        /// <summary>
        /// 
        /// 服务器数组
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public CFG_SERVER_INFO[] stuServers;
    }

    /// <summary>
    /// 
    /// 服务器
    /// </summary>
    public struct CFG_SERVER_INFO
    {
        /// <summary>
        /// 
        /// 服务器端口号
        /// </summary>
        public int nPort;
        /// <summary>
        /// 
        /// IP地址或网络名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szAddress;
    }

    /// <summary>
    /// 
    /// 带宽不足时码流策略
    /// </summary>
    public enum EM_STREAM_POLICY
    {
        UNKNOWN,
        /// <summary>
        /// 
        /// 无策略,不开启使能"None"
        /// </summary>
        NONE = 1,
        /// <summary>
        /// 
        /// 画质优先"Quality"
        /// </summary>
        QUALITY,
        /// <summary>
        /// 
        /// 流畅度优先"Fluency"
        /// </summary>
        FLUENCY,
        /// <summary>
        /// 
        /// 自动"AutoAdapt"
        /// </summary>
        AUTOADAPT,
    }
}
