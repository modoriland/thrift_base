using System;
using System.Collections.Generic;

namespace Paul
{
    static class Random
    {
        public static int Get()
        {
            return ThreadSafeRandom.Next();
        }
        public static int Get(int maxValue)
        {
            return ThreadSafeRandom.Next(maxValue);
        }
        // 주의: System.Random 과 달리 maxValue 를 포함한 범위에서 리턴한다. (float 과의 일관성을 위하여..)
        public static int Get(int minValue, int maxValue)
        {
            return ThreadSafeRandom.Next(minValue, maxValue + 1);
        }
        public static float Get(float minValue, float maxValue)
        {
            return minValue + (float)ThreadSafeRandom.NextDouble() * (maxValue - minValue);
        }
        public static float GetFloat()
        {
            return (float)ThreadSafeRandom.NextDouble();
        }

        // -----------------------------------------------------------------------------------
        private static readonly System.Random _seed =
            new System.Random((int)(DateTime.Now.Ticks >> 17));   // current time roughly in 10-ms

        [ThreadStatic]
        private static System.Random _local = null;
        private static System.Random ThreadSafeRandom
        {
            get
            {
                if (_local == null)
                {
                    lock (_seed)
                    {
                        if (_local == null)
                            _local = new System.Random(_seed.Next());
                    }
                }
                return _local;
            }
        }
    }

    static class RandomShuffle_Extension
    {
        public static T RandomSelect<T>(this IList<T> list)
        {
            if (0 < list.Count)
                return list[Random.Get(0, list.Count - 1)];
            return default(T);
        }

        public static void RandomShuffle<T>(this IList<T> list)
        {
            int count = list.Count;
            while (count > 1)
            {
                --count;
                int rnd_i = Random.Get(count);
                T value = list[rnd_i];
                list[rnd_i] = list[count];
                list[count] = value;
            }
        }
    }
}
