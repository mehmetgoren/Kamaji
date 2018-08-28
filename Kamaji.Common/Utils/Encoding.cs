namespace Kamaji.Common
{
    using System;
    using System.Text;

    public static class Serializer
    {
        public static string ToBaseb64(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            var bytes = Encoding.UTF8.GetBytes(str);
            var base64 = Convert.ToBase64String(bytes);
            return base64;
        }

        public static string FromBase64(string base64)
        {
            if (String.IsNullOrEmpty(base64))
                return base64;

            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
