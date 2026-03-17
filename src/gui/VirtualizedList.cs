using System;
using System.Collections.Generic;
using UnityEngine;

namespace CheatMenu;

/// <summary>
/// Generic virtualized list that calculates visible item ranges for efficient rendering.
/// Only renders items that are visible in the scroll window, enabling smooth performance
/// with large lists (300+ entries).
/// </summary>
/// <typeparam name="T">Type of items in the list</typeparam>
public class VirtualizedList<T>
{
    /// <summary>
    /// The list of items to virtualize
    /// </summary>
    public List<T> Items { get; set; }
    
    /// <summary>
    /// Height of each item in the list
    /// </summary>
    public float ItemHeight { get; set; }
    
    /// <summary>
    /// Height of the viewport/visible area
    /// </summary>
    public float ViewportHeight { get; set; }
    
    /// <summary>
    /// Current scroll position (Y offset)
    /// </summary>
    public float ScrollPosition { get; set; }
    
    /// <summary>
    /// Optional: Extra padding at top of each item
    /// </summary>
    public float ItemPadding { get; set; }
    
    /// <summary>
    /// Optional: Extra buffer items to render above/below visible area for smooth scrolling
    /// </summary>
    public int BufferCount { get; set; } = 2;
    
    // Cached calculated values
    private int _firstVisibleIndex;
    private int _lastVisibleIndex;
    private float _totalHeight;
    private bool _isDirty = true;
    
    /// <summary>
    /// Index of the first visible item
    /// </summary>
    public int FirstVisibleIndex => _firstVisibleIndex;
    
    /// <summary>
    /// Index of the last visible item
    /// </summary>
    public int LastVisibleIndex => _lastVisibleIndex;
    
    /// <summary>
    /// Number of visible items
    /// </summary>
    public int VisibleItemCount => Math.Max(0, _lastVisibleIndex - _firstVisibleIndex + 1);
    
    /// <summary>
    /// Total height of all items combined
    /// </summary>
    public float TotalHeight
    {
        get
        {
            if (_isDirty) Recalculate();
            return _totalHeight;
        }
    }
    
    /// <summary>
    /// Creates an empty virtualized list
    /// </summary>
    public VirtualizedList()
    {
        Items = new List<T>();
        ItemHeight = 25f;
        ViewportHeight = 200f;
        ScrollPosition = 0f;
        ItemPadding = 0f;
    }
    
    /// <summary>
    /// Creates a virtualized list with the specified items
    /// </summary>
    public VirtualizedList(List<T> items, float itemHeight, float viewportHeight)
    {
        Items = items ?? new List<T>();
        ItemHeight = itemHeight;
        ViewportHeight = viewportHeight;
        ScrollPosition = 0f;
        ItemPadding = 0f;
    }
    
    /// <summary>
    /// Updates the viewport and scroll position, recalculating visible range
    /// </summary>
    public void Update(float viewportHeight, float scrollPosition)
    {
        ViewportHeight = viewportHeight;
        ScrollPosition = scrollPosition;
        _isDirty = true;
        Recalculate();
    }
    
    /// <summary>
    /// Recalculates the visible range based on current scroll position
    /// </summary>
    private void Recalculate()
    {
        if (Items == null || Items.Count == 0 || ItemHeight <= 0)
        {
            _firstVisibleIndex = 0;
            _lastVisibleIndex = -1;
            _totalHeight = 0;
            _isDirty = false;
            return;
        }
        
        float effectiveItemHeight = ItemHeight + ItemPadding;
        
        // Calculate total height
        _totalHeight = Items.Count * effectiveItemHeight;
        
        // Clamp scroll position to valid range
        float maxScroll = Math.Max(0, _totalHeight - ViewportHeight);
        float clampedScroll = Math.Max(0, Math.Min(ScrollPosition, maxScroll));
        
        // Calculate first visible index (O(1) using division)
        _firstVisibleIndex = (int)(clampedScroll / effectiveItemHeight);
        
        // Add buffer for smooth scrolling
        _firstVisibleIndex = Math.Max(0, _firstVisibleIndex - BufferCount);
        
        // Calculate last visible index
        int itemsVisible = (int)(ViewportHeight / effectiveItemHeight) + 1;
        _lastVisibleIndex = _firstVisibleIndex + itemsVisible + (BufferCount * 2);
        
        // Clamp to bounds
        _lastVisibleIndex = Math.Min(_lastVisibleIndex, Items.Count - 1);
        
        _isDirty = false;
    }
    
    /// <summary>
    /// Gets all items that are currently visible
    /// </summary>
    public IEnumerable<T> GetVisibleItems()
    {
        if (_isDirty) Recalculate();
        
        if (_firstVisibleIndex < 0 || _firstVisibleIndex >= Items.Count)
            yield break;
        
        for (int i = _firstVisibleIndex; i <= _lastVisibleIndex && i < Items.Count; i++)
        {
            yield return Items[i];
        }
    }
    
    /// <summary>
    /// Gets the visible range as a tuple (startIndex, endIndex)
    /// </summary>
    public (int start, int end) GetVisibleRange()
    {
        if (_isDirty) Recalculate();
        return (_firstVisibleIndex, _lastVisibleIndex);
    }
    
    /// <summary>
    /// Gets the Y position for a specific item index
    /// </summary>
    public float GetItemYPosition(int index)
    {
        float effectiveItemHeight = ItemHeight + ItemPadding;
        return index * effectiveItemHeight;
    }
    
    /// <summary>
    /// Gets the rect for a specific item at the given index within the viewport
    /// </summary>
    public Rect GetItemRect(int index, float viewportWidth)
    {
        float effectiveItemHeight = ItemHeight + ItemPadding;
        return new Rect(0, GetItemYPosition(index) - ScrollPosition, viewportWidth, ItemHeight);
    }
    
    /// <summary>
    /// Scrolls to make the item at the specified index visible
    /// </summary>
    public float ScrollTo(int index)
    {
        if (Items == null || index < 0 || index >= Items.Count)
            return ScrollPosition;
        
        float effectiveItemHeight = ItemHeight + ItemPadding;
        float targetScroll = index * effectiveItemHeight - (ViewportHeight / 2) + (effectiveItemHeight / 2);
        
        // Clamp to valid range
        float maxScroll = Math.Max(0, _totalHeight - ViewportHeight);
        ScrollPosition = Math.Max(0, Math.Min(targetScroll, maxScroll));
        
        _isDirty = true;
        return ScrollPosition;
    }
    
    /// <summary>
    /// Forces recalculation on next access
    /// </summary>
    public void Invalidate()
    {
        _isDirty = true;
    }
}

/// <summary>
/// Static utility class for calculating visible range without needing an instance
/// </summary>
public static class VirtualizedListHelper
{
    /// <summary>
    /// Calculates the visible range of items for a virtualized list
    /// </summary>
    /// <param name="scrollY">Current scroll position</param>
    /// <param name="viewportHeight">Height of the visible area</param>
    /// <param name="itemHeight">Height of each item</param>
    /// <param name="totalItems">Total number of items</param>
    /// <param name="bufferCount">Number of buffer items to render above/below</param>
    /// <returns>Tuple of (startIndex, endIndex)</returns>
    public static (int start, int end) CalculateVisibleRange(
        float scrollY, 
        float viewportHeight, 
        float itemHeight, 
        int totalItems,
        int bufferCount = 2)
    {
        if (totalItems == 0 || itemHeight <= 0)
            return (0, -1);
        
        // Calculate first visible index
        int firstVisible = (int)(scrollY / itemHeight);
        firstVisible = Math.Max(0, firstVisible - bufferCount);
        
        // Calculate last visible index
        int itemsVisible = (int)(viewportHeight / itemHeight) + 1;
        int lastVisible = firstVisible + itemsVisible + (bufferCount * 2);
        lastVisible = Math.Min(lastVisible, totalItems - 1);
        
        return (firstVisible, lastVisible);
    }
    
    /// <summary>
    /// Calculates the total height of a list
    /// </summary>
    public static float CalculateTotalHeight(int itemCount, float itemHeight, float padding = 0)
    {
        if (itemCount == 0 || itemHeight <= 0)
            return 0;
        return itemCount * (itemHeight + padding);
    }
}

/// <summary>
/// Renders virtualized lists for Unity IMGUI with selection support.
/// Handles scrollbar positioning and smooth scrolling.
/// </summary>
public class VirtualizedListDrawer
{
    /// <summary>
    /// Default item height for list items
    /// </summary>
    public const float DefaultItemHeight = 25f;
    
    /// <summary>
    /// Default spacing between items
    /// </summary>
    public const float DefaultItemSpacing = 2f;
    
    private Vector2 _scrollPosition;
    private float _totalContentHeight;
    private Rect _viewportRect;
    private int _lastItemCount;
    private float _lastItemHeight;
    
    /// <summary>
    /// Current scroll position
    /// </summary>
    public Vector2 ScrollPosition => _scrollPosition;
    
    /// <summary>
    /// Creates a new VirtualizedListDrawer
    /// </summary>
    public VirtualizedListDrawer()
    {
        _scrollPosition = Vector2.zero;
        _totalContentHeight = 0;
        _viewportRect = new Rect(0, 0, 0, 0);
    }
    
    /// <summary>
    /// Renders a virtualized list with custom item rendering
    /// </summary>
    /// <param name="position">Position and size of the list area</param>
    /// <param name="items">List of items to render</param>
    /// <param name="itemHeight">Height of each item</param>
    /// <param name="itemRenderer">Action to render each item (index, rect)</param>
    /// <returns>Updated scroll position</returns>
    public Vector2 Render(
        Rect position, 
        List<string> items, 
        float itemHeight,
        Action<int, Rect> itemRenderer)
    {
        return Render(position, items, itemHeight, 0f, itemRenderer);
    }
    
    /// <summary>
    /// Renders a virtualized list with custom item rendering
    /// </summary>
    /// <param name="position">Position and size of the list area</param>
    /// <param name="items">List of items to render</param>
    /// <param name="itemHeight">Height of each item</param>
    /// <param name="itemSpacing">Spacing between items</param>
    /// <param name="itemRenderer">Action to render each item (index, rect)</param>
    /// <returns>Updated scroll position</returns>
    public Vector2 Render(
        Rect position, 
        List<string> items, 
        float itemHeight,
        float itemSpacing,
        Action<int, Rect> itemRenderer)
    {
        if (items == null || items.Count == 0)
            return _scrollPosition;
        
        // Invalidate cache if item count or height changed
        float effectiveItemHeight = itemHeight + itemSpacing;
        if (_lastItemCount != items.Count || _lastItemHeight != effectiveItemHeight)
        {
            _totalContentHeight = items.Count * effectiveItemHeight;
            _lastItemCount = items.Count;
            _lastItemHeight = effectiveItemHeight;
        }
        
        // Calculate viewport
        float contentWidth = position.width - 20; // Account for scrollbar
        
        // Begin scroll view
        _scrollPosition = GUI.BeginScrollView(
            position,
            _scrollPosition,
            new Rect(0, 0, contentWidth, _totalContentHeight),
            false,
            true
        );
        
        // Calculate visible range
        var (startIndex, endIndex) = CalculateVisibleRange(
            _scrollPosition.y,
            position.height,
            effectiveItemHeight,
            items.Count
        );
        
        // Render visible items
        for (int i = startIndex; i <= endIndex && i < items.Count; i++)
        {
            float yPos = i * effectiveItemHeight - _scrollPosition.y;
            Rect itemRect = new Rect(0, yPos, contentWidth, itemHeight);
            itemRenderer(i, itemRect);
        }
        
        GUI.EndScrollView();
        
        return _scrollPosition;
    }
    
    /// <summary>
    /// Renders a virtualized selectable list
    /// </summary>
    /// <param name="position">Position and size of the list area</param>
    /// <param name="items">List of items to render</param>
    /// <param name="selectedIndex">Currently selected index</param>
    /// <param name="itemHeight">Height of each item</param>
    /// <param name="itemRenderer">Action to render each item (index, rect, isSelected)</param>
    /// <returns>Index of the newly selected item (-1 if none)</returns>
    public int RenderSelectable(
        Rect position,
        List<string> items,
        int selectedIndex,
        float itemHeight,
        Action<int, Rect, bool> itemRenderer)
    {
        return RenderSelectable(position, items, selectedIndex, itemHeight, 0f, itemRenderer);
    }
    
    /// <summary>
    /// Renders a virtualized selectable list
    /// </summary>
    /// <param name="position">Position and size of the list area</param>
    /// <param name="items">List of items to render</param>
    /// <param name="selectedIndex">Currently selected index</param>
    /// <param name="itemHeight">Height of each item</param>
    /// <param name="itemSpacing">Spacing between items</param>
    /// <param name="itemRenderer">Action to render each item (index, rect, isSelected)</param>
    /// <returns>Index of the newly selected item (-1 if none)</returns>
    public int RenderSelectable(
        Rect position,
        List<string> items,
        int selectedIndex,
        float itemHeight,
        float itemSpacing,
        Action<int, Rect, bool> itemRenderer)
    {
        if (items == null || items.Count == 0)
            return -1;
        
        int newSelectedIndex = selectedIndex;
        
        // Invalidate cache if item count or height changed
        float effectiveItemHeight = itemHeight + itemSpacing;
        if (_lastItemCount != items.Count || _lastItemHeight != effectiveItemHeight)
        {
            _totalContentHeight = items.Count * effectiveItemHeight;
            _lastItemCount = items.Count;
            _lastItemHeight = effectiveItemHeight;
        }
        
        // Calculate viewport
        float contentWidth = position.width - 20; // Account for scrollbar
        
        // Begin scroll view
        _scrollPosition = GUI.BeginScrollView(
            position,
            _scrollPosition,
            new Rect(0, 0, contentWidth, _totalContentHeight),
            false,
            true
        );
        
        // Calculate visible range
        var (startIndex, endIndex) = CalculateVisibleRange(
            _scrollPosition.y,
            position.height,
            effectiveItemHeight,
            items.Count
        );
        
        // Render visible items
        for (int i = startIndex; i <= endIndex && i < items.Count; i++)
        {
            float yPos = i * effectiveItemHeight - _scrollPosition.y;
            Rect itemRect = new Rect(0, yPos, contentWidth, itemHeight);
            
            // Check for click
            if (Event.current.type == EventType.MouseDown && 
                itemRect.Contains(Event.current.mousePosition))
            {
                newSelectedIndex = i;
            }
            
            itemRenderer(i, itemRect, i == selectedIndex);
        }
        
        GUI.EndScrollView();
        
        return newSelectedIndex;
    }
    
    /// <summary>
    /// Calculates the visible range of items
    /// </summary>
    public static (int start, int end) CalculateVisibleRange(
        float scrollY, 
        float viewportHeight, 
        float itemHeight, 
        int totalItems)
    {
        return VirtualizedListHelper.CalculateVisibleRange(scrollY, viewportHeight, itemHeight, totalItems);
    }
    
    /// <summary>
    /// Scrolls to make the specified index visible
    /// </summary>
    public void ScrollTo(int index, float itemHeight, int totalItems)
    {
        float effectiveItemHeight = itemHeight + DefaultItemSpacing;
        float targetY = index * effectiveItemHeight - (_viewportRect.height / 2) + (effectiveItemHeight / 2);
        float maxScroll = Math.Max(0, (totalItems * effectiveItemHeight) - _viewportRect.height);
        _scrollPosition.y = Math.Max(0, Math.Min(targetY, maxScroll));
    }
    
    /// <summary>
    /// Resets the scroll position to top
    /// </summary>
    public void ResetScroll()
    {
        _scrollPosition = Vector2.zero;
    }
    
    /// <summary>
    /// Sets the scroll position directly
    /// </summary>
    public void SetScrollPosition(Vector2 position)
    {
        _scrollPosition = position;
    }
    
    /// <summary>
    /// Gets the current scroll Y position
    /// </summary>
    public float GetScrollY()
    {
        return _scrollPosition.y;
    }
}

/// <summary>
/// Simplified virtualized list for string items with built-in button rendering
/// </summary>
public class SimpleVirtualizedList
{
    private readonly VirtualizedListDrawer _drawer;
    private readonly List<string> _items;
    private float _itemHeight;
    private float _itemSpacing;
    private int _selectedIndex;
    
    public Vector2 ScrollPosition => _drawer.ScrollPosition;
    public int SelectedIndex => _selectedIndex;
    
    public SimpleVirtualizedList(float itemHeight = VirtualizedListDrawer.DefaultItemHeight, float itemSpacing = VirtualizedListDrawer.DefaultItemSpacing)
    {
        _drawer = new VirtualizedListDrawer();
        _items = new List<string>();
        _itemHeight = itemHeight;
        _itemSpacing = itemSpacing;
        _selectedIndex = -1;
    }
    
    public void SetItems(List<string> items)
    {
        _items.Clear();
        if (items != null)
            _items.AddRange(items);
    }
    
    /// <summary>
    /// Renders the list with default button styling
    /// </summary>
    public int Render(Rect position, GUIStyle buttonStyle)
    {
        return Render(position, buttonStyle, null);
    }
    
    /// <summary>
    /// Renders the list with default button styling and selection support
    /// </summary>
    public int Render(Rect position, GUIStyle buttonStyle, GUIStyle selectedStyle)
    {
        int newSelected = _selectedIndex;
        
        newSelected = _drawer.RenderSelectable(
            position,
            _items,
            _selectedIndex,
            _itemHeight,
            _itemSpacing,
            (index, rect, isSelected) =>
            {
                GUIStyle style = isSelected && selectedStyle != null ? selectedStyle : buttonStyle;
                if (GUI.Button(rect, _items[index], style))
                {
                    newSelected = index;
                }
            }
        );
        
        _selectedIndex = newSelected;
        return _selectedIndex;
    }
    
    /// <summary>
    /// Renders the list with custom rendering delegate
    /// </summary>
    public Vector2 RenderCustom(Rect position, Action<int, Rect, bool> itemRenderer)
    {
        return RenderCustom(position, _selectedIndex, itemRenderer, out int newSelected);
    }
    
    /// <summary>
    /// Renders the list with custom rendering delegate and selection
    /// </summary>
    public Vector2 RenderCustom(Rect position, int selectedIndex, Action<int, Rect, bool> itemRenderer, out int newSelectedIndex)
    {
        newSelectedIndex = _drawer.RenderSelectable(
            position,
            _items,
            selectedIndex,
            _itemHeight,
            _itemSpacing,
            itemRenderer
        );
        
        _selectedIndex = newSelectedIndex;
        return _drawer.ScrollPosition;
    }
    
    /// <summary>
    /// Scrolls to make the specified index visible
    /// </summary>
    public void ScrollToIndex(int index)
    {
        _drawer.ScrollTo(index, _itemHeight, _items.Count);
    }
    
    /// <summary>
    /// Resets scroll position to top
    /// </summary>
    public void ResetScroll()
    {
        _drawer.ResetScroll();
    }
    
    /// <summary>
    /// Sets the selected index
    /// </summary>
    public void SetSelectedIndex(int index)
    {
        _selectedIndex = Math.Max(-1, Math.Min(index, _items.Count - 1));
    }
}
