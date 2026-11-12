# ApeFree.Cake2D.Gdi

[![NuGet](https://img.shields.io/nuget/v/ApeFree.Cake2D.Gdi.svg?style=flat-square)](https://www.nuget.org/packages/ApeFree.Cake2D.Gdi)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**ApeFree.Cake2D.Gdi** 是 `ApeFree.Cake2D` 平面绘图框架基于 **Windows GDI+** 技术的渲染实现库。它提供了基于 WinForms 的画布控件（如 `Cake2DControl`、`Cake2DBox`），让开发者能在 Windows 窗体应用中开箱即用地展示并交互 `Cake2D` 矢量图形图层。

---

## 🌟 核心特性

- 🖥 **原生 GDI+ 与跨目标支持**：提供 `GdiRenderCanvas` 资源池化（Pen/CosmeticPen/Brush/Font）高效渲染，覆盖 `net452`、`net472`、`netstandard2.0`、`net6.0-windows`、`net8.0-windows`、`net10.0-windows`（非桌面目标仅暴露 `GdiRenderCanvas`，Windows 桌面目标包含完整 WinForms 控件）。
- 📦 **开箱即用的 WinForms 控件**：
  - `Cake2DControl`：支持鼠标平移、缩放、图元点击、悬停与事件冒泡的交互画布控件。
  - `Cake2DBox`：继承自 `Cake2DControl` 的轻量级画布控件。
  - `Cake2DCanvasForm`：内置 `Cake2DControl` 的独立画布窗口基类。

---

## 📦 安装

```bash
dotnet add package ApeFree.Cake2D.Gdi --version 1.1.0-alpha61112
```

---

## 🚀 快速上手

在 WinForms 窗体中添加 `Cake2DControl` 控件并绘制图形：

```csharp
using System.Drawing;
using System.Windows.Forms;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Gdi.Controls;
using ApeFree.Cake2D.Math;

// 1. 初始化控件
var cakeControl = new Cake2DControl { Dock = DockStyle.Fill };
this.Controls.Add(cakeControl);

// 2. 创建并添加 2D 图元
var circle = new CircleElement
{
    Radius = 40,
    FillColor = Color.LightGreen,
    StrokeColor = Color.DarkGreen,
    Position = new Vector2D(100, 100)
};

cakeControl.Scene.Add(circle);

// 3. 刷新画布
cakeControl.Invalidate();
```

---

## 📄 开源许可证

本项目基于 [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0) 协议开源。