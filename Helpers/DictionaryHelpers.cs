using System.Collections.Generic;

namespace GaleForceCore.Helpers
{
    public static class DictionaryHelpers
    {
        public static T2 Get<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key)
        {
            if(!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, default(T2));
            }

            return dictionary[key];
        }
    }
}
