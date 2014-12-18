﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TwistedLogik.Nucleus;
using TwistedLogik.Nucleus.Collections;
using TwistedLogik.Nucleus.Data;
using TwistedLogik.Ultraviolet.Content;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D;
using TwistedLogik.Ultraviolet.Input;
using TwistedLogik.Ultraviolet.UI.Presentation.Animations;
using TwistedLogik.Ultraviolet.UI.Presentation.Styles;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Controls
{
    /// <summary>
    /// Represents the method that is called when a UI element is drawn.
    /// </summary>
    /// <param name="element">The element being drawn.</param>
    /// <param name="time">Time elapsed since the last call to <see cref="UltravioletContext.Draw(UltravioletTime)"/>.</param>
    /// <param name="spriteBatch">The sprite batch with which to draw the view.</param>
    public delegate void UIElementDrawingEventHandler(UIElement element, UltravioletTime time, SpriteBatch spriteBatch);

    /// <summary>
    /// Represents the method that is called when a UI element is updated.
    /// </summary>
    /// <param name="element">The element being updated.</param>
    /// <param name="time">Time elapsed since the last call to <see cref="UltravioletContext.Draw(UltravioletTime)"/>.</param>
    public delegate void UIElementUpdatingEventHandler(UIElement element, UltravioletTime time);

    /// <summary>
    /// Represents the method that is called when the mouse cursor enters or leaves a UI element.
    /// </summary>
    /// <param name="element">The element that raised the event.</param>
    /// <param name="device">The mouse device.</param>
    public delegate void UIElementMouseEventHandler(UIElement element, MouseDevice device);

    /// <summary>
    /// Represents the method that is called when a button is pressed or released while an element is under the mouse.
    /// </summary>
    /// <param name="element">The element that raised the event.</param>
    /// <param name="device">The mouse device.</param>
    /// <param name="button">The mouse button that was pressed or released.</param>
    public delegate void UIElementMouseButtonEventHandler(UIElement element, MouseDevice device, MouseButton button);

    /// <summary>
    /// Represents the method that is called when a UI element raises an event.
    /// </summary>
    /// <param name="element">The element that raised the event.</param>
    public delegate void UIElementEventHandler(UIElement element);

    /// <summary>
    /// The base class for all UI elements.
    /// </summary>
    [UIElement("Element")]
    public abstract class UIElement : DependencyObject
    {
        /// <summary>
        /// Represents a method which sets the value of a styled property on a UI element.
        /// </summary>
        /// <param name="element">The UI element on which to set the style.</param>
        /// <param name="value">The string representation of the value to set for the style.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        internal delegate void StyleSetter(UIElement element, String value, IFormatProvider provider);

        /// <summary>
        /// Initialies the <see cref="UIElement"/> type.
        /// </summary>
        static UIElement()
        {
            miFromString = typeof(ObjectResolver).GetMethod("FromString", new Type[] { typeof(String), typeof(Type), typeof(IFormatProvider) });
            miSetStyledValue = typeof(DependencyObject).GetMethod("SetStyledValue");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UIElement"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="id">The element's unique identifier within its view.</param>
        public UIElement(UltravioletContext uv, String id)
        {
            Contract.Require(uv, "uv");

            this.uv                = uv;
            this.id                = id;
            this.classes           = new UIElementClassCollection(this);
            this.visualStateGroups = new VisualStateGroupCollection(this);
            
            var attr = (UIElementAttribute)GetType().GetCustomAttributes(typeof(UIElementAttribute), false).SingleOrDefault();
            if (attr != null)
            {
                this.name = attr.Name;
            }

            CreateStyleSetters();
        }

        /// <summary>
        /// Calculates the element's recommended size based on its content
        /// and the specified constraints.
        /// </summary>
        /// <param name="width">The element's recommended width.</param>
        /// <param name="height">The element's recommended height.</param>
        public virtual void CalculateRecommendedSize(ref Int32? width, ref Int32? height)
        {

        }

        /// <summary>
        /// Releases resources associated with the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Recursively clears the local values of all of the container's dependency properties
        /// and all of the dependency properties of the container's descendents.
        /// </summary>
        public virtual void ClearLocalValuesRecursive()
        {
            ClearLocalValues();
        }

        /// <summary>
        /// Recursively clears the styled values of all of the container's dependency properties
        /// and all of the dependency properties of the container's descendents.
        /// </summary>
        public virtual void ClearStyledValuesRecursive()
        {
            ClearStyledValues();
            ClearVisualStateTransitions();
        }

        /// <summary>
        /// Resets the element's visual state transitions.
        /// </summary>
        public void ClearVisualStateTransitions()
        {
            visualStateGroups.ClearVisualStateTransitions();
        }

        /// <summary>
        /// Resets the element's visual state transitions.
        /// </summary>
        public virtual void ClearVisualStateTransitionsRecursive()
        {
            ClearVisualStateTransitions();
        }

        /// <summary>
        /// Called when the element should reload its content.
        /// </summary>
        public void ReloadContent()
        {
            ReloadFont();
            ReloadBackgroundImage();

            OnReloadingContent();
        }

        /// <summary>
        /// Called when the element and its children should reload their content.
        /// </summary>
        public virtual void ReloadContentRecursive()
        {
            ReloadContent();
        }

        /// <summary>
        /// Converts a position in screen space to a position in element space.
        /// </summary>
        /// <param name="x">The x-coordinate of the screen space position to convert.</param>
        /// <param name="y">The y-coordinate of the screen space position to convert.</param>
        /// <returns>The converted element space position.</returns>
        public Vector2 ScreenPositionToElementPosition(Int32 x, Int32 y)
        {
            return new Vector2(x - AbsoluteScreenX, y - AbsoluteScreenY);
        }

        /// <summary>
        /// Converts a position in screen space to a position in element space.
        /// </summary>
        /// <param name="position">The screen space position to convert.</param>
        /// <returns>The converted element space position.</returns>
        public Vector2 ScreenPositionToElementPosition(Vector2 position)
        {
            return ScreenPositionToElementPosition((Int32)position.X, (Int32)position.Y);
        }

        /// <summary>
        /// Converts a position in element space to a position in screen space.
        /// </summary>
        /// <param name="x">The x-coordinate of the element space position to convert.</param>
        /// <param name="y">The y-coordinate of the element space position to convert.</param>
        /// <returns>The converted screen space position.</returns>
        public Vector2 ElementPositionToScreenPosition(Int32 x, Int32 y)
        {
            return new Vector2(x + AbsoluteScreenX, y + AbsoluteScreenY);
        }

        /// <summary>
        /// Converts a position in element space to a position in screen space.
        /// </summary>
        /// <param name="position">The element space position to convert.</param>
        /// <returns>The converted screen space position.</returns>
        public Vector2 ElementPositionToScreenPosition(Vector2 position)
        {
            return ElementPositionToScreenPosition((Int32)position.X, (Int32)position.Y);
        }

        /// <summary>
        /// Gets the element at the specified point in element space.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to evaluate.</param>
        /// <param name="y">The y-coordinate of the point to evaluate.</param>
        /// <returns>The element at the specified point in element space, or null if no such element exists.</returns>
        public UIElement GetElementAtPoint(Int32 x, Int32 y)
        {
            return GetElementAtPointInternal(x, y);
        }

        /// <summary>
        /// Gets the element at the specified point in element space.
        /// </summary>
        /// <param name="position">The point to evaluate.</param>
        /// <returns>The element at the specified point in element space, or null if no such element exists.</returns>
        public UIElement GetElementAtPoint(Vector2 position)
        {
            return GetElementAtPointInternal((Int32)position.X, (Int32)position.Y);
        }

        /// <summary>
        /// Gets the Ultraviolet context that created the element.
        /// </summary>
        public UltravioletContext Ultraviolet
        {
            get { return uv; }
        }

        /// <summary>
        /// Gets the element's collection of styling classes.
        /// </summary>
        public UIElementClassCollection Classes
        {
            get { return classes; }
        }

        /// <summary>
        /// Gets the element's collection of visual state groups.
        /// </summary>
        public VisualStateGroupCollection VisualStateGroups
        {
            get { return visualStateGroups; }
        }

        /// <summary>
        /// Gets the element's unique identifier within its view.
        /// </summary>
        public String ID
        {
            get { return id; }
        }

        /// <summary>
        /// Gets the element's name based on its type.
        /// </summary>
        public String Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the <see cref="UIView"/> that is the top-level container for this element.
        /// </summary>
        public UIView View
        {
            get { return view; }
        }

        /// <summary>
        /// Gets the element's view model.
        /// </summary>
        public Object ViewModel
        {
            get { return viewModel; }
        }

        /// <summary>
        /// Gets the <see cref="UIContainer"/> that contains this element.
        /// </summary>
        public UIContainer Container
        {
            get { return container; }
        }

        /// <summary>
        /// Gets the element's bounding box in element space.
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, CalculatedWidth, CalculatedHeight); }
        }

        /// <summary>
        /// Gets the element's bounding box in screen space.
        /// </summary>
        public Rectangle ScreenBounds
        {
            get { return new Rectangle(AbsoluteScreenX, AbsoluteScreenY, CalculatedWidth, CalculatedHeight); }
        }

        /// <summary>
        /// Gets a value indicating whether this is an anonymous element.
        /// </summary>
        /// <remarks>An anonymous element is one which has no explicit identifier.</remarks>
        public Boolean Anonymous
        {
            get { return String.IsNullOrEmpty(id); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the element is enabled.
        /// </summary>
        public Boolean Enabled
        {
            get { return GetValue<Boolean>(dpEnabled); }
            set { SetValue<Boolean>(dpEnabled, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the element is visible.
        /// </summary>
        public Boolean Visible
        {
            get { return GetValue<Boolean>(dpVisible); }
            set { SetValue<Boolean>(dpVisible, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the mouse cursor is hovering over this element.
        /// </summary>
        public Boolean Hovering
        {
            get { return hovering; }
            private set
            {
                if (hovering != value)
                {
                    hovering = value;
                    OnHoveringChanged();
                }
            }
        }

        /// <summary>
        /// Gets the element's width in device independent pixel units (1/96 of an inch).
        /// </summary>
        public Double Width
        {
            get { return GetValue<Double>(dpWidth); }
            set { SetValue<Double>(dpWidth, value); }
        }

        /// <summary>
        /// Gets the element's height in device independent pixel units (1/96 of an inch).
        /// </summary>
        public Double Height
        {
            get { return GetValue<Double>(dpHeight); }
            set { SetValue<Double>(dpHeight, value); }
        }

        /// <summary>
        /// Gets or sets the amount of padding around the element's content.
        /// </summary>
        public Int32 Padding
        {
            get { return GetValue<Int32>(dpPadding); }
            set { SetValue<Int32>(dpPadding, value); }
        }

        /// <summary>
        /// Gets or sets the element's font color.
        /// </summary>
        public Color FontColor
        {
            get { return GetValue<Color>(dpFontColor); }
            set { SetValue<Color>(dpFontColor, value); }
        }

        /// <summary>
        /// Gets the asset identifier of the element's font.
        /// </summary>
        public SourcedVal<AssetID> FontAssetID
        {
            get { return GetValue<SourcedVal<AssetID>>(dpFontAssetID); }
            set { SetValue<SourcedVal<AssetID>>(dpFontAssetID, value); }
        }

        /// <summary>
        /// Gets the element's font.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
        }

        /// <summary>
        /// Gets or sets the element's background color.
        /// </summary>
        public Color BackgroundColor
        {
            get { return GetValue<Color>(dpBackgroundColor); }
            set { SetValue<Color>(dpBackgroundColor, value); }
        }

        /// <summary>
        /// Gets or sets the element's background image.
        /// </summary>
        public SourcedRef<StretchableImage9> BackgroundImage
        {
            get { return GetValue<SourcedRef<StretchableImage9>>(dpBackgroundImage); }
            set { SetValue<SourcedRef<StretchableImage9>>(dpBackgroundImage, value); }
        }

        /// <summary>
        /// Occurs when the element is being drawn.
        /// </summary>
        public event UIElementDrawingEventHandler Drawing;

        /// <summary>
        /// Occurs when the element is being updated.
        /// </summary>
        public event UIElementUpdatingEventHandler Updating;

        /// <summary>
        /// Occurs when the element gains mouse capture.
        /// </summary>
        public event UIElementEventHandler GainedMouseCapture;

        /// <summary>
        /// Occurs when the element loses mouse capture.
        /// </summary>
        public event UIElementEventHandler LostMouseCapture;

        /// <summary>
        /// Occurs when the mouse cursor enters the element.
        /// </summary>
        public event UIElementMouseEventHandler MouseEnter;

        /// <summary>
        /// Occurs when the mouse cursor leaves the element.
        /// </summary>
        public event UIElementMouseEventHandler MouseLeave;

        /// <summary>
        /// Occurs when a mouse button is pressed while the cursor is over the element.
        /// </summary>
        public event UIElementMouseButtonEventHandler MouseButtonPressed;

        /// <summary>
        /// Occurs when a mouse button is released while the cursor is over the element.
        /// </summary>
        public event UIElementMouseButtonEventHandler MouseButtonReleased;

        /// <summary>
        /// Occurs when the value of the <see cref="Enabled"/> property changes.
        /// </summary>
        public event UIElementEventHandler EnabledChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="Visible"/> property changes.
        /// </summary>
        public event UIElementEventHandler VisibleChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="Hovering"/> property changes.
        /// </summary>
        public event UIElementEventHandler HoveringChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="Width"/> property changes.
        /// </summary>
        public event UIElementEventHandler WidthChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="Height"/> property changes.
        /// </summary>
        public event UIElementEventHandler HeightChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="Padding"/> property changes.
        /// </summary>
        public event UIElementEventHandler PaddingChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="FontColor"/> property changes.
        /// </summary>
        public event UIElementEventHandler FontColorChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="FontAssetID"/> property changes.
        /// </summary>
        public event UIElementEventHandler FontAssetIDChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="BackgroundColor"/> property changes.
        /// </summary>
        public event UIElementEventHandler BackgroundColorChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="BackgroundImage"/> property changes.
        /// </summary>
        public event UIElementEventHandler BackgroundImageChanged;

        /// <summary>
        /// Finds a styled dependency property according to its styling name.
        /// </summary>
        /// <param name="name">The styling name of the dependency property to retrieve.</param>
        /// <param name="type">The type to search for a dependency property.</param>
        /// <returns>The <see cref="DependencyProperty"/> instance which matches the specified styling name, or <c>null</c> if no
        /// such dependency property exists on this object.</returns>
        internal static DependencyProperty FindStyledDependencyProperty(String name, Type type)
        {
            Contract.RequireNotEmpty("name", name);
            Contract.Require(type, "type");

            lock (styleSyncObject)
            {
                while (type != null)
                {
                    Dictionary<String, DependencyProperty> styledPropertiesForCurrentType;
                    if (styledProperties.TryGetValue(type, out styledPropertiesForCurrentType))
                    {
                        DependencyProperty dp;
                        if (styledPropertiesForCurrentType.TryGetValue(name, out dp))
                        {
                            return dp;
                        }
                    }

                    type = type.BaseType;
                }
            }
            return null;
        }

        /// <summary>
        /// Searches the object for a dependency property which matches the specified name.
        /// </summary>
        /// <param name="name">The name of the dependency property for which to search.</param>
        /// <returns>The <see cref="DependencyProperty"/> instance which matches the specified name, or <c>null</c> if no
        /// such property exists on this object.</returns>
        internal DependencyProperty FindDependencyPropertyByName(DependencyPropertyName name)
        {
            if (name.IsAttachedProperty)
            {
                if (Container != null && String.Equals(Container.Name, name.Container, StringComparison.OrdinalIgnoreCase))
                {
                    return DependencyProperty.FindByName(name.Name, Container.GetType());
                }
                return null;
            }
            return DependencyProperty.FindByName(name.Name, GetType());
        }

        /// <summary>
        /// Finds a styled dependency property according to its styling name.
        /// </summary>
        /// <param name="name">The styling name of the dependency property to retrieve.</param>
        /// <returns>The <see cref="DependencyProperty"/> instance which matches the specified styling name, or <c>null</c> if no
        /// such dependency property exists on this object.</returns>
        internal DependencyProperty FindStyledDependencyProperty(String name)
        {
            Contract.RequireNotEmpty(name, "name");

            return FindStyledDependencyProperty(name, GetType());
        }

        /// <summary>
        /// Applies a style to the element.
        /// </summary>
        /// <param name="style">The style which is being applied.</param>
        /// <param name="selector">The selector which caused the style to be applied.</param>
        /// <param name="attached">A value indicating whether thie style represents an attached property.</param>
        internal void ApplyStyle(UvssStyle style, UvssSelector selector, Boolean attached)
        {
            Contract.Require(style, "style");
            Contract.Require(selector, "selector");

            var name  = style.Name;
            var value = style.Value.Trim();

            if (name == "transition")
            {
                ApplyStyledVisualStateTransition(style, value);
            }
            else
            {
                var setter = attached ? Container.GetStyleSetter(name, selector.PseudoClass) : GetStyleSetter(name, selector.PseudoClass);
                if (setter == null)
                    return;

                setter(this, value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Applies a visual state transition to the element.
        /// </summary>
        /// <param name="style">The style which defines the state transition.</param>
        /// <param name="value">The transition value.</param>
        internal void ApplyStyledVisualStateTransition(UvssStyle style, String value)
        {
            Contract.Require(style, "style");
            Contract.RequireNotEmpty(value, "value");

            if (View != null && View.Stylesheet != null)
            {
                var storyboard = View.Stylesheet.InstantiateStoryboardByName(Ultraviolet, value);
                if (storyboard != null)
                {
                    var group = default(String);
                    var from  = default(String);
                    var to    = default(String);

                    switch (style.Arguments.Count)
                    {
                        case 2:
                            group = style.Arguments[0];
                            from  = null;
                            to    = style.Arguments[1];
                            break;

                        case 3:
                            group = style.Arguments[0];
                            from  = style.Arguments[1];
                            to    = style.Arguments[2];
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    VisualStateGroups.SetVisualStateTransition(group, from, to, storyboard);
                }
            }
        }

        /// <summary>
        /// Begins playing the specified storyboard on this element.
        /// </summary>
        /// <param name="storyboard">The storyboard to play on this element.</param>
        internal void BeginStoryboard(Storyboard storyboard)
        {
            StoryboardClock existingClock;
            storyboardClocks.TryGetValue(storyboard, out existingClock);

            var clock = RetrieveStoryboardClock(storyboard);
            storyboardClocks[storyboard] = clock;

            ApplyStoryboard(storyboard, clock, this);

            clock.Start();

            if (existingClock != null)
            {
                existingClock.Stop();
                ReleaseStoryboardClock(existingClock);
            }
        }

        /// <summary>
        /// Stops playing the specified storyboard on this element.
        /// </summary>
        /// <param name="storyboard">The storyboard to stop playing on this element.</param>
        internal void StopStoryboard(Storyboard storyboard)
        {
            StoryboardClock clock;
            if (storyboardClocks.TryGetValue(storyboard, out clock))
            {
                clock.Stop();
                storyboardClocks.Remove(storyboard);
                ReleaseStoryboardClock(clock);
            }
        }

        /// <summary>
        /// Applies the specified storyboard to this element.
        /// </summary>
        /// <param name="storyboard">The storyboard being applied to the element.</param>
        /// <param name="clock">The storyboard clock that tracks playback.</param>
        /// <param name="root">The root element to which the storyboard is being applied.</param>
        internal virtual void ApplyStoryboard(Storyboard storyboard, StoryboardClock clock, UIElement root)
        {
            foreach (var target in storyboard.Targets)
            {
                var targetAppliesToElement = false;
                if (target.Selector == null)
                {
                    if (this == root)
                    {
                        targetAppliesToElement = true;
                    }
                }
                else
                {
                    targetAppliesToElement = target.Selector.MatchesElement(this, root);
                }

                if (targetAppliesToElement)
                {
                    foreach (var animation in target.Animations)
                    {
                        var dp = FindDependencyPropertyByName(animation.Key);
                        if (dp != null)
                        {
                            Animate(dp, animation.Value, clock);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the element.
        /// </summary>
        /// <param name="time">Time elapsed since the last call to <see cref="UltravioletContext.Draw(UltravioletTime)"/>.</param>
        /// <param name="spriteBatch">The sprite batch with which to draw the view.</param>
        internal virtual void Draw(UltravioletTime time, SpriteBatch spriteBatch)
        {
            OnDrawing(time, spriteBatch);
        }

        /// <summary>
        /// Updates the element's state.
        /// </summary>
        /// <param name="time">Time elapsed since the last call to <see cref="UltravioletContext.Draw(UltravioletTime)"/>.</param>
        internal virtual void Update(UltravioletTime time)
        {
            foreach (var clock in storyboardClocks)
                clock.Value.Update(time);

            Digest(time);
            OnUpdating(time);
        }

        /// <summary>
        /// Updates the view model associated with this element.
        /// </summary>
        /// <param name="viewModel">The view model to associate with this element.</param>
        internal virtual void UpdateViewModel(Object viewModel)
        {
            this.viewModel = viewModel;
        }

        /// <summary>
        /// Updates the view associated with this element.
        /// </summary>
        /// <param name="view">The view to associate with this element.</param>
        internal virtual void UpdateView(UIView view)
        {
            if (this.view != null)
                this.view.UnregisterElementID(this);

            this.view = view;

            if (this.view != null)
                this.view.RegisterElementID(this);

            if (view == null || view.Stylesheet == null)
            {
                ClearStyledValues();
                ClearVisualStateTransitions();
            }
            else
            {
                view.Stylesheet.ApplyStylesRecursively(this);
            }

            UpdateViewModel(view == null ? null : view.ViewModel);

            ReloadContent();
        }

        /// <summary>
        /// Updates the container which holds this element.
        /// </summary>
        /// <param name="container">The container to associate with this element.</param>
        internal virtual void UpdateContainer(UIContainer container)
        {
            this.container = container;

            var view = (container == null) ? null : container.View;
            if (view != this.view)
            {
                UpdateView(view);
            }
        }

        /// <summary>
        /// Gets the element at the specified point in element space.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to evaluate.</param>
        /// <param name="y">The y-coordinate of the point to evaluate.</param>
        /// <returns>The element at the specified point in element space, or null if no such element exists.</returns>
        internal virtual UIElement GetElementAtPointInternal(Int32 x, Int32 y)
        {
            return Bounds.Contains(x, y) ? this : null;
        }

        /// <summary>
        /// Gets the x-coordinate of the element's absolute screen position.
        /// </summary>
        internal virtual Int32 AbsoluteScreenXInternal
        {
            get { return (Container == null ? 0 : Container.AbsoluteScreenX) + containerRelativeX; }
        }

        /// <summary>
        /// Gets the y-coordinate of the element's absolute screen position.
        /// </summary>
        internal virtual Int32 AbsoluteScreenYInternal
        {
            get { return (Container == null ? 0 : Container.AbsoluteScreenY) + containerRelativeY; }
        }

        /// <summary>
        /// Raises the <see cref="GainedMouseCapture"/> event.
        /// </summary>
        protected internal virtual void OnGainedMouseCapture()
        {
            var temp = GainedMouseCapture;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="LostMouseCapture"/> event.
        /// </summary>
        protected internal virtual void OnLostMouseCapture()
        {
            var temp = LostMouseCapture;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseEnter"/> event.
        /// </summary>
        /// <param name="device">The mouse device.</param>
        protected internal virtual void OnMouseEnter(MouseDevice device)
        {
            Hovering = true;

            var temp = MouseEnter;
            if (temp != null)
            {
                temp(this, device);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseLeave"/> event.
        /// </summary>
        /// <param name="device">The mouse device.</param>
        protected internal virtual void OnMouseLeave(MouseDevice device)
        {
            Hovering = false;

            var temp = MouseLeave;
            if (temp != null)
            {
                temp(this, device);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseButtonPressed"/> event.
        /// </summary>
        /// <param name="device">The mouse device.</param>
        /// <param name="button">The mouse button that was pressed or released.</param>
        protected internal virtual void OnMouseButtonPressed(MouseDevice device, MouseButton button)
        {
            var temp = MouseButtonPressed;
            if (temp != null)
            {
                temp(this, device, button);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseButtonReleased"/> event.
        /// </summary>
        /// <param name="device">The mouse device.</param>
        /// <param name="button">The mouse button that was pressed or released.</param>
        protected internal virtual void OnMouseButtonReleased(MouseDevice device, MouseButton button)
        {
            var temp = MouseButtonReleased;
            if (temp != null)
            {
                temp(this, device, button);
            }
        }

        /// <summary>
        /// Gets the dependency object's containing object.
        /// </summary>
        protected internal sealed override DependencyObject DependencyContainer
        {
            get { return Container; }
        }

        /// <summary>
        /// Gets or sets the data source from which the object's dependency properties will retrieve values if they are data bound.
        /// </summary>
        protected internal sealed override Object DependencyDataSource
        {
            get { return ViewModel; }
        }

        /// <summary>
        /// Gets the element's area relative to its container after layout has been performed.
        /// </summary>
        protected internal Rectangle ContainerRelativeLayout
        {
            get { return new Rectangle(containerRelativeX, containerRelativeY, calculatedWidth, calculatedHeight); }
            set
            {
                containerRelativeX = value.X;
                containerRelativeY = value.Y;
                calculatedWidth = value.Width;
                calculatedHeight = value.Height;
                OnContainerRelativeLayoutChanged();
            }
        }

        /// <summary>
        /// Gets the x-coordinate of the element's absolute screen position.
        /// </summary>
        protected internal Int32 AbsoluteScreenX
        {
            get { return AbsoluteScreenXInternal; }
        }

        /// <summary>
        /// Gets the y-coordinate of the element's absolute screen position.
        /// </summary>
        protected internal Int32 AbsoluteScreenY
        {
            get { return AbsoluteScreenYInternal; }
        }

        /// <summary>
        /// Gets the x-coordinate of the element relative to its container after layout has been performed.
        /// </summary>
        protected internal Int32 ContainerRelativeX
        {
            get { return containerRelativeX; }
            internal set { containerRelativeX = value; }
        }

        /// <summary>
        /// Gets the y-coordinate of the element relative to its container after layout has been performed.
        /// </summary>
        protected internal Int32 ContainerRelativeY
        {
            get { return containerRelativeY; }
            internal set { containerRelativeY = value; }
        }

        /// <summary>
        /// Gets the element's width as calculated during layout.
        /// </summary>
        protected internal Int32 CalculatedWidth
        {
            get { return calculatedWidth; }
            internal set { calculatedWidth = value; }
        }

        /// <summary>
        /// Gets the element's height as calculated during layout.
        /// </summary>
        protected internal Int32 CalculatedHeight
        {
            get { return calculatedHeight; }
            internal set { calculatedHeight = value; }
        }

        /// <summary>
        /// Releases resources associated with the object.
        /// </summary>
        /// <param name="disposing"><c>true</c> if the object is being disposed; <c>false</c> if the object is being finalized.</param>
        protected virtual void Dispose(Boolean disposing)
        {

        }

        /// <summary>
        /// Called when the element should reload its content.
        /// </summary>
        protected virtual void OnReloadingContent()
        {

        }

        /// <summary>
        /// Raises the <see cref="Drawing"/> event.
        /// </summary>
        /// <param name="time">Time elapsed since the last call to <see cref="UltravioletContext.Draw(UltravioletTime)"/>.</param>
        /// <param name="spriteBatch">The sprite batch with which to draw the view.</param>
        protected virtual void OnDrawing(UltravioletTime time, SpriteBatch spriteBatch)
        {
            var temp = Drawing;
            if (temp != null)
            {
                temp(this, time, spriteBatch);
            }
        }

        /// <summary>
        /// Raises the <see cref="Updating"/> event.
        /// </summary>
        /// <param name="time">Time elapsed since the last call to <see cref="UltravioletContext.Draw(UltravioletTime)"/>.</param>
        protected virtual void OnUpdating(UltravioletTime time)
        {
            var temp = Updating;
            if (temp != null)
            {
                temp(this, time);
            }
        }

        /// <summary>
        /// Called when the element's container-relative layout changes.
        /// </summary>
        protected virtual void OnContainerRelativeLayoutChanged()
        {

        }

        /// <summary>
        /// Raises the <see cref="EnabledChanged"/> event.
        /// </summary>
        protected virtual void OnEnabledChanged()
        {
            var temp = EnabledChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="VisibleChanged"/> event.
        /// </summary>
        protected virtual void OnVisibleChanged()
        {
            var temp = VisibleChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="HoveringChanged"/> event.
        /// </summary>
        protected virtual void OnHoveringChanged()
        {
            var temp = HoveringChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="WidthChanged"/> event.
        /// </summary>
        protected virtual void OnWidthChanged()
        {
            var temp = WidthChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="HeightChanged"/> event.
        /// </summary>
        protected virtual void OnHeightChanged()
        {
            var temp = HeightChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="PaddingChanged"/> event.
        /// </summary>
        protected virtual void OnPaddingChanged()
        {
            var temp = PaddingChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="FontColorChanged"/> event.
        /// </summary>
        protected virtual void OnFontColorChanged()
        {
            var temp = FontColorChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="FontAssetIDChanged"/> event.
        /// </summary>
        protected virtual void OnFontAssetIDChanged()
        {
            var temp = FontAssetIDChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="BackgroundColorChanged"/> event.
        /// </summary>
        protected virtual void OnBackgroundColorChanged()
        {
            var temp = BackgroundColorChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="BackgroundImageChanged"/> event.
        /// </summary>
        protected virtual void OnBackgroundImageChanged()
        {
            var temp = BackgroundImageChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Loads the specified asset from the global content manager.
        /// </summary>
        /// <typeparam name="TOutput">The type of object being loaded.</typeparam>
        /// <param name="asset">The identifier of the asset to load.</param>
        /// <returns>The asset that was loaded.</returns>
        protected TOutput LoadGlobalContent<TOutput>(AssetID asset)
        {
            if (View == null)
                return default(TOutput);

            return View.LoadLocalContent<TOutput>(asset);
        }

        /// <summary>
        /// Loads the specified asset from the local content manager.
        /// </summary>
        /// <typeparam name="TOutput">The type of object being loaded.</typeparam>
        /// <param name="asset">The identifier of the asset to load.</param>
        /// <returns>The asset that was loaded.</returns>
        protected TOutput LoadLocalContent<TOutput>(AssetID asset)
        {
            if (View == null)
                return default(TOutput);

            return View.LoadLocalContent<TOutput>(asset);
        }

        /// <summary>
        /// Loads the specified sourced asset.
        /// </summary>
        /// <typeparam name="TOutput">The type of object being loaded.</typeparam>
        /// <param name="asset">The identifier of the asset to load.</param>
        /// <returns>The asset that was loaded.</returns>
        protected TOutput LoadContent<TOutput>(SourcedVal<AssetID> asset)
        {
            if (View == null)
                return default(TOutput);

            return View.LoadContent<TOutput>(asset);
        }

        /// <summary>
        /// Loads the specified image from the global content manager.
        /// </summary>
        /// <param name="image">The identifier of the image to load.</param>
        protected void LoadGlobalContent(StretchableImage9 image)
        {
            if (View == null)
                return;

            View.LoadGlobalContent(image);
        }

        /// <summary>
        /// Loads the specified image from the local content manager.
        /// </summary>
        /// <param name="image">The identifier of the image to load.</param>
        protected void LoadLocalContent(StretchableImage9 image)
        {
            if (View == null)
                return;

            View.LoadLocalContent(image);
        }

        /// <summary>
        /// Loads the specified sourced image.
        /// </summary>
        /// <param name="image">The identifier of the image to load.</param>
        protected void LoadContent(SourcedRef<StretchableImage9> image)
        {
            if (View == null)
                return;

            View.LoadContent(image);
        }

        /// <summary>
        /// Reloads the element's font.
        /// </summary>
        protected void ReloadFont()
        {
            this.font = LoadContent<SpriteFont>(FontAssetID);
        }

        /// <summary>
        /// Reloads the element's background image.
        /// </summary>
        protected void ReloadBackgroundImage()
        {
            LoadContent(BackgroundImage);
        }

        /// <summary>
        /// Draws the element's background image.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch with which to draw.</param>
        protected void DrawBackgroundImage(SpriteBatch spriteBatch)
        {
            var bgColor = BackgroundColor;
            var bgImage = BackgroundImage.Value;

            if (bgColor.Equals(Color.Transparent))
                return;

            if (bgImage == null || !bgImage.IsLoaded)
            {
                var area = new RectangleF(AbsoluteScreenX, AbsoluteScreenY, CalculatedWidth, CalculatedHeight);
                spriteBatch.Draw(UIElementResources.BlankTexture, area, bgColor);
            }
            else
            {
                var effects  = SpriteEffects.None;
                var origin   = new Vector2(CalculatedWidth / 2f, CalculatedHeight / 2f);
                var position = new Vector2(
                    AbsoluteScreenX + (CalculatedWidth / 2f),
                    AbsoluteScreenY + (CalculatedHeight / 2f));

                spriteBatch.DrawImage(bgImage, position, CalculatedWidth, CalculatedHeight, bgColor, 0f, origin, effects, 0f);
            }
        }

        /// <summary>
        /// Gets the style setter for the style with the specified name.
        /// </summary>
        /// <param name="name">The name of the style for which to retrieve a setter.</param>
        /// <returns>A function to set the value of the specified style.</returns>
        private StyleSetter GetStyleSetter(String name)
        {
            return GetStyleSetter(name, null);
        }

        /// <summary>
        /// Gets the style setter for the style with the specified name.
        /// </summary>
        /// <param name="name">The name of the style for which to retrieve a setter.</param>
        /// <param name="pseudoClass">The pseudo-class of the style for which to retrieve a setter.</param>
        /// <returns>A function to set the value of the specified style.</returns>
        private StyleSetter GetStyleSetter(String name, String pseudoClass)
        {
            var currentType = GetType();

            lock (styleSetters)
            {
                while (currentType != null && typeof(UIElement).IsAssignableFrom(currentType))
                {   
                    Dictionary<UvssStyleKey, StyleSetter> styleSettersForCurrentType;
                    if (styleSetters.TryGetValue(currentType, out styleSettersForCurrentType))
                    {
                        StyleSetter setter;
                        if (styleSettersForCurrentType.TryGetValue(new UvssStyleKey(name, pseudoClass), out setter))
                        {
                            return setter;
                        }
                    }

                    currentType = currentType.BaseType;
                }
            }

            return null;
        }

        /// <summary>
        /// Dynamically compiles a collection of lambda methods which can be used to apply styles
        /// to the element's properties.
        /// </summary>
        private void CreateStyleSetters()
        {
            var currentType = GetType();

            lock (styleSyncObject)
            {
                while (currentType != null && typeof(UIElement).IsAssignableFrom(currentType))
                {
                    Dictionary<UvssStyleKey, StyleSetter> styleSettersForCurrentType;
                    Dictionary<String, DependencyProperty> styledPropertiesForCurrentType;
                    if (!styleSetters.TryGetValue(currentType, out styleSettersForCurrentType))
                    {
                        styleSettersForCurrentType     = new Dictionary<UvssStyleKey, StyleSetter>();
                        styledPropertiesForCurrentType = new Dictionary<String, DependencyProperty>();

                        var styledDependencyProperties = 
                            from field in currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                            let attr = field.GetCustomAttributes(typeof(StyledAttribute), false).SingleOrDefault()
                            let type = field.FieldType
                            let name = field.Name
                            where
                                attr != null &&
                                type == typeof(DependencyProperty)
                            select new { Attribute = (StyledAttribute)attr, FieldInfo = field };

                        foreach (var prop in styledDependencyProperties)
                        {
                            var dp                  = (DependencyProperty)prop.FieldInfo.GetValue(null);
                            var dpType              = Type.GetTypeFromHandle(dp.PropertyType);

                            var setStyledValue      = miSetStyledValue.MakeGenericMethod(dpType);

                            var expParameterElement = Expression.Parameter(typeof(UIElement), "element");
                            var expParameterValue   = Expression.Parameter(typeof(String), "value");
                            var expParameterFmtProv = Expression.Parameter(typeof(IFormatProvider), "provider");
                            var expResolveValue     = Expression.Convert(Expression.Call(miFromString, expParameterValue, Expression.Constant(dpType), expParameterFmtProv), dpType);
                            var expCallMethod       = Expression.Call(expParameterElement, setStyledValue, Expression.Constant(dp), expResolveValue);

                            var lambda = Expression.Lambda<StyleSetter>(expCallMethod, expParameterElement, expParameterValue, expParameterFmtProv).Compile();

                            var styleKey = new UvssStyleKey(prop.Attribute.Name, prop.Attribute.PseudoClass);
                            styleSettersForCurrentType[styleKey] = lambda;
                            styledPropertiesForCurrentType[prop.Attribute.Name] = dp;
                        }

                        styleSetters[currentType]     = styleSettersForCurrentType;
                        styledProperties[currentType] = styledPropertiesForCurrentType;
                    }

                    currentType = currentType.BaseType;
                }
            }
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Enabled"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleEnabledChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;
            element.OnEnabledChanged();
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Visible"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleVisibleChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;
            element.OnVisibleChanged();
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Width"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleWidthChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;
            element.OnWidthChanged();
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Height"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleHeightChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;
            element.OnHeightChanged();
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Padding"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandlePaddingChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;
            element.OnPaddingChanged();
        }

        /// <summary>
        /// Occurs when the value of the <see cref="FontColor"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleFontColorChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;
            element.OnFontColorChanged();
        }

        /// <summary>
        /// Occurs when the value of the <see cref="FontAssetID"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleFontAssetIDChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;
            if (element.Container != null)
            {
                element.Container.PerformLayout(element);
            }
            element.ReloadFont();
            element.OnFontAssetIDChanged();
        }

        /// <summary>
        /// Occurs when the value of the <see cref="BackgroundColor"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleBackgroundColorChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;
            element.OnBackgroundColorChanged();
        }

        /// <summary>
        /// Occurs when the value of the <see cref="BackgroundImage"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The object that raised the event.</param>
        private static void HandleBackgroundImageChanged(DependencyObject dobj)
        {
            var element = (UIElement)dobj;
            element.ReloadBackgroundImage();
            element.OnBackgroundImageChanged();
        }

        /// <summary>
        /// Retrieves a storyboard clock from the pool.
        /// </summary>
        /// <param name="storyboard">The storyboard which the clock will track.</param>
        /// <returns>The storyboard clock that was retrieved from the pool.</returns>
        private static StoryboardClock RetrieveStoryboardClock(Storyboard storyboard)
        {
            StoryboardClock clock;
            lock (storyboardClockPool)
            {
                clock = storyboardClockPool.Retrieve();
            }
            clock.ChangeStoryboard(storyboard);
            return clock;
        }

        /// <summary>
        /// Releases a storyboard clock back into the pool.
        /// </summary>
        /// <param name="clock">The clock to release back into the pool.</param>
        private static void ReleaseStoryboardClock(StoryboardClock clock)
        {
            Contract.Require(clock, "clock");

            lock (storyboardClockPool)
            {
                storyboardClockPool.Release(clock);
            }
        }

        // Dependency properties.
        private static readonly DependencyProperty dpEnabled = DependencyProperty.Register("Enabled", typeof(Boolean), typeof(UIElement),
            new DependencyPropertyMetadata(HandleEnabledChanged, () => true, DependencyPropertyOptions.None));

        [Styled("width")]
        private static readonly DependencyProperty dpWidth = DependencyProperty.Register("Width", typeof(Double), typeof(UIElement),
            new DependencyPropertyMetadata(HandleWidthChanged, () => Double.NaN, DependencyPropertyOptions.None));
        [Styled("height")]
        private static readonly DependencyProperty dpHeight = DependencyProperty.Register("Height", typeof(Double), typeof(UIElement),
            new DependencyPropertyMetadata(HandleHeightChanged, () => Double.NaN, DependencyPropertyOptions.None));

        [Styled("visible")]
        private static readonly DependencyProperty dpVisible = DependencyProperty.Register("Visible", typeof(Boolean), typeof(UIElement),
            new DependencyPropertyMetadata(HandleVisibleChanged, () => true, DependencyPropertyOptions.None));

        [Styled("padding")]
        private static readonly DependencyProperty dpPadding = DependencyProperty.Register("Padding", typeof(Int32), typeof(UIElement),
            new DependencyPropertyMetadata(HandlePaddingChanged, null, DependencyPropertyOptions.None));

        [Styled("font-color")]
        private static readonly DependencyProperty dpFontColor = DependencyProperty.Register("FontColor", typeof(Color), typeof(UIElement),
            new DependencyPropertyMetadata(HandleFontColorChanged, () => Color.White, DependencyPropertyOptions.Inherited));
        [Styled("font-asset")]
        private static readonly DependencyProperty dpFontAssetID = DependencyProperty.Register("FontAssetID", typeof(SourcedVal<AssetID>), typeof(UIElement),
            new DependencyPropertyMetadata(HandleFontAssetIDChanged, null, DependencyPropertyOptions.Inherited));

        [Styled("background-color")]
        private static readonly DependencyProperty dpBackgroundColor = DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(UIElement),
            new DependencyPropertyMetadata(HandleBackgroundColorChanged, () => Color.White, DependencyPropertyOptions.None));
        [Styled("background-image")]
        private static readonly DependencyProperty dpBackgroundImage = DependencyProperty.Register("BackgroundImage", typeof(SourcedRef<StretchableImage9>), typeof(UIElement),
            new DependencyPropertyMetadata(HandleBackgroundImageChanged, null, DependencyPropertyOptions.None));

        // Property values.
        private readonly UltravioletContext uv;
        private readonly String id;
        private readonly String name;
        private readonly UIElementClassCollection classes;
        private readonly VisualStateGroupCollection visualStateGroups;
        private Object viewModel;
        private UIView view;
        private UIContainer container;
        private Boolean hovering;
        private Int32 containerRelativeX;
        private Int32 containerRelativeY;
        private Int32 calculatedWidth;
        private Int32 calculatedHeight;
        private SpriteFont font;

        // Storyboard clocks.
        private static readonly IPool<StoryboardClock> storyboardClockPool = 
            new ExpandingPool<StoryboardClock>(64, () => new StoryboardClock());
        private readonly Dictionary<Storyboard, StoryboardClock> storyboardClocks = 
            new Dictionary<Storyboard, StoryboardClock>();

        // Functions for setting styles on known element types.
        private static readonly MethodInfo miFromString;
        private static readonly MethodInfo miSetStyledValue;
        private static readonly Object styleSyncObject = new Object();
        private static readonly Dictionary<Type, Dictionary<String, DependencyProperty>> styledProperties = 
            new Dictionary<Type, Dictionary<String, DependencyProperty>>();
        private static readonly Dictionary<Type, Dictionary<UvssStyleKey, StyleSetter>> styleSetters = 
            new Dictionary<Type, Dictionary<UvssStyleKey, StyleSetter>>();
    }
}
