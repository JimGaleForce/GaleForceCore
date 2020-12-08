using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace GaleForceCore.Helpers
{
    public static class DynamicHelpers
    {
        /// <summary>
        /// Converts to an object to an expandoobject.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>ExpandoObject.</returns>
        public static ExpandoObject ToExpandoObject(this object obj)
        {
            if(obj == null)
            {
                return null;
            }

            var expando = new ExpandoObject() as IDictionary<string, Object>;

            foreach(PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
            {
                expando.Add(property.Name, property.GetValue(obj));
            }

            return (ExpandoObject)expando;
        }
    }
}
