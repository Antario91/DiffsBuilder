using DiffsBuilder;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scc.Portal.Orchard.AuditTrails.DiffsBuilder
{


    public static class DiffsBuilder
    {
        public class Diffs
        {
            public string FieldName { get; set; }
            public OperationType OperationType { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
            public ICollection<Diffs> ChildDiffs { get; set; } = new List<Diffs>();
        }

        public static IEnumerable<Diffs> BuildDiffs(Object state1, Object state2)
        {
            //ComparisonConfig config = new ComparisonConfig();
            //config.MaxDifferences = int.MaxValue;
            //config.CustomComparers = new List<BaseTypeComparer>();
            //config.CustomComparers.Add(new ExpandoObjectComparer(RootComparerFactory.GetRootComparer()));
            //config.CustomComparers.Add(new FirstNullClassComparer(RootComparerFactory.GetRootComparer()));
            //config.CustomComparers.Add(new CollectionComparer(RootComparerFactory.GetRootComparer()));
            ComparisonConfig config = new ComparisonConfig();
            config.MaxDifferences = int.MaxValue;
            config.CompareChildren = true;
            config.CustomComparers.Add(new CustomListComparer(RootComparerFactory.GetRootComparer()));
            config.CustomComparers.Add(new CustomExpandoObjectComparer(RootComparerFactory.GetRootComparer()));
            config.CustomComparers.Add(new NullToObjectComparer(RootComparerFactory.GetRootComparer()));
            CompareLogic compareLogic = new CompareLogic(config);
            ComparisonResult result = compareLogic.Compare(state1, state2);
            IEnumerable<Diffs> diffs = DiffsFromComparisonResult(result);
            return diffs;
            //return diffs.Select(Translate);
        }

        private static OperationType ResolveOperationType(object oldObj, object newObj)
        {
            if (oldObj == null && newObj != null)
            {
                return OperationType.Inserted;
            }
            else if (oldObj != null && newObj != null)
            {
                return OperationType.Modified;
            }
            else if (oldObj != null && newObj == null)
            {
                return OperationType.Deleted;
            }
            else
            {
                throw new Exception();
            }
        }

        private static string ResolveListParentProperty(string propertyName)
        {
            return propertyName.Contains('[')
                ? propertyName.Substring(0, propertyName.IndexOf('['))
                : propertyName;
        }

        private static string ResolveParent(string propertyName)
        {
            return propertyName.Contains('.')
                ? propertyName.Substring(0, propertyName.LastIndexOf('.'))
                : "";
        }

        private static string ResolveChild(string propertyName)
        {
            if (propertyName.Contains("["))
            {
                Regex regex = new Regex(@"\w+\[\d+\]\.\w+$");
                return regex.Match(propertyName).Value;
            }
            
            //if (propertyName.Contains("["))
            //{
            //    if (propertyName.Contains("."))
            //    {
            //        foreach propertyName.Split('.');
            //    }
            //}
            return propertyName.Contains(".")
            ? propertyName.Substring(propertyName.LastIndexOf(".") + 1)
            : propertyName;
        }

        //private static string ResolveGrantParent(string propertyName)
        //{
        //    return ResolveParent(ResolveParent(propertyName)).Equals(propertyName)
        //        ? ""
        //        : ResolveParent(ResolveParent(propertyName));
        //}

        //private static void ClearChildProperties(IList<Diffs> diffs)
        //{
        //    diffs.ToList().ForEach(d =>
        //    {
        //        d.FieldName = ResolveChild(d.FieldName);
        //        d.ChildDiffs
        //    });
        //}

        private static IEnumerable<Diffs> DiffsFromComparisonResult(ComparisonResult comparisonResult)
        {
            //IList<Diffs> result = new List<Diffs>();
            //string parentMarker = null;
            //string parent = null;
            IList<Diffs> childDiffs = new List<Diffs>();

            IDictionary<string, IList<Diffs>> childMapping = new Dictionary<string, IList<Diffs>>();
            foreach (Difference difference in comparisonResult.Differences/*.Reverse<Difference>()*/)
            {
                if (!childMapping.TryGetValue(ResolveListParentProperty(difference.ParentPropertyName), out childDiffs))
                {
                    childDiffs = new List<Diffs>();
                    childMapping.Add(ResolveListParentProperty(difference.ParentPropertyName), childDiffs);
                }

                var diffs = new Diffs
                {
                    FieldName = ResolveChild(difference.PropertyName),
                    OldValue = difference.Object1Value,
                    NewValue = difference.Object2Value,
                    OperationType = ResolveOperationType(difference.Object1Value, difference.Object2Value)
                };

                childDiffs.Add(diffs);
            }

            var enumerator = childMapping.Keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
                //ResolveParent(key)//with lastIndexOf delimiter (получить коллекцию от ключ-однаНода)
                var diff = new Diffs
                {
                    FieldName = ResolveChild(enumerator.Current),
                    OldValue = "",
                    NewValue = "",
                    OperationType = OperationType.Modified,
                    ChildDiffs = childMapping[enumerator.Current]
                };

                if (!enumerator.Current.Equals(""))
                    childMapping[ResolveParent(enumerator.Current)].Add(diff);
                //childMapping.Remove(enumerator.Current);
            }

            //ClearChildProperties(childMapping[""]);

            return childMapping[""];



            //IEnumerable<Diffs> diffs = Enumerable.Empty<Diffs>();
            //if (!result.AreEqual)
            //{
            //    Diffs root = new Diffs();
            //    root.ChildDiffs = new List<Diffs>();
            //    IDictionary<string, Diffs> nodePaths = new Dictionary<string, Diffs>();
            //    nodePaths[string.Empty] = root;
            //    foreach (Difference currentDiff in result.Differences)
            //    {
            //        String oldValue = getValue(currentDiff.Object1, currentDiff.Object1Value);
            //        String newValue = getValue(currentDiff.Object2, currentDiff.Object2Value);
            //        Diffs currentChild = new Diffs()
            //        {
            //            FieldName = propertyNameFromPath(currentDiff.PropertyName),
            //            OldValue = oldValue,
            //            NewValue = newValue
            //        };
            //        Diffs parent = getParent(currentDiff.ParentPropertyName, nodePaths);
            //        parent.ChildDiffs.Add(currentChild);
            //    }
            //    diffs = root.ChildDiffs;
            //}

            //return diffs;
        }

        private static string propertyNameFromPath(string path)
        {
            string propertyName = path;
            int index = path.LastIndexOf(".");
            if (index != -1)
            {
                propertyName = path.Substring(index + 1);
            }
            return propertyName;
        }

        // TODO this method is currently recursive !!! - consider refuse recursion in order not to get StackOverflow
        private static Diffs getParent(string parentPath, IDictionary<string, Diffs> nodePaths)
        {
            Diffs parent = null;
            if (nodePaths.ContainsKey(parentPath))
            {
                parent = nodePaths[parentPath];
            }
            if (parent == null)
            {
                parent = new Diffs()
                {
                    ChildDiffs = new List<Diffs>(),
                    FieldName = parentPath
                };
                nodePaths[parentPath] = parent;
                String parentsParentPath = string.Empty;
                int index = parentPath.LastIndexOf(".");
                if (index == -1)
                {
                    nodePaths[parentsParentPath].ChildDiffs.Add(parent);
                }
                else
                {
                    parentsParentPath = parentPath.Substring(0, index);
                    Diffs parentsParent = getParent(parentsParentPath, nodePaths);
                    parentsParent.ChildDiffs.Add(parent);
                }
            }
            return parent;
        }

        private static string getValue(object obj, string objValue)
        {
            String value = null;
            if (obj != null)
            {
                value = objValue;
            }
            return value;
        }

        //private static AuditTrails.Diffs Translate(Diffs diffs)
        //{
        //    return new AuditTrails.Diffs
        //    (
        //        diffs.FieldName,
        //        diffs.OperationType,
        //        diffs.Code,
        //        diffs.OldValue,
        //        diffs.NewValue,
        //        diffs.ChildDiffs != null && diffs.ChildDiffs.Any() ? diffs.ChildDiffs.Select(Translate) : Enumerable.Empty<AuditTrails.Diffs>()
        //    );
        //}
    }
}