using System.Collections.Generic;

internal static class CompatibilityExtensions
{
#if NETSTANDARD2_0
	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value)
	{
		key = keyValuePair.Key;
		value = keyValuePair.Value;
	}
#endif
}
