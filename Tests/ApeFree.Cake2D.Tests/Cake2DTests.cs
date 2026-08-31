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
    }
}