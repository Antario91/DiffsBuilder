using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffsBuilder
{
    class SchoolFlowJSON
    {
        static void Maind(string[] args)
        {
            Pupil pashka = new Pupil
            {
                Name = "Pashka",
                Age = 15
            };

            Pupil jeka = new Pupil
            {
                Name = "Jeka",
                Age = 18
            };

            Pupil sashka = new Pupil
            {
                Name = "Sashka",
                Age = 21
            };

            Pupil stasik = new Pupil
            {
                Name = "Stasik",
                Age = 35
            };

            School school96 = new School
            {
                Name = "96",
                Pupil = new List<Pupil>()
                {
                    pashka
                }
            };

            School school143 = new School
            {
                Name = "143",
                Pupil = new List<Pupil>()
                {
                    pashka, jeka, sashka
                }
            };

            School school54 = new School
            {
                Name = "54",
                Pupil = new List<Pupil>
                {
                    stasik, pashka, sashka
                }
            };

            var serialized96 = JsonConvert.SerializeObject(school96);
            var serialized143 = JsonConvert.SerializeObject(school143);
            var serialized54 = JsonConvert.SerializeObject(school54);

            var jdp = new JsonDiffPatch();
            JToken diffResult = jdp.Diff(serialized143, serialized54);

            var output = jdp.Patch(serialized143, diffResult);
            var output1 = jdp.Patch(serialized54, diffResult);
            var output2 = jdp.Unpatch(serialized143, diffResult);
            var output3 = jdp.Unpatch(serialized54, diffResult);

            var obj1 = new { Key = 1 };
            var obj2 = new { Key = 2 };
            var serObj1 = JsonConvert.SerializeObject(obj1);
            var serObj2 = JsonConvert.SerializeObject(obj2);

            JToken patch = jdp.Diff(serObj1, serObj2);
            var output111 = jdp.Patch(serialized143, patch);
            var output11 = jdp.Patch(serObj1, patch);
            var output21 = jdp.Unpatch(serObj2, patch);

            var left = JToken.FromObject(school143);
            var right = JToken.FromObject(school54);
            JToken patch1 = jdp.Diff(left, right);
            var result = patch1.Values();

            //var output1111 = jdp.Patch(left, patch1);

            Console.WriteLine(patch1.ToString());


            var a = true;
        }
    }
}
