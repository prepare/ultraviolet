﻿using System;
using TwistedLogik.Nucleus;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Elements
{
    /// <summary>
    /// Represents a grid of columns and columns which can contain child elements in each cell.
    /// </summary>
    [UIElement("Grid", "TwistedLogik.Ultraviolet.UI.Presentation.Elements.Templates.Grid.xml")]
    public partial class Grid : Panel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="id">The element's unique identifier within its view.</param>
        public Grid(UltravioletContext uv, String id)
            : base(uv, id)
        {
            this.rowDefinitions    = new RowDefinitionCollection(this);
            this.columnDefinitions = new ColumnDefinitionCollection(this);
        }

        /// <summary>
        /// Sets a value that indicates which row a specified content element should occupy within its <see cref="Grid"/> container.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="row">The index of the row that the element should occupy.</param>
        public static void SetRow(UIElement element, Int32 row)
        {
            Contract.Require(element, "element");

            element.SetValue<Int32>(RowProperty, row);
        }

        /// <summary>
        /// Sets a value that indicates which column a specified content element should occupy within its <see cref="Grid"/> container.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="column">The index of the column that the element should occupy.</param>
        public static void SetColumn(UIElement element, Int32 column)
        {
            Contract.Require(element, "element");

            element.SetValue<Int32>(ColumnProperty, column);
        }

        /// <summary>
        /// Sets a value that indicates the total number of rows that the specified element spans within its parent grid.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="rowSpan">The total number of rows that the specified element spans within its parent grid.</param>
        public static void SetRowSpan(UIElement element, Int32 rowSpan)
        {
            Contract.Require(element, "element");

            element.SetValue<Int32>(RowSpanProperty, rowSpan);
        }

        /// <summary>
        /// Sets a value that indicates the total number of columns that the specified element spans within its parent grid.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="columnSpan">The total number of columns that the specified element spans within its parent grid.</param>
        public static void SetColumnSpan(UIElement element, Int32 columnSpan)
        {
            Contract.Require(element, "element");

            element.SetValue<Int32>(ColumnSpanProperty, columnSpan);
        }

        /// <summary>
        /// Gets a value that indicates which row a specified content element should occupy within its <see cref="Grid"/> container.
        /// </summary>
        /// <param name="element">The element to evaluate.</param>
        /// <returns>The index of the row that the element should occupy.</returns>
        public static Int32 GetRow(UIElement element)
        {
            Contract.Require(element, "element");

            return element.GetValue<Int32>(RowProperty);
        }

        /// <summary>
        /// Gets a value that indicates which column a specified content element should occupy within its <see cref="Grid"/> container.
        /// </summary>
        /// <param name="element">The element to evaluate.</param>
        /// <returns>The index of the column that the element should occupy.</returns>
        public static Int32 GetColumn(UIElement element)
        {
            Contract.Require(element, "element");

            return element.GetValue<Int32>(ColumnProperty);
        }

        /// <summary>
        /// Gets a value that indicates the total number of rows that the specified element spans within its parent grid.
        /// </summary>
        /// <param name="element">The element to evaluate.</param>
        /// <returns>The total number of rows that the specified element spans within its parent grid.</returns>
        public static Int32 GetRowSpan(UIElement element)
        {
            Contract.Require(element, "element");

            return element.GetValue<Int32>(RowSpanProperty);
        }

        /// <summary>
        /// Gets a value that indicates the total number of columns that the specified element spans within its parent grid.
        /// </summary>
        /// <param name="element">The element to evaluate.</param>
        /// <returns>The total number of columns that the specified element spans within its parent grid.</returns>
        public static Int32 GetColumnSpan(UIElement element)
        {
            Contract.Require(element, "element");

            return element.GetValue<Int32>(ColumnSpanProperty);
        }

        /// <summary>
        /// Gets the grid's collection of column definitions.
        /// </summary>
        public RowDefinitionCollection RowDefinitions
        {
            get { return rowDefinitions; }
        }

        /// <summary>
        /// Gets the grid's collection of column definitions.
        /// </summary>
        public ColumnDefinitionCollection ColumnDefinitions
        {
            get { return columnDefinitions; }
        }

        /// <summary>
        /// Identifies the Row attached property.
        /// </summary>
        public static readonly DependencyProperty RowProperty = DependencyProperty.Register("Row", typeof(Int32), typeof(Grid),
            new DependencyPropertyMetadata(HandleRowChanged, () => 0, DependencyPropertyOptions.AffectsMeasure));

        /// <summary>
        /// Identifies the Column attached property.
        /// </summary>
        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register("Column", typeof(Int32), typeof(Grid),
            new DependencyPropertyMetadata(HandleColumnChanged, () => 0, DependencyPropertyOptions.AffectsMeasure));

        /// <summary>
        /// Identifies the <see cref="RowSpan"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RowSpanProperty = DependencyProperty.Register("RowSpan", typeof(Int32), typeof(Grid),
            new DependencyPropertyMetadata(null, () => 1, DependencyPropertyOptions.None));

        /// <summary>
        /// Identifies the <see cref="ColumnSpan"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.Register("ColumnSpan", typeof(Int32), typeof(Grid),
            new DependencyPropertyMetadata(null, () => 1, DependencyPropertyOptions.None));

        /// <summary>
        /// Occurs when the grid's column definitions are modified.
        /// </summary>
        protected internal virtual void OnColumnsModified()
        {
            InvalidateMeasure();
        }

        /// <summary>
        /// Occurs when the grid's column definitions are modified.
        /// </summary>
        protected internal virtual void OnRowsModified()
        {
            InvalidateMeasure();
        }

        /// <inheritdoc/>
        protected override void DrawChildren(UltravioletTime time, DrawingContext dc)
        {
            /* NOTE:
             * By pre-emptively setting the clip region in the Grid itself, we can prevent
             * multiple batch flushes when drawing consecutive children within the same clip region.
             * This is because DrawingContext evaluates the current clip state and only performs
             * a flush if something has changed. */

            var clip = (RectangleD?)null;

            foreach (var child in Children)
            {
                if (child.ClipRectangle != clip)
                {
                    if (clip.HasValue)
                    {
                        clip = null;
                        dc.PopClipRectangle();
                    }

                    if (child.ClipRectangle.HasValue)
                    {
                        clip = child.ClipRectangle;
                        dc.PushClipRectangle(clip.Value);
                    }
                }

                child.Draw(time, dc);
            }

            if (clip.HasValue)
                dc.PopClipRectangle();

            base.DrawChildren(time, dc);
        }

        /// <inheritdoc/>
        protected override Size2D MeasureContent(Size2D availableSize)
        {
            foreach (var child in Children)
                child.Measure(availableSize);

            var desiredWidth  = MeasureWidth(availableSize.Width, true);
            var desiredHeight = MeasureHeight(availableSize.Height, true);

            var contentSize = new Size2D(desiredWidth, desiredHeight);
            return contentSize;
        }

        /// <inheritdoc/>
        protected override Size2D ArrangeContent(Size2D finalSize, ArrangeOptions options)
        {
            MeasureWidth(RenderContentRegion.Width);
            MeasureHeight(RenderContentRegion.Height);

            var grx = 0.0;
            var gry = 0.0;

            for (var col = 0; col < ColumnDefinitions.Count; col++)
            {
                ColumnDefinitions[col].OffsetX = grx;
                grx += ColumnDefinitions[col].MeasuredWidth;
            }

            for (var row = 0; row < RowDefinitions.Count; row++)
            {
                RowDefinitions[row].OffsetY = gry;
                gry += RowDefinitions[row].MeasuredHeight;
            }

            UpdateCellMetadata(finalSize);

            foreach (var cell in cells)
            {
                foreach (var child in cell.Elements)
                {
                    var col = GetColumn(child);
                    var row = GetRow(child);
                    var colSpan = GetColumnSpan(child);
                    var rowSpan = GetRowSpan(child);

                    var childRect = GetLayoutRegion(col, row, colSpan, rowSpan);

                    child.Arrange(childRect);
                }
            }

            return finalSize;
        }

        /// <inheritdoc/>
        protected override void PositionContent(Point2D position)
        {
            foreach (var child in Children)
                child.Position(position);

            base.PositionContent(position);
        }

        /// <inheritdoc/>
        protected override UIElement GetChildAtPoint(Double x, Double y, Boolean isHitTest)
        {
            var col  = GetColumnAtPoint(x, y);
            var row  = GetRowAtPoint(x, y);
            var cell = cells[(row * ColumnCount) + col];

            for (int i = cell.Elements.Count - 1; i >= 0; i--)
            {
                var child = cell.Elements[i];
                var childRelX = x - child.RelativeBounds.X;
                var childRelY = y - child.RelativeBounds.Y;

                var childMatch = child.GetElementAtPoint(childRelX, childRelY, isHitTest);
                if (childMatch != null)
                {
                    return childMatch;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the number of columns in the grid, including any implicit columns.
        /// </summary>
        protected Int32 RowCount
        {
            get { return rowDefinitions.Count == 0 ? 1 : rowDefinitions.Count; }
        }

        /// <summary>
        /// Gets the number of columns in the grid, including any implicit columns.
        /// </summary>
        protected Int32 ColumnCount
        {
            get { return columnDefinitions.Count == 0 ? 1 : columnDefinitions.Count; }
        }

        /// <summary>
        /// Occurs when the value of the Row attached property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleRowChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;

            var grid = element.Parent as Grid;
            if (grid != null)
                grid.InvalidateMeasure();
        }

        /// <summary>
        /// Occurs when the value of the Column attached property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleColumnChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;

            var grid = element.Parent as Grid;
            if (grid != null)
                grid.InvalidateMeasure();
        }

        /// <summary>
        /// Gets the index of the row at the specified column in element space.
        /// </summary>
        /// <param name="x">The x-coordinate in element space to evaluate.</param>
        /// <param name="y">The y-coordinate in element space to evaluate.</param>
        /// <returns>The index of the column at the specified point in element space.</returns>
        private Int32 GetColumnAtPoint(Double x, Double y)
        {
            var position = 0.0;

            x -= RenderContentRegion.X;
            y -= RenderContentRegion.Y;

            for (int i = 0; i < ColumnCount; i++)
            {
                var width = (i >= ColumnDefinitions.Count) ? RenderContentRegion.Width : ColumnDefinitions[i].MeasuredWidth;
                if (x >= position && x < position + width)
                {
                    return i;
                }
                position += width;
            }

            return 0;
        }

        /// <summary>
        /// Gets the index of the row at the specified point in element space.
        /// </summary>
        /// <param name="x">The x-coordinate in element space to evaluate.</param>
        /// <param name="y">The y-coordinate in element space to evaluate.</param>
        /// <returns>The index of the row at the specified point in element space.</returns>
        private Int32 GetRowAtPoint(Double x, Double y)
        {
            var position = 0.0;

            x -= RenderContentRegion.X;
            y -= RenderContentRegion.Y;

            for (int i = 0; i < RowCount; i++)
            {
                var height = (i >= RowDefinitions.Count) ? RenderContentRegion.Height : RowDefinitions[i].MeasuredHeight;
                if (y >= position && y < position + height)
                {
                    return i;
                }
                position += height;
            }

            return 0;
        }

        /// <summary>
        /// Measures the combined width of the grid's columns.
        /// </summary>
        /// <param name="available">The amount of available space for columns.</param>
        /// <param name="treatStarAsAuto">A value indicating whether to treat star (*) columns as if they were Auto columns.</param>
        /// <returns>The desired width for the grid's columns.</returns>
        private Double MeasureWidth(Double available, Boolean treatStarAsAuto = false)
        {
            if (ColumnDefinitions.Count == 0)
                return treatStarAsAuto ? MeasureAutoColumn(available, null, 0) : available;

            var proportionalFactor = 0.0;

            available = MeasureStaticColumns(available);
            available = MeasureAutoColumns(available, treatStarAsAuto, out proportionalFactor);

            if (!treatStarAsAuto)
            {
                var proportionalUnit = (proportionalFactor == 0) ? 0 : available / proportionalFactor;
                MeasureProportionalColumns(proportionalUnit);
            }

            var desired = 0.0;

            foreach (var column in ColumnDefinitions)
                desired += column.MeasuredWidth;

            return desired;
        }

        /// <summary>
        /// Measures the width of the grid's static columns.
        /// </summary>
        /// <param name="available">The amount of remaining space available for columns.</param>
        /// <returns>The amount of space remaining after accounting for static columns.</returns>
        private Double MeasureStaticColumns(Double available)
        {
            for (int i = 0; i < ColumnDefinitions.Count; i++)
            {
                var column = ColumnDefinitions[i];
                if (column.Width.GridUnitType != GridUnitType.Pixel)
                    continue;

                available -= MeasureStaticColumn(column, i);
            }

            return Math.Max(0, available);
        }

        /// <summary>
        /// Calculates the size of an statically-sized column.
        /// </summary>
        /// <param name="column">The column for which to calculate a size.</param>
        /// <param name="ix">The index of the column within the grid.</param>
        /// <returns>The actual size of the column in pixels.</returns>
        private Double MeasureStaticColumn(ColumnDefinition column, Int32 ix)
        {
            Double lower, upper;
            LayoutUtil.GetBoundedMeasure(column.Width.Value, column.MinWidth, column.MaxWidth, out lower, out upper);
            column.MeasuredWidth = lower;
            return lower;
        }

        /// <summary>
        /// Measures the width of the grid's Auto columns.
        /// </summary>
        /// <param name="available">The amount of remaining space available for columns.</param>
        /// <param name="treatStarAsAuto">A value indicating whether to treat Auto columns as if they were proportional columns.</param>
        /// <param name="proportionalFactor">The combined proportional measurement factor for all of the grid's proportional columns.</param>
        /// <returns>The amount of space remaining after accounting for Auto columns.</returns>
        private Double MeasureAutoColumns(Double available, Boolean treatStarAsAuto, out Double proportionalFactor)
        {
            proportionalFactor = 0.0;

            for (int i = 0; i < ColumnDefinitions.Count; i++)
            {
                var column = ColumnDefinitions[i];
                switch (column.Width.GridUnitType)
                {
                    case GridUnitType.Star:
                        if (treatStarAsAuto)
                        {
                            available -= MeasureAutoColumn(available, column, i);
                        }
                        proportionalFactor += column.Width.Value;
                        break;

                    case GridUnitType.Auto:
                        available -= MeasureAutoColumn(available, column, i);
                        break;
                }
            }

            return Math.Max(0, available);
        }

        /// <summary>
        /// Calculates the size of an Auto-sized column.
        /// </summary>
        /// <param name="available">The amount of remaining space available for columns.</param>
        /// <param name="column">The column for which to calculate a size.</param>
        /// <param name="ix">The index of the column within the grid.</param>
        /// <returns>The actual size of the column in pixels.</returns>
        private Double MeasureAutoColumn(Double available, ColumnDefinition column, Int32 ix)
        {
            var size = 0.0;

            foreach (var child in Children)
            {
                if (!LayoutUtil.IsSpaceFilling(child))
                    continue;

                if (GetColumn(child) != ix)
                    continue;

                var childSize = child.DesiredSize.Width;
                if (childSize > size)
                {
                    size = childSize;
                }
            }

            size = Math.Min(available, size);

            if (column != null)
            {
                column.MeasuredWidth = size;
            }

            return size;
        }

        /// <summary>
        /// Calculates and updates the size of proportional columns.
        /// </summary>
        /// <param name="proportionalUnit">The size of the base unit (corresponding to 1*) for proportional columns, in pixels.</param>
        private void MeasureProportionalColumns(Double proportionalUnit)
        {
            foreach (var column in ColumnDefinitions)
            {
                if (column.Width.GridUnitType != GridUnitType.Star)
                    continue;

                column.MeasuredWidth = column.Width.Value * proportionalUnit;
            }
        }

        /// <summary>
        /// Measures the combined height of the grid's rows.
        /// </summary>
        /// <param name="available">The amount of available space for columns.</param>
        /// <param name="treatStarAsAuto">A value indicating whether to treat star (*) rows as if they were Auto rows.</param>
        /// <returns>The desired height for the grid's columns.</returns>
        private Double MeasureHeight(Double available, Boolean treatStarAsAuto = false)
        {
            if (RowDefinitions.Count == 0)
                return treatStarAsAuto ? MeasureAutoRow(available, null, 0) : available;

            var proportionalFactor = 0.0;

            available = MeasureStaticRows(available);
            available = MeasureAutoRows(treatStarAsAuto, available, out proportionalFactor);

            if (!treatStarAsAuto)
            {
                var proportionalUnit = (proportionalFactor == 0) ? 0 : available / proportionalFactor;
                MeasureProportionalRows(proportionalUnit);
            }

            var desired = 0.0;

            foreach (var column in RowDefinitions)
                desired += column.MeasuredHeight;

            return desired;
        }

        /// <summary>
        /// Measures the width of the grid's static rows.
        /// </summary>
        /// <param name="available">The amount of remaining space available for rows.</param>
        /// <returns>The amount of space remaining after accounting for static rows.</returns>
        private Double MeasureStaticRows(Double available)
        {
            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                var row = RowDefinitions[i];
                if (row.Height.GridUnitType != GridUnitType.Pixel)
                    continue;

                available -= MeasureStaticRow(row, i);
            }

            return Math.Max(0, available);
        }

        /// <summary>
        /// Calculates the size of an statically-sized row.
        /// </summary>
        /// <param name="row">The row for which to calculate a size.</param>
        /// <param name="ix">The index of the row within the grid.</param>
        /// <returns>The measured size of the row.</returns>
        private Double MeasureStaticRow(RowDefinition row, Int32 ix)
        {
            Double lower, upper;
            LayoutUtil.GetBoundedMeasure(row.Height.Value, row.MinHeight, row.MaxHeight, out lower, out upper);
            row.MeasuredHeight = lower;
            return lower;
        }

        /// <summary>
        /// Measures the width of the grid's Auto rows.
        /// </summary>
        /// <param name="available">The amount of remaining space for available rows.</param>
        /// <param name="treatStarAsAuto">A value indicating whether to treat Auto rows as if they were proportional rows.</param>
        /// <param name="proportionalFactor">The combined proportional measurement factor for all of the grid's proportional rows.</param>
        /// <returns>The amount of space remaining after accounting for Auto rows.</returns>
        private Double MeasureAutoRows(Boolean treatStarAsAuto, Double available, out Double proportionalFactor)
        {
            proportionalFactor = 0.0;

            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                var row = RowDefinitions[i];
                switch (row.Height.GridUnitType)
                {
                    case GridUnitType.Star:
                        if (treatStarAsAuto)
                        {
                            available -= MeasureAutoRow(available, row, i);
                        }
                        proportionalFactor += row.Height.Value;
                        break;

                    case GridUnitType.Auto:
                        available -= MeasureAutoRow(available, row, i);
                        break;
                }
            }

            return Math.Max(0, available);
        }

        /// <summary>
        /// Calculates the size of an Auto-sized row.
        /// </summary>
        /// <param name="available">The amount of remaining space for available rows.</param>
        /// <param name="row">The row for which to calculate a size.</param>
        /// <param name="ix">The index of the row within the grid.</param>
        /// <returns>The measured size of the row.</returns>
        private Double MeasureAutoRow(Double available, RowDefinition row, Int32 ix)
        {
            var size = 0.0;

            foreach (var child in Children)
            {
                if (!LayoutUtil.IsSpaceFilling(child))
                    continue;

                if (GetRow(child) != ix)
                    continue;

                var childSize = child.DesiredSize.Height;
                if (childSize > size)
                {
                    size = childSize;
                }
            }

            size = Math.Min(available, size);

            if (row != null)
            {
                row.MeasuredHeight = size;
            }

            return size;
        }

        /// <summary>
        /// Calculates and updates the size of proportional rows.
        /// </summary>
        /// <param name="proportionalUnit">The size of the base unit (corresponding to 1*) for proportional rows, in pixels.</param>
        private void MeasureProportionalRows(Double proportionalUnit)
        {
            foreach (var row in RowDefinitions)
            {
                if (row.Height.GridUnitType != GridUnitType.Star)
                    continue;

                row.MeasuredHeight = row.Height.Value * proportionalUnit;
            }
        }

        /// <summary>
        /// Updates the metadata for each of the grid's cells.
        /// </summary>
        /// <param name="finalSize">The size of the layout region that has been assigned to the grid by its parent.</param>
        private void UpdateCellMetadata(Size2D finalSize)
        {
            ExpandCellMetadataArray();

            for (var row = 0; row < RowCount; row++)
            {
                var rowdef = (row >= RowDefinitions.Count) ? null : RowDefinitions[row];

                for (var col = 0; col < ColumnCount; col++)
                {
                    var coldef = (col >= ColumnDefinitions.Count) ? null : ColumnDefinitions[col];

                    var cell = cells[(row * ColumnCount) + col];

                    cell.OffsetX = (coldef == null) ? 0 : coldef.OffsetX;
                    cell.OffsetY = (rowdef == null) ? 0 : rowdef.OffsetY;
                    cell.Width   = (coldef == null) ? RenderContentRegion.Width : coldef.MeasuredWidth;
                    cell.Height  = (rowdef == null) ? RenderContentRegion.Height : rowdef.MeasuredHeight;

                    cell.Elements.Clear();
                }
            }

            foreach (var child in Children)
            {
                var row = Grid.GetRow(child);
                var col = Grid.GetColumn(child);

                var cell = cells[(row * ColumnCount) + col];
                cell.Elements.Add(child);
            }
        }

        /// <summary>
        /// If necessary, expands the cell metadata array to accomodate all of the grid's rows and columns.
        /// </summary>
        private void ExpandCellMetadataArray()
        {
            if (cells.Length >= RowCount * ColumnCount)
                return;

            cells = new CellMetadata[RowCount * ColumnCount];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new CellMetadata();
            }
        }

        /// <summary>
        /// Gets the layout region for an element with the specified column and row properties.
        /// </summary>
        /// <param name="col">The element's column index.</param>
        /// <param name="row">The element's row index.</param>
        /// <param name="colSpan">The element's column span.</param>
        /// <param name="rowSpan">The element's row span.</param>
        /// <returns>The layout region for an element with the specified column and row properties.</returns>
        private RectangleD GetLayoutRegion(Int32 col, Int32 row, Int32 colSpan, Int32 rowSpan)
        {
            var cell = cells[(row * ColumnCount) + col];

            var x = cell.OffsetX;
            var y = cell.OffsetY;

            var width = 0.0;
            var height = 0.0;

            for (int i = 0; i < colSpan; i++)
            {
                cell = cells[(row * ColumnCount) + col + i];
                width += cell.Width;
            }

            for (int i = 0; i < rowSpan; i++)
            {
                cell = cells[((row + i) * ColumnCount) + col];
                height += cell.Height;
            }

            return new RectangleD(x, y, width, height);
        }

        // Property values.
        private readonly RowDefinitionCollection rowDefinitions;
        private readonly ColumnDefinitionCollection columnDefinitions;

        // Cached cell metadata.
        private CellMetadata[] cells = new[] { new CellMetadata() };
    }
}
