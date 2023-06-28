# 高铁控制中心上位机软件Demo

## 项目介绍

该软件是之前参加通信与控制系统的比赛所需要用到的软件，于是学习制作了一个。因为我没有系统学习过C#，可能代码不是很规范，仅做参考。

该工程使用Visual Studio 2019开发。



## 硬件环境介绍

1. PC与PLC（西门子 S7-200）通过485串口相连。

2. PC与列控中心（Android系统设备）通过局域网相连，TCP协议通信。



## 功能介绍

1. 输入串口号、波特率等信息，确保与PLC程序的配置一致，点击“打开串口”成功连接PLC。

2. 输入上位机的主机IP（不要用localhost和127.0.0.1），成功连接列控中心。

3. PLC发送车内数据给上位机，如温度、湿度、光照等，上位机显示数据。

4. 列控中心发送数据给上位机，如车速、列车区间、区间情况等信息，上位机显示。

5. 【功能没做】列车到达站点，点击“进站”，需完成一次开关门（PLC判断操作完成后发信号给上位机）才能点击“出站”。



## 界面展示

![gt1](https://github.com/huanfenz/ControlCenter/assets/49386166/60f02841-c2e8-42ea-bb61-de3ea9bc8a0e)

![gt2](https://github.com/huanfenz/ControlCenter/assets/49386166/b5a710c6-7951-45cd-b8b7-5f3ce56e5453)

![gt3](https://github.com/huanfenz/ControlCenter/assets/49386166/43ed39b6-10f3-4e73-b189-8acebb35fd1f)



