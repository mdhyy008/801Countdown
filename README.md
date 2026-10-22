# 801Countdown

一个功能丰富的 Windows 桌面倒计时工具，支持 Web 远程控制、PPT 联动和多种音效提示。

## 功能

- **倒计时计时** — 自定义时长，支持暂停/重置、结束前提醒
- **多种倒计时样式** — 圆角、胶囊、霓虹、渐变、描边、透明等
- **Web 远程控制** — 浏览器远程操控倒计时，支持二维码扫码连接、访问端口与密码配置、分项权限控制
- **PPT 联动** — 自动检测 PowerPoint/WPS 全屏状态，支持翻页联动、幻灯片缩略图预览、翻页笔（COM / 模拟按键）与电子讲台模式
- **音效提示** — 内置微波炉、马里奥金币等音效，结束与结束前提醒可选
- **多种显示模式** — 置顶显示、多位置布局、全屏模式、透明度与字号快捷调整
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
