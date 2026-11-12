# ApeFree.Cake2D.Skia

[![NuGet](https://img.shields.io/nuget/v/ApeFree.Cake2D.Skia.svg?style=flat-square)](https://www.nuget.org/packages/ApeFree.Cake2D.Skia)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**ApeFree.Cake2D.Skia** 是 `ApeFree.Cake2D` 平面绘图框架基于 **Google Skia (SkiaSharp)** 高性能 2D 矢量渲染引擎的实现库。具备更强的图形抗锯齿渲染品质、GPU 硬件加速与跨平台能力。

---

## 🌟 核心特性

- ⚡ **高性能 SkiaSharp 渲染**：支持亚像素抗锯齿、仿射变换栈下推、虚线原生句柄安全释放，以及阶梯式全球化多语言字体回退与缓存池（避免中文等多语种方块字形）。
- 🎛 **WinForms 适配与跨平台支持**：在 `net462`、`net472`、`net6.0-windows`、`net8.0-windows`、`net10.0-windows` 下提供 `Cake2DSkiaControl` 与 `Cake2DSkiaBox`；同时在 `netstandard2.0`、`netstandard2.1`、`net6.0`、`net8.0`、`net10.0` 下提供纯跨平台 `SkiaRenderCanvas`。
- 🌐 **高帧率与流畅缩放**：面对复杂大型矢量图元时仍能保持极高的缩放与漫游帧率。

---

## 📦 安装

```bash
dotnet add package ApeFree.Cake2D.Skia --version 1.1.0-alpha61112
```

---

## 🚀 快速上手

```csharp
using System.Drawing;
using System.Windows.Forms;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Skia.Controls;

// 1. 初始化 Skia 绘图控件
var skiaControl = new Cake2DSkiaControl { Dock = DockStyle.Fill };
this.Controls.Add(skiaControl);

// 2. 添加多边形
var polygon = new PolygonElement(new[]
{
    new PointF(100, 50),
    new PointF(150, 150),
    new PointF(50, 150)
})
{
    FillColor = Color.FromArgb(180, Color.Coral),
    StrokeColor = Color.DarkRed,
    LineWidth = 2f
};

skiaControl.Scene.Add(polygon);
skiaControl.Invalidate();
```

---

## 📄 开源许可证

本项目基于 [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0) 协议开源。