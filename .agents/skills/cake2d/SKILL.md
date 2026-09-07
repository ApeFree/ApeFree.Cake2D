---
name: cake2d
description: ApeFree.Cake2D 二维图层化矢量绘图与交互框架使用指南。涵盖基于 GDI+ 的 WinForms 控件（Cake2DControl）、场景图与视口坐标系转换、全套图元（Rectangle、Circle、Polygon、Line、Text 等）、交互事件与动态重绘开发规范及完整 API 字典。
---

# ApeFree.Cake2D 绘图框架使用指南

`ApeFree.Cake2D` 是一个轻量级、图层化、面向对象抽象的 2D 矢量平面绘图与交互框架。它工作于上层 UI 框架与底层图形系统之间，通过**场景图（Scene Graph）**与**面向对象图元（Element2D）**彻底摆脱传统 GDI+ 逐像素硬编码绘制的痛点。

默认使用 **GDI+** 作为渲染提供程序（配合 `ApeFree.Cake2D.Gdi` 扩展包提供的 `Cake2DControl` 控件）。

---

## 1. 核心架构与心智模型

### 1.1 场景图模式 (Scene Graph)
在原生 GDI+ 中，在 `OnPaint` 中每次必须从头到尾用 `Graphics.DrawLine`、`Graphics.DrawRectangle` 重写所有绘制，一旦需要对某个图形进行点击、选中、拖拽、修改颜色或平移，代码中将充斥复杂的数学换算与分支逻辑。

Cake2D 将每一个图形抽象为一个独立对象（`Element2D`）：
- 每个图元自包含**位置（Position）**、**旋转（Rotation）**、**缩放（Scale）**、**局部锚点（Origin）**、**外观样式（FillColor, StrokeColor, LineWidth, IsDashed）**与**层级（ZIndex）**。
- 图元支持树状父子层级嵌套（`GroupElement` 与 `child.Parent`），变换矩阵自顶向下自动级联传递。
- 用户只需将图元加入场景 `scene.Add(element)`，框架会自动完成裁剪、排序、视口投影、碰撞检测与图元鼠标事件捕获。

```
+-------------------------------------------------------------+
| Cake2DControl (WinForms UserControl 视口宿主)                |
|   +-------------------------------------------------------+ |
|   | Scene2D (场景容器，持有背景色、抗锯齿、图元列表)        | |
|   |   +-- Viewport2D (视口摄像机: 缩放、平移、坐标投影)     | |
|   |   +-- ElementInteractionManager (鼠标悬停/点击事件分发) | |
|   |   +-- Elements (图元集合，按 ZIndex 升序分层渲染)       | |
|   |         |-- GridElement (ZIndex: -1000)               | |
|   |         |-- AxisElement (ZIndex: -900)                | |
|   |         |-- RectangleElement / CircleElement          | |
|   |         |-- PolygonElement / LineElement              | |
|   |         |-- TextElement / ImageElement                | |
|   |         +-- GroupElement (组合容器)                   | |
|   +-------------------------------------------------------+ |
+-------------------------------------------------------------+
```

### 1.2 两个坐标系：世界坐标 vs 屏幕坐标
Cake2D 内部严格区分两个坐标系：
1. **世界坐标系（World Space）**：
   - 业务逻辑、图元尺寸和位置**全部且只能使用世界坐标**。
   - 坐标值通常为连续浮点数（`float`），不受控件尺寸、窗体缩放或视口移动影响。
   - 视口支持两种 Y 轴方向：
     - `Viewport.YAxisUpwards = false`（**默认**）：屏幕方向，Y 轴向下增长，适合常规 UI、流程图、拓扑图、文档设计。
     - `Viewport.YAxisUpwards = true`：数学/CAD 方向，Y 轴向上增长，适合工程 CAD、地图 GIS、函数图象。
2. **屏幕像素坐标系（Screen Space）**：
   - 控件本身的像素坐标（整数点，左上角为 `(0, 0)`）。
   - 视口对象 `scene.Viewport` 负责两者间的双向无缝转换：
     - `PointF worldPt = viewport.ScreenToWorld(screenPt);`
     - `PointF screenPt = viewport.WorldToScreen(worldPt);`

---

## 2. 快速起步与环境集成

### 2.1 引入 NuGet 依赖
在 `.csproj` 中添加核心包与 GDI+ 适配器包：
```xml
<ItemGroup>
  <PackageReference Include="ApeFree.Cake2D" Version="1.0.2.0-beta260905" />
  <PackageReference Include="ApeFree.Cake2D.Gdi" Version="1.0.2.0-beta260905" />
</ItemGroup>
```

### 2.2 最简初始化代码
在 WinForms 窗体中拖入或代码创建 `Cake2DControl`：
```csharp
using System;
using System.Drawing;
using System.Windows.Forms;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Elements.Billboards;
using ApeFree.Cake2D.Gdi.Controls;

public class MyDrawingForm : Form
{
    private Cake2DControl _cakeControl;

    public MyDrawingForm()
    {
        Size = new Size(1000, 700);
        Text = "Cake2D 快速上手";

        // 1. 创建 Cake2D 视口控件
        _cakeControl = new Cake2DControl
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        Controls.Add(_cakeControl);

        // 2. 获取场景引用
        var scene = _cakeControl.Scene;

        // 3. 创建并添加矩形图元
        var rect = new RectangleElement(x: -100, y: -50, width: 200, height: 100)
        {
            Name = "卡片矩形",
            FillColor = Color.FromArgb(220, 59, 130, 246),
            StrokeColor = Color.FromArgb(29, 78, 216),
            LineWidth = 2f,
            ZIndex = 1
        };
        scene.Add(rect);

        // 4. 创建并添加文字图元
        var label = new TextElement("Hello Cake2D!", x: 0, y: 0)
        {
            FontSize = 14f,
            IsBold = true,
            StrokeColor = Color.White,
            Alignment = TextAlignment.MiddleCenter,
            ZIndex = 2
        };
        scene.Add(label);

        // 5. 将视口中心设置到原点，视野宽度 600
        scene.Viewport.SetView(PointF.Empty, 600f);
    }
}
```

---

## 3. 视口摄像机控制 (Viewport2D)

`scene.Viewport` 负责无限画布的视野投影与漫游。

### 3.1 默认手势
`Cake2DControl` 默认启用了 `DefaultPanZoomState` 交互状态机：
- **平移（Pan）**：按住**鼠标左键**或**鼠标中键**拖动画布。
- **以鼠标指针为中心缩放（Zoom）**：滚动**鼠标滚轮**，光标所指的世界坐标点在缩放过程中位置保持固定。

### 3.2 常用代码控制 API
```csharp
var vp = scene.Viewport;

// 1. 设置视野中心点与可视宽度（自动等比适配高度）
vp.SetView(new PointF(0, 0), width: 800f);

// 2. 全图自适应（将指定的包围盒以适宜比例居中完整呈现）
RectangleF contentBounds = new RectangleF(-300, -200, 600, 400);
vp.ZoomToFit(contentBounds, marginRatio: 0.1f);

// 3. 切换数学 Y 轴向上 / 屏幕 Y 轴向下
vp.YAxisUpwards = false; // false=屏幕向下; true=数学向上

// 4. 坐标转换
Point screenMouse = control.PointToClient(Cursor.Position);
PointF worldMouse = vp.ScreenToWorld(screenMouse);
```

---

## 4. 图元模型与交互机制

### 4.1 图元属性规范
所有图元均派生自抽象基类 `Element2D`：

| 属性名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `Name` | `string` | 图元唯一或描述性名称 |
| `Tag` | `object` | 绑定的自定义业务数据对象 |
| `Position` | `Vector2D` | 本地空间坐标（相对于父容器，根图元对应世界坐标） |
| `Rotation` | `float` | 旋转角度（度数，顺时针） |
| `Scale` | `Vector2D` | 缩放系数，默认 `Vector2D.One` (`1, 1`) |
| `Origin` | `Vector2D` | 旋转和缩放的自身锚点（默认为自身原点 `(0, 0)`） |
| `ZIndex` | `int` | 渲染层级，值越小越先绘制（处于底层），值越大越在顶层 |
| `IsVisible` | `bool` | 是否可见，为 `false` 时跳过渲染与交互 |
| `IsInteractive` | `bool` | 是否响应鼠标交互事件，默认为 `true` |
| `IsHovered` | `bool` | 只读，表示当前鼠标是否悬停在图元上方 |
| `StrokeColor` | `Color?` | 描边边框颜色，为 `null` 时不绘制边框 |
| `FillColor` | `Color?` | 内部填充颜色，为 `null` 时透明不填充 |
| `LineWidth` | `float` | 描边线宽（像素），为 `0` 时采用场景默认线宽 |
| `IsDashed` | `bool` | 是否使用虚线描边 |

### 4.2 交互事件与高亮响应
`Element2D` 原生内置精准的几何命中测试与鼠标事件。命中基于图元自身的数学轮廓（包含矩阵变换与旋转），而非粗糙的矩形外框。

```csharp
var circle = new CircleElement(0, 0, 60)
{
    Name = "交互圆形",
    FillColor = Color.LightSkyBlue,
    StrokeColor = Color.DodgerBlue,
    LineWidth = 2f
};

// 鼠标移入：变色高亮
circle.MouseEnter += (sender, e) =>
{
    circle.FillColor = Color.Orange;
    circle.StrokeColor = Color.DarkOrange;
    _cakeControl.Invalidate(); // 请求重绘
};

// 鼠标移出：还原颜色
circle.MouseLeave += (sender, e) =>
{
    circle.FillColor = Color.LightSkyBlue;
    circle.StrokeColor = Color.DodgerBlue;
    _cakeControl.Invalidate();
};

// 鼠标单击
circle.Click += (sender, e) =>
{
    MessageBox.Show($"点击了图元 [{circle.Name}]，点击位置世界坐标: ({e.WorldLocation.X:F1}, {e.WorldLocation.Y:F1})");
};

scene.Add(circle);
```

> [!IMPORTANT]
> **动态刷新铁律**：
> 当在定时器、后台线程或事件回调中修改了图元的属性（如坐标、颜色、多边形端点、旋转角度等）后，**必须显式调用控件的 `cakeControl.Invalidate()`** 来请求重绘。图元本身是纯内存数据模型，不会自动触发 UI 无效化。

---

## 5. 典型实战范例

### 5.1 实战案例 A：CAD 风格绘图与图层组合
本例展示：网格图层、坐标轴、带旋转的矩形、多边形、文本标签以及图层组合（`GroupElement`）。

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;
using ApeFree.Cake2D.Elements.Billboards;
using ApeFree.Cake2D.Elements.Containers;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Gdi.Controls;
using ApeFree.Cake2D.Math;

public class CadDemoPanel : Panel
{
    private Cake2DControl _control;

    public CadDemoPanel()
    {
        Dock = DockStyle.Fill;
        _control = new Cake2DControl { Dock = DockStyle.Fill };
        Controls.Add(_control);

        var scene = _control.Scene;
        scene.BackgroundColor = Color.FromArgb(248, 250, 252);

        // 1. 底层网格与坐标轴
        scene.Add(new GridElement { GridSpacing = 50f, AutoScale = true });
        scene.Add(new AxisElement());

        // 2. 旋转的矩形工件
        var workpiece = new RectangleElement(-150, -80, 300, 160)
        {
            Name = "机械构件",
            Rotation = 15f,
            Origin = new Vector2D(150, 80), // 以自身中心为旋转点
            FillColor = Color.FromArgb(180, 203, 213, 225),
            StrokeColor = Color.FromArgb(71, 85, 105),
            LineWidth = 2f,
            ZIndex = 5
        };
        scene.Add(workpiece);

        // 3. 任意闭合多边形
        var poly = new PolygonElement(new[]
        {
            new PointF(100, 50),
            new PointF(220, 80),
            new PointF(260, 180),
            new PointF(140, 220),
            new PointF(80, 140)
        })
        {
            Name = "检测区域多边形",
            FillColor = Color.FromArgb(140, 34, 197, 94),
            StrokeColor = Color.FromArgb(21, 128, 61),
            LineWidth = 2f,
            ZIndex = 6
        };
        scene.Add(poly);

        // 4. 图层组合（GroupElement）
        var sensorGroup = new GroupElement { Position = new Vector2D(-200, 120), ZIndex = 10 };
        sensorGroup.AddChild(new CircleElement(0, 0, 15) { FillColor = Color.Red, StrokeColor = Color.DarkRed });
        sensorGroup.AddChild(new TextElement("Sensor-01", 20, -5) { FontSize = 10f, StrokeColor = Color.Black });
        scene.Add(sensorGroup);

        // 5. 居中视图
        scene.Viewport.SetView(PointF.Empty, 900f);
    }
}
```

### 5.2 实战案例 B：动态数据图表 / 动画更新（时钟与仪表盘）
本例展示：利用 `Timer` 高频更新图元的几何端点，通过修改已有图元属性而非重新实例化对象，实现极致流畅的 60FPS 动态渲染。

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ApeFree.Cake2D.Elements.Billboards;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Gdi.Controls;
using ApeFree.Cake2D.Math;

public class ClockDemoControl : UserControl
{
    private Cake2DControl _control;
    private Timer _timer;
    private LineElement _hourHand;
    private LineElement _minuteHand;
    private LineElement _secondHand;
    private PolygonElement _indicatorPoly;
    private TextElement _digitalText;

    public ClockDemoControl()
    {
        DoubleBuffered = true;
        _control = new Cake2DControl { Dock = DockStyle.Fill };
        Controls.Add(_control);

        BuildClockScene();

        _timer = new Timer { Interval = 100 };
        _timer.Tick += (s, e) => UpdateClock();
        _timer.Start();
    }

    private void BuildClockScene()
    {
        var scene = _control.Scene;
        scene.Viewport.SetView(PointF.Empty, 400f);

        float r = 140f;

        // 1. 表盘与刻度
        scene.Add(new CircleElement(0, 0, r)
        {
            FillColor = Color.White,
            StrokeColor = Color.FromArgb(30, 41, 59),
            LineWidth = 3f,
            ZIndex = 1
        });

        for (int i = 0; i < 60; i++)
        {
            float angle = i * 6f - 90f;
            bool isMajor = i % 5 == 0;
            float rIn = isMajor ? r - 15f : r - 8f;
            scene.Add(new LineElement(
                new Vector2D(Math2D.CalculatePointOnCircle(PointF.Empty, rIn, angle)),
                new Vector2D(Math2D.CalculatePointOnCircle(PointF.Empty, r - 3f, angle)))
            {
                StrokeColor = isMajor ? Color.Black : Color.LightGray,
                LineWidth = isMajor ? 2.5f : 1f,
                IsInteractive = false,
                ZIndex = 2
            });
        }

        // 2. 指针关联动态多边形
        _indicatorPoly = new PolygonElement(new PointF[3])
        {
            FillColor = Color.FromArgb(50, 59, 130, 246),
            StrokeColor = Color.FromArgb(120, 59, 130, 246),
            LineWidth = 1f,
            ZIndex = 3
        };
        scene.Add(_indicatorPoly);

        // 3. 时、分、秒指针
        _hourHand = new LineElement(Vector2D.Zero, new Vector2D(0, -60))
        {
            StrokeColor = Color.FromArgb(30, 41, 59),
            LineWidth = 5f,
            ZIndex = 4
        };
        _minuteHand = new LineElement(Vector2D.Zero, new Vector2D(0, -90))
        {
            StrokeColor = Color.FromArgb(22, 163, 74),
            LineWidth = 3.5f,
            ZIndex = 5
        };
        _secondHand = new LineElement(Vector2D.Zero, new Vector2D(0, -115))
        {
            StrokeColor = Color.FromArgb(220, 38, 38),
            LineWidth = 1.5f,
            ZIndex = 6
        };
        scene.Add(_hourHand);
        scene.Add(_minuteHand);
        scene.Add(_secondHand);

        // 4. 中心圆点与数字时间
        scene.Add(new CircleElement(0, 0, 5) { FillColor = Color.Red, ZIndex = 7 });
        _digitalText = new TextElement("", 0, 45)
        {
            FontSize = 10f,
            StrokeColor = Color.Gray,
            Alignment = TextAlignment.MiddleCenter,
            ZIndex = 8
        };
        scene.Add(_digitalText);
    }

    private void UpdateClock()
    {
        var now = DateTime.Now;
        float totalSec = now.Hour * 3600f + now.Minute * 60f + now.Second + now.Millisecond / 1000f;

        float sAngle = (totalSec % 60f) / 60f * 360f - 90f;
        float mAngle = (totalSec % 3600f) / 3600f * 360f - 90f;
        float hAngle = (totalSec % 43200f) / 43200f * 360f - 90f;

        PointF pSec = Math2D.CalculatePointOnCircle(PointF.Empty, 115f, sAngle);
        PointF pMin = Math2D.CalculatePointOnCircle(PointF.Empty, 90f, mAngle);
        PointF pHour = Math2D.CalculatePointOnCircle(PointF.Empty, 60f, hAngle);

        _secondHand.End = new Vector2D(pSec);
        _minuteHand.End = new Vector2D(pMin);
        _hourHand.End = new Vector2D(pHour);

        // 更新多边形顶点
        _indicatorPoly.Points = new List<PointF> { pSec, pMin, pHour };
        _digitalText.Text = now.ToString("HH:mm:ss");

        // 刷新渲染
        _control.Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer?.Dispose();
        base.Dispose(disposing);
    }
}
```

---

## 6. 完整 API 规格速查字典

### 6.1 核心命名空间一览
- `ApeFree.Cake2D.Scene`：场景、视口、重绘调度
- `ApeFree.Cake2D.Elements`：图元抽象基类 `Element2D`
- `ApeFree.Cake2D.Elements.Primitives`：几何图元（矩形、正圆、椭圆、线段、多边形、折线）
- `ApeFree.Cake2D.Elements.Billboards`：注记与展示图元（文本、图片）
- `ApeFree.Cake2D.Elements.Containers`：图元容器（组合组、背景自适应网格、坐标轴）
- `ApeFree.Cake2D.Math`：二维数学算法与数据结构（`Vector2D`, `Matrix3x2`, `Math2D`）
- `ApeFree.Cake2D.Events`：图元交互鼠标事件参数（`ElementMouseEventArgs`）
- `ApeFree.Cake2D.Gdi.Controls`：基于 WinForms 的 GDI+ 视口控件（`Cake2DControl`, `Cake2DBox`）

---

### 6.2 `ApeFree.Cake2D.Elements.Element2D` (抽象基类)

```csharp
public abstract class Element2D
{
    // 标识与交互
    public string Name { get; set; }
    public object Tag { get; set; }
    public bool IsVisible { get; set; }           // 默认: true
    public bool IsInteractive { get; set; }       // 默认: true
    public bool IsHovered { get; }                // 是否处于悬停中
    public int ZIndex { get; set; }               // 排序层级，默认: 0

    // 空间变换
    public Vector2D Position { get; set; }        // 局部原点位置
    public float Rotation { get; set; }           // 旋转角（度，顺时针）
    public Vector2D Scale { get; set; }           // 缩放，默认: (1, 1)
    public Vector2D Origin { get; set; }          // 自身旋转缩放锚点，默认: (0, 0)

    // 外观样式
    public Color? StrokeColor { get; set; }       // 描边色（null 为不描边）
    public Color? FillColor { get; set; }         // 填充色（null 为不填充）
    public float LineWidth { get; set; }          // 描边线宽（<=0 采用全局场景默认）
    public bool IsDashed { get; set; }            // 是否为虚线

    // 树状层次结构
    public Element2D Parent { get; }
    public List<Element2D> Children { get; }
    public void AddChild(Element2D child);
    public void RemoveChild(Element2D child);

    // 矩阵变换与包围盒
    public virtual Matrix3x2 GetLocalMatrix();
    public virtual Matrix3x2 GetWorldMatrix();
    public PointF LocalToWorld(PointF localPt);
    public PointF WorldToLocal(PointF worldPt);
    public abstract RectangleF GetLocalBounds();
    public virtual RectangleF GetWorldBounds();
    public virtual bool HitTest(PointF worldPoint);

    // 鼠标事件
    public event EventHandler<ElementMouseEventArgs> MouseEnter;
    public event EventHandler<ElementMouseEventArgs> MouseLeave;
    public event EventHandler<ElementMouseEventArgs> MouseDown;
    public event EventHandler<ElementMouseEventArgs> MouseMove;
    public event EventHandler<ElementMouseEventArgs> MouseUp;
    public event EventHandler<ElementMouseEventArgs> Click;
    public event EventHandler<ElementMouseEventArgs> DoubleClick;
}
```

---

### 6.3 具体图元列表 (`ApeFree.Cake2D.Elements.*`)

#### 1. `RectangleElement : Element2D`
- **构造函数**：
  - `RectangleElement()`
  - `RectangleElement(float width, float height)`
  - `RectangleElement(float x, float y, float width, float height)`
- **特有属性**：
  - `float Width { get; set; }`
  - `float Height { get; set; }`

#### 2. `CircleElement : Element2D`
- **构造函数**：
  - `CircleElement()`
  - `CircleElement(float radius)`
  - `CircleElement(float centerX, float centerY, float radius)`
- **特有属性**：
  - `float Radius { get; set; }` (以 Position 为圆心)

#### 3. `EllipseElement : Element2D`
- **构造函数**：
  - `EllipseElement()`
  - `EllipseElement(float radiusX, float radiusY)`
  - `EllipseElement(float centerX, float centerY, float radiusX, float radiusY)`
- **特有属性**：
  - `float RadiusX { get; set; }`
  - `float RadiusY { get; set; }`

#### 4. `LineElement : Element2D`
- **构造函数**：
  - `LineElement()`
  - `LineElement(Vector2D start, Vector2D end)`
  - `LineElement(float x1, float y1, float x2, float y2)`
- **特有属性**：
  - `Vector2D Start { get; set; }`
  - `Vector2D End { get; set; }`

#### 5. `PolylineElement : Element2D` (连续折线)
- **构造函数**：
  - `PolylineElement()`
  - `PolylineElement(IEnumerable<PointF> points)`
- **特有属性**：
  - `List<PointF> Points { get; set; }`

#### 6. `PolygonElement : Element2D` (闭合多边形)
- **构造函数**：
  - `PolygonElement()`
  - `PolygonElement(IEnumerable<PointF> points)`
- **特有属性/只读**：
  - `List<PointF> Points { get; set; }`
  - `float Area { get; }` (面积)
  - `float Perimeter { get; }` (周长)
  - `PointF Centroid { get; }` (几何质心)

#### 7. `TextElement : Element2D` (文本注记)
- **构造函数**：
  - `TextElement()`
  - `TextElement(string text, float x = 0, float y = 0)`
- **特有属性**：
  - `string Text { get; set; }`
  - `string FontName { get; set; }` (默认 `"Arial"`)
  - `float FontSize { get; set; }` (默认 `12f`)
  - `bool IsBold { get; set; }`
  - `TextAlignment Alignment { get; set; }` (支持 `TopLeft`, `TopCenter`, `TopRight`, `MiddleLeft`, `MiddleCenter`, `MiddleRight`, `BottomLeft`, `BottomCenter`, `BottomRight`)

#### 8. `ImageElement : Element2D` (图片纹理)
- **构造函数**：
  - `ImageElement()`
  - `ImageElement(object image, float width, float height)`
- **特有属性**：
  - `object Image { get; set; }` (WinForms GDI+ 下传入 `System.Drawing.Image` 或 `Bitmap`)
  - `float Width { get; set; }`
  - `float Height { get; set; }`

#### 9. `GroupElement : Element2D` (图元编组/图层)
- 组合多个子图元，对 GroupElement 的 `Position`、`Rotation`、`Scale` 进行设置会同步对内部所有 Children 生效。

#### 10. `GridElement : Element2D` (背景网格图层)
- 自动根据当前视口漫游视野绘制网格，推荐 `ZIndex = -1000`。
- `float GridSpacing { get; set; }` (世界间距)
- `bool AutoScale { get; set; }` (视口缩放时是否自动细分/合并网格线密度)

#### 11. `AxisElement : Element2D` (坐标系十字轴图层)
- 自动在视口中绘制穿过 `(0, 0)` 原点的 X 轴与 Y 轴及原点小圆点，推荐 `ZIndex = -900`。
- `Color XAxisColor { get; set; }`
- `Color YAxisColor { get; set; }`

---

### 6.4 `ApeFree.Cake2D.Scene.Scene2D` 与 `Viewport2D`

#### `Scene2D`
```csharp
public class Scene2D
{
    public Viewport2D Viewport { get; set; }
    public Color BackgroundColor { get; set; }         // 场景背景色，默认 White
    public float DefaultLineWidth { get; set; }        // 场景默认线宽，默认 1.5f
    public bool AntiAlias { get; set; }                // 是否开启高品质抗锯齿，默认 true
    public List<Element2D> Elements { get; }           // 顶层图元列表
    public ViewportInteractionState CurrentState { get; set; } // 当前手势状态机
    public ElementInteractionManager InteractionManager { get; }
    
    public void Add(Element2D element);
    public void Remove(Element2D element);
    public void Clear();
    public void RequestInvalidate();                   // 触发重绘请求事件
    public event EventHandler InvalidateRequested;
}
```

#### `Viewport2D`
```csharp
public class Viewport2D
{
    public int Width { get; set; }                     // 视口像素宽
    public int Height { get; set; }                    // 视口像素高
    public PointF ViewCenter { get; set; }             // 视口中心的世界坐标
    public float ViewWidth { get; set; }               // 视口可视的世界宽度
    public float ViewHeight { get; }                   // 视口可视的世界高度（由长宽比自动计算）
    public bool YAxisUpwards { get; set; }             // false: 屏幕向下; true: 数学向上
    public float PixelsPerUnit { get; }                // 比例尺 (Pixels/Unit)
    public float UnitPerPixel { get; }                 // 比例尺 (Units/Pixel)
    
    public void Resize(int width, int height);
    public void SetView(PointF center, float width);
    public void Pan(float deltaScreenX, float deltaScreenY);
    public void ZoomAroundScreenPoint(PointF screenPoint, float factor);
    public void ZoomToFit(RectangleF worldBounds, float marginRatio = 0.1f);
    
    public PointF ScreenToWorld(PointF screenPt);
    public PointF WorldToScreen(PointF worldPt);
    public RectangleF GetCurrentWorldViewRect();       // 当前视野覆盖的世界矩形 (AABB)
    public event EventHandler ViewChanged;
}
```

---

### 6.5 `ApeFree.Cake2D.Gdi.Controls.Cake2DControl`

```csharp
public class Cake2DControl : System.Windows.Forms.UserControl
{
    // 绑定的 2D 场景，内部已自动配置双缓冲与鼠标交互拦截
    public Scene2D Scene { get; set; }
    
    // 显式请求控件重绘
    public void Invalidate();
}
```

---

### 6.6 辅助数学计算类 (`ApeFree.Cake2D.Math.Math2D`)

- `float Math2D.ToRadians(float degrees)`：角度转弧度
- `float Math2D.ToDegrees(float radians)`：弧度转角度
- `PointF Math2D.CalculatePointOnCircle(PointF center, float radius, float angleInDegrees)`：计算圆周上任意指定角度的点
- `float Math2D.Distance(PointF p1, PointF p2)`：计算两点间欧式距离
- `bool Math2D.IsPointInPolygon(PointF[] polygon, PointF pt)`：多边形射线法命中判定
- `bool Math2D.IsPointNearLine(PointF pt, PointF lineStart, PointF lineEnd, float tolerance)`：点在线段附近的拾取容差判定
- `RectangleF Math2D.GetBounds(IEnumerable<PointF> points)`：计算点集的 AABB 包围矩形
- `float Math2D.CalculatePolygonArea(PointF[] points)`：格林公式计算多边形面积

---

## 7. 常见误区与避坑指南 (Do's & Don'ts)

| 场景 | ❌ 错误做法 (Don't) |  正确做法 (Do) |
| :--- | :--- | :--- |
| **图形绘制** | 重写 `OnPaint` 并调用 `Graphics.Draw...` 逐个绘制 | 创建 `Element2D` 对象加入 `scene.Add(...)`，让框架负责渲染管线 |
| **坐标混淆** | 把鼠标像素点 `(e.X, e.Y)` 直接赋给图元的 `Position` | 通过 `scene.Viewport.ScreenToWorld(e.Location)` 换算为世界坐标再赋值 |
| **动态动画** | 定时器中每次调用 `scene.Clear()` 并重新 `new` 所有图元 | 保持图元实例，仅修改图元的 `Position`、`Rotation`、`Points` 或 `End` 并调用 `control.Invalidate()` |
| **层级遮挡** | 依靠添加顺序控制遮挡关系 | 使用 `element.ZIndex` 明确分层（底层用负数，业务层 1~10，高亮/顶层浮窗 100+） |
| **悬停不响应** | 以为不需要开启交互 | 检查是否将图元的 `IsInteractive` 误设为了 `false` |
| **背景被遮** | 给网格 `GridElement` 赋予了默认的 `ZIndex = 0` | 网格设为 `ZIndex = -1000`，坐标轴设为 `ZIndex = -900` |