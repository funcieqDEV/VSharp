namespace VSharpLib
{
    using System;
    using System.Collections.Generic;
    using VSharp;

    [Module]
    public class array
    {
        /// <summary>
        /// Returns the number of elements in the list.
        /// </summary>
        public int length(List<object> list)
        {
            return list.Count;
        }

        /// <summary>
        /// Checks if the list is empty.
        /// </summary>
        public bool isEmpty(List<object> list)
        {
            return list.Count == 0;
        }

        /// <summary>
        /// Gets the element at the specified index in the list.
        /// </summary>
        /// <param name="list">The list to retrieve the element from.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns>The element at the specified index.</returns>
        public object getElementAt(List<object> list, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                throw new ArgumentOutOfRangeException("Index out of bounds.");
            }
            return list[index];
        }

        /// <summary>
        /// Adds an element to the end of the list.
        /// </summary>
        public void addElement(List<object> list, object element)
        {
            list.Add(element);
        }

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="list">The list from which to remove the element.</param>
        /// <param name="index">The index of the element to remove.</param>
        public void removeElementAt(List<object> list, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                throw new ArgumentOutOfRangeException("Index out of bounds.");
            }
            list.RemoveAt(index);
        }

        /// <summary>
        /// Clears all elements from the list.
        /// </summary>
        public void clear(List<object> list)
        {
            list.Clear();
        }

        /// <summary>
        /// Checks if the list contains a specific element.
        /// </summary>
        public bool contains(List<object> list, object element)
        {
            return list.Contains(element);
        }

        /// <summary>
        /// Returns the index of the specified element in the list, or -1 if not found.
        /// </summary>
        public int indexOf(List<object> list, object element)
        {
            return list.IndexOf(element);
        }

        /// <summary>
        /// Returns a sorted copy of the list.
        /// </summary>
        public List<object> sort(List<object> list)
        {
            List<object> sortedList = new List<object>(list);
            QuickSort(sortedList, 0, sortedList.Count - 1);
            return sortedList;
        }

        private void QuickSort(List<object> list, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(list, low, high);
                QuickSort(list, low, pivotIndex - 1);
                QuickSort(list, pivotIndex + 1, high);
            }
        }

        private int Partition(List<object> list, int low, int high)
        {
            object pivot = list[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (list[j] is IComparable comparableElement)
                {
                    if (comparableElement.CompareTo(pivot) <= 0)
                    {
                        i++;
                        Swap(list, i, j);
                    }
                }
                else
                {
                    throw new ArgumentException("Elements must implement IComparable.");
                }
            }
            Swap(list, i + 1, high);
            return i + 1;
        }

        private void Swap(List<object> list, int i, int j)
        {
            object temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        /// <summary>
        /// Reverses the order of elements in the list.
        /// </summary>
        public List<object> reverse(List<object> list)
        {
            List<object> reversedList = new List<object>(list);
            reversedList.Reverse();
            return reversedList;
        }

        /// <summary>
        /// Finds the first element in the list that matches a condition.
        /// </summary>
        public object Find(List<object> list, Predicate<object> match)
        {
            return list.Find(match);
        }

        /// <summary>
        /// Returns a slice (sublist) of the list from start index to end index (exclusive).
        /// </summary>
        public List<object> slice(List<object> list, int start, int end)
        {
            if (start < 0 || end > list.Count || start > end)
            {
                throw new ArgumentOutOfRangeException("Invalid slice range.");
            }
            return list.GetRange(start, end - start);
        }

        /// <summary>
        /// Concatenates two lists into a new list.
        /// </summary>
        public List<object> concat(List<object> list1, List<object> list2)
        {
            List<object> result = new List<object>(list1);
            result.AddRange(list2);
            return result;
        }

        /// <summary>
        /// Removes duplicate elements from the list.
        /// </summary>
        public List<object> unique(List<object> list)
        {
            HashSet<object> uniqueElements = new HashSet<object>(list);
            return new List<object>(uniqueElements);
        }
    }
}
