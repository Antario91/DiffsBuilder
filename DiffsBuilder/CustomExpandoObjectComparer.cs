using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace DiffsBuilder
{

    public class CustomExpandoObjectComparer : BaseTypeComparer
    {

        private readonly CustomPropertyComparer propertyComparer;
        private readonly FieldComparer fieldComparer;

        public CustomExpandoObjectComparer(RootComparer rootComparer) : base(rootComparer)
        {
            propertyComparer = new CustomPropertyComparer(rootComparer);
            fieldComparer = new FieldComparer(rootComparer);
        }

        public override void CompareType(CompareParms parms)
        {
            parms.Object1Type = parms.Object1?.GetType();
            parms.Object2Type = parms.Object2?.GetType();
            
            IDictionary<string, object> object1Properties = (IDictionary<string, object>)parms.Object1;
            IDictionary<string, object> object2Properties = (IDictionary<string, object>)parms.Object2;

            object object1 = null;
            object object2 = null;
            foreach (var propertyName in GetComparableProperties(object1Properties.Keys, object2Properties.Keys))
            {
                if (parms.Result.ExceededDifferences)
                    return;

                CompareParms childParms = new CompareParms
                {
                    Result = parms.Result,
                    Config = parms.Config,
                    ParentObject1 = parms.Object1,
                    ParentObject2 = parms.Object2,
                    Object1 = object1Properties.TryGetValue(propertyName, out object1) ? object1 : null,
                    Object2 = object2Properties.TryGetValue(propertyName, out object2) ? object2 : null,
                    BreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, propertyName)
                };

                RootComparer.Compare(childParms);
            }
        }

        private ISet<string> GetComparableProperties(ICollection<string> object1Properties, ICollection<string> object2Properties)
        {
            ISet<string> result = new HashSet<string>(object1Properties);
            result.UnionWith(object2Properties);
            return result;
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return typeof(ExpandoObject).IsAssignableFrom(type1) && type2 == null
                || typeof(ExpandoObject).IsAssignableFrom(type2) && type1 == null
                || typeof(ExpandoObject).IsAssignableFrom(type1) && typeof(ExpandoObject).IsAssignableFrom(type2);
        }
    }

}