using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Nop.Plugin.Misc.Nexport.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Get the display text for an enum
        /// </summary>
        /// <param name="enumValue">The enum</param>
        /// <returns>Returns the enum display text if existed</returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue
                .GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .GetName();
        }
    }
}
