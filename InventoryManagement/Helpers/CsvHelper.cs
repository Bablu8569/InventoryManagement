using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Helpers
{
    public static class CsvHelper
    {
        public static string ConvertToCsv<T>(List<T> data, string[] headers, System.Func<T, string[]> getRowData)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers));

            foreach (var item in data)
            {
                var row = getRowData(item);
                for (int i = 0; i < row.Length; i++)
                {
                    row[i] = $"\"{row[i]?.Replace("\"", "\"\"")}\"";
                }
                sb.AppendLine(string.Join(",", row));
            }
            return sb.ToString();
        }
    }
}