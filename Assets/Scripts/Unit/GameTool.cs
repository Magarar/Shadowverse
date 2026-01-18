using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unit
{
    /// <summary>
    /// Generic static functions for TcgEngine
    /// </summary>
    public static class GameTool
    {
        private const string UidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static Random random = new Random();

        //Generate a random string to use as UID
        public static string GenerateRandomID(int min = 9, int max = 15)
        {
            int length = random.Next(min, max);
            string uniqueID = "";
            for (int i = 0; i < length; i++)
            {
                uniqueID += UidChars[random.Next(UidChars.Length - 1)];
            }
            return uniqueID;
        }
        
        //Generate a random int
        public static int GenerateRandomInt()
        {
            return random.Next(int.MinValue, int.MaxValue);
        }
        
        //Generate a random ulong
        public static ulong GenerateRandomUInt64()
        {
            ulong id = (uint)random.Next(int.MinValue, int.MaxValue); //Cast to uint before casting to ulong
            uint bid = (uint)random.Next(int.MinValue, int.MaxValue);
            id = id << 32;
            id = id | bid;
            return id;
        }
        
        //在列表中随机选取X个元素（相同的元素不能被选取两次，除非它在列表中出现两次或两次以上）
        public static List<T> PickXRandom<T>(List<T> source, List<T> dest, int x)
        {
            if (source.Count <= x || x <= 0)
                return source; //No need to pick anything

            if (dest.Count > 0)
                dest.Clear();

            for (int i = 0; i < x; i++)
            {
                int r = random.Next(source.Count);
                dest.Add(source[r]);
                source.RemoveAt(r);
            }

            return dest;
        }
        
        //以最优化的方式克隆字符串列表（尽量避免添加/删除）
        public static void CloneList(List<string> source, List<string> dest)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (i < dest.Count)
                    dest[i] = source[i];
                else
                    dest.Add(source[i]);
            }

            if (dest.Count > source.Count)
                dest.RemoveRange(source.Count, dest.Count - source.Count);
        }
        
        //以最优化的方式克隆列表，只克隆列表，保留元素引用
        public static void CloneListRef<T>(List<T> source, List<T> dest) where T : class
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (i < dest.Count)
                    dest[i] = source[i];
                else
                    dest.Add(source[i]);
            }

            if (dest.Count > source.Count)
                dest.RemoveRange(source.Count, dest.Count - source.Count);
        }
        
        //Same a previous function, but elements could be null
        public static void CloneListRefNull<T>(List<T> source, ref List<T> dest) where T : class
        {
            //Source is null, set destination null
            if (source == null)
            {
                dest = null;
                return;
            }

            //Dest is null
            dest ??= new List<T>();

            //Both arent null, clone
            CloneListRef(source, dest);
        }


        public static bool IsMobile()
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN
            return true;
#else
            return UnityEngine.Device.Application.isMobilePlatform;
#endif
        }
        
        //检查是否使用通用渲染管道
        //如果此函数返回编译错误（因为没有安装URP，而你不想要它，你可以简单地注释代码并返回false
        public static bool IsURP()
        {
            if (GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset)
                return true;
            return false;
        }

    }
}