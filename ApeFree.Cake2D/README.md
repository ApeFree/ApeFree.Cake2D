# ApeFree.Cake2D

[![NuGet](https://img.shields.io/nuget/v/ApeFree.Cake2D.svg?style=flat-square)](https://www.nuget.org/packages/ApeFree.Cake2D)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**ApeFree.Cake2D** 是一个轻量级、图层化、面向对象抽象的 2D 平面绘图与交互框架。它工作于上层 UI 框架与底层渲染引擎之间，通过抽象出统一的图形元素（`Element2D`）、图层管理与交互控制器，帮助开发者摆脱底层逐帧逐行绘制代码的泥潭，轻松构建 CAD、流程图、拓扑图、仪表盘等复杂矢量交互界面。

---

## 🌟 核心特性

- 🍰 **图层化元素模型**：所有图形都是对象（`Element2D`），支持对颜色、位置、缩放、旋转、可见性及 `ZIndex` 层级顺序进行动态修改，内置本地/世界变换矩阵脏标记缓存与视口视锥剔除（`EnableFrustumCulling`）。
- 📐 **丰富的图形元素体系**：
  - 基础图元：矩形 (`RectangleElement`)、圆形 (`CircleElement`)、椭圆 (`EllipseElement`)、线条 (`LineElement`)、多段线 (`PolylineElement`)、多边形 (`PolygonElement`) 等。
  - 广告牌/注记：图片 (`ImageElement`)、文本 (`TextElement`，支持全局默认字体 `DefaultFontName` 与 CAD 模式下正向阅读渲染)。
  - 容器/标尺：网格图层 (`GridElement`)、坐标轴 (`AxisElement`)、组合元素 (`GroupElement`，支持中空穿透拾取)。
- 🖱 **交互支持**：内置平移、缩放、焦点与鼠标捕获管理、树状事件冒泡路由及碰撞命中测试（`ElementInteractionManager`、`DefaultPanZoomState`）。
- 🔌 **渲染引擎无关与多目标支持**：核心库覆盖 `net452`、`net472`、`netstandard2.0`、`netstandard2.1`、`net6.0`、`net8.0`、`net10.0`，可通过 `ApeFree.Cake2D.Gdi` 或 `ApeFree.Cake2D.Skia` 实现多平台渲染。

---

## 📦 安装

通过 NuGet 命令行安装：

```bash
dotnet add package ApeFree.Cake2D --version 1.1.0-alpha61112
```

---

## 🚀 快速上手

```csharp
using System.Drawing;
using ApeFree.Cake2D.Elements.Billboards;
using ApeFree.Cake2D.Elements.Containers;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Math;

// 1. 创建图形组合
var group = new GroupElement();

// 2. 添加矩形和文字
var rect = new RectangleElement
{
    Width = 120,
    Height = 60,
    FillColor = Color.LightSkyBlue,
    StrokeColor = Color.DodgerBlue,
    LineWidth = 2f
};

var text = new TextElement
{
    Text = "Cake2D 节点",
    StrokeColor = Color.DarkSlateBlue,
    Position = new Vector2D(10, 20)
};

group.AddChild(rect);
group.AddChild(text);

// 3. 将图形加入 Scene2D 并交由 GDI/Skia 渲染控件或画布进行渲染
```

---

## 🧩 渲染后端扩展

| 扩展包 | 描述 |
| :--- | :--- |
| **`ApeFree.Cake2D.Gdi`** | 基于 Windows GDI+ 引擎的渲染控件与画布实现（适用于传统 WinForms） |
| **`ApeFree.Cake2D.Skia`** | 基于 Google SkiaSharp 高性能跨平台图形引擎的渲染控件与画布实现 |

---

## 📄 开源许可证

本项目基于 [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0) 协议开源。