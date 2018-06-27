using JsonDiffPatchDotNet;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffsBuilder
{
    class Program
    {
        static void Mains(string[] args)
        {
            Test3 test3Old1 = new Test3
            {
                Strings = new List<string> { "qwe", "asd" }
            };

            Test3 test3Old2 = new Test3
            {
                Strings = new List<string> { "zxc", "vbn" }
            };

            Test3 test3new1 = new Test3
            {
                Strings = new List<string> { "ghj", "kl;" }
            };

            TestAudit2 testAudit2Old1 = new TestAudit2
            {
                StringProp = "testAudit2Old1",
                Tests3 = new Test3[] { test3Old1 }
            };

            TestAudit2 testAudit2New1 = new TestAudit2
            {
                StringProp = "testAudit2New1",
                Tests3 = new Test3[] { test3Old1, test3new1 }
            };

            TestAudit2 testAudit2Old2 = new TestAudit2
            {
                StringProp = "testAudit2Old2",
                Tests3 = new Test3[] { test3Old2 }
            };



            TestAudit1 testAudit1Old = new TestAudit1
            {
                Name = "NameOld",
                Number = 1,
                TestAudits = new List<TestAudit2> { testAudit2Old1 }
            };

            TestAudit1 testAudit1New = new TestAudit1
            {
                Name = "NameNew",
                Number = 2,
                TestAudits = new List<TestAudit2> { testAudit2New1, testAudit2Old2 }
            };

            //var testAudit1OldJSON = JsonConvert.SerializeObject(testAudit1Old);
            //var testAudit1NewJSON = JsonConvert.SerializeObject(testAudit1New);

            //var jdp = new JsonDiffPatch();
            //JToken diffResult = jdp.Diff(testAudit1OldJSON, testAudit1NewJSON);

            //var a = true;

            //ComparisonConfig config = new ComparisonConfig();
            //config.MaxDifferences = int.MaxValue;
            //config.CompareChildren = true;
            //config.IgnoreCollectionOrder = true;
            ////config.CustomComparers = new List<BaseTypeComparer>();
            ////config.CustomComparers.Add(new ExpandoObjectComparer(RootComparerFactory.GetRootComparer()));
            ////config.CustomComparers.Add(new FirstNullClassComparer(RootComparerFactory.GetRootComparer()));
            ////config.CustomComparers.Add(new CollectionComparer(RootComparerFactory.GetRootComparer()));


            //CompareLogic compareLogic = new CompareLogic(config);
            //ComparisonResult result = compareLogic.Compare(test3Old1, test3Old2);

            //var b = JsonConvert.SerializeObject(new A { Prop1 = 1, Prop2 = "prop2" });

            var a = true;
        }
    }

    class A
    {
        public int Prop1 { get; set; }
        [JsonIgnore]
        public string Prop2 { get; set; }
    }
}
