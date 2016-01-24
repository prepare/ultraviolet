﻿using System;
using System.Globalization;
using TwistedLogik.Nucleus;
using TwistedLogik.Nucleus.Data;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Styles
{
	/// <summary>
	/// Represents one of the conditions of a property trigger.
	/// </summary>
	public class UvssPropertyTriggerCondition
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="UvssPropertyTriggerCondition"/> class.
		/// </summary>
		/// <param name="op">A <see cref="TriggerComparisonOp"/> value that specifies the type of comparison performed by this condition.</param>
		/// <param name="dpropName">The name of the dependency property to evaluate.</param>
		/// <param name="refval">The reference value to compare to the value of the dependency property.</param>
		/// <param name="culture">The culture to use when parsing the condition's value, or <see langword="culture"/> to
		/// use the default culture (en-US).</param>
		internal UvssPropertyTriggerCondition(TriggerComparisonOp op, String dpropName, String refval, CultureInfo culture)
		{
			this.op = op;
			this.dpropName = new UvmlName(dpropName);
			this.refval = refval;
			this.culture = culture ?? UvssDocument.DefaultCulture;
		}

        /// <summary>
        /// Evaluates whether the condition is true for the specified object.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="dobj">The object against which to evaluate the trigger condition.</param>
        /// <returns><c>true</c> if the condition is true for the specified object; otherwise, <c>false</c>.</returns>
        internal Boolean Evaluate(UltravioletContext uv, DependencyObject dobj)
        {
            Contract.Require(uv, "uv");
            Contract.Require(dobj, "dobj");

            var dprop = DependencyProperty.FindByStylingName(uv, dobj, dpropName.Owner, dpropName.Name);
            if (dprop == null)
                return false;

            var refvalCacheType = (refvalCache == null) ? null : refvalCache.GetType();
            if (refvalCacheType == null || (refvalCacheType != dprop.PropertyType &&  refvalCacheType != dprop.UnderlyingType))
            {
                refvalCache = ObjectResolver.FromString(refval, dprop.PropertyType, culture);
            }

            var comparison = TriggerComparisonCache.Get(dprop.PropertyType, op);
            if (comparison == null)
                throw new InvalidOperationException(PresentationStrings.InvalidTriggerComparison.Format(dpropName, op, dprop.PropertyType));

            return comparison(dobj, dprop, refvalCache);
        }

        /// <summary>
        /// Gets the comparison operation performed by this condition.
        /// </summary>
        public TriggerComparisonOp ComparisonOperation
        {
            get { return op; }
        }

        /// <summary>
        /// Gets the name of the dependency property which is evaluated by this condition.
        /// </summary>
        public UvmlName DependencyPropertyName
        {
            get { return dpropName; }
        }

        /// <summary>
        /// Gets a string which represents the reference value for this condition.
        /// </summary>
        public String ReferenceValue
        {
            get { return refval; }
        }

        // State values.
        private readonly TriggerComparisonOp op;
        private readonly UvmlName dpropName;
        private readonly String refval;
		private readonly CultureInfo culture;
		private Object refvalCache;
    }
}
