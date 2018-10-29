namespace KamajiTests
{
    //using Kamaji.Data.Mongo.Models;
    //using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    static class Program
    {
        static void Main(string[] args)
        {
            TestScanSave().Wait();
           // new QueueTests().Test();
        }

        static async Task TestScanSave()
        {
            int count = 0;
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += (o, e) =>
            {
                int kalan = count++ % 3;
                if (kalan == 0)
                {
                    Console.Clear();
                    Console.Write("Testing");
                }
                Console.Write('.');
            };
            timer.Start();
            Stopwatch bench = Stopwatch.StartNew();
            try
            {
                //await new ScanControllerTests().SaveScanPrerequisiteTest();
                // await new ScanControllerTests().SaveScanResourceTest();
                 await new ScanControllerTests().SaveScanTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Not Ok!!!... " + ex.Message);
                return;
            }
            // await new ScanControllerTests().AddScanPrerequisiteTest();
            //  await new ScanControllerTests().AddScanResourceTest
            //  await new ScanControllerTests().AddScanTest();
            bench.Stop();
            timer.Stop();


            Console.WriteLine("it's ok. Elapsed: " + bench.Elapsed);
            Console.ReadKey();
        }

        //static void KeepThat()
        //{

        //    var props = typeof(Scan).GetProperties();

        //    var prop = props.Where(p => p.GetCustomAttribute<BsonIdAttribute>() != null);


        //    //Burada express ve puputier ile ilkj web scan e başla.
        //}
    }
}
