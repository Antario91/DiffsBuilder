using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Scc.Portal.Orchard.AuditTrails;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffsBuilder
{
    class SchoolFlowComparer
    {
        static void Main(string[] args)
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
                },
                Room = new Room { Color = "red", Table = new Table { Form = "circle" } }
            };

            School school54 = new School
            {
                Name = "54",
                Pupil = new List<Pupil>
                {
                    jeka, pashka
                },
                Room = new Room { Color = "green", Table = new Table { Form = "rectangle" } }
            };

            ReadOnlySchool school104 = new ReadOnlySchool
            {
                Name = "104",
                pupil = new List<Pupil>
                {
                    jeka, pashka
                }
            };

            ComparisonConfig config = new ComparisonConfig();
            config.MaxDifferences = int.MaxValue;
            config.CompareChildren = true;
            //config.IgnoreCollectionOrder = true;
            config.CustomComparers.Add(new CustomListComparer(RootComparerFactory.GetRootComparer()));
            config.CustomComparers.Add(new CustomExpandoObjectComparer(RootComparerFactory.GetRootComparer()));
            config.CustomComparers.Add(new NullToObjectComparer(RootComparerFactory.GetRootComparer()));
            ////config.CustomComparers = new List<BaseTypeComparer>();
            ////config.CustomComparers.Add(new ExpandoObjectComparer(RootComparerFactory.GetRootComparer()));
            ////config.CustomComparers.Add(new FirstNullClassComparer(RootComparerFactory.GetRootComparer()));
            ////config.CustomComparers.Add(new CollectionComparer(RootComparerFactory.GetRootComparer()));

            CompareLogic compareLogic = new CompareLogic(config);
            //ComparisonResult result = compareLogic.Compare(school143, school54);

            //var diffsResult = Scc.Portal.Orchard.AuditTrails.DiffsBuilder.DiffsBuilder.BuildDiffs(school143, school54);

            dynamic a = new ExpandoObject();
            dynamic b = new ExpandoObject();

            a.Table = "Table";
            b.Table = "Chair";
            b.Wall = "Rectangle";
            b.Pupil = new HashSet<Pupil>
                {
                    jeka, pashka
                };

            ComparisonResult expandoResult = compareLogic.Compare(a, b);

            var diffsResult = Scc.Portal.Orchard.AuditTrails.DiffsBuilder.DiffsBuilder.BuildDiffs(a, b);

            var yes = true;
        }
    }
}
