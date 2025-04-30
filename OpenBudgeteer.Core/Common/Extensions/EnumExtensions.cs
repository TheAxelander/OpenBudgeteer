using System;
using System.Linq;

namespace OpenBudgeteer.Core.Common.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetStringValue(this Enum value) {
            // Get the type
            var type = value.GetType();

            // Get fieldinfo for this type
            var fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            var attribs = fieldInfo?.GetCustomAttributes(
                typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attribs?.Length > 0 ? attribs[0].StringValue : string.Empty;
        }
        
        /// <summary>
        /// Checks if the passed value within the defined scope of Enum values 
        /// </summary>
        /// <param name="value">Value that should be checked</param>
        /// <param name="values">Defined scope of allowed Enum values</param>
        /// <returns>Returns true if the passed value is within the defined scope</returns>
        public static bool In(this Enum value, params Enum[] values)
        {
            return values.Contains(value);
        }
    }
}