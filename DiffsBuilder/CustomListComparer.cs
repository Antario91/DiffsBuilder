using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using KellermanSoftware.CompareNetObjects.IgnoreOrderTypes;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare objects that implement IList
    /// </summary>
    public class CustomListComparer : BaseTypeComparer
    {
        private const string iReadOnlyList = "IReadOnlyList";

        private readonly CustomPropertyComparer _propertyComparer;
        private readonly FieldComparer _fieldComparer;

        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public CustomListComparer(RootComparer rootComparer) : base(rootComparer)
        {
            _propertyComparer = new CustomPropertyComparer(rootComparer);
            _fieldComparer = new FieldComparer(rootComparer);
        }

        /// <summary>
        /// Returns true if both objects implement IList
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            //stype1.IsAssignableFrom(typeof(IReadOnlyList<>))
            return TypeHelper.IsIList(type1) && TypeHelper.IsIList(type2)
                 || IsIReadOnlyList(type1) && type1 == type2
                 || (TypeHelper.IsIList(type1) || IsIReadOnlyList(type1)) && type2 == null
                 || (TypeHelper.IsIList(type2) || IsIReadOnlyList(type2)) && type1 == null;
        }


        /// <summary>
        /// Compare two objects that implement IList
        /// </summary>
        public override void CompareType(CompareParms parms)
        {
            Type t1 = parms.Object1?.GetType();
            Type t2 = parms.Object2?.GetType();

            //Check if the class type should be excluded based on the configuration
            //if (ExcludeLogic.ShouldExcludeClass(parms.Config, t1, t2))
            //    return;

            parms.Object1Type = t1;
            parms.Object2Type = t2;

            if (parms.Result.ExceededDifferences)
                return;

            CompareItems(parms);


            //Properties on the root of a collection
            //CompareProperties(parms);
           // CompareFields(parms);


        }

        private void CompareFields(CompareParms parms)
        {
            if (parms.Config.CompareFields)
            {
                _fieldComparer.PerformCompareFields(parms);
            }
        }

        private void CompareProperties(CompareParms parms)
        {
            if (parms.Config.CompareProperties)
            {
                _propertyComparer.PerformCompareProperties(parms, true);
            }
        }


        private void CompareItems(CompareParms parms)
        {
            int count = 0;
            IEnumerator enumerator1 = ((IList)parms.Object1)?.GetEnumerator();
            IEnumerator enumerator2 = ((IList)parms.Object2)?.GetEnumerator();

            var isNextObj1Exists = enumerator1 != null ? enumerator1.MoveNext() : false;
            var isNextObj2Exists = enumerator2 != null ? enumerator2.MoveNext() : false;

            while (isNextObj1Exists || isNextObj2Exists)
            {
                string currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, string.Empty, string.Empty, count);

                CompareParms childParms = new CompareParms
                {
                    Result = parms.Result,
                    Config = parms.Config,
                    ParentObject1 = parms.Object1,
                    ParentObject2 = parms.Object2,
                    Object1 = isNextObj1Exists ? enumerator1.Current : null,
                    Object2 = isNextObj2Exists ? enumerator2.Current : null,
                    BreadCrumb = currentBreadCrumb
                };

                RootComparer.Compare(childParms);

                if (parms.Result.ExceededDifferences)
                    return;

                count++;
                isNextObj1Exists = enumerator1 != null ? enumerator1.MoveNext() : false;
                isNextObj2Exists = enumerator2 != null ? enumerator2.MoveNext() : false;
            }
        }

        public static bool IsIReadOnlyList(Type type)
        {
            if (type != null)
            {
                foreach (var implementedInterface in type.GetInterfaces())
                {
                    if (implementedInterface.Name.Contains(iReadOnlyList))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
