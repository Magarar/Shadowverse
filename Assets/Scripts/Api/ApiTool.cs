using System;
using Unit;
using UnityEngine;

namespace Api
{
    /// <summary>
    /// Useful tool static functions for the ApiClient
    /// </summary>
    public class ApiTool
    {
        public static T JsonToObject<T>(string json)
        {
            try
            {
                T value = JsonUtility.FromJson<T>(json);
                return value;
            }
            catch (Exception e)
            {
                
            }
            return (T)Activator.CreateInstance(typeof(T));
        }

        public static T[] JsonToArray<T>(string json)
        {
            ListJson<T> list = new()
            {
                list = Array.Empty<T>()
            };
            try
            {
                string wrap_json = "{ \"list\": " + json + "}";
                list = JsonUtility.FromJson<ListJson<T>>(wrap_json);
                return list.list;
            }
            catch (Exception e)
            {
                
            }
            return Array.Empty<T>();
        }
        
        public static string ToJson(object data)
        {
            return JsonUtility.ToJson(data);
        }
        
        public static int ParseInt(string intStr, int defaultVal = 0)
        {
            bool success = int.TryParse(intStr, out int val);
            return success ? val : defaultVal;
        }
    }
}