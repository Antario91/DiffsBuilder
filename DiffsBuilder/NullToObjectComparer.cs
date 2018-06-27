using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiffsBuilder
{
    public class NullToObjectComparer : BaseTypeComparer
    {
        private readonly IEnumerable<string> ignoreProperties = new List<string>() { "Count", "Capacity", "Item" };
        private static readonly CustomListComparer listComparer = new CustomListComparer(RootComparerFactory.GetRootComparer());
        private static readonly CustomExpandoObjectComparer customExpandoObjectComparer = new CustomExpandoObjectComparer(RootComparerFactory.GetRootComparer());
        public NullToObjectComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        public override void CompareType(CompareParms parms)
        {
            if (TypeHelper.IsIList(parms.Object1?.GetType())
                || TypeHelper.IsIList(parms.Object2?.GetType())
                || CustomListComparer.IsIReadOnlyList(parms.Object1?.GetType())
                || CustomListComparer.IsIReadOnlyList(parms.Object2?.GetType()))
            {
                listComparer.CompareType(parms);
            }

            if (typeof(ExpandoObject).IsAssignableFrom(parms.Object1?.GetType())
                || typeof(ExpandoObject).IsAssignableFrom(parms.Object2?.GetType()))
            {
                customExpandoObjectComparer.CompareType(parms);
            }

            if (typeof(string).Equals(parms.Object1?.GetType())
                || typeof(string).Equals(parms.Object2?.GetType()))
            {

            }

            if (parms.Object1 != null && parms.Object1.GetType().IsClass
                || parms.Object1 != null && parms.Object1.GetType().IsStruct()
                || parms.Object2 != null && parms.Object2.GetType().IsClass
                || parms.Object2 != null && parms.Object2.GetType().IsStruct())
            {
                var properties = parms.Object1 == null ? parms.Object2.GetType().GetProperties() : parms.Object1.GetType().GetProperties();

                foreach(var property in properties)
                {
                    if (ignoreProperties.Contains(property.Name))
                    {
                        continue;
                    }
                    CompareParms childParms = new CompareParms
                    {
                        Result = parms.Result,
                        Config = parms.Config,
                        ParentObject1 = parms.Object1,
                        ParentObject2 = parms.Object2,
                        Object1 = parms.Object1 != null ? property.GetValue(parms.Object1) : null,
                        Object2 = parms.Object2 != null ? property.GetValue(parms.Object2) : null,
                        BreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, property.Name)
                    };
                    RootComparer.Compare(childParms);
                }
                
            }


            //var propertiesInfo = parms.Object1 == null ? parms.Object2.GetType().GetProperties() : parms.Object1.GetType().GetProperties();

            if (parms.Object1 != null && parms.Object1.GetType().IsPrimitive
                || parms.Object2 != null && parms.Object2.GetType().IsPrimitive)
            {
                Difference difference = new Difference
                {
                    ParentObject1 = parms.Object1,
                    ParentObject2 = parms.Object2,
                    PropertyName = parms.BreadCrumb,
                    Object1Value = NiceString(parms.Object1),
                    Object2Value = NiceString(parms.Object2),
                    Object1 = parms.Object1,
                    Object2 = parms.Object2
                };
                AddDifference(parms.Result, difference);
            }

            //foreach (var propertyInfo in propertiesInfo)
            //{

            //    Difference difference = new Difference
            //    {
            //        ParentObject1 = parms.Object1,
            //        ParentObject2 = parms.Object2,
            //        PropertyName = propertyInfo.Name,
            //        Object1Value = NiceString(parms.Object1 == null ? null : propertyInfo.GetValue(parms.Object1)),
            //        Object2Value = NiceString(parms.Object2 == null ? null : propertyInfo.GetValue(parms.Object2)),
            //        Object1 = parms.Object1 == null ? null : propertyInfo.GetValue(parms.Object1),
            //        Object2 = parms.Object2 == null ? null : propertyInfo.GetValue(parms.Object2)
            //    };
            //}

        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return type1 == null && !typeof(string).Equals(type2)
                || type2 == null && !typeof(string).Equals(type1);
        }
    }
}
