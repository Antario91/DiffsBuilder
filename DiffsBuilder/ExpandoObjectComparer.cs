using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Web;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.IgnoreOrderTypes;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace Scc.Portal.Orchard.AuditTrails
{
    public class ExpandoObjectComparer : BaseTypeComparer
    {
        private readonly CustomPropertyComparer propertyComparer;
        private readonly FieldComparer fieldComparer;

        public ExpandoObjectComparer(RootComparer rootComparer) : base(rootComparer)
        {
            propertyComparer = new CustomPropertyComparer(rootComparer);
            fieldComparer = new FieldComparer(rootComparer);
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            bool match1 = typeof(ExpandoObject).IsAssignableFrom(type1);
            bool match2 = typeof(ExpandoObject).IsAssignableFrom(type2);
            return match1 && match2;
        }

        public override void CompareType(CompareParms parms)
        {
            //This should never happen, null check happens one level up
            if (parms.Object1 == null || parms.Object2 == null)
                return;

            if (parms.Result.ExceededDifferences)
                return;

            if (parms.Config.IgnoreCollectionOrder)
            {
                //Objects must be the same length
                bool countsDifferent = DictionaryCountsDifferent(parms);
                IgnoreOrderLogic logic = new IgnoreOrderLogic(RootComparer);
                logic.CompareEnumeratorIgnoreOrder(parms, countsDifferent);
            }
            else
            {
                CompareEachItem(parms);
            }
        }

        private void CompareEachItem(CompareParms parms)
        {
            var enumerator1 = ((IDictionary<string, object>)parms.Object1).GetEnumerator();

            IDictionary<string, object> object2 = ((IDictionary<string, object>)parms.Object2);

            ISet<string> object1Keys = new HashSet<string>();

            while (enumerator1.MoveNext())
            {
                string currentKey = enumerator1.Current.Key;
                object1Keys.Add(currentKey);

                string currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, currentKey);

                if (object2.ContainsKey(currentKey))
                {
                    object object2Value = object2[currentKey];

                    CompareParms childParms = new CompareParms
                    {
                        Result = parms.Result,
                        Config = parms.Config,
                        ParentObject1 = parms.Object1,
                        ParentObject2 = parms.Object2,
                        Object1 = enumerator1.Current.Value,
                        Object2 = object2Value,
                        BreadCrumb = currentBreadCrumb
                    };

                    RootComparer.Compare(childParms);

                    if (parms.Result.ExceededDifferences)
                        return;
                }
                else
                {
                    // deleted properties
                    Difference difference = new Difference
                    {
                        ParentObject1 = parms.Object1,
                        ParentObject2 = parms.Object2,
                        PropertyName = currentBreadCrumb,
                        Object1Value = enumerator1.Current.Value.ToString(),
                        Object2Value = null,
                        ChildPropertyName = null,
                        Object1 = enumerator1.Current.Value,
                        Object2 = null
                    };

                    AddDifference(parms.Result, difference);
                }
            }


            IEnumerable<KeyValuePair<string, object>> addedEntries =
                object2.Where(currentEntry => !object1Keys.Contains(currentEntry.Key));

            addDifferences(addedEntries, object2, parms.BreadCrumb, parms.Result);
        }

        public void AddDifferences(ExpandoObject state, string path, ComparisonResult comparisonResult)
        {
            addDifferences(state, state, path, comparisonResult);
        }

        private void addDifferences(IEnumerable<KeyValuePair<string, object>> addedEntries, object parentObject,
            string parentPath, ComparisonResult comparisonResult)
        {
            addedEntries.ToList().ForEach(entry =>
            {
                string propertyPath = parentPath + "." + entry.Key;
                if (typeof(IDictionary<string, object>).IsAssignableFrom(entry.Value.GetType()) &&
                    ((IDictionary<string, object>)entry.Value).Any())
                {
                    IEnumerable<KeyValuePair<string, object>> childEntries = (IDictionary<string, object>)entry.Value;
                    addDifferences(childEntries, entry, propertyPath, comparisonResult);
                }
                else
                {
                    Difference difference = new Difference
                    {
                        ParentObject1 = null,
                        ParentObject2 = entry,
                        PropertyName = propertyPath,
                        Object1Value = null,
                        Object2Value = entry.ToString(),
                        ChildPropertyName = null,
                        Object1 = null,
                        Object2 = entry
                    };

                    AddDifference(comparisonResult, difference);
                }
            });
        }

        private bool DictionaryCountsDifferent(CompareParms parms)
        {
            IDictionary<string, object> iDict1 = parms.Object1 as IDictionary<string, object>;
            IDictionary<string, object> iDict2 = parms.Object2 as IDictionary<string, object>;

            if (iDict1 == null)
                throw new ArgumentException("parms.Object1");

            if (iDict2 == null)
                throw new ArgumentException("parms.Object2");

            if (iDict1.Count != iDict2.Count)
            {
                Difference difference = new Difference
                {
                    ParentObject1 = parms.ParentObject1,
                    ParentObject2 = parms.ParentObject2,
                    PropertyName = parms.BreadCrumb,
                    Object1Value = iDict1.Count.ToString(CultureInfo.InvariantCulture),
                    Object2Value = iDict2.Count.ToString(CultureInfo.InvariantCulture),
                    ChildPropertyName = "Count",
                    Object1 = iDict1,
                    Object2 = iDict2
                };

                AddDifference(parms.Result, difference);

                return true;
            }
            return false;
        }
    }
}