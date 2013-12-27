// On GitHub:
// https://github.com/ysharplanguage/FastJsonParser

using System;
using System.Collections.Generic;
using System.Text;

// For the JavaScriptSerializer
using System.Web.Script.Serialization;

// JSON.NET 5.0 r8
using Newtonsoft.Json;

// ServiceStack 3.9.59
using ServiceStack.Text;

// Our stuff
using System.Text.Json;

namespace Test
{
    class Program
    {
        const string OJ_TEST_FILE_PATH = @"..\..\TestData\_oj-highly-nested.json.txt";
        const string BOON_SMALL_TEST_FILE_PATH = @"..\..\TestData\boon-small.json.txt";
        const string TINY_TEST_FILE_PATH = @"..\..\TestData\tiny.json.txt";
        const string SMALL_TEST_FILE_PATH = @"..\..\TestData\small.json.txt";
        const string FATHERS_TEST_FILE_PATH = @"..\..\TestData\fathers.json.txt";
        const string HUGE_TEST_FILE_PATH = @"..\..\TestData\huge.json.txt";

        static void LoopTest(string parserName, Func<string, object> parseFunc, string testFile, int count)
        {
            Console.Clear();
            Console.WriteLine("Parser: {0}", parserName);
            Console.WriteLine();
            Console.WriteLine("Loop Test File: {0}", testFile);
            Console.WriteLine("Iterations: {0}", count.ToString("0,0"));
            Console.WriteLine();
            Console.WriteLine("Press ESC to skip this test or any other key to start...");
            Console.WriteLine();
            if (Console.ReadKey().KeyChar == 27)
                return;

            System.Threading.Thread.MemoryBarrier();
            var initialMemory = System.GC.GetTotalMemory(true);

            var json = System.IO.File.ReadAllText(testFile);
            var st = DateTime.Now;
            var l = new List<object>();
            for (var i = 0; i < count; i++)
                l.Add(parseFunc(json));
            var tm = (int)DateTime.Now.Subtract(st).TotalMilliseconds;

            System.Threading.Thread.MemoryBarrier();
            var finalMemory = System.GC.GetTotalMemory(true);
            var consumption = finalMemory - initialMemory;

            System.Diagnostics.Debug.Assert(l.Count == count);

            Console.WriteLine();
            Console.WriteLine("... Done, in {0} ms. Throughput: {1} characters / second.", tm.ToString("0,0"), (1000 * (decimal)(count * json.Length) / (tm > 0 ? tm : 1)).ToString("0,0.00"));
            Console.WriteLine();
            Console.WriteLine("\tMemory used : {0}", ((decimal)finalMemory).ToString("0,0"));
            Console.WriteLine();

            GC.Collect();
            Console.WriteLine("Press a key...");
            Console.WriteLine();
            Console.ReadKey();
        }

        static void Test(string parserName, Func<string, object> parseFunc, string testFile)
        {
            Console.Clear();
            Console.WriteLine("Parser: {0}", parserName);
            Console.WriteLine();
            Console.WriteLine("Test File: {0}", testFile);
            Console.WriteLine();
            Console.WriteLine("Press ESC to skip this test or any other key to start...");
            Console.WriteLine();
            if (Console.ReadKey().KeyChar == 27)
                return;

            System.Threading.Thread.MemoryBarrier();
            var initialMemory = System.GC.GetTotalMemory(true);

            var json = System.IO.File.ReadAllText(testFile);
            var st = DateTime.Now;
            var o = parseFunc(json);
            var tm = (int)DateTime.Now.Subtract(st).TotalMilliseconds;

            System.Threading.Thread.MemoryBarrier();
            var finalMemory = System.GC.GetTotalMemory(true);
            var consumption = finalMemory - initialMemory;

            Console.WriteLine();
            Console.WriteLine("... Done, in {0} ms. Throughput: {1} characters / second.", tm.ToString("0,0"), (1000 * (decimal)json.Length / (tm > 0 ? tm : 1)).ToString("0,0.00"));
            Console.WriteLine();
            Console.WriteLine("\tMemory used : {0}", ((decimal)finalMemory).ToString("0,0"));
            Console.WriteLine();

            if (o is FathersData)
            {
                Console.WriteLine("Fathers : {0}", ((FathersData)o).fathers.Length.ToString("0,0"));
                Console.WriteLine();
            }
            GC.Collect();
            Console.WriteLine("Press a key...");
            Console.WriteLine();
            Console.ReadKey();
        }

        public class BoonSmall
        {
            public string debug { get; set; }
            public IList<int> nums { get; set; }
        }

        public enum Status
        {
            Single,
            Married,
            Divorced
        }

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            // Both string and integral enum value representations can be parsed:
            public Status Status { get; set; }
            public string Address { get; set; }
            // Just to be sure we support that one, too:
            public IEnumerable<int> Scores { get; set; }
            public object Data { get; set; }
        }

        public class FathersData
        {
            public Father[] fathers { get; set; }
        }

        public class Father
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool married { get; set; }
            // Lists...
            public List<Son> sons { get; set; }
            // ... or arrays for collections, that's fine:
            public Daughter[] daughters { get; set; }
        }

        public class Son
        {
            public int age { get; set; }
            public string name { get; set; }
        }

        public class Daughter
        {
            public int age { get; set; }
            public string name { get; set; }
        }

        static void SpeedTests()
        {
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, OJ_TEST_FILE_PATH, 10000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, OJ_TEST_FILE_PATH, 10000);
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, OJ_TEST_FILE_PATH, 10000);
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<object>, OJ_TEST_FILE_PATH, 10000);

            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, OJ_TEST_FILE_PATH, 100000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, OJ_TEST_FILE_PATH, 100000);
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, OJ_TEST_FILE_PATH, 100000);
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<object>, OJ_TEST_FILE_PATH, 100000);

            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 1000000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 1000000);
            LoopTest("ServiceStack", new JsonSerializer<BoonSmall>().DeserializeFromString, BOON_SMALL_TEST_FILE_PATH, 1000000);
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 1000000);

            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 10000000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 10000000);
            LoopTest("ServiceStack", new JsonSerializer<BoonSmall>().DeserializeFromString, BOON_SMALL_TEST_FILE_PATH, 10000000);
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 10000000);

            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<Person>, TINY_TEST_FILE_PATH, 10000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<Person>, TINY_TEST_FILE_PATH, 10000);
            LoopTest("ServiceStack", new JsonSerializer<Person>().DeserializeFromString, TINY_TEST_FILE_PATH, 10000);
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<Person>, TINY_TEST_FILE_PATH, 10000);

            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<Person>, TINY_TEST_FILE_PATH, 100000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<Person>, TINY_TEST_FILE_PATH, 100000);
            LoopTest("ServiceStack", new JsonSerializer<Person>().DeserializeFromString, TINY_TEST_FILE_PATH, 100000);
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<Person>, TINY_TEST_FILE_PATH, 100000);

            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<Person>, TINY_TEST_FILE_PATH, 1000000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<Person>, TINY_TEST_FILE_PATH, 1000000);
            LoopTest("ServiceStack", new JsonSerializer<Person>().DeserializeFromString, TINY_TEST_FILE_PATH, 1000000);
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<Person>, TINY_TEST_FILE_PATH, 1000000);

            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, SMALL_TEST_FILE_PATH, 10000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, SMALL_TEST_FILE_PATH, 10000);
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, SMALL_TEST_FILE_PATH, 10000);
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<object>, SMALL_TEST_FILE_PATH, 10000);

            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, SMALL_TEST_FILE_PATH, 100000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, SMALL_TEST_FILE_PATH, 100000);//(JSON.NET: OutOfMemoryException)
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, SMALL_TEST_FILE_PATH, 100000);
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<object>, SMALL_TEST_FILE_PATH, 100000);

            var msJss = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
            Test(typeof(JavaScriptSerializer).FullName, msJss.Deserialize<FathersData>, FATHERS_TEST_FILE_PATH);
            Test("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<FathersData>, FATHERS_TEST_FILE_PATH);
            Test("ServiceStack", new JsonSerializer<FathersData>().DeserializeFromString, FATHERS_TEST_FILE_PATH);
            Test(typeof(JsonParser).FullName, new JsonParser().Parse<FathersData>, FATHERS_TEST_FILE_PATH);

            Test(typeof(JavaScriptSerializer).FullName, msJss.DeserializeObject, HUGE_TEST_FILE_PATH);
            Test("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, HUGE_TEST_FILE_PATH);//(JSON.NET: OutOfMemoryException)
            //Test("ServiceStack", new JsonSerializer<object>().DeserializeFromString, HUGE_TEST_FILE_PATH);
            Test(typeof(JsonParser).FullName, new JsonParser().Parse<object>, HUGE_TEST_FILE_PATH);

            StreamTest();
        }

        static void StreamTest()
        {
            Console.Clear();
            System.Threading.Thread.MemoryBarrier();
            var initialMemory = System.GC.GetTotalMemory(true);
            using (var reader = new System.IO.StreamReader(FATHERS_TEST_FILE_PATH))
            {
                Console.WriteLine("\"Fathers\" Test... streamed (press a key)");
                Console.WriteLine();
                Console.ReadKey();
                var st = DateTime.Now;
                var o = new JsonParser().Parse<FathersData>(reader);
                var tm = (int)DateTime.Now.Subtract(st).TotalMilliseconds;

                System.Threading.Thread.MemoryBarrier();
                var finalMemory = System.GC.GetTotalMemory(true);
                var consumption = finalMemory - initialMemory;

                System.Diagnostics.Debug.Assert(o.fathers.Length == 30000);
                Console.WriteLine();
                Console.WriteLine("... {0} ms", tm);
                Console.WriteLine();
                Console.WriteLine("\tMemory used : {0}", ((decimal)finalMemory).ToString("0,0"));
                Console.WriteLine();
            }
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            SpeedTests();
        }
    }
}
