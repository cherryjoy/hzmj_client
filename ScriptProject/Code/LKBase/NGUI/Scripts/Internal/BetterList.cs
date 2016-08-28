//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// This improved version of the List<> doesn't release the buffer on Clear(), resulting in better performance and less garbage collection.
/// </summary>

public class BetterList<T>
{
#if UNITY_FLASH

	List<T> mList = new List<T>();
	
	/// <summary>
	/// Direct access to the buffer. Note that you should not use its 'Length' parameter, but instead use BetterList.Length.
	/// </summary>
	
	public T this[int i]
	{
		get { return mList[i]; }
		set { mList[i] = value; }
	}
	
	/// <summary>
	/// Compatibility with the non-flash syntax.
	/// </summary>
	
	public List<T> buffer { get { return mList; } }

	/// <summary>
	/// Direct access to the buffer's size. Note that it's only public for speed and efficiency. You shouldn't modify it.
	/// </summary>

	public int size { get { return mList.Count; } }

	/// <summary>
	/// For 'foreach' functionality.
	/// </summary>

	public IEnumerator<T> GetEnumerator () { return mList.GetEnumerator(); }

	/// <summary>
	/// Clear the array by resetting its size to zero. Note that the memory is not actually released.
	/// </summary>

	public void Clear () { mList.Clear(); }

	/// <summary>
	/// Clear the array and release the used memory.
	/// </summary>

	public void Release () { mList.Clear(); }

	/// <summary>
	/// Add the specified item to the end of the list.
	/// </summary>

	public void Add (T item) { mList.Add(item); }

	/// <summary>
	/// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
	/// </summary>

	public void Remove (T item) { mList.Remove(item); }

	/// <summary>
	/// Remove an item at the specified index.
	/// </summary>

	public void RemoveAt (int index) { mList.RemoveAt(index); }

	/// <summary>
	/// Mimic List's ToArray() functionality, except that in this case the list is resized to match the current size.
	/// </summary>

	public T[] ToArray () { return mList.ToArray(); }

#else

	/// <summary>
	/// Direct access to the buffer. Note that you should not use its 'Length' parameter, but instead use BetterList.Length.
	/// </summary>

	public T[] buffer = new T[32];

	/// <summary>
	/// Direct access to the buffer's size. Note that it's only public for speed and efficiency. You shouldn't modify it.
	/// </summary>

	public int size = 0;

	/// <summary>
	/// For 'foreach' functionality.
	/// </summary>

	public IEnumerator<T> GetEnumerator ()
	{
		//if (buffer != null)
		{
			for (int i = 0; i < size; ++i)
			{
				yield return buffer[i];
			}
		}
	}
	
	/// <summary>
	/// Convenience function. I recommend using .buffer instead.
	/// </summary>
	
	public T this[int i]
	{
		get { return buffer[i]; }
		set { buffer[i] = value; }
	}

	/// <summary>
	/// Helper function that expands the size of the array, maintaining the content.
	/// </summary>

	void AllocateMore ()
	{
		T[] newList = /*(buffer != null) ? */new T[Mathf.Max(buffer.Length << 1, 32)];// : new T[32];
		/*if (buffer != null && size > 0) */buffer.CopyTo(newList, 0);
		buffer = newList;
	}

	void AllocateMore (int require)
	{
		int needLength = require + size;
		int allocatLength = buffer.Length;
		
		if (allocatLength > needLength)
			return;
		
		while(allocatLength < needLength)
			allocatLength = allocatLength << 1;
		
		T[] newList = new T[Mathf.Max(allocatLength, 32)];
		buffer.CopyTo(newList, 0);
		buffer = newList;
	}

	/// <summary>
	/// Trim the unnecessary memory, resizing the buffer to be of 'Length' size.
	/// Call this function only if you are sure that the buffer won't need to resize anytime soon.
	/// </summary>

	void Trim ()
	{
		if (size > 0)
		{
			if (size < buffer.Length)
			{
				T[] newList = new T[size];
				for (int i = 0; i < size; ++i) newList[i] = buffer[i];
				buffer = newList;
			}
		}
		else buffer = null;
	}

	/// <summary>
	/// Clear the array by resetting its size to zero. Note that the memory is not actually released.
	/// </summary>

	public void Clear () { size = 0; }

	/// <summary>
	/// Clear the array and release the used memory.
	/// </summary>

	public void Release () { size = 0; buffer = null; }

	/// <summary>
	/// Add the specified item to the end of the list.
	/// </summary>

	public void Add (T item)
	{
		if (/*buffer == null || */size == buffer.Length) AllocateMore();
		buffer[size++] = item;
	}

	public void Add (BetterList<T> list, int length)
	{
		AllocateMore(length);
		T[] array = list.ToArray();
		if (array == null)
			return;
		
		System.Array.Copy(array, 0, buffer, size, length); 
		size += length;
	}

	/// <summary>
	/// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
	/// </summary>

	public void Remove (T item)
	{
		//if (buffer != null)
		{
			EqualityComparer<T> comp = EqualityComparer<T>.Default;

			for (int i = 0; i < size; ++i)
			{
				if (comp.Equals(buffer[i], item))
				{
					--size;
					buffer[i] = default(T);
					for (int b = i; b < size; ++b) buffer[b] = buffer[b + 1];
					return;
				}
			}
		}
	}

	/// <summary>
	/// Remove an item at the specified index.
	/// </summary>

	public void RemoveAt (int index)
	{
		if (/*buffer != null && */index < size)
		{
			--size;
			buffer[index] = default(T);
			for (int b = index; b < size; ++b) buffer[b] = buffer[b + 1];
		}
	}

    /// <summary>
    /// List.Sort equivalent. Manual sorting causes no GC allocations.
    /// </summary>

    [DebuggerHidden]
    [DebuggerStepThrough]
    public void Sort(CompareFunc comparer)
    {
        int start = 0;
        int max = size - 1;
        bool changed = true;

        while (changed)
        {
            changed = false;

            for (int i = start; i < max; ++i)
            {
                // Compare the two values
                if (comparer(buffer[i], buffer[i + 1]) > 0)
                {
                    // Swap the values
                    T temp = buffer[i];
                    buffer[i] = buffer[i + 1];
                    buffer[i + 1] = temp;
                    changed = true;
                }
                else if (!changed)
                {
                    // Nothing has changed -- we can start here next time
                    start = (i == 0) ? 0 : i - 1;
                }
            }
        }
    }

	/// <summary>
	/// Mimic List's ToArray() functionality, except that in this case the list is resized to match the current size.
	/// </summary>

	public T[] ToArray () { Trim(); return buffer; }

	public void Sort()
	{
		System.Array.Sort(buffer,0,size);
	}

    /// <summary>
    /// Comparison function should return -1 if left is less than right, 1 if left is greater than right, and 0 if they match.
    /// </summary>

    public delegate int CompareFunc(T left, T right);

#endif
}
