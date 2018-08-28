namespace Kamaji.Node
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Kamaji.Common.Models;

    public class WorkerServiceList : IEnumerable<WorkerServiceBase>
    {
        public static readonly WorkerServiceList Instance = new WorkerServiceList();
        private WorkerServiceList() { }


        private readonly IDictionary<string, WorkerServiceBase> dic = new ConcurrentDictionary<string, WorkerServiceBase>();


        private static string GetKey(ScanModel model) => model.ResourceName + model.Asset;

        public int Count => this.dic.Count;

        public void Add(WorkerServiceBase item) => this.dic.Add(GetKey(item.Model), item);

        //public void Clear() => this.dic.Clear();

        public WorkerServiceBase Find(ScanModel model)
        {
            this.dic.TryGetValue(GetKey(model), out WorkerServiceBase service);
            return service;
        }

      //  public bool Remove(WorkerServiceBase item) => this.dic.Remove(GetKey(item.Model));


        public IEnumerator<WorkerServiceBase> GetEnumerator() => this.dic.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
