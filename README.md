# 801Countdown

一个功能丰富的 Windows 桌面倒计时工具，支持 Web 远程控制、PPT 联动和多种音效提示。

## 功能

- **倒计时计时** — 默认可设置 800 秒倒计时，支持暂停/重置
- **Web 远程控制** — 通过浏览器（端口 9051）远程操控倒计时，含二维码扫码连接
- **PPT 联动** — 自动检测 PowerPoint/WPS 全屏播放状态，支持翻页联动及幻灯片信息显示
- **音效提示** — 倒计时结束时播放自定义音效
- **多种显示模式** — 置顶显示、多位置布局、全屏模式
- **自定义设置** — 可调节倒计时时长、外观样式等

## 技术栈

- .NET Framework 4.8
- Windows Forms (WinForms)
- HttpListener 内嵌 Web 服务器
- Microsoft Office Interop PowerPoint
- QRCoder 二维码生成

## 使用文档

查看详细操作指南：[801Countdown使用说明](801CountdownApp1使用说明.md)

## 官方网站

[https://mdhyy.cn/tools/page/801CountdownApp1/](https://mdhyy.cn/tools/page/801CountdownApp1/)

## 快速开始

### 系统要求

- Windows 7 及以上
- .NET Framework 4.8

### 使用

直接运行 `801CountdownApp1.exe` 即可启动。程序会自动注册 HTTP 端口（9051）以便 Web 远程控制。

> 首次使用 Web 远程控制可能需要以管理员身份运行一次，以注册 URL ACL。

### 从源码构建

```bash
git clone https://github.com/mdhyy008/801Countdown.git
cd 801Countdown
# 用 Visual Studio 2019+ 打开 801CountdownApp1.sln，还原 NuGet 包后构建
```

## 开源协议

本项目基于 [MIT License](LICENSE) 开源。
