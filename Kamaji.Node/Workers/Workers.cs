namespace Kamaji.Node
{
    using Kamaji.Common.Models;
    using Kamaji.Worker;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Workers : IDisposable
    {
        internal static readonly Workers Instance = new Workers();

        private Workers() { }


        private readonly IDictionary<string, IWorker> _workers = new ConcurrentDictionary<string, IWorker>();

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        internal async Task<IWorker> GetWorker(ScanModel model)
        {
            if (!_workers.TryGetValue(model.ResourceName, out IWorker worker))
            {
                await _semaphoreSlim.WaitAsync();
                try
                {
                    DirectoryInfo resourceFolder = await ScanResourceManager.GetResourceDirectory(model);

                    Type IWorkerType = typeof(IWorker);
                    HashSet<string> assemblies = new HashSet<string>(AppDomain.CurrentDomain.GetAssemblies().Select(i => new AssemblyName(i.FullName).Name));
                    foreach (FileInfo fi in resourceFolder.GetFiles())
                    {           
                        if (fi.Extension == ".dll")
                        {
                            string assemblyName = fi.Name.Substring(0, fi.Name.Length - 4);
                            if (!assemblies.Contains(assemblyName))
                            {
                                Assembly asm = Assembly.UnsafeLoadFrom(fi.FullName);//her türlü yüklenmeyen assembly' ler appdomain' e yüklensin diyte break ile çıkmıyoruz 
                                if (null == worker)
                                {
                                    foreach (Type type in asm.GetTypes())
                                    { 
                                        if (!type.IsAbstract && IWorkerType.IsAssignableFrom(type))
                                        {
                                            worker = (IWorker)Activator.CreateInstance(type);
                                            await worker.SetupEnvironment();
                                            _workers.Add(model.ResourceName, worker);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }

                if (null == worker)
                    throw new InvalidOperationException();
            }

            return worker;
        }

        public void Dispose()
        {
            foreach (var worker in this._workers.Values)
            {
                try
                {
                    worker.Dispose();
                }
                catch(Exception ex)
                {
                    Common.Utility.CreateLogger(nameof(Workers), nameof(Dispose)).Code(514).Error(ex).SaveAsync().Wait();
                }
            }
        }
    }
}
