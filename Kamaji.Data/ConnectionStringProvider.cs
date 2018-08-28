namespace Kamaji.Data
{
    using System;

    public interface IConnectionStringProvider
    {
        string ConnectionString { get; }
        string DatabaseName { get; }
    }

}
