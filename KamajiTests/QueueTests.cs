namespace KamajiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ReliableQueue;
    using System.Diagnostics;

    [TestClass]
    public class QueueTests
    {
        [TestMethod]
        public void Test()
        {
            const int len = 10000;
            var q = Queue.Instance;

            using (var tran = q.StartTransaction<int>())
            {
                Stopwatch bench = Stopwatch.StartNew();
                for (int j = 0; j < len; ++j)
                {
                    tran.Enqueue(j);
                }

                for (int j = 0; j < len; ++j)
                {
                    var m = tran.Dequeue();
                    System.Console.WriteLine(m);
                }

                tran.Commit();
                bench.Stop();
                System.Console.WriteLine(bench.ElapsedMilliseconds);
            }

            Assert.IsTrue(true);
        }
    }
}
