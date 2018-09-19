using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFMLib.Extenders
{
    public static class IntExtender
    {
        public static string ToEnumString<TEnum>(this int enumValue)
        {
            var enumString = enumValue.ToString();

            if (Enum.IsDefined(typeof(TEnum), enumValue))
            {
                enumString = ((TEnum)Enum.ToObject(typeof(TEnum), enumValue)).ToString();
            }

            return enumString;
        }

        public static string ToEnumString<T>(this T enumValue) where T : struct
        {
            Dictionary<int, string> ts = new Dictionary<int, string>();

            foreach (T value in Enum.GetValues(typeof(T)))
            {
                ts.Add(Convert.ToInt32(value), value.ToString());
            }

            int enumValueInt = Convert.ToInt32(enumValue);

            var enumString = ts.Keys
                .Where(v => (v & enumValueInt) == v)
                .Select( v => ts[v]);

            return string.Join(", ", enumString);
        }
    }
}