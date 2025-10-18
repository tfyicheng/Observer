using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Observer
{
    public class IntCompareToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 默认设置
            int[] targetValues = { 1 }; // 默认匹配 1
            Visibility hiddenVisibility = Visibility.Hidden;

            if (parameter == null) return hiddenVisibility;

            string paramStr = parameter.ToString().Trim();
            if (string.IsNullOrEmpty(paramStr)) return hiddenVisibility;

            // 使用 '-' 分隔值和隐藏模式（避免逗号问题）
            int dashIndex = paramStr.IndexOf('-');
            string valuesPart;
            string modePart = "Hidden";

            if (dashIndex >= 0)
            {
                valuesPart = paramStr.Substring(0, dashIndex);
                modePart = paramStr.Substring(dashIndex + 1).Trim().ToLower();
            }
            else
            {
                valuesPart = paramStr; // 没有 '-'，只解析值
            }

            // 解析隐藏方式
            switch (modePart)
            {
                case "collapsed":
                    hiddenVisibility = Visibility.Collapsed;
                    break;
                case "hidden":
                case "": // 允许空
                    hiddenVisibility = Visibility.Hidden;
                    break;
                default:
                    hiddenVisibility = Visibility.Hidden;
                    break;
            }

            // 解析多个值（用 | 分隔）
            string[] valueStrings = valuesPart.Split('|');
            var validValues = new List<int>();

            foreach (string str in valueStrings)
            {
                if (!string.IsNullOrWhiteSpace(str) && int.TryParse(str.Trim(), out int val))
                {
                    validValues.Add(val);
                }
            }

            if (validValues.Count == 0)
                return hiddenVisibility;

            // 检查绑定值是否匹配
            if (int.TryParse(value?.ToString(), out int actualValue))
            {
                foreach (int target in validValues)
                {
                    if (actualValue == target)
                        return Visibility.Visible;
                }
            }

            return hiddenVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("不支持反向转换。");
        }
    }

}
