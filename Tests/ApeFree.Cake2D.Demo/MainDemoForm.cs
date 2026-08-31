using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ApeFree.Cake2D.Elements;
using ApeFree.Cake2D.Elements.Billboards;
using ApeFree.Cake2D.Elements.Containers;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Events;
using ApeFree.Cake2D.Gdi.Controls;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;
using ApeFree.Cake2D.Scene;
#if !NET452 && !NET461
using ApeFree.Cake2D.Skia.Controls;
#endif

namespace ApeFree.Cake2D.Demo
{
    public class MainDemoForm : Form
    {
        private Scene2D _scene;
        private Cake2DControl _gdiControl;
#if !NET452 && !NET461
        private Cake2DSkiaControl _skiaControl;
#endif
        private Control _activeControl;

        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblEngine;
        private ToolStripStatusLabel _lblCoords;
        private ToolStripStatusLabel _lblInfo;
        private Timer _clockTimer;

        // Clock demo fields
        private LineElement _clockHour;
        private LineElement _clockMinute;
        private LineElement _clockSecond;
        private PolygonElement _clockPoly;
        private TextElement _clockTimeText;

        public MainDemoForm()
        {
            Text = "ApeFree.Cake2D - 跨引擎交互式演示";
            Size = new Size(1180, 800);
            StartPosition = FormStartPosition.CenterScreen;

            _clockTimer = new Timer { Interval = 200 };
            _clockTimer.Tick += ClockTimer_Tick;

            _scene = new Scene2D { BackgroundColor = Color.FromArgb(245, 247, 250) };

            InitializeControls();
            LoadPrimitivesScene();
            SwitchEngine(0);
        }

        private void InitializeControls()
        {
            // 视口控件
            _gdiControl = new Cake2DControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Scene = _scene
            };
            _gdiControl.MouseMove += Viewport_MouseMove;

#if !NET452 && !NET461
            _skiaControl = new Cake2DSkiaControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Scene = _scene,
                Visible = false
            };
            _skiaControl.MouseMove += Viewport_MouseMove;
            Controls.Add(_skiaControl);
#endif

            // 顶部工具栏
            var toolStrip = new ToolStrip { ImageScalingSize = new Size(20, 20) };

            var lblEngineTitle = new ToolStripLabel("渲染引擎:");
            var cmbEngine = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190 };
#if !NET452 && !NET461
            cmbEngine.Items.AddRange(new object[] { "SkiaSharp 引擎 (Cake2D.Skia)", "GDI+ 引擎 (Cake2D.Gdi)" });
#else
            cmbEngine.Items.AddRange(new object[] { "GDI+ 引擎 (Cake2D.Gdi)" });
#endif
            cmbEngine.SelectedIndex = 0;
            cmbEngine.SelectedIndexChanged += (s, e) => SwitchEngine(cmbEngine.SelectedIndex);

            var lblSceneTitle = new ToolStripLabel("演示场景:");
            var cmbScene = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 };
            cmbScene.Items.AddRange(new object[] { "1. 综合图元交互测试", "2. 交互式时钟演示", "3. 多维可交互雷达图" });
            cmbScene.SelectedIndex = 0;
            cmbScene.SelectedIndexChanged += (s, e) => SwitchScene(cmbScene.SelectedIndex);

            var btnFit = new ToolStripButton("全图适应 (Zoom to Fit)");
            btnFit.Click += (s, e) => ZoomToFit();

            var btnReset = new ToolStripButton("重置原点");
            btnReset.Click += (s, e) => _scene.Viewport.SetView(PointF.Empty, 800f);

            var btnToggleY = new ToolStripButton("Y轴: 屏幕向下");
            btnToggleY.Click += (s, e) =>
            {
                _scene.Viewport.YAxisUpwards = !_scene.Viewport.YAxisUpwards;
                btnToggleY.Text = $"Y轴: {(_scene.Viewport.YAxisUpwards ? "数学向上" : "屏幕向下")}";
                _activeControl?.Invalidate();
            };

            toolStrip.Items.AddRange(new ToolStripItem[]
            {
                lblEngineTitle, cmbEngine, new ToolStripSeparator(),
                lblSceneTitle, cmbScene, new ToolStripSeparator(),
                btnFit, btnReset, new ToolStripSeparator(),
                btnToggleY
            });
            Controls.Add(toolStrip);

            // 底部状态栏
            _statusStrip = new StatusStrip();
            _lblEngine = new ToolStripStatusLabel("引擎: GDI+");
            _lblCoords = new ToolStripStatusLabel("坐标: X=0.00, Y=0.00");
            _lblInfo = new ToolStripStatusLabel("提示: 鼠标左键/中键拖动画布，滚轮以光标为中心缩放，悬停/点击测试图元");
            _statusStrip.Items.AddRange(new ToolStripItem[] { _lblEngine, new ToolStripSeparator(), _lblCoords, new ToolStripSeparator(), _lblInfo });
            Controls.Add(_statusStrip);

            Controls.Add(_gdiControl);
        }

        private void SwitchEngine(int index)
        {
#if !NET452 && !NET461
            if (index == 0) // SkiaSharp
            {
                _gdiControl.Visible = false;
                _skiaControl.Visible = true;
                _skiaControl.BringToFront();
                _activeControl = _skiaControl;
                _lblEngine.Text = "当前引擎: SkiaSharp (ApeFree.Cake2D.Skia)";
                _skiaControl.Invalidate();
                return;
            }
#endif
            _gdiControl.Visible = true;
            _gdiControl.BringToFront();
            _activeControl = _gdiControl;
            _lblEngine.Text = "当前引擎: GDI+ (ApeFree.Cake2D.Gdi)";
            _gdiControl.Invalidate();
        }

        private void SwitchScene(int sceneIndex)
        {
            _clockTimer.Stop();
            _scene.Elements.Clear();

            switch (sceneIndex)
            {
                case 0:
                    LoadPrimitivesScene();
                    break;
                case 1:
                    LoadClockScene();
                    _clockTimer.Start();
                    break;
                case 2:
                    LoadRadarChartScene();
                    break;
            }

            _activeControl?.Invalidate();
        }

        #region Scene 1: 综合图元交互测试
        private void LoadPrimitivesScene()
        {
            _scene.BackgroundColor = Color.FromArgb(245, 247, 250);
            _scene.Viewport.SetView(PointF.Empty, 800f);

            _scene.Add(new GridElement { GridSpacing = 50f });
            _scene.Add(new AxisElement());

            var title = new TextElement("Cake2D 跨后端场景图演示", -200, -250)
            {
                FontSize = 18f,
                IsBold = true,
                StrokeColor = Color.FromArgb(30, 41, 59),
                ZIndex = 10
            };
            _scene.Add(title);

            var rect1 = new RectangleElement(-300, -150, 180, 100)
            {
                Name = "蓝色矩形",
                FillColor = Color.FromArgb(180, 59, 130, 246),
                StrokeColor = Color.FromArgb(29, 78, 216),
                LineWidth = 2f,
                ZIndex = 1
            };
            BindElementEvents(rect1);
            _scene.Add(rect1);

            var rect2 = new RectangleElement(-50, -150, 120, 120)
            {
                Name = "旋转紫色矩形",
                Rotation = 30f,
                Origin = new Vector2D(60, 60),
                FillColor = Color.FromArgb(180, 168, 85, 247),
                StrokeColor = Color.FromArgb(107, 33, 168),
                LineWidth = 2f,
                ZIndex = 2
            };
            BindElementEvents(rect2);
            _scene.Add(rect2);

            var circle = new CircleElement(200, -100, 60)
            {
                Name = "橙色正圆",
                FillColor = Color.FromArgb(180, 249, 115, 22),
                StrokeColor = Color.FromArgb(194, 65, 12),
                LineWidth = 2f,
                ZIndex = 3
            };
            BindElementEvents(circle);
            _scene.Add(circle);

            var ellipse = new EllipseElement(-200, 100, 90, 50)
            {
                Name = "绿色椭圆",
                FillColor = Color.FromArgb(180, 34, 197, 94),
                StrokeColor = Color.FromArgb(21, 128, 61),
                LineWidth = 2f,
                ZIndex = 2
            };
            BindElementEvents(ellipse);
            _scene.Add(ellipse);

            var poly = new PolygonElement(new[]
            {
                new PointF(50, 50),
                new PointF(120, 50),
                new PointF(150, 120),
                new PointF(85, 170),
                new PointF(20, 120)
            })
            {
                Name = "黄色多边形",
                FillColor = Color.FromArgb(180, 234, 179, 8),
                StrokeColor = Color.FromArgb(161, 98, 7),
                LineWidth = 2f,
                ZIndex = 2
            };
            BindElementEvents(poly);
            _scene.Add(poly);

            var line = new LineElement(new Vector2D(-300, 220), new Vector2D(250, 220))
            {
                Name = "虚线分割线",
                StrokeColor = Color.FromArgb(148, 163, 184),
                LineWidth = 2f,
                IsDashed = true,
                ZIndex = 1
            };
            _scene.Add(line);

            var tip = new TextElement("将鼠标悬停在图元上会高亮，点击可在下方查看图元名称", -200, 250)
            {
                FontSize = 11f,
                StrokeColor = Color.FromArgb(100, 116, 139),
                ZIndex = 10
            };
            _scene.Add(tip);
        }
        #endregion

        #region Scene 2: 交互式时钟演示
        private void LoadClockScene()
        {
            _scene.BackgroundColor = Color.FromArgb(248, 250, 252);
            _scene.Viewport.SetView(PointF.Empty, 450f);

            float radius = 150f;

            // 表盘
            var dial = new CircleElement(0, 0, radius)
            {
                Name = "表盘",
                FillColor = Color.White,
                StrokeColor = Color.FromArgb(30, 41, 59),
                LineWidth = 3f,
                ZIndex = 1
            };
            _scene.Add(dial);

            // 60 刻度
            for (int i = 0; i < 60; i++)
            {
                float angle = i * 6f - 90f;
                bool isHour = i % 5 == 0;
                float rOut = radius - 4f;
                float rIn = isHour ? radius - 18f : radius - 10f;

                var tick = new LineElement(
                    new Vector2D(Math2D.CalculatePointOnCircle(PointF.Empty, rIn, angle)),
                    new Vector2D(Math2D.CalculatePointOnCircle(PointF.Empty, rOut, angle)))
                {
                    StrokeColor = isHour ? Color.FromArgb(30, 41, 59) : Color.FromArgb(148, 163, 184),
                    LineWidth = isHour ? 2.5f : 1f,
                    IsInteractive = false,
                    ZIndex = 2
                };
                _scene.Add(tick);
            }

            // 12 数字
            for (int h = 1; h <= 12; h++)
            {
                float angle = h * 30f - 90f;
                PointF p = Math2D.CalculatePointOnCircle(PointF.Empty, radius - 32f, angle);
                var num = new TextElement(h.ToString(), p.X, p.Y)
                {
                    FontSize = 11f,
                    IsBold = true,
                    StrokeColor = Color.FromArgb(51, 65, 85),
                    Alignment = TextAlignment.MiddleCenter,
                    IsInteractive = false,
                    ZIndex = 3
                };
                _scene.Add(num);
            }

            // 指针关联三角形
            _clockPoly = new PolygonElement(new PointF[3])
            {
                Name = "指针关联三角形",
                FillColor = Color.FromArgb(40, 59, 130, 246),
                StrokeColor = Color.FromArgb(140, 59, 130, 246),
                LineWidth = 1.2f,
                ZIndex = 4
            };
            _clockPoly.MouseEnter += (s, e) =>
            {
                _clockPoly.FillColor = Color.FromArgb(90, 59, 130, 246);
                _lblInfo.Text = "悬停于指针端点连接的多边形区域";
                _activeControl?.Invalidate();
            };
            _clockPoly.MouseLeave += (s, e) =>
            {
                _clockPoly.FillColor = Color.FromArgb(40, 59, 130, 246);
                _activeControl?.Invalidate();
            };
            _scene.Add(_clockPoly);

            // 时分秒针
            _clockHour = new LineElement(Vector2D.Zero, new Vector2D(0, -65))
            {
                Name = "时针",
                StrokeColor = Color.FromArgb(29, 78, 216),
                LineWidth = 5f,
                ZIndex = 5
            };
            BindHandHover(_clockHour, 5f);
            _scene.Add(_clockHour);

            _clockMinute = new LineElement(Vector2D.Zero, new Vector2D(0, -100))
            {
                Name = "分针",
                StrokeColor = Color.FromArgb(22, 163, 74),
                LineWidth = 3.5f,
                ZIndex = 6
            };
            BindHandHover(_clockMinute, 3.5f);
            _scene.Add(_clockMinute);

            _clockSecond = new LineElement(Vector2D.Zero, new Vector2D(0, -125))
            {
                Name = "秒针",
                StrokeColor = Color.FromArgb(220, 38, 38),
                LineWidth = 1.8f,
                ZIndex = 7
            };
            BindHandHover(_clockSecond, 1.8f);
            _scene.Add(_clockSecond);

            var pin = new CircleElement(0, 0, 5)
            {
                FillColor = Color.FromArgb(220, 38, 38),
                StrokeColor = Color.Black,
                LineWidth = 1f,
                IsInteractive = false,
                ZIndex = 8
            };
            _scene.Add(pin);

            _clockTimeText = new TextElement(DateTime.Now.ToString("HH:mm:ss"), 0, 50)
            {
                FontSize = 10.5f,
                StrokeColor = Color.FromArgb(100, 116, 139),
                Alignment = TextAlignment.MiddleCenter,
                IsInteractive = false,
                ZIndex = 3
            };
            _scene.Add(_clockTimeText);

            UpdateClockHands();
        }

        private void BindHandHover(LineElement hand, float normalWidth)
        {
            Color normalColor = hand.StrokeColor ?? Color.Black;
            hand.MouseEnter += (s, e) =>
            {
                hand.LineWidth = normalWidth + 2.5f;
                hand.StrokeColor = Color.FromArgb(234, 88, 12);
                _lblInfo.Text = $"当前悬停: {hand.Name}";
                Cursor = Cursors.Hand;
                _activeControl?.Invalidate();
            };
            hand.MouseLeave += (s, e) =>
            {
                hand.LineWidth = normalWidth;
                hand.StrokeColor = normalColor;
                Cursor = Cursors.Default;
                _activeControl?.Invalidate();
            };
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateClockHands();
        }

        private void UpdateClockHands()
        {
            if (_clockHour == null || _clockMinute == null || _clockSecond == null) return;

            var now = DateTime.Now;
            float totalSeconds = now.Hour * 3600f + now.Minute * 60f + now.Second + now.Millisecond / 1000f;

            float sAngle = (totalSeconds % 60f) / 60f * 360f - 90f;
            float mAngle = (totalSeconds % 3600f) / 3600f * 360f - 90f;
            float hAngle = (totalSeconds % 43200f) / 43200f * 360f - 90f;

            PointF pSec = Math2D.CalculatePointOnCircle(PointF.Empty, 125f, sAngle);
            PointF pMin = Math2D.CalculatePointOnCircle(PointF.Empty, 100f, mAngle);
            PointF pHour = Math2D.CalculatePointOnCircle(PointF.Empty, 65f, hAngle);

            _clockSecond.End = new Vector2D(pSec);
            _clockMinute.End = new Vector2D(pMin);
            _clockHour.End = new Vector2D(pHour);

            _clockPoly.Points = new List<PointF> { pSec, pMin, pHour };
            _clockTimeText.Text = now.ToString("HH:mm:ss");

            _activeControl?.Invalidate();
        }
        #endregion

        #region Scene 3: 多维度雷达图演示
        private void LoadRadarChartScene()
        {
            _scene.BackgroundColor = Color.White;
            _scene.Viewport.SetView(PointF.Empty, 480f);

            float maxRadius = 150f;
            int ringCount = 5;
            string[] areas = { "物理攻击 (ATK)", "防御韧性 (DEF)", "生命气血 (HP)", "移动速度 (SPD)", "暴击几率 (CRT)", "法术增伤 (MAG)" };
            int axisCount = areas.Length;

            // 1. 同心多边形圈
            for (int r = 1; r <= ringCount; r++)
            {
                float radius = (maxRadius / ringCount) * r;
                PointF[] ringPts = new PointF[axisCount];
                for (int a = 0; a < axisCount; a++)
                {
                    float angle = (360f / axisCount) * a - 90f;
                    ringPts[a] = Math2D.CalculatePointOnCircle(PointF.Empty, radius, angle);
                }

                _scene.Add(new PolygonElement(ringPts)
                {
                    StrokeColor = Color.FromArgb(226, 232, 240),
                    LineWidth = r == ringCount ? 1.5f : 1f,
                    IsInteractive = false,
                    ZIndex = 1
                });
            }

            // 2. 辐射轴与文本
            for (int a = 0; a < axisCount; a++)
            {
                float angle = (360f / axisCount) * a - 90f;
                PointF pEnd = Math2D.CalculatePointOnCircle(PointF.Empty, maxRadius, angle);
                _scene.Add(new LineElement(Vector2D.Zero, new Vector2D(pEnd))
                {
                    StrokeColor = Color.FromArgb(203, 213, 225),
                    LineWidth = 1.2f,
                    IsInteractive = false,
                    ZIndex = 2
                });

                PointF pLabel = Math2D.CalculatePointOnCircle(PointF.Empty, maxRadius + 22f, angle);
                _scene.Add(new TextElement(areas[a], pLabel.X, pLabel.Y)
                {
                    FontSize = 9.5f,
                    IsBold = true,
                    StrokeColor = Color.FromArgb(71, 85, 105),
                    Alignment = TextAlignment.MiddleCenter,
                    IsInteractive = false,
                    ZIndex = 3
                });
            }

            // 3. 数据系列
            var seriesData = new[]
            {
                new { Name = "战士 (Warrior)", Color = Color.FromArgb(239, 68, 68), Rates = new[] { 0.95f, 0.75f, 0.90f, 0.40f, 0.60f, 0.20f } },
                new { Name = "法师 (Mage)", Color = Color.FromArgb(59, 130, 246), Rates = new[] { 0.20f, 0.35f, 0.45f, 0.55f, 0.85f, 1.00f } },
                new { Name = "刺客 (Assassin)", Color = Color.FromArgb(16, 185, 129), Rates = new[] { 0.85f, 0.30f, 0.40f, 0.95f, 0.95f, 0.35f } }
            };

            var tooltip = new TextElement("鼠标悬停在雷达数据区域上可查看数值并高亮", 0, -maxRadius - 32f)
            {
                FontSize = 11f,
                IsBold = true,
                StrokeColor = Color.FromArgb(30, 41, 59),
                Alignment = TextAlignment.MiddleCenter,
                IsInteractive = false,
                ZIndex = 100
            };
            _scene.Add(tooltip);

            foreach (var s in seriesData)
            {
                PointF[] pts = new PointF[axisCount];
                for (int a = 0; a < axisCount; a++)
                {
                    float angle = (360f / axisCount) * a - 90f;
                    pts[a] = Math2D.CalculatePointOnCircle(PointF.Empty, maxRadius * s.Rates[a], angle);
                }

                Color baseColor = s.Color;
                var poly = new PolygonElement(pts)
                {
                    Name = s.Name,
                    StrokeColor = baseColor,
                    FillColor = Color.FromArgb(100, baseColor),
                    LineWidth = 2f,
                    ZIndex = 5
                };

                var captured = s;
                poly.MouseEnter += (sender, e) =>
                {
                    poly.FillColor = Color.FromArgb(180, baseColor);
                    poly.LineWidth = 3.5f;
                    poly.ZIndex = 20;
                    tooltip.Text = $"【{captured.Name}】: {string.Join(" | ", captured.Rates.Select((r, i) => $"{areas[i].Split(' ')[0]}: {(int)(r * 100)}%"))}";
                    tooltip.StrokeColor = baseColor;
                    _lblInfo.Text = $"已高亮雷达数据系列: {captured.Name}";
                    Cursor = Cursors.Hand;
                    _activeControl?.Invalidate();
                };

                poly.MouseLeave += (sender, e) =>
                {
                    poly.FillColor = Color.FromArgb(100, baseColor);
                    poly.LineWidth = 2f;
                    poly.ZIndex = 5;
                    tooltip.Text = "鼠标悬停在雷达数据区域上可查看数值并高亮";
                    tooltip.StrokeColor = Color.FromArgb(30, 41, 59);
                    _lblInfo.Text = "提示: 鼠标左键/中键拖动画布，滚轮以光标为中心缩放，悬停/点击测试图元";
                    Cursor = Cursors.Default;
                    _activeControl?.Invalidate();
                };

                _scene.Add(poly);
            }
        }
        #endregion

        private void ZoomToFit()
        {
            var allBounds = _scene.Elements.Select(elem => elem.GetWorldBounds()).ToList();
            if (allBounds.Count > 0)
            {
                var pts = allBounds.SelectMany(r => new[]
                {
                    new PointF(r.Left, r.Top),
                    new PointF(r.Right, r.Bottom)
                });
                _scene.Viewport.ZoomToFit(Math2D.GetBounds(pts));
                _activeControl?.Invalidate();
            }
        }

        private void BindElementEvents(Element2D elem)
        {
            Color? originalFill = elem.FillColor;

            elem.MouseEnter += (s, e) =>
            {
                if (originalFill.HasValue)
                {
                    elem.FillColor = Color.FromArgb(230, originalFill.Value.R, originalFill.Value.G, originalFill.Value.B);
                }
                _lblInfo.Text = $"当前悬停: [{elem.Name}]";
                _activeControl?.Invalidate();
            };

            elem.MouseLeave += (s, e) =>
            {
                elem.FillColor = originalFill;
                _lblInfo.Text = "提示: 鼠标左键/中键拖动画布，滚轮以光标为中心缩放，悬停/点击测试图元";
                _activeControl?.Invalidate();
            };

            elem.Click += (s, e) =>
            {
                _lblInfo.Text = $"已点击图元: [{elem.Name}] 世界坐标: ({e.WorldLocation.X:F1}, {e.WorldLocation.Y:F1})";
            };
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            PointF world = _scene.Viewport.ScreenToWorld(e.Location);
            _lblCoords.Text = $"屏幕: ({e.X}, {e.Y})  |  世界: ({world.X:F2}, {world.Y:F2})";
        }
    }
}