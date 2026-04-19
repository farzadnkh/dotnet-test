using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Contract.Commons.Helpers;

public static class EnumHelper
{
    public static List<SelectListItem> GetEnumTypeOptions<T>(T selectedValue) where T : Enum
    {
        var result = new List<SelectListItem>();
        bool isAllSelected = selectedValue.Equals(default(T));

        result.AddRange([.. Enum.GetValues(typeof(T))
            .Cast<T>()
            .Where(t => !GetEnumDisplayName(t).Equals("none", StringComparison.CurrentCultureIgnoreCase))
            .Select(t => new SelectListItem
            {
                Value = Convert.ToInt32(t).ToString(),
                Text = GetEnumDisplayName(t),
                Selected = t.Equals(selectedValue)
            })]);

        return result;
    }

    public static List<SelectListItem> GetEnumTypeOptionsWithNoneProperty<T>(T selectedValue) where T : Enum
    {
        var result = new List<SelectListItem>();
        bool isAllSelected = selectedValue.Equals(default(T));

        result.AddRange([.. Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(t => new SelectListItem
            {
                Value = Convert.ToInt32(t).ToString(),
                Text = GetEnumDisplayName(t),
                Selected = t.Equals(selectedValue)
            })]);

        return result;
    }

    public static List<SelectListItem> GetEnumTypeOptions<T>(bool isList = true) where T : Enum
    {
        if (isList)
           return GetEnumTypeOptions<T>();

        var result = new List<SelectListItem>();
        result.AddRange([.. Enum.GetValues(typeof(T))
            .Cast<T>()
            .Where(t => !GetEnumDisplayName(t).Equals("none", StringComparison.CurrentCultureIgnoreCase))
            .Select(t => new SelectListItem
            {
                Value = Convert.ToInt32(t).ToString(),
                Text = GetEnumDisplayName(t),
            })]);

        return result;
    }

    private static List<SelectListItem> GetEnumTypeOptions<T>() where T : Enum
    {
        var result = new List<SelectListItem>
        {
            new() {
                Value = "",
                Text = "All",
            }
        };

        result.AddRange([.. Enum.GetValues(typeof(T))
            .Cast<T>()
            .Where(t => !GetEnumDisplayName(t).Equals("none", StringComparison.CurrentCultureIgnoreCase))
            .Select(t => new SelectListItem
            {
                Value = Convert.ToInt32(t).ToString(),
                Text = GetEnumDisplayName(t)
            })]);

        return result;
    }

    public static string GetEnumDisplayName<T>(T enumValue) where T : Enum
    {
        var member = typeof(T).GetMember(enumValue.ToString()).FirstOrDefault();
        if (member != null)
        {
            var displayAttribute = member.GetCustomAttributes(typeof(DisplayAttribute), false)
                                         .OfType<DisplayAttribute>()
                                         .FirstOrDefault();
            if (displayAttribute != null && !string.IsNullOrWhiteSpace(displayAttribute.Name))
            {
                return displayAttribute.Name;
            }
        }
        return enumValue.ToString();
    }
}
