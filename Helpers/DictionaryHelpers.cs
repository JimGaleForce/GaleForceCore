using System.Collections.Generic;

namespace GaleForceCore.Helpers
{
    public static class DictionaryHelpers
    {
        /// <summary>
        /// Gets a key, adds if missing.
        /// </summary>
        /// <typeparam name="T1">The type of the key.</typeparam>
        /// <typeparam name="T2">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value</returns>
        public static T2 GetOrAdd<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key)
        {
            if(!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, default(T2));
            }

            return dictionary[key];
        }
    }
}
