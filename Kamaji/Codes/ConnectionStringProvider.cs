namespace Kamaji
{
    using Kamaji.Data;

    internal sealed class ConnectionStringProvider : IConnectionStringProvider
    {
        public string ConnectionString => DataSources.Jsons.AppSettings.Config.ConnectionString;

        public string DatabaseName => "Kamaji";
    }
}
