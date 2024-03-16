using System;

namespace OpenBudgeteer.Core.Common.Extensions
{
    /// <summary>
    /// This attribute is used to represent a string value for a value in an enum.
    /// </summary>
    public class StringValueAttribute : Attribute {

        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string StringValue { get; protected set; }

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value"></param>
        public StringValueAttribute(string value)
        {
            StringValue = value;
        }
    }
}