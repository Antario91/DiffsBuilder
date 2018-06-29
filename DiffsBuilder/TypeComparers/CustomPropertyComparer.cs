using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Net;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare two properties (Note inherits from BaseComparer instead of TypeComparer
    /// </summary>
    public class CustomPropertyComparer : BaseComparer
    {
        private readonly RootComparer _rootComparer;
        private readonly IndexerComparer _indexerComparer;

        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public CustomPropertyComparer(RootComparer rootComparer)
        {
            _rootComparer = rootComparer;
            _indexerComparer = new IndexerComparer(rootComparer);
        }

        /// <summary>
        /// Compare the properties of a class
        /// </summary>
        public void PerformCompareProperties(CompareParms parms, bool ignoreBaseList= false)
        {
            if (parms.Object1 == null && parms.Object2 == null)
            {
                throw new ArgumentNullException("parms.Object1 and parms.Object2");
            }

            string[] baseList = {"Count", "Capacity", "Item"};

            IDictionary<string, PropertyEntity> object1Properties = 
                parms.Object1 == null 
                ? new Dictionary<string, PropertyEntity>() 
                : GetCurrentProperties(parms, parms.Object1, parms.Object1Type).ToDictionary(p => p.Name);
            IDictionary<string, PropertyEntity> object2Properties = 
                parms.Object2 == null 
                ? new Dictionary<string, PropertyEntity>() 
                : GetCurrentProperties(parms, parms.Object2, parms.Object2Type).ToDictionary(p => p.Name);
            IEnumerable<string> propertyNames = object1Properties.Keys.Union(object2Properties.Keys);

            PropertyEntity object1Property = null;
            PropertyEntity object2Property = null;
            foreach (string name in propertyNames)
            {
                if (ignoreBaseList && baseList.Contains(name))
                    continue;

                object1Properties.TryGetValue(name, out object1Property);
                object2Properties.TryGetValue(name, out object2Property);
                CompareProperty(parms, object1Property, object2Property);

                if (parms.Result.ExceededDifferences)
                    return;
            }
        }

        private IDictionary<string, PropertyEntity> GetPropertyNameToValueMapping(ISet<PropertyEntity> properties)
        {
            return properties.ToDictionary(p => p.Name);
        }

        private ISet<PropertyEntity> GetComparableProperties(CompareParms parms)
        {
            ISet<PropertyEntity> comparableProperties = new HashSet<PropertyEntity>();
            ISet<PropertyEntity> object1Properties = parms.Object1 == null ? new HashSet<PropertyEntity>() : GetCurrentProperties(parms, parms.Object1, parms.Object1Type);
            ISet<PropertyEntity> object2Properties = parms.Object2 == null ? new HashSet<PropertyEntity>() : GetCurrentProperties(parms, parms.Object2, parms.Object2Type);
            comparableProperties.UnionWith(object1Properties);
            comparableProperties.UnionWith(object2Properties);
            return comparableProperties;
        }

        /// <summary>
        /// Compare a single property of a class
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="info"></param>
        /// <param name="object2Properties"></param>
        private void CompareProperty(CompareParms parms, PropertyEntity object1Property, PropertyEntity object2Property)
        {
            //If we can't read it, skip it
            if (object1Property?.CanRead == false || object2Property.CanRead == false)
                return;

            //Skip if this is a shallow compare
            if (!parms.Config.CompareChildren && TypeHelper.CanHaveChildren(object1Property?.PropertyType)
                || !parms.Config.CompareChildren && TypeHelper.CanHaveChildren(object2Property.PropertyType))
                return;

            //Skip if it should be excluded based on the configuration
            if (object1Property?.PropertyInfo != null && ExcludeLogic.ShouldExcludeMember(parms.Config, object1Property.PropertyInfo)
                || object2Property.PropertyInfo != null && ExcludeLogic.ShouldExcludeMember(parms.Config, object2Property.PropertyInfo))
                return;

            //If we should ignore read only, skip it
            if (!parms.Config.CompareReadOnly && object1Property?.CanWrite == false
                || !parms.Config.CompareReadOnly && object2Property.CanWrite == false)
                return;

            //If the property does not exist, and we are ignoring the object types, skip it
            //We need to disscus this!!!!
            //if (parms.Config.IgnoreObjectTypes && secondObjectInfo == null)
            //    return;

            object objectValue1 = object1Property?.Value;
            object objectValue2 = object2Property?.Value;

            /*
            //need deep investigation about indexerComparer!!!
            if (!IsValidIndexer(parms.Config, object1Property, parms.BreadCrumb))
            {
                objectValue1 = object1Property.Value;
                objectValue2 = object2Property != null ? object2Property.Value : null;
            }
            else
            {
                _indexerComparer.CompareIndexer(parms, object1Property);
                return;
            }
            //need deep investigation about indexerComparer!!!
            */

            bool object1IsParent = objectValue1 != null && (objectValue1 == parms.Object1 || parms.Object1.GetHashCode().Equals(objectValue1.GetHashCode()));
            bool object2IsParent = objectValue2 != null && (objectValue2 == parms.Object2 || parms.Object2.GetHashCode().Equals(objectValue2.GetHashCode()));

            //Skip properties where both point to the corresponding parent
            if ((TypeHelper.IsClass(object1Property?.PropertyType) || TypeHelper.IsInterface(object1Property?.PropertyType) || TypeHelper.IsStruct(object1Property?.PropertyType)
                || TypeHelper.IsClass(object2Property.PropertyType) || TypeHelper.IsInterface(object2Property.PropertyType) || TypeHelper.IsStruct(object2Property.PropertyType))
                && (object1IsParent && object2IsParent))
            {
                return;
            }

            string currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, object1Property.Name);

            CompareParms childParms = new CompareParms
            {
                Result = parms.Result,
                Config = parms.Config,
                ParentObject1 = parms.Object1,
                ParentObject2 = parms.Object2,
                Object1 = objectValue1,
                Object2 = objectValue2,
                BreadCrumb = currentBreadCrumb
            };

            _rootComparer.Compare(childParms);
        }

        private static PropertyEntity GetSecondObjectInfo(PropertyEntity info, List<PropertyEntity> object2Properties)
        {
            foreach (var object2Property in object2Properties)
            {
                if (info.Name == object2Property.Name)
                    return object2Property;
            }

            return null;
        }

        /////////////////////////////////////////////////////////////

        private static ISet<PropertyEntity> GetCurrentProperties(CompareParms parms, object objectValue, Type objectType)
        {
            return HandleDynamicObject(objectValue, objectType)
                   ?? HandleInterfaceMembers(parms, objectValue, objectType)
                   ?? HandleNormalProperties(parms, objectValue, objectType);
        }

        private static ISet<PropertyEntity> HandleNormalProperties(CompareParms parms, object objectValue, Type objectType)
        {
            ISet<PropertyEntity> currentProperties = new HashSet<PropertyEntity>();

            var properties = objectType.GetProperties();

            foreach (var property in properties)
            {
                PropertyEntity propertyEntity = new PropertyEntity();
                propertyEntity.IsDynamic = false;
                propertyEntity.Name = property.Name;
                propertyEntity.CanRead = property.CanRead;
                propertyEntity.CanWrite = property.CanWrite;
                propertyEntity.PropertyType = property.PropertyType;
#if !PORTABLE && !DNCORE
                propertyEntity.ReflectedType = property.ReflectedType;
#endif
                propertyEntity.Indexers.AddRange(property.GetIndexParameters());
                propertyEntity.DeclaringType = objectType;

                if (propertyEntity.CanRead && (propertyEntity.Indexers.Count == 0))
                {
                    try
                    {
                        propertyEntity.Value = property.GetValue(objectValue, null);
                    }
                    catch (System.Reflection.TargetInvocationException)
                    {
                    }
                }

                propertyEntity.PropertyInfo = property;

                currentProperties.Add(propertyEntity);
            }

            return currentProperties;
        }

        private static ISet<PropertyEntity> HandleInterfaceMembers(CompareParms parms, object objectValue, Type objectType)
        {
            ISet<PropertyEntity> currentProperties = new HashSet<PropertyEntity>();

            if (parms.Config.InterfaceMembers.Count > 0)
            {
                Type[] interfaces = objectType.GetInterfaces();

                foreach (var type in parms.Config.InterfaceMembers)
                {
                    if (interfaces.Contains(type))
                    {
                        var properties = type.GetProperties();

                        foreach (var property in properties)
                        {
                            PropertyEntity propertyEntity = new PropertyEntity();
                            propertyEntity.IsDynamic = false;
                            propertyEntity.Name = property.Name;
                            propertyEntity.CanRead = property.CanRead;
                            propertyEntity.CanWrite = property.CanWrite;
                            propertyEntity.PropertyType = property.PropertyType;
                            propertyEntity.Indexers.AddRange(property.GetIndexParameters());
                            propertyEntity.DeclaringType = objectType;

                            if (propertyEntity.Indexers.Count == 0)
                            {
                                propertyEntity.Value = property.GetValue(objectValue, null);
                            }

                            propertyEntity.PropertyInfo = property;
                            currentProperties.Add(propertyEntity);
                        }
                    }
                }
            }

            if (currentProperties.Count == 0)
                return null;

            return currentProperties;
        }

        private static ISet<PropertyEntity> HandleDynamicObject(object objectValue, Type objectType)
        {
            ISet<PropertyEntity> currentProperties = null;

            if (TypeHelper.IsDynamicObject(objectType))
            {
                currentProperties = new HashSet<PropertyEntity>();
                IDictionary<string, object> propertyValues = (IDictionary<string, object>)objectValue;

                foreach (var propertyValue in propertyValues)
                {
                    PropertyEntity propertyEntity = new PropertyEntity();
                    propertyEntity.IsDynamic = true;
                    propertyEntity.Name = propertyValue.Key;
                    propertyEntity.Value = propertyValue.Value;
                    propertyEntity.CanRead = true;
                    propertyEntity.CanWrite = true;
                    propertyEntity.DeclaringType = objectType;

                    if (propertyValue.Value == null)
                    {
                        propertyEntity.PropertyType = null;
                        propertyEntity.ReflectedType = null;
                    }
                    else
                    {
                        propertyEntity.PropertyType = propertyValue.GetType();
                        propertyEntity.ReflectedType = propertyEntity.PropertyType;
                    }

                    currentProperties.Add(propertyEntity);
                }
            }

            return currentProperties;
        }

        private bool IsValidIndexer(ComparisonConfig config, PropertyEntity info, string breadCrumb)
        {
            if (info.Indexers.Count == 0)
            {
                return false;
            }

            if (info.Indexers.Count > 1)
            {
                if (config.SkipInvalidIndexers)
                    return false;

                throw new Exception("Cannot compare objects with more than one indexer for object " + breadCrumb);
            }

            if (info.Indexers[0].ParameterType != typeof(Int32))
            {
                if (config.SkipInvalidIndexers)
                    return false;

                throw new Exception("Cannot compare objects with a non integer indexer for object " + breadCrumb);
            }

#if !DNCORE
            var type = info.ReflectedType;
#else
            var type = info.DeclaringType;
#endif
            if (type == null)
            {
                if (config.SkipInvalidIndexers)
                    return false;

                throw new Exception("Cannot compare objects with a null indexer for object " + breadCrumb);
            }

            if (type.GetProperty("Count") == null
                || type.GetProperty("Count").PropertyType != typeof(Int32))
            {
                if (config.SkipInvalidIndexers)
                    return false;

                throw new Exception("Indexer must have a corresponding Count property that is an integer for object " + breadCrumb);
            }

            return true;
        }
    }
}
