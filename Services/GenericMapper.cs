using System.Globalization;

namespace MigradorCUAD.Services
{
    public static class GenericMapper
    {
        public static List<T> MapToList<T>(List<Dictionary<string, string>> data)
            where T : new()
        {
            var result = new List<T>();

            foreach (var row in data)
            {
                T obj = new T();

                foreach (var prop in typeof(T).GetProperties())
                {
                    if (!row.ContainsKey(prop.Name))
                        continue;

                    var value = row[prop.Name];

                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    object convertedValue = ConvertValue(value, prop.PropertyType);

                    prop.SetValue(obj, convertedValue);
                }

                result.Add(obj);
            }

            return result;
        }

        private static object ConvertValue(string value, Type targetType)
        {
            if (targetType == typeof(int))
                return int.Parse(value);

            if (targetType == typeof(decimal))
                return decimal.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(DateTime))
                return DateTime.Parse(value);

            if (targetType == typeof(string))
                return value;

            return Convert.ChangeType(value, targetType);
        }
    }
}
