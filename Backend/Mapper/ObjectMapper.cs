using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



[AttributeUsage(AttributeTargets.Class)]
public class MapsToModelAttribute : Attribute
{
    public Type TargetClass { get; }

    public MapsToModelAttribute(Type targetClass)
    {
        TargetClass = targetClass;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MapsToAttribute : Attribute
{
    public string OriginalPropertyName { get; }

    public MapsToAttribute(string originalPropertyName)
    {
        OriginalPropertyName = originalPropertyName;
    }
}

public static class ObjectMapper
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

    private static void SetUnixTimestampFromDateTime(object source, object target, PropertyInfo sourceProperty, PropertyInfo targetProperty)
    {
        var dateTimeValue = (DateTime)sourceProperty.GetValue(source);
        targetProperty.SetValue(target, (int)dateTimeValue.Subtract(UnixEpoch).TotalSeconds);
    }
    private static void SetDateTimeFromUnixTimestamp(object source, object target, PropertyInfo sourceProperty, PropertyInfo targetProperty)
    {
        var unixTimestampValue = (int)(sourceProperty.GetValue(source) ?? 0);
        var dateTimeValue = UnixEpoch.AddSeconds(unixTimestampValue);
        targetProperty.SetValue(target, dateTimeValue);
    }

    private static void SetCollectionProperty(object source, object target, PropertyInfo sourceProperty, PropertyInfo targetProperty, Action<PropertyInfo[], PropertyInfo[], object, object, bool> mapAction, bool sourceisdto)
    {
        var sourceCollection = sourceProperty.GetValue(source);
        var targetCollection = targetProperty.GetValue(target) as IList;

        if (sourceCollection != null)
        {
            var sourceListType = sourceProperty.PropertyType;
            var targetListType = targetProperty.PropertyType;
            var listType = typeof(List<>).MakeGenericType(targetListType.GetGenericArguments().FirstOrDefault());

            if (targetCollection == null)
                targetCollection = (IList)Activator.CreateInstance(listType);

            foreach (var listItem in (IEnumerable)sourceCollection)
            {
                var sourceItemType = sourceListType.GetGenericArguments()[0];
                var targetItemType = targetListType.GetGenericArguments()[0];
                var mappedItem = Activator.CreateInstance(targetItemType);

                var entityProperties = sourceisdto ? targetItemType.GetProperties() : sourceItemType.GetProperties();
                var dtoProperties = sourceisdto ? sourceItemType.GetProperties() : targetItemType.GetProperties();
                mapAction(entityProperties, dtoProperties, listItem, mappedItem, sourceisdto);
                targetCollection.Add(mappedItem);
            }
        }

        targetProperty.SetValue(target, targetCollection);
    }

    private static void SetPropertyValue(object source, object target, PropertyInfo sourceProperty, PropertyInfo targetProperty)
    {
        var value = sourceProperty.GetValue(source);
        targetProperty.SetValue(target, value);
    }

    private static void MapProperties(PropertyInfo[] entityProperties, PropertyInfo[] dtoProperties, object source, object target, bool sourceisdto)
    {
        foreach (var dtoProperty in dtoProperties)
        {
            var mapsToAttribute = dtoProperty.GetCustomAttribute<MapsToAttribute>();
            if (mapsToAttribute != null)
            {
                var entityProperty = entityProperties.FirstOrDefault(p =>
                    p.Name.Equals(mapsToAttribute.OriginalPropertyName, StringComparison.OrdinalIgnoreCase));

                if (entityProperty != null)
                {
                    var sourceProperty = sourceisdto ? dtoProperty : entityProperty;
                    var targetProperty = sourceisdto ? entityProperty : dtoProperty;

                    if (targetProperty.PropertyType == typeof(DateTime) && (sourceProperty.PropertyType == typeof(int) || sourceProperty.PropertyType == typeof(int?)))
                    {
                        SetDateTimeFromUnixTimestamp(source, target, sourceProperty, targetProperty);
                    }
                    else if (sourceProperty.PropertyType == typeof(DateTime) && (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(int?)))
                    {
                        SetUnixTimestampFromDateTime(source, target, sourceProperty, targetProperty);
                    }
                    else if (typeof(ICollection<>).IsAssignableFrom(targetProperty.PropertyType) || typeof(IList).IsAssignableFrom(targetProperty.PropertyType) ||
                            (targetProperty.PropertyType.IsGenericType && targetProperty.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
                    {
                        SetCollectionProperty(source, target, sourceProperty, targetProperty, MapProperties, sourceisdto);
                    }
                    else if (typeof(ICollection<>).IsAssignableFrom(sourceProperty.PropertyType) || typeof(IList).IsAssignableFrom(sourceProperty.PropertyType) ||
                            (sourceProperty.PropertyType.IsGenericType && sourceProperty.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
                    {
                        SetCollectionProperty(source, target, sourceProperty, targetProperty, MapProperties, sourceisdto);

                    }
                    else
                    {
                        SetPropertyValue(source, target, sourceProperty, targetProperty);
                    }
                }
            }
        }
    }


    public static TDTO MapToDto<TEntity, TDTO>(TEntity entity, TDTO dto = null)
        where TEntity : class
        where TDTO : class, new()
    {
        if (entity == null)
            return null;

        if (dto == null)
            dto = new TDTO();

        MapProperties(typeof(TEntity).GetProperties(), typeof(TDTO).GetProperties(), entity, dto, false);

        return dto;
    }



    public static TEntity MapFromDto<TDTO, TEntity>(TDTO dto, TEntity entity = null)
        where TDTO : class
        where TEntity : class, new()
    {
        if (dto == null)
            return null;

        if (entity == null)
            entity = new TEntity();

        MapProperties(typeof(TEntity).GetProperties(), typeof(TDTO).GetProperties(), dto, entity, true);

        return entity;
    }

}