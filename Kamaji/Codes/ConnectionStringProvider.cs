namespace Kamaji
{
    using Kamaji.Data;

    internal sealed class ConnectionStringProvider : IConnectionStringProvider
    {
        public string ConnectionString => "mongodb://192.168.70.129:27017";

        public string DatabaseName => "Kamaji";
    }
}
