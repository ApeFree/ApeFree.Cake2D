using System;
using System.Drawing;
using System.Linq;
using ApeFree.Cake2D.Elements.Primitives;
using ApeFree.Cake2D.Elements.Containers;
using ApeFree.Cake2D.Elements.Billboards;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;
using ApeFree.Cake2D.Scene;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApeFree.Cake2D.Tests
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void Vector2D_BasicOperations()
        {
            var v1 = new Vector2D(3, 4);
            var v2 = new Vector2D(1, 2);

            Assert.AreEqual(5f, v1.Length, 1e-4f);
            Assert.AreEqual(new Vector2D(4, 6), v1 + v2);
            Assert.AreEqual(new Vector2D(2, 2), v1 - v2);
            Assert.AreEqual(new Vector2D(6, 8), v1 * 2f);

            var norm = v1.Normalized();
            Assert.AreEqual(1f, norm.Length, 1e-4f);
            Assert.AreEqual(0.6f, norm.X, 1e-4f);
            Assert.AreEqual(0.8f, norm.Y, 1e-4f);
        }

        [TestMethod]
        public void Vector2D_StrictEqualsAndNearlyEquals()
        {
            var v1 = new Vector2D(1.0f, 2.0f);
            var v2 = new Vector2D(1.0f, 2.0f);
            var v3 = new Vector2D(1.0000005f, 2.0f);

            Assert.IsTrue(v1 == v2);
            Assert.AreEqual(v1.GetHashCode(), v2.GetHashCode());

            // 严格判等
            Assert.IsFalse(v1 == v3);

            // 容差近似判等
            Assert.IsTrue(v1.NearlyEquals(v3, 1e-5f));
            Assert.IsFalse(v1.NearlyEquals(v3, 1e-7f));
        }

        [TestMethod]
        public void Matrix3x2_TransformAndInvert()
        {
            var mTrans = Matrix3x2.CreateTranslation(10, 20);
            var mScale = Matrix3x2.CreateScale(2, 3);
            var mCompound = mScale * mTrans; // Scale then Translate

            var pt = new PointF(5, 5);
            var transformed = mCompound.TransformPoint(pt);
            // 5 * 2 + 10 = 20; 5 * 3 + 20 = 35
            Assert.AreEqual(20f, transformed.X, 1e-4f);
            Assert.AreEqual(35f, transformed.Y, 1e-4f);

            bool invSuccess = Matrix3x2.Invert(mCompound, out var mInv);
            Assert.IsTrue(invSuccess);

            var original = mInv.TransformPoint(transformed);
            Assert.AreEqual(5f, original.X, 1e-4f);
            Assert.AreEqual(5f, original.Y, 1e-4f);
        }

        [TestMethod]
        public void Math2D_PolygonGeometry()
        {
            // 顺时针单位正方形 (0,0), (10,0), (10,10), (0,10)
            PointF[] poly = new[]
            {
                new PointF(0, 0),
                new PointF(10, 0),
                new PointF(10, 10),
                new PointF(0, 10)
            };

            float area = Math2D.CalculatePolygonArea(poly);
            Assert.AreEqual(100f, area, 1e-4f);

            float perimeter = Math2D.CalculatePolygonPerimeter(poly);
            Assert.AreEqual(40f, perimeter, 1e-4f);

            PointF centroid = Math2D.CalculatePolygonCentroid(poly);
            Assert.AreEqual(5f, centroid.X, 1e-4f);
            Assert.AreEqual(5f, centroid.Y, 1e-4f);

            Assert.IsTrue(Math2D.IsPointInPolygon(poly, new PointF(5, 5)));
            Assert.IsTrue(Math2D.IsPointInPolygon(poly, new PointF(1, 1)));
            Assert.IsFalse(Math2D.IsPointInPolygon(poly, new PointF(15, 5)));
            Assert.IsFalse(Math2D.IsPointInPolygon(poly, new PointF(-1, 5)));
        }
    }

    [TestClass]
    public class ViewportTests
    {
        [TestMethod]
        public void ScreenToWorld_WorldToScreen_Invertible()
        {
            var vp = new Viewport2D();
            vp.Resize(800, 600);
            vp.SetView(new PointF(100, 200), 500f);

            // Test normal mode (YAxisUpwards = false)
            vp.YAxisUpwards = false;
            PointF screenPt = new PointF(350, 250);
            PointF worldPt = vp.ScreenToWorld(screenPt);
            PointF backToScreen = vp.WorldToScreen(worldPt);

            Assert.AreEqual(screenPt.X, backToScreen.X, 1e-3f);
            Assert.AreEqual(screenPt.Y, backToScreen.Y, 1e-3f);

            // Test Cartesian / Math mode (YAxisUpwards = true)
            vp.YAxisUpwards = true;
            worldPt = vp.ScreenToWorld(screenPt);
            backToScreen = vp.WorldToScreen(worldPt);

            Assert.AreEqual(screenPt.X, backToScreen.X, 1e-3f);
            Assert.AreEqual(screenPt.Y, backToScreen.Y, 1e-3f);
        }

        [TestMethod]
        public void ZoomAroundScreenPoint_PreservesWorldPointUnderCursor()
        {
            var vp = new Viewport2D();
            vp.Resize(800, 600);
            vp.SetView(new PointF(0, 0), 1000f);

            PointF cursorScreen = new PointF(600, 450);
            PointF worldBefore = vp.ScreenToWorld(cursorScreen);

            // 放大 2 倍
            vp.ZoomAroundScreenPoint(cursorScreen, 2.0f);

            PointF worldAfter = vp.ScreenToWorld(cursorScreen);

            Assert.AreEqual(worldBefore.X, worldAfter.X, 1e-3f);
            Assert.AreEqual(worldBefore.Y, worldAfter.Y, 1e-3f);
        }
    }

    [TestClass]
    public class SceneGraphTests
    {
        [TestMethod]
        public void Hierarchy_WorldMatrixComputation()
        {
            var parent = new GroupElement
            {
                Position = new Vector2D(100, 100),
                Scale = new Vector2D(2, 2)
            };

            var child = new RectangleElement(50, 50)
            {
                Position = new Vector2D(10, 20)
            };

            parent.AddChild(child);

            // child world position should be: (10 * 2) + 100 = 120, (20 * 2) + 100 = 140
            PointF childWorldOrigin = child.LocalToWorld(new PointF(0, 0));
            Assert.AreEqual(120f, childWorldOrigin.X, 1e-4f);
            Assert.AreEqual(140f, childWorldOrigin.Y, 1e-4f);

            // child world end (50, 50) should be: 120 + (50 * 2) = 220, 140 + (50 * 2) = 240
            PointF childWorldEnd = child.LocalToWorld(new PointF(50, 50));
            Assert.AreEqual(220f, childWorldEnd.X, 1e-4f);
            Assert.AreEqual(240f, childWorldEnd.Y, 1e-4f);

            // Hit test
            Assert.IsTrue(child.HitTest(new PointF(150, 150)));
            Assert.IsFalse(child.HitTest(new PointF(50, 50)));
        }

        [TestMethod]
        public void Element2D_TransformDirtyFlags_CachingWorks()
        {
            var elem = new RectangleElement(10, 20)
            {
                Position = new Vector2D(50, 60)
            };

            // 第一次获取计算矩阵
            Matrix3x2 m1 = elem.GetWorldMatrix();
            Assert.AreEqual(50f, m1.M31);
            Assert.AreEqual(60f, m1.M32);

            // 再次获取应直接命中缓存
            Matrix3x2 m2 = elem.GetWorldMatrix();
            Assert.AreEqual(m1, m2);

            // 修改属性后自动置脏并刷新
            elem.Position = new Vector2D(80, 90);
            Matrix3x2 m3 = elem.GetWorldMatrix();
            Assert.AreEqual(80f, m3.M31);
            Assert.AreEqual(90f, m3.M32);
        }

        [TestMethod]
        public void GroupElement_BoundsCalculation_NoDoubleTransform()
        {
            var group = new GroupElement
            {
                Position = new Vector2D(100, 100)
            };

            var child = new RectangleElement(0, 0, 50, 50);
            group.AddChild(child);

            // Group's local bounds should be relative to group: [0, 0, 50, 50]
            RectangleF localBounds = group.GetLocalBounds();
            Assert.AreEqual(0f, localBounds.Left, 1e-4f);
            Assert.AreEqual(0f, localBounds.Top, 1e-4f);
            Assert.AreEqual(50f, localBounds.Width, 1e-4f);
            Assert.AreEqual(50f, localBounds.Height, 1e-4f);

            // Group's world bounds should be: [100, 100, 50, 50] (NOT [200, 200, 50, 50])
            RectangleF worldBounds = group.GetWorldBounds();
            Assert.AreEqual(100f, worldBounds.Left, 1e-4f);
            Assert.AreEqual(100f, worldBounds.Top, 1e-4f);
            Assert.AreEqual(50f, worldBounds.Width, 1e-4f);
            Assert.AreEqual(50f, worldBounds.Height, 1e-4f);
        }

        [TestMethod]
        public void GroupElement_HollowPenetration_DoesNotBlockBlankArea()
        {
            var group = new GroupElement();
            var child1 = new RectangleElement(0, 0, 20, 20);
            var child2 = new RectangleElement(100, 100, 20, 20);
            group.AddChild(child1);
            group.AddChild(child2);

            // Group 的外接矩形覆盖 [0, 0] 到 [120, 120]
            RectangleF localBounds = group.GetLocalBounds();
            Assert.IsTrue(localBounds.Contains(new PointF(50, 50)));

            // 但是点击 (50, 50) 的空白区域时，GroupElement 自身 HitTest 必须返回 false，支持中空穿透！
            Assert.IsFalse(group.HitTest(new PointF(50, 50)));
        }

        [TestMethod]
        public void SubtreeWorldBounds_EncompassesOffscreenParentWithOnscreenChild()
        {
            var parent = new GroupElement
            {
                Position = new Vector2D(2000, 2000)
            };

            var child = new RectangleElement(0, 0, 50, 50)
            {
                Position = new Vector2D(-2000, -2000)
            };
            parent.AddChild(child);

            RectangleF subtreeBounds = parent.GetSubtreeWorldBounds();
            // Child is at world (0, 0, 50, 50), parent subtree bounds must cover [0, 50]
            Assert.IsTrue(subtreeBounds.Left <= 0f);
            Assert.IsTrue(subtreeBounds.Top <= 0f);
            Assert.IsTrue(subtreeBounds.Right >= 50f);
            Assert.IsTrue(subtreeBounds.Bottom >= 50f);
        }

        [TestMethod]
        public void TextElement_CenteredAlignment_BoundsOffset()
        {
            var text = new TextElement("ABC", 0, 0)
            {
                FontSize = 20f,
                Alignment = TextAlignment.MiddleCenter
            };

            RectangleF bounds = text.GetLocalBounds();
            // MiddleCenter should have bounds centered around (0,0)
            Assert.IsTrue(bounds.Left < 0f, "Left should be negative for MiddleCenter");
            Assert.IsTrue(bounds.Right > 0f, "Right should be positive for MiddleCenter");
            Assert.IsTrue(bounds.Top < 0f, "Top should be negative for MiddleCenter");
            Assert.IsTrue(bounds.Bottom > 0f, "Bottom should be positive for MiddleCenter");
        }

        [TestMethod]
        public void ElementInteraction_FocusAndCapture()
        {
            var scene = new Scene2D();
            scene.Viewport.Resize(800, 600);
            scene.Viewport.SetView(new PointF(0, 0), 800f);

            var rect1 = new RectangleElement(10, 10, 50, 50);
            var rect2 = new RectangleElement(100, 100, 50, 50);
            scene.Add(rect1);
            scene.Add(rect2);

            var mgr = scene.InteractionManager;

            // Click rect1
            var downArgs1 = new ApeFree.Cake2D.Events.ElementMouseEventArgs(
                new PointF(20, 20), new PointF(20, 20), ApeFree.Cake2D.Events.CakeMouseButton.Left, 1, 0);
            bool handled1 = mgr.OnMouseDown(downArgs1);

            Assert.IsTrue(handled1, "Hit interactive element should be handled by default");
            Assert.IsTrue(rect1.IsFocused);
            Assert.IsFalse(rect2.IsFocused);
            Assert.AreEqual(rect1, mgr.CapturedElement);

            // Click rect2 -> rect1 should lose focus
            var downArgs2 = new ApeFree.Cake2D.Events.ElementMouseEventArgs(
                new PointF(120, 120), new PointF(120, 120), ApeFree.Cake2D.Events.CakeMouseButton.Left, 1, 0);
            bool handled2 = mgr.OnMouseDown(downArgs2);

            Assert.IsTrue(handled2);
            Assert.IsFalse(rect1.IsFocused, "rect1 should lose focus");
            Assert.IsTrue(rect2.IsFocused, "rect2 should be focused");

            // Click blank -> both lose focus
            var downArgsBlank = new ApeFree.Cake2D.Events.ElementMouseEventArgs(
                new PointF(500, 500), new PointF(500, 500), ApeFree.Cake2D.Events.CakeMouseButton.Left, 1, 0);
            bool handledBlank = mgr.OnMouseDown(downArgsBlank);

            Assert.IsFalse(handledBlank, "Blank click should not be handled");
            Assert.IsFalse(rect1.IsFocused);
            Assert.IsFalse(rect2.IsFocused);
            Assert.IsNull(mgr.FocusedElement);
        }

        [TestMethod]
        public void HitTest_VariousElements()
        {
            var rect = new RectangleElement(0, 0, 100, 50);
            Assert.IsTrue(rect.HitTest(new PointF(50, 25)));
            Assert.IsFalse(rect.HitTest(new PointF(150, 25)));

            var circle = new CircleElement(0, 0, 50);
            Assert.IsTrue(circle.HitTest(new PointF(30, 30))); // 30^2 + 30^2 = 1800 < 2500
            Assert.IsFalse(circle.HitTest(new PointF(40, 40))); // 40^2 + 40^2 = 3200 > 2500

            var line = new LineElement(0, 0, 100, 0);
            Assert.IsTrue(line.HitTest(new PointF(50, 2))); // within 5px tolerance
            Assert.IsFalse(line.HitTest(new PointF(50, 20)));
        }
    }

    [TestClass]
    public class SkiaRenderTests
    {
        [TestMethod]
        public void SkiaRenderCanvas_RenderSceneOnSKBitmap()
        {
            var scene = new Scene2D();
            scene.BackgroundColor = Color.White;
            scene.Viewport.Resize(400, 300);

            var rect = new RectangleElement(10, 10, 100, 50)
            {
                FillColor = Color.Blue,
                StrokeColor = Color.Red,
                LineWidth = 2f
            };
            var text = new TextElement("Hello Skia", 50, 50)
            {
                StrokeColor = Color.Black,
                FontSize = 14f
            };
            scene.Add(rect);
            scene.Add(text);

            using (var bitmap = new SkiaSharp.SKBitmap(400, 300))
            using (var canvas = new SkiaSharp.SKCanvas(bitmap))
            {
                var renderCanvas = new ApeFree.Cake2D.Skia.SkiaRenderCanvas(canvas);
                var engine = new Engine2D();
                engine.Render(scene, renderCanvas);

                // Check pixel rendering at background (should not be empty / default transparent)
                var pixel = bitmap.GetPixel(0, 0);
                Assert.AreEqual(255, pixel.Alpha);
                Assert.AreEqual(255, pixel.Red);
                Assert.AreEqual(255, pixel.Green);
                Assert.AreEqual(255, pixel.Blue);
            }
        }

        [TestMethod]
        public void GridElement_ExtremeViewport_NoInfiniteLoop()
        {
            var scene = new Scene2D();
            scene.Viewport.Resize(0, 0); // Collapsed viewport
            var grid = new GridElement { AutoScale = true, GridSpacing = 50f };
            scene.Add(grid);

            using (var bmp = new Bitmap(10, 10))
            using (var g = Graphics.FromImage(bmp))
            {
                var canvas = new ApeFree.Cake2D.Gdi.GdiRenderCanvas(g);
                var engine = new Engine2D();
                // Should not hang or throw exception
                engine.Render(scene, canvas);
            }

            // Test massive span with AutoScale = false
            scene.Viewport.Resize(800, 600);
            scene.Viewport.SetView(new PointF(0, 0), 1_000_000f);
            grid.AutoScale = false;
            using (var bmp = new Bitmap(10, 10))
            using (var g = Graphics.FromImage(bmp))
            {
                var canvas = new ApeFree.Cake2D.Gdi.GdiRenderCanvas(g);
                var engine = new Engine2D();
                // Safety line clamp prevents freezing
                engine.Render(scene, canvas);
            }
        }

        [TestMethod]
        public void TextElement_MixedCharacterBounds_Accurate()
        {
            var text = new TextElement("Hello 世界", 0, 0)
            {
                FontSize = 10f
            };

            RectangleF bounds = text.GetLocalBounds();
            // 6 ASCII chars (6 * 0.6 = 3.6) + 2 Wide chars (2 * 1.0 = 2.0) = 5.6 * 10 = 56px
            Assert.AreEqual(56f, bounds.Width, 1e-3f);
            Assert.AreEqual(12.5f, bounds.Height, 1e-3f);
        }

        [TestMethod]
        public void Interaction_EventBubbling_ToParent()
        {
            var scene = new Scene2D();
            scene.Viewport.Resize(800, 600);
            scene.Viewport.SetView(new PointF(0, 0), 800f);

            var group = new GroupElement();
            var childRect = new RectangleElement(10, 10, 50, 50);
            group.AddChild(childRect);
            scene.Add(group);

            bool childClicked = false;
            bool parentClicked = false;

            // 常规业务代码：子节点不手写 e.Handled = false，事件也应自然向上冒泡传递至父容器
            childRect.MouseDown += (s, e) =>
            {
                childClicked = true;
            };

            group.MouseDown += (s, e) =>
            {
                parentClicked = true;
            };

            var mgr = scene.InteractionManager;
            var downArgs = new ApeFree.Cake2D.Events.ElementMouseEventArgs(
                new PointF(20, 20), new PointF(20, 20), ApeFree.Cake2D.Events.CakeMouseButton.Left, 1, 0);

            bool handled = mgr.OnMouseDown(downArgs);

            Assert.IsTrue(childClicked, "Child element should receive MouseDown first");
            Assert.IsTrue(parentClicked, "Parent container should naturally receive bubbled MouseDown without special code");
            Assert.IsTrue(handled, "Hit interactive tree should be handled by default to prevent canvas pan");
        }

        [TestMethod]
        public void Interaction_EventBubbling_InterceptionByChild()
        {
            var scene = new Scene2D();
            scene.Viewport.Resize(800, 600);
            scene.Viewport.SetView(new PointF(0, 0), 800f);

            var group = new GroupElement();
            var childRect = new RectangleElement(10, 10, 50, 50);
            group.AddChild(childRect);
            scene.Add(group);

            bool childClicked = false;
            bool parentClicked = false;

            // 子节点显式标记 Handled = true 时，应主动阻断向上冒泡
            childRect.MouseDown += (s, e) =>
            {
                childClicked = true;
                e.Handled = true;
            };

            group.MouseDown += (s, e) =>
            {
                parentClicked = true;
            };

            var mgr = scene.InteractionManager;
            var downArgs = new ApeFree.Cake2D.Events.ElementMouseEventArgs(
                new PointF(20, 20), new PointF(20, 20), ApeFree.Cake2D.Events.CakeMouseButton.Left, 1, 0);

            bool handled = mgr.OnMouseDown(downArgs);

            Assert.IsTrue(childClicked);
            Assert.IsFalse(parentClicked, "Parent should NOT receive event when child explicitly sets Handled = true");
            Assert.IsTrue(handled);
        }

        [TestMethod]
        public void TextElement_CadMode_RotationAndBaselineDirection()
        {
            var scene = new Scene2D();
            scene.Viewport.YAxisUpwards = true; // CAD 模式
            scene.Viewport.Resize(800, 600);
            scene.Viewport.SetView(new PointF(0, 0), 800f);

            var text = new TextElement("CAD Pipe", 0, 0)
            {
                Rotation = 30f // 世界中沿 X 轴逆时针（向 +Y 方向）旋转 30 度
            };
            scene.Add(text);

            var context = new RenderContext2D(scene);
            Matrix3x2 transform = context.GetElementToScreenMatrix(text);

            if (context.Viewport.YAxisUpwards)
            {
                transform = new Matrix3x2(
                    transform.M11,
                    transform.M12,
                    -transform.M21,
                    -transform.M22,
                    transform.M31,
                    transform.M32
                );
            }

            // 1. 基线方向：世界中 Y 轴向上，旋转 +30 度代表朝右上倾斜。
            // 在屏幕空间中，向上对应负 Y，因此屏幕基线角度应为 -30 度（即 330 度）
            PointF pOrigin = transform.TransformPoint(new PointF(0, 0));
            PointF pBaselineEnd = transform.TransformPoint(new PointF(100, 0));
            float screenDx = pBaselineEnd.X - pOrigin.X;
            float screenDy = pBaselineEnd.Y - pOrigin.Y;

            Assert.IsTrue(screenDx > 0f, "CAD baseline should extend rightwards on screen");
            Assert.IsTrue(screenDy < 0f, "CAD +30deg should point upwards on screen (negative screen Y)");

            // 2. 变换行列式检查：必须为正数，杜绝镜像字形
            float det = transform.M11 * transform.M22 - transform.M12 * transform.M21;
            Assert.IsTrue(det > 0f, "Transform determinant must be positive so text glyphs are not mirrored");
        }

        [TestMethod]
        public void GdiRenderCanvas_ContinuousZoom_NoCrashOrThrashing()
        {
            using (var bmp = new Bitmap(100, 100))
            using (var g = Graphics.FromImage(bmp))
            {
                var canvas = new ApeFree.Cake2D.Gdi.GdiRenderCanvas(g);

                // 模拟用户连续鼠标滚轮缩放 1000 帧，涉及不同尺度和线宽，确保不会抛出 ArgumentException 或引发内存崩溃
                for (int i = 1; i <= 1000; i++)
                {
                    float scale = 0.1f + (i * 0.01f);
                    canvas.PushTransform(Matrix3x2.CreateScale(scale, scale));
                    canvas.DrawLine(new PointF(0, 0), new PointF(50, 50), Color.Red, 1.0f);
                    canvas.DrawLine(new PointF(10, 10), new PointF(60, 60), Color.Blue, 2.5f);
                    canvas.DrawRectangle(new RectangleF(5, 5, 20, 20), Color.Green, 1.0f);
                    canvas.PopTransform();
                }
            }
        }

        [TestMethod]
        public void Interaction_UnhandledEvent_ReleasesCapture()
        {
            var scene = new Scene2D();
            scene.Viewport.Resize(800, 600);
            scene.Viewport.SetView(new PointF(0, 0), 800f);

            var rect = new RectangleElement(10, 10, 50, 50);
            rect.MouseDown += (s, e) =>
            {
                e.Handled = false; // Intentionally don't consume, allowing viewport panning
            };
            scene.Add(rect);

            var mgr = scene.InteractionManager;
            var downArgs = new ApeFree.Cake2D.Events.ElementMouseEventArgs(
                new PointF(20, 20), new PointF(20, 20), ApeFree.Cake2D.Events.CakeMouseButton.Left, 1, 0);

            bool handled = mgr.OnMouseDown(downArgs);

            Assert.IsFalse(handled, "Unhandled event should return false to allow viewport pan");
            Assert.IsNull(mgr.CapturedElement, "Capture should be released if event wasn't handled");
        }

        [TestMethod]
        public void TextElement_DefaultFontName_InheritanceAndConfiguration()
        {
            string originalDefault = TextElement.DefaultFontName;
            try
            {
                // 1. 默认为 null
                TextElement.DefaultFontName = null;
                var elem1 = new TextElement("Sample");
                Assert.IsNull(elem1.FontName, "When DefaultFontName is null, FontName should be null by default");

                // 2. 配置全局 DefaultFontName
                TextElement.DefaultFontName = "Microsoft YaHei";
                var elem2 = new TextElement("Sample");
                Assert.AreEqual("Microsoft YaHei", elem2.FontName, "Element should inherit updated DefaultFontName");

                // 3. 实例显式赋值后，覆盖全局配置
                elem2.FontName = "Arial";
                Assert.AreEqual("Arial", elem2.FontName, "Explicit assignment should override DefaultFontName");
            }
            finally
            {
                TextElement.DefaultFontName = originalDefault;
            }
        }

        [TestMethod]
        public void SkiaRenderCanvas_MultiLanguage_NoMissingGlyphsOrExceptions()
        {
            using (var surface = SkiaSharp.SKSurface.Create(new SkiaSharp.SKImageInfo(400, 300)))
            {
                var canvas = new ApeFree.Cake2D.Skia.SkiaRenderCanvas(surface.Canvas);

                // 涵盖纯英(Tier 1)、中文(Tier 2)、俄文(Tier 2)、韩文(Tier 2)、泰文(Tier 2)、中韩泰多语种同串(Tier 3)
                var multiLangSamples = new[]
                {
                    ("ASCII", "Hello Cake2D 12345!"),
                    ("中文 (Fallback from Arial)", "Cake2D 跨后端场景图演示"),
                    ("俄文 (Russian)", "Привет, мир!"),
                    ("韩文 (Korean)", "안녕하세요 Cake2D"),
                    ("日文 (Japanese)", "こんにちは Cake2D"),
                    ("泰文 (Thai)", "สวัสดีชาวโลก"),
                    ("阿拉伯文 (Arabic)", "مرحبا بالعالم"),
                    ("多语系单串混排 (Tier 3 Multi-Run)", "中国(CHN) - 한국(KOR) - ไทย(THA) - English!"),
                };

                foreach (var (lang, text) in multiLangSamples)
                {
                    // 故意显式传入纯西文字体 "Arial"，测试底层的自愈与回退机制
                    canvas.DrawText(text, new PointF(10, 10), Color.Black, "Arial", 12f, false, TextAlignment.TopLeft);
                    canvas.DrawText(text, new PointF(100, 100), Color.Blue, null, 14f, true, TextAlignment.MiddleCenter);
                }
            }
        }

        [TestMethod]
        public void SkiaRenderCanvas_HighFrequencyDraw_NoCrashOrLeak()
        {
            using (var surface = SkiaSharp.SKSurface.Create(new SkiaSharp.SKImageInfo(200, 200)))
            {
                var canvas = new ApeFree.Cake2D.Skia.SkiaRenderCanvas(surface.Canvas);

                // 模拟高频重绘 1000 次不同文本与中英混合内容
                for (int i = 0; i < 1000; i++)
                {
                    canvas.DrawText($"Frame #{i}: 刷新测试 - 속도 {i % 100}%", new PointF(50, 50), Color.Red, "Arial", 10f, false, TextAlignment.TopLeft);
                }
            }
        }
    }
}
