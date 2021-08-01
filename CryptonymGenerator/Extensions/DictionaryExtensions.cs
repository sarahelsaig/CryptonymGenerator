namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TK, TV>(
            this Dictionary<TK, TV> dictionary,
            IEnumerable<KeyValuePair<TK, TV>> pairsToAdd)
        {
            foreach (var (key, value) in pairsToAdd)
            {
                dictionary[key] = value;
            }
        }
    }
}