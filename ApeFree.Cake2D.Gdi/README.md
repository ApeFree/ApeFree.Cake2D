# ApeFree.Cake2D.Gdi

[![NuGet](https://img.shields.io/nuget/v/ApeFree.Cake2D.Gdi.svg?style=flat-square)](https://www.nuget.org/packages/ApeFree.Cake2D.Gdi)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**ApeFree.Cake2D.Gdi** 是 `ApeFree.Cake2D` 平面绘图框架基于 **Windows GDI+** 技术的渲染实现库。它提供了基于 WinForms 的画布控件（如 `Cake2DControl`、`Cake2DBox`），让开发者能在 Windows 窗体应用中开箱即用地展示并交互 `Cake2D` 矢量图形图层。

---

## 🌟 核心特性

- 🖥 **原生 GDI+ 硬件/软件加速**：在 Windows 平台上提供极简、低依赖的渲染通道。
- 📦 **开箱即用的 WinForms 控件**：
  - `Cake2DControl`：支持鼠标平移、缩放、图元点击与悬停事件的交互画布控件。
  - `Cake2DBox`：用于静态或图层展示的轻量化控件。
  - `Cake2DCanvasForm`：全屏或独立调试用画布窗口。

---

## 📦 安装

```bash
dotnet add package ApeFree.Cake2D.Gdi
```

---

## 🚀 快速上手

在 WinForms 窗体中添加 `Cake2DControl` 控件并绘制图形：

```csharp
using System.Drawing;
using ApeFree.Cake2D.Controls;
using ApeFree.Cake2D.Elements.Primitives;

// 1. 初始化控件
var cakeControl = new Cake2DControl { Dock = DockStyle.Fill };
this.Controls.Add(cakeControl);

// 2. 创建并添加 2D 图元
var circle = new CircleElement
{
    Radius = 40,
    FillColor = Color.LightGreen,
    StrokeColor = Color.DarkGreen,
    Location = new PointF(100, 100)
};

cakeControl.Canvas.Elements.Add(circle);

// 3. 刷新画布
cakeControl.Invalidate();
```

---

## 📄 开源许可证

本项目基于 [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0) 协议开源。