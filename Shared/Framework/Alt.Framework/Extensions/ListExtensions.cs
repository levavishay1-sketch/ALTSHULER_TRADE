
using System.Collections.Generic;

namespace Alt.Framework.Extensions
{
    public static class ListExtensions
    {
        public static List<List<T>> ToChunks<T>(this List<T> list, int chunkSize = 100)
        {
            List<List<T>> retVal = new List<List<T>>();
            int index = 0;
            while (index < list.Count)
            {
                int count = list.Count - index > chunkSize ? chunkSize : list.Count - index;
                retVal.Add(list.GetRange(index, count));

                index += chunkSize;
            }
            return retVal;
        }
    }
}
