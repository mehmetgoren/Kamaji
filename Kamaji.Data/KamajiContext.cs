namespace Kamaji.Data
{
    using Kamaji.Data.Models;
    using System;

    public interface IKamajiModelFactory
    {
        IScanPrerequisiteModel CreateScanPrerequisiteModel();

        IScanInstanceModel CreateScanInstanceModel();

        IScanModel CreateScanModel();


        IScanResourceModel CreateScanResourceModel();

        INodeModel CreateNodeModel();

        IAuthModel CreateAuthModel();
    }

    public interface IKamajiContext : IDisposable
    {
        void Init(IConnectionStringProvider connectionStringProvider);


        IKamajiModelFactory ModelFactory { get; }
        DateTime GetDbDateTime();


        IAuthRepository Authes { get; }

        INodeRepository Nodes { get; }

        IScanRepository Scans { get; }

        IScanPrerequisiteRepository ScanPrerequisites { get; }

        IScanResourceRepository ScanResources { get; }

        IScanInstanceRepository ScanInstances { get; }
    }

    public abstract class KamajiContext<TId> : IKamajiContext
    {
        public abstract void Init(IConnectionStringProvider connectionStringProvider);


        public abstract IKamajiModelFactory ModelFactory { get; }
        public abstract DateTime GetDbDateTime();



        public abstract IAuthRepository Authes { get; }

        public abstract INodeRepository Nodes { get; }

        public abstract IScanRepository Scans { get; }

        public abstract IScanPrerequisiteRepository ScanPrerequisites { get; }

        public abstract IScanResourceRepository ScanResources { get; }

        public abstract IScanInstanceRepository ScanInstances { get; }



        public void Dispose() => this.Dispose(true);
        public abstract void Dispose(bool disposing);
    }
}
