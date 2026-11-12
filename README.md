# ApeFree.Cake2D 平面绘图框架

[![NuGet](https://img.shields.io/nuget/v/ApeFree.Cake2D.svg?style=flat-square)](https://www.nuget.org/packages/ApeFree.Cake2D)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**Cake2D** 的定位并不是 QT、WPF、WinForms 这种很重的 UI 框架，而是工作于 UI 框架与底层渲染引擎之间。使用 GDI+、Skia 等绘图引擎直接绘制复杂界面时，往往需要按流水账逐层硬编码绘制，一旦需求改动（增减图形元素，修改颜色、位置、尺寸或交互），代码就会充斥大量分支与坐标换算。**Cake2D** 正是为了解决这个问题，抽象出场景图与面向对象图元（`Element2D`），帮助开发者更优雅、高效地操作已绘制的每一个图形。

## 特性

- **图层与场景图模型**：将所有图形抽象为 `Element2D` 对象，支持层级排序（`ZIndex`）、树状父子组合（`GroupElement`）与局部/世界仿射矩阵级联变换（带脏标记缓存）。
- **丰富的图元体系**：
  - 几何图元：矩形（`RectangleElement`）、圆形（`CircleElement`）、椭圆（`EllipseElement`）、线段（`LineElement`）、折线（`PolylineElement`）、多边形（`PolygonElement`）。
  - 注记与展示：文本（`TextElement`，支持全局默认字体与多语言自适应回退）、图像（`ImageElement`）。
  - 辅助容器：自适应网格（`GridElement`）、坐标轴（`AxisElement`）、组合容器（`GroupElement`）。
- **完整交互与事件冒泡**：内置视口平移/缩放、命中测试、鼠标独占捕获、树状事件冒泡路由与视口视锥剔除（`EnableFrustumCulling`）。
- **双渲染后端与多目标框架**：
  - `ApeFree.Cake2D`：核心场景与图元抽象库（`net452`、`net472`、`netstandard2.0`、`netstandard2.1`、`net6.0`、`net8.0`、`net10.0`）。
  - `ApeFree.Cake2D.Gdi`：基于 GDI+ 的渲染实现与 WinForms 控件（`net452`、`net472`、`netstandard2.0`、`net6.0-windows`、`net8.0-windows`、`net10.0-windows`）。
  - `ApeFree.Cake2D.Skia`：基于 SkiaSharp 的跨平台高性能渲染实现与 WinForms 控件（`netstandard2.0`、`netstandard2.1`、`net6.0`、`net8.0`、`net10.0`、`net462`、`net472`、`net6.0-windows`、`net8.0-windows`、`net10.0-windows`）。

## 安装

### NuGet 安装

```bash
# 核心库
dotnet add package ApeFree.Cake2D --version 1.1.0-alpha61112

# 选择 GDI+ 渲染后端（WinForms / .NET Framework / System.Drawing）
dotnet add package ApeFree.Cake2D.Gdi --version 1.1.0-alpha61112

# 或选择 SkiaSharp 高性能跨平台渲染后端
dotnet add package ApeFree.Cake2D.Skia --version 1.1.0-alpha61112
```

### 手动安装

下载源代码并在解决方案中直接引用 `ApeFree.Cake2D` 以及对应的渲染后端项目（`ApeFree.Cake2D.Gdi` 或 `ApeFree.Cake2D.Skia`）。

## 使用

以 WinForms GDI+ 控件 `Cake2DControl` 为例（`Cake2DSkiaControl` 用法完全一致）：

```csharp
using System.Drawing;
using System.Windows.Forms;
using ApeFree.Cake2D.Elements.Billboards;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Gdi.Controls;
using ApeFree.Cake2D.Math;

var control = new Cake2DControl { Dock = DockStyle.Fill };

var rect = new RectangleElement(0, 0, 160, 80)
{
    FillColor = Color.LightSkyBlue,
    StrokeColor = Color.DodgerBlue,
    LineWidth = 2f
};

var label = new TextElement("Hello Cake2D", 80, 40)
{
    StrokeColor = Color.DarkSlateBlue,
    FontSize = 14f
};

control.Scene.Add(rect);
control.Scene.Add(label);
control.Invalidate();
```

## 贡献

如果你发现了任何问题或者有任何建议，欢迎提交 Issue 或者 Pull Request。

## 许可证

详情请参见 [LICENSE](LICENSE) 文件（Apache-2.0）。
