using System.Collections.Generic;

namespace Unit
{
    ///<总结>
    ///有用的优化工具，让您在不实例化新数组的情况下使用数组
    ///</摘要>
    public class ListSwap<T>
    {
        public List<T> swap1 = new List<T>();
        public List<T> swap2 = new List<T>();

        //Return any array
        public List<T> Get()
        {
            swap1.Clear(); //Clear before using
            return swap1;
        }

        //Return the OTHER array (because skip is already in use)
        public List<T> GetOther(List<T> skip)
        {
            if (skip == swap1)
            {
                swap2.Clear(); //Clear before using
                return swap2;
            }
            swap1.Clear(); //Clear before using
            return swap1;
        }

        //Clear both
        public void Clear()
        {
            swap1.Clear();
            swap2.Clear();
        }
    }
}