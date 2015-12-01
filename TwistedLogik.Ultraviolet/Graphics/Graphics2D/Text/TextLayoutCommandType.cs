﻿namespace TwistedLogik.Ultraviolet.Graphics.Graphics2D.Text
{
    /// <summary>
    /// Represents the types of commands produced by the text layout engine.
    /// </summary>
    public enum TextLayoutCommandType
    {
        /// <summary>
        /// No command type.
        /// </summary>
        None,

        /// <summary>
        /// Represents a command containing the metadata for a block of text.
        /// </summary>
        BlockInfo,

        /// <summary>
        /// Represents a command containing the metadata for a line of text.
        /// </summary>
        LineInfo,

        /// <summary>
        /// Represents a command to print a string of text.
        /// </summary>
        Text,

        /// <summary>
        /// Represents a command to print an icon.
        /// </summary>
        Icon,

        /// <summary>
        /// Represents a command to toggle the bold font style.
        /// </summary>
        ToggleBold,

        /// <summary>
        /// Represents a command to toggle the italic font style.
        /// </summary>
        ToggleItalic,

        /// <summary>
        /// Represents a command to push a style onto the style stack.
        /// </summary>
        PushStyle,

        /// <summary>
        /// Represents a command to push a font onto the font stack.
        /// </summary>
        PushFont,

        /// <summary>
        /// Represents a command to push a color onto the color stack.
        /// </summary>
        PushColor,
        
        /// <summary>
        /// Represents a command to push a glyph shader onto the glyph shader stack.
        /// </summary>
        PushGlyphShader,

        /// <summary>
        /// Represents a command to pop a style off of the style stack.
        /// </summary>
        PopStyle,

        /// <summary>
        /// Represents a command to pop a font off of the font stack.
        /// </summary>
        PopFont,

        /// <summary>
        /// Represents a command to pop a color off of the color stack.
        /// </summary>
        PopColor,

        /// <summary>
        /// Represents a command to pop a glyph shader off of the glyph shader stack.
        /// </summary>
        PopGlyphShader,

        /// <summary>
        /// Represents a command to change the source string.
        /// </summary>
        ChangeSourceString,

        /// <summary>
        /// Represents a command to change the source string builder.
        /// </summary>
        ChangeSourceStringBuilder,

        /// <summary>
        /// Represents a command to draw a hyphen character.
        /// </summary>
        Hyphen,

        /// <summary>
        /// Represents a command to insert a line break character.
        /// </summary>
        LineBreak,
    }
}
