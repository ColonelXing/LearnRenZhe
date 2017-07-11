using UnityEngine;
using System.Collections.Generic;

public class SecurityDictionary<TKey, TValue>
{

	private Dictionary<TKey, TValue> dic = null;
	private System.Object obj = null;

	public SecurityDictionary()
	{
		dic = new Dictionary<TKey, TValue>();
		obj = new System.Object();
	}

	public bool Add(TKey key, TValue v)
	{
		lock(obj)
		{
			if(dic.ContainsKey(key))
			{
				//Debugger.Log(key);
				return false;
			}
			dic.Add(key, v);

			return true;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			lock(obj)
			{
				return dic[key];
			}
		}
		set
		{
		}
	}

	public bool ContainsKey(TKey key)
	{
		if(dic.ContainsKey(key))
			return true;

		return false;
	}

	public void Clear()
	{
		lock(obj)
		{
			dic.Clear();
		}
	}

	public int Count
	{
		get{
            lock (obj)
            {
                return dic.Count;
            }        
        }
	}
}
