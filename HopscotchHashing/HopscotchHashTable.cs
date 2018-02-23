using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HopscotchHashing
{
    /// <summary>
    /// HopscotchHash实现
    /// </summary>
    public class HopscotchHashTable<T>
        where T:class
    {
        #region 私有字段
        /// <summary>
        /// 容量
        /// </summary>
        private int m_nElementSize;

        /// <summary>
        /// Hash表主体
        /// </summary>
        private HashItem<T>[] m_thisTable;

        /// <summary>
        /// 最大装填因子
        /// </summary>
        private const double MAX_LOAD = 0.5d;

        /// <summary>
        /// 默认容量
        /// </summary>
        private const int DEFAULT_SIZE = 100;

        /// <summary>
        /// 最大的跳跃距离
        /// </summary>
        private const int MAX_DISTANT = 4;

        /// <summary>
        /// 最大调整循环次数
        /// </summary>
        private const int MAX_HOPSCOTCHLOOPCOUNT = 5; 
        #endregion

        /// <summary>
        /// 构造HashTable
        /// </summary>
        /// <param name="size">初始化的长度</param>
        public HopscotchHashTable(int size)
        {
            PrepareFiled(size);
        }

        /// <summary>
        /// 构造HashTable
        /// </summary>
        public HopscotchHashTable()
        {
            PrepareFiled(DEFAULT_SIZE);
        }

        /// <summary>
        /// HashTable是否包含输入
        /// </summary>
        /// <param name="inputValue">输入封装</param>
        /// <returns>是否包含</returns>
        public bool Contaions(T inputValue)
        {
            return -1 != FindPostion(inputValue);
   
        }

        /// <summary>
        /// 在HashTable移除输入
        /// </summary>
        /// <param name="inputValue">输入封装</param>
        public void Remove(T inputValue)
        {
            //获得实际位置
            int postion = FindPostion(inputValue);
            //获得基础Hash
            int useHash = CalculateHash(inputValue);

            if (-1!= postion)
            {
                m_thisTable[postion].ThisValue = null;
                //调整基础Hash处的位置描述;
                m_thisTable[useHash].Distance = (m_thisTable[useHash].Distance - (1 << (MAX_DISTANT - 1 - postion + useHash)));
                m_nElementSize--;
            }
        }

        /// <summary>
        /// 寻找输入在HashTable中的索引
        /// </summary>
        /// <param name="inputValue">输入封装</param>
        /// <returns>索引</returns>
        public int FindPostion(T inputValue)
        {
            //获得基础Hash
            int useHash = CalculateHash(inputValue);

            //获得距离字段
            int tempDistance = m_thisTable[useHash].Distance;

            //在距离内寻找
            for (int oneIndex = 0; oneIndex < MAX_DISTANT; oneIndex++)
            {
                //若与基础Hash关联
                if ((tempDistance >> oneIndex) % 2 == 1)
                {
                    //判断是否相等
                    if (m_thisTable[useHash + MAX_DISTANT - 1 - oneIndex].Equals(inputValue))
                    {
                        return useHash + MAX_DISTANT - 1 - oneIndex;
                    }
                }
            }

            return -1;
        }
        /// <summary>
        /// 获取指定索引
        /// </summary>
        /// <param name="inputIndex"></param>
        /// <returns></returns>
        public T ElementAt(int inputIndex)
        {
            return m_thisTable[inputIndex].ThisValue;
        }

        /// <summary>
        /// 插入封装
        /// </summary>
        /// <param name="inputValue">输入封装</param>
        /// <exception cref="ArgumentException">已存在或在设置要求内无法插入时</exception>
        public void Instert(T inputValue)
        {
            //若已存在 抛出异常
            if (Contaions(inputValue))
            {
                throw new ArgumentException();
            }

            //若需扩容则动态扩容
            if (m_nElementSize > (int)(m_thisTable.Length * MAX_LOAD))
            {
                ReHash();
            }

            //Hopscotch插入
            if (false == HopscotchInstert(inputValue))
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// 初始化字段
        /// </summary>
        /// <param name="size">输入的容量</param>
        private void PrepareFiled(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException();
            }

            m_thisTable = new HashItem<T>[size];

            for (int oneIndex = 0; oneIndex < size; oneIndex++)
            {
                m_thisTable[oneIndex] = new HashItem<T>(null);
            }

            m_nElementSize = 0;
        }

        /// <summary>
        /// 计算实际使用Hash值
        /// </summary>
        /// <param name="inputValue">输入封装1</param>
        /// <returns>实际使用Hash（索引值）</returns>
        private int CalculateHash(T inputValue)
        {
            int baseHash = inputValue.GetHashCode();
            baseHash = Math.Abs(baseHash);

            baseHash %= m_thisTable.Length;

            return baseHash;
        }

        /// <summary>
        /// HashTable扩容
        /// </summary>
        private void ReHash()
        {
            HashItem<T>[] backTable = m_thisTable;

            m_thisTable = new HashItem<T>[(int)(m_thisTable.Length / MAX_LOAD)];

            foreach (var oneValue in backTable)
            {
                if (null != oneValue.ThisValue)
                {
                    Instert(oneValue.ThisValue);
                }
            }
        }

        /// <summary>
        /// Hopscotch插入
        /// </summary>
        /// <param name="inputValue">需插入的对象</param>
        /// <returns>插入是否成功</returns>
        private bool HopscotchInstert(T inputValue)
        {
            //循环数
            int loopCount = 0;

            while (true)
            {
                //循环数检查
                loopCount++;

                if (loopCount > MAX_HOPSCOTCHLOOPCOUNT)
                {
                    return false;
                }

                //获取基础Hash
                int useIndex = CalculateHash(inputValue);

                //备份
                int backHash = useIndex;

                //寻找可用位置
                while (m_thisTable[useIndex].ThisValue != null)
                {
                    useIndex++;
                }

                //简单插入
                if (BaseInstert(inputValue,useIndex,backHash))
                {
                    return true;
                }

                //Hopscotch 调整
                while (true)
                {
                    //是否已移动
                    bool hasMoved = false;

                    //向前移动距离循环
                    for (int posMoveIndex = MAX_DISTANT - 1; posMoveIndex > 0; posMoveIndex--)
                    {
                        //向前移动后向后移动循环
                        for (int distanceIndex = MAX_DISTANT - 1; distanceIndex > MAX_DISTANT - 1 - posMoveIndex; distanceIndex--)
                        {
                            //若向前移动后可向后移动
                            if ((m_thisTable[useIndex - posMoveIndex].Distance >> distanceIndex )%2 == 1)
                            {
                                //获取需调整封装
                                var basItem = m_thisTable[useIndex - posMoveIndex + MAX_DISTANT - 1 - distanceIndex];

                                //封装向后移动
                                m_thisTable[useIndex].ThisValue = basItem.ThisValue;

                                //修改位置信息
                                basItem.Distance = basItem.Distance - (1 << distanceIndex) + 1;

                                //原位置值调整
                                basItem.ThisValue = null;

                                //null值位置更新
                                useIndex = useIndex - posMoveIndex + MAX_DISTANT - 1 - distanceIndex;

                                //尝试插入
                                if (BaseInstert(inputValue, useIndex, backHash))
                                {
                                    return true;
                                }
                                else
                                {
                                    hasMoved = true;
                                    break;
                                }
                            }
                        }
                        //若已移动过但距离不够发起下次循环
                        if (hasMoved)
                        {
                            break;
                        }
                    }

                    //若无法移动重建Table
                    if (!hasMoved)
                    {
                        break;
                    }

                }

                ReHash();
            }
        }

        /// <summary>
        /// 基础的插入方法
        /// </summary>
        /// <param name="inputValue">输入的封装</param>
        /// <param name="useIndex">使用的Index</param>
        /// <param name="backHash">基础Hash值</param>
        /// <returns>是否插入</returns>
        private bool BaseInstert(T inputValue, int useIndex,int backHash)
        {
            //判断距离
            if (useIndex <= backHash + MAX_DISTANT - 1)
            {
                //设置值
                m_thisTable[useIndex].ThisValue = inputValue;
                //调整基础位置处的Distance描述
                m_thisTable[backHash].Distance = m_thisTable[useIndex].Distance + (1 << backHash + MAX_DISTANT - 1 - useIndex);
                m_nElementSize++;
                return true;
            }
            return false;
        }


    }
}
