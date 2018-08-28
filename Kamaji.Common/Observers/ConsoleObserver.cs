namespace Kamaji.Common
{
    using Newtonsoft.Json;
    using System;

    public sealed class ConsoleObserver : IObserver
    {
        public static readonly ConsoleObserver Instance = new ConsoleObserver();
        private ConsoleObserver() { }

        private readonly Random _rnd = new Random(); 
        public void Notify(string id, string message, object args)
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = (ConsoleColor)_rnd.Next(1, 16);
            Console.WriteLine($"{(!String.IsNullOrEmpty(id) ? id + " says : " : "")}'{message}', {(null != args ? JsonConvert.SerializeObject(args): "")}");
            Console.ForegroundColor = temp;
        }
    }
}
