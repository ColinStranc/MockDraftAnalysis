using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class ListUtility
    {
        public static List<T> GetFirstElements<T>(Queue<T> allElements, int count)
        {
            List<T> returnedElements = new List<T>();

            for (int i = 0; i < count; i++)
            {
                if (allElements.Count == 0) break;

                returnedElements.Add(allElements.Dequeue());
            }

            return returnedElements;
        }
    }
}
