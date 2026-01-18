using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.IO;

namespace HotUpdateScript
{
    public class LoadDll : MonoBehaviour
    {

        void Start()
        {
            // Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if !UNITY_EDITOR
        Assembly hotUpdateAss = Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/HotUpdate.dll.bytes"));
#else
            // Editor下无需加载，直接查找获得HotUpdate程序集
            Assembly hotUpdateAss = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#endif
            Debug.Log($"{hotUpdateAss.GetName().Name}");
            Type type = hotUpdateAss.GetType("HotUpdate.HotUpdate.Hello");
            type.GetMethod("Run").Invoke(null, null);
        }
    }
}