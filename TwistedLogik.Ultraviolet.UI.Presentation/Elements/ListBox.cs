﻿using System;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Elements
{
    /// <summary>
    /// Represents a list of selectable items.
    /// </summary>
    [UIElement("ListBox", "TwistedLogik.Ultraviolet.UI.Presentation.Elements.Templates.ListBox.xml")]
    public class ListBox : ItemsControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="id">The unique identifier of this element within its layout.</param>
        public ListBox(UltravioletContext uv, String id)
            : base(uv, id)
        {

        }

        /// <inheritdoc/>
        protected override UIElement CreateItemContainer()
        {
            return new ListBoxItem(Ultraviolet, null);
        }

        /// <inheritdoc/>
        protected override Boolean IsItemContainer(UIElement element)
        {
            return element is ListBoxItem;
        }

        /// <inheritdoc/>
        protected override Boolean IsItemContainerForItem(UIElement container, Object item)
        {
            var lbi = container as ListBoxItem;
            if (lbi == null)
                return false;

            return lbi.Content == item;
        }

        /// <inheritdoc/>
        protected override void AssociateItemContainerWithItem(UIElement container, Object item)
        {
            var lbi = container as ListBoxItem;
            if (lbi == null)
                return;

            lbi.Content = item;
        }
    }
}
