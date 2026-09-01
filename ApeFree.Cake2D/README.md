# ApeFree.Cake2D

[![NuGet](https://img.shields.io/nuget/v/ApeFree.Cake2D.svg?style=flat-square)](https://www.nuget.org/packages/ApeFree.Cake2D)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**ApeFree.Cake2D** 是一个轻量级、图层化、面向对象抽象的 2D 平面绘图与交互框架。它工作于上层 UI 框架与底层渲染引擎之间，通过抽象出统一的图形元素（`Element2D`）、图层管理与交互控制器，帮助开发者摆脱底层逐帧逐行绘制代码的泥潭，轻松构建 CAD、流程图、拓扑图、仪表盘等复杂矢量交互界面。

---

## 🌟 核心特性

- 🍰 **图层化元素模型**：所有图形都是对象（`Element2D`），支持对图形的颜色、位置、缩放、旋转、可见性及层级顺序进行动态修改与独立控制。
- 📐 **丰富的图形元素体系**：
  - 基础图元：矩形 (`RectangleElement`)、圆形 (`CircleElement`)、椭圆 (`EllipseElement`)、线条 (`LineElement`)、多段线 (`PolylineElement`)、多边形 (`PolygonElement`) 等。
  - 广告牌/注记：图片 (`ImageElement`)、富文本 (`TextElement`)。
  - 容器/标尺：网格图层 (`GridElement`)、坐标轴 (`AxisElement`)、组合元素 (`GroupElement`)。
- 🖱 **交互支持**：内置平移、缩放、框选、拖拽及鼠标事件捕获（`ElementInteractionManager`、`DefaultPanZoomState`）。
- 🔌 **渲染引擎无关**：核心库仅定义抽象图元与画布规范，可通过 `ApeFree.Cake2D.Gdi`（GDI+）或 `ApeFree.Cake2D.Skia`（SkiaSharp）实现跨平台极速渲染。

---

## 📦 安装

通过 NuGet 命令行安装：

```bash
dotnet add package ApeFree.Cake2D
```

---

## 🚀 快速上手

```csharp
using ApeFree.Cake2D.Elements;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Elements.Billboards;
using System.Drawing;

// 1. 创建图形组合
var group = new GroupElement();

// 2. 添加矩形和文字
var rect = new RectangleElement
{
    Size = new SizeF(120, 60),
    FillColor = Color.LightSkyBlue,
    StrokeColor = Color.DodgerBlue,
    StrokeWidth = 2
};

var text = new TextElement
{
    Text = "Cake2D 节点",
    FontColor = Color.DarkSlateBlue,
    Location = new PointF(10, 20)
};

group.Add(rect);
group.Add(text);

// 3. 将图形交由渲染控件或画布进行渲染（参考 GDI/Skia 扩展库）
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