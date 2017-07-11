using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SecurityLinkList<T> : MonoBehaviour {

    private LinkedList<T> linkedList = null;
	private System.Object obj = null;

	public SecurityLinkList()
	{
      linkedList=new LinkedList<T>();
	  obj = new System.Object();

	}

	public void AddFirst(T t)
	{
		lock(obj)
		{
		    linkedList.AddFirst(t);
		}
	}
    public void AddLast (T t)
    {
        lock (obj)
        {
            linkedList.AddLast(t);
        }
    }
    public void RemoveFirst()
    {
        lock (obj)
        {
            linkedList.RemoveFirst();
        }
    }
    public void RemoveLast()
    {
        lock (obj)
        {
            linkedList.RemoveLast();
        }
    }

    public void Remove(T t)
    {
        lock (obj)
        {
            linkedList.Remove(t);
        }
    }

    public bool Constains(T t)
    {
        lock (obj)
        {
            return linkedList.Contains(t);
        }
    }

    public T Last( )
    {
        lock (obj)
        {
            return linkedList.Last.Value;
        }
    }
    public T First()
    {
        lock (obj)
        {
            return linkedList.First.Value;
        }
    }
    public int Count
	{
		get{
                 lock(obj)
                 {
                     return linkedList.Count;
                 }
           }
	}

}
