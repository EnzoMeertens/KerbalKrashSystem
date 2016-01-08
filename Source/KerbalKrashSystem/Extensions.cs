using System.Text;

namespace KerbalKrashSystem
{
    public static class Extensions
    {
        private static readonly StringBuilder StringBuilder = new StringBuilder();

        public static string Append(this string @string, params object[] values)
        {
            StringBuilder.Clear();
            StringBuilder.Append(@string);

            foreach (object value in values)
                StringBuilder.Append(value);

            return StringBuilder.ToString();
        }
    }
}