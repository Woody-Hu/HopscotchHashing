using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HopscotchHashing
{
    /// <summary>
    /// 待散列维护的数据封装
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class HashItem<T>
        where T : class
    {
        /// <summary>
        /// 内部封装的值
        /// </summary>
        private T m_thisValue = null;

        /// <summary>
        /// 维护的近邻队列（二进制）
        /// </summary>
        private int m_nDistance;

        /// <summary>
        /// 构造封装
        /// </summary>
        /// <param name="inputValue"></param>
        internal HashItem (T inputValue)
        {
            m_thisValue = inputValue;
        }
           
        /// <summary>
        /// 内部封装的值
        /// </summary>
        internal T ThisValue
        {
            get
            {
                return m_thisValue;
            }

            set
            {
                m_thisValue = value;
            }
        }

        /// <summary>
        /// 维护的近邻队列（二进制）
        /// </summary>
        internal int Distance
        {
            get
            {
                return m_nDistance;
            }

            set
            {
                m_nDistance = value;
            }
        }
    }
}
