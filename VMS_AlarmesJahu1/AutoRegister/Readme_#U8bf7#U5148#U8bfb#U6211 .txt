[Introduction]
The demo program introduces listening,device configuration, adding device, modifying device, deleting device, batch import, export, cleaning device list, login, logout, auto reconnection callback, starting real time monitor, stopping real time monitor, starting voice intercom, stopping voice intercom and snapshot.

[Interfaces]
NETClient.Init Initialize SDK and set disconnection callback
NETClient.SetDVRMessCallBack Set snapshot callback
NETClient.ListenServer  Start listening service
NETClient.StopListenServer Stop listening service
NETClient.Cleanup Release SDK resources
NETClient.Login Login
NETClient.Logout Logout
NETClient.RealPlay Start real time monitor
NETClient.StopRealPlay Stop real time monitor
NETClient.SetDeviceMode  Set voice intercom mode
NETClient.SetDeviceMode  Start voice intercom
NETClient.RecordStart Start PC sound record
NETClient.RecordStart Stop PC sound record
NETClient.StopTalk Stop voice intercom
NETClient.SnapPictureEx Snapshot
NETClient.GetNewDevConfig Get device configuration
NETClient.GetNewDevConfig Set device configuration
NETClient.Cleanup Release SDK resources

[Notice]
When the compiling environment is VS2010, NETSDKCS library can support the version of .NET Framework 4.0 or newer. If you want to use the version older than .NET Framework 4.0, change the method of using NetSDK.cs in IntPtr.Add. We will not support the modification.
Copy all file in the libs directory of General_NetSDK_ChnEng_CSharpXX_IS_VX.XXX.XXXXXXXX.X.R.XXXXXX to the build directory of bin directory of the corresponding demo programs.

【演示程序功能】
1、演示程序介绍了主动注册监听、设备主动注册的配置、增加设备、修改设备、删除设备、设备批量导入、导出、设备列表清空、设备注册后登录设备、设备登出、设备断线自动重连、打开实时监视、关闭实时监视、打开对讲、停止对讲、抓图。

【接口列表】
NETClient.Init SDK初始化与设置断线回调
NETClient.SetSnapRevCallBack 设置抓图回调
NETClient.ListenServer 主动注册服务开始监听
NETClient.StopListenServer 主动注册服务停止监听
NETClient.Cleanup 释放SDK资源
NETClient.Login 设备登录
NETClient.Logout 设备登出
NETClient.RealPlay 实时监视
NETClient.StopRealPlay 停止实时监视
NETClient.SetDeviceMode 设置对讲模式
NETClient.StartTalk 开始对讲
NETClient.RecordStart 开始录音
NETClient.RecordStop 停止录音
NETClient.StopTalk 停止对讲
NETClient.SnapPictureEx 抓图
NETClient.GetNewDevConfig 获取主动注册配置信息
NETClient.GetNewDevConfig 设置主动注册配置信息
NETClient.Cleanup 释放SDK资源

【注意事项】
1、编译环境为VS2010，NETSDKCS库最低只支持.NET Framework 4.0,如用户需要支持低于4.0的版本需要更改NetSDK.cs文件中使用到IntPtr.Add的方法,我们不提供修改。
2、请把General_NetSDK_ChnEng_CSharpXX_IS_VX.XXX.XXXXXXXX.X.R.XXXXXX中libs目录中的所有文件复制到对应该演示程序中bin目录下的生成目录中。
