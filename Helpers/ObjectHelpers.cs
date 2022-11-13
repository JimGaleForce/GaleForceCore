namespace GaleForceCore.Helpers
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class ObjectHelpers
    {
        public static TTarget CopyShallow<TTarget>(this Object source)
        {
            Type sourceType = source.GetType();
            Type targetType = typeof(TTarget);

            PropertyInfo[] sourceProperties = sourceType.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            FieldInfo[] sourceFields = sourceType.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            PropertyInfo[] targetProperties = targetType.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            FieldInfo[] targetFields = targetType.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            TTarget target = (TTarget)Activator.CreateInstance(typeof(TTarget));

            foreach (PropertyInfo property in sourceProperties)
            {
                var targetProperty = targetProperties.FirstOrDefault(t => t.Name == property.Name);
                if (targetProperty != null)
                {
                    try
                    {
                        targetProperty.SetValue(target, property.GetValue(source, null), null);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            foreach (FieldInfo field in sourceFields)
            {
                var targetField = targetFields.FirstOrDefault(t => t.Name == field.Name);
                if (targetField != null)
                {
                    targetField.SetValue(target, field.GetValue(source));
                }
            }

            return target;
        }
    }
}
