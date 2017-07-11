using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SecurityQueue<T>{

	private Queue<T> queue = null;
	private System.Object obj = null;

	public SecurityQueue()
	{
		queue = new Queue<T>();
		obj = new System.Object();
	}

	public void Enqueue(T t)
	{
		lock(obj)
		{
			queue.Enqueue(t);
		}
	}

	public int Count
	{
		get{
                 lock(obj)
                { 
                return queue.Count;
                }
           }
	}

	public bool Dequeue(ref T t)
	{
		lock(obj)
		{
			if(queue.Count <= 0)
				return false;
			t = queue.Dequeue();
			return true;
		}
	}
}
