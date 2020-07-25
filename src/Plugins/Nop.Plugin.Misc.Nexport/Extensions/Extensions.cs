﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Misc.Nexport.Extensions
{
    public static class Extensions
    {
        public static string GetDisplayName<TEnum>(this TEnum enumValue)
        {
            return enumValue
                .GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .GetName();
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                email = Regex.Replace(email, "(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200.0));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            try
            {
                return Regex.IsMatch(email, "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250.0));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            string DomainMapper(Match match)
            {
                var ascii = new IdnMapping().GetAscii(match.Groups[2].Value);
                return match.Groups[1].Value + ascii;
            }
        }

        public static bool IsValidUrl(this string url)
        {
            var tryCreateResult = Uri.TryCreate(url, UriKind.Absolute, out var uriResult);
            return tryCreateResult && uriResult != null;
        }

        public static SelectList ToNexportSelectList<TEnum>(this TEnum enumObj,
            bool markCurrentAsSelected = true, int[] valuesToExclude = null) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("An Enumeration type is required.", nameof(enumObj));

            var values = from TEnum enumValue in Enum.GetValues(typeof(TEnum))
                where valuesToExclude == null || !valuesToExclude.Contains(Convert.ToInt32(enumValue))
                select new { ID = Convert.ToInt32(enumValue), Name = GetDisplayName(enumValue) };
            object selectedValue = null;
            if (markCurrentAsSelected)
                selectedValue = Convert.ToInt32(enumObj);
            return new SelectList(values, "ID", "Name", selectedValue);
        }
    }
}
