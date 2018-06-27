using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace Scc.Portal.Orchard.AuditTrails
{
    public class FirstNullClassComparer : BaseTypeComparer
    {
        public FirstNullClassComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        public override void CompareType(CompareParms parms)
        {
            object object2 = parms.Object2;
            PropertyInfo[] properties = object2.GetType().GetProperties();
            if (properties != null)
            {
                addDifferences(properties, object2, parms.BreadCrumb, parms.Result);
            }
        }

        // TODO this method is currently recursive !!! - consider refuse recursion in order not to get StackOverflow
        private void addDifferences(PropertyInfo[] properties, object parentObject, string parentPath, ComparisonResult result)
        {
            properties.ToList().ForEach(property =>
            {
                Object currentObject = null;
                try
                {
                    currentObject = property.GetValue(parentObject);
                }
                catch (Exception exc)
                {
                    // TODO check
                    // cannot get property
                }
                if (currentObject != null)
                {
                    string currentPath = parentPath + "." + property.Name;
                    if (typeof(ExpandoObject).IsAssignableFrom(currentObject.GetType()))
                    {
                        var comparer = new ExpandoObjectComparer(RootComparer);
                        comparer.AddDifferences((ExpandoObject)currentObject, currentPath, result);
                    }
                    else if (typeMatches(currentObject.GetType()))
                    {
                        PropertyInfo[] childProperties = currentObject.GetType().GetProperties();
                        if (childProperties != null)
                        {
                            addDifferences(childProperties, currentObject, currentPath, result);
                        }
                    }
                    else
                    {
                        Difference difference = new Difference
                        {
                            ParentObject1 = null,
                            ParentObject2 = parentObject,
                            PropertyName = currentPath,
                            Object1Value = NiceString(null),
                            Object2Value = NiceString(currentObject),
                            Object1 = null,
                            Object2 = currentObject
                        };

                        AddDifference(result, difference);
                    }
                }
            });
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return type1 == null && typeMatches(type2);
        }

        private bool typeMatches(Type type)
        {
            return TypeHelper.IsClass(type) && type != typeof(string);
        }
    }
}