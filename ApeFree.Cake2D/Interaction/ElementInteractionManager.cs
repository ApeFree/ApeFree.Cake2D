using System;
using System.Collections.Generic;
using System.Drawing;
using ApeFree.Cake2D.Elements;
using ApeFree.Cake2D.Events;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Interaction
{
    /// <summary>
    /// 场景图元鼠标交互管理器，负责拾取图元并触发 MouseEnter/Leave/Down/Up/Click 等事件。
    /// 采用树状短路拾取与包围盒粗筛，消灭高频移动下的内存分配与全量堆排序。
    /// </summary>
    public class ElementInteractionManager
    {
        private readonly Scene2D _scene;
        private readonly List<Element2D> _sortedRoots = new List<Element2D>();
        private readonly List<Element2D> _childSortBuffer = new List<Element2D>();
        private Element2D _hoveredElement;
        private Element2D _pressedElement;
        private Element2D _capturedElement;
        private Element2D _focusedElement;

        public bool EnableInteraction { get; set; } = true;

        /// <summary>当前获取键盘/输入焦点的图元</summary>
        public Element2D FocusedElement
        {
            get => _focusedElement;
            set
            {
                if (_focusedElement == value) return;
                if (_focusedElement != null)
                {
                    _focusedElement.IsFocused = false;
                }
                _focusedElement = value;
                if (_focusedElement != null)
                {
                    _focusedElement.IsFocused = true;
                }
            }
        }

        /// <summary>当前被鼠标独占捕获的图元</summary>
        public Element2D CapturedElement => _capturedElement;

        public ElementInteractionManager(Scene2D scene)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        /// <summary>查找包含指定世界坐标的最顶层图元（采用树状回溯与短路退出）</summary>
        public Element2D HitTest(PointF worldPoint)
        {
            if (!EnableInteraction || _scene.Elements.Count == 0) return null;

            _sortedRoots.Clear();
            _sortedRoots.AddRange(_scene.Elements);
            // 根节点按 ZIndex 降序（最上层图元优先拾取测试）
            _sortedRoots.Sort((a, b) => b.ZIndex.CompareTo(a.ZIndex));

            for (int i = 0; i < _sortedRoots.Count; i++)
            {
                Element2D hit = HitTestElementTree(_sortedRoots[i], worldPoint);
                if (hit != null) return hit;
            }
            return null;
        }

        private Element2D HitTestElementTree(Element2D element, PointF worldPoint)
        {
            if (!element.IsVisible) return null;

            // 1. 如果存在子节点，优先测试子节点（按子节点的局部 ZIndex 降序拾取）
            int childCount = element.Children.Count;
            if (childCount > 0)
            {
                if (childCount == 1)
                {
                    Element2D hit = HitTestElementTree(element.Children[0], worldPoint);
                    if (hit != null) return hit;
                }
                else
                {
                    // 检测子节点是否已为 ZIndex 降序（最普遍情况下所有子节点 ZIndex 均为 0）
                    bool isDescending = true;
                    for (int i = 0; i < childCount - 1; i++)
                    {
                        if (element.Children[i].ZIndex < element.Children[i + 1].ZIndex)
                        {
                            isDescending = false;
                            break;
                        }
                    }

                    if (isDescending)
                    {
                        for (int i = 0; i < childCount; i++)
                        {
                            Element2D hit = HitTestElementTree(element.Children[i], worldPoint);
                            if (hit != null) return hit;
                        }
                    }
                    else
                    {
                        _childSortBuffer.Clear();
                        _childSortBuffer.AddRange(element.Children);
                        _childSortBuffer.Sort((a, b) => b.ZIndex.CompareTo(a.ZIndex));
                        for (int i = 0; i < _childSortBuffer.Count; i++)
                        {
                            Element2D hit = HitTestElementTree(_childSortBuffer[i], worldPoint);
                            if (hit != null) return hit;
                        }
                    }
                }
            }

            // 2. 测试自身（若开启了交互）
            if (element.IsInteractive)
            {
                RectangleF bounds = element.GetWorldBounds();
                // AABB 包围盒粗筛快速剔除（若不在包围盒内直接跳过昂贵的几何精确判定）
                if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0 || bounds.Contains(worldPoint))
                {
                    if (element.HitTest(worldPoint))
                    {
                        return element;
                    }
                }
            }

            return null;
        }

        public bool OnMouseDown(ElementMouseEventArgs e)
        {
            if (!EnableInteraction) return false;

            Element2D hit = HitTest(e.WorldLocation);
            _pressedElement = hit;
            _capturedElement = hit; // 启用鼠标捕获

            // 焦点状态管理：点击图元获焦，点击空白清空焦点
            FocusedElement = hit;

            if (hit != null)
            {
                // 命中了可交互图元，默认消费事件，阻止视口平移抢夺控制权
                e.Handled = true;
                hit.OnMouseDown(e);
                return e.Handled;
            }
            return false;
        }

        public bool OnMouseMove(ElementMouseEventArgs e)
        {
            if (!EnableInteraction) return false;

            // 若存在被捕获的图元（正在拖拽中），鼠标事件独占派发给该图元，杜绝高速移动下丢帧断脱
            if (_capturedElement != null)
            {
                _capturedElement.OnMouseMove(e);
                return e.Handled;
            }

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
            Element2D target = _capturedElement ?? _pressedElement;
            _capturedElement = null; // 释放鼠标捕获

            if (target != null)
            {
                target.OnMouseUp(e);
                handled = e.Handled;

                if (target == _hoveredElement || HitTest(e.WorldLocation) == target)
                {
                    if (e.Clicks >= 2)
                        target.OnDoubleClick(e);
                    else
                        target.OnClick(e);
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
