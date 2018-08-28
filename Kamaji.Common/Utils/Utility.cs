namespace Kamaji.Common
{
    using SQLog;
    using System;
    using System.IO;
    using System.Reflection;

    public static class Utility
    {
        public static string GetExecutionPath() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


        public static Logger CreateLogger(string type, string method)
        {
            if (String.IsNullOrEmpty(type))
                throw new ArgumentNullException(nameof(method));
            if (String.IsNullOrEmpty(type))
                throw new ArgumentNullException(nameof(method));

            return Logger.Create(type, method).OnSaveCompleted(Console.WriteLine);
        }
    }
}
