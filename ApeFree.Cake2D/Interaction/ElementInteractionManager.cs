using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ApeFree.Cake2D.Elements;
using ApeFree.Cake2D.Events;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Interaction
{
    /// <summary>
    /// 场景图元鼠标交互管理器，负责拾取图元并触发 MouseEnter/Leave/Down/Up/Click 等事件。
    /// </summary>
    public class ElementInteractionManager
    {
        private readonly Scene2D _scene;
        private Element2D _hoveredElement;
        private Element2D _pressedElement;

        public bool EnableInteraction { get; set; } = true;

        public ElementInteractionManager(Scene2D scene)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        /// <summary>查找包含指定世界坐标的最顶层图元</summary>
        public Element2D HitTest(PointF worldPoint)
        {
            List<Element2D> allElements = new List<Element2D>();
            CollectInteractiveElements(_scene.Elements, allElements);

            // 按 ZIndex 降序查找（最上层图元优先命中）
            foreach (var element in allElements.OrderByDescending(e => e.ZIndex))
            {
                if (element.HitTest(worldPoint))
                {
                    return element;
                }
            }
            return null;
        }

        private void CollectInteractiveElements(IEnumerable<Element2D> elements, List<Element2D> result)
        {
            foreach (var elem in elements)
            {
                if (!elem.IsVisible) continue;
                if (elem.IsInteractive) result.Add(elem);
                if (elem.Children.Count > 0)
                {
                    CollectInteractiveElements(elem.Children, result);
                }
            }
        }

        public bool OnMouseDown(ElementMouseEventArgs e)
        {
            if (!EnableInteraction) return false;

            Element2D hit = HitTest(e.WorldLocation);
            _pressedElement = hit;

            if (hit != null)
            {
                hit.IsFocused = true;
                hit.OnMouseDown(e);
                return e.Handled;
            }
            return false;
        }

        public bool OnMouseMove(ElementMouseEventArgs e)
        {
            if (!EnableInteraction) return false;

            Element2D hit = HitTest(e.WorldLocation);

            if (hit != _hoveredElement)
            {
                if (_hoveredElement != null)
                {
                    _hoveredElement.IsHovered = false;
                    _hoveredElement.OnMouseLeave(e);
                }
                _hoveredElement = hit;
                if (_hoveredElement != null)
                {
                    _hoveredElement.IsHovered = true;
                    _hoveredElement.OnMouseEnter(e);
                }
            }

            if (_hoveredElement != null)
            {
                _hoveredElement.OnMouseMove(e);
                return e.Handled;
            }
            return false;
        }

        public bool OnMouseUp(ElementMouseEventArgs e)
        {
            if (!EnableInteraction) return false;

            bool handled = false;
            if (_pressedElement != null)
            {
                _pressedElement.OnMouseUp(e);
                handled = e.Handled;

                if (_pressedElement == _hoveredElement)
                {
                    if (e.Clicks >= 2)
                        _pressedElement.OnDoubleClick(e);
                    else
                        _pressedElement.OnClick(e);
                }
                _pressedElement = null;
            }
            else if (_hoveredElement != null)
            {
                _hoveredElement.OnMouseUp(e);
                handled = e.Handled;
            }

            return handled;
        }
    }
}