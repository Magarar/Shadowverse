using System.Collections;
using UnityEngine;

namespace Unit
{
    public class TimeToolMono : MonoBehaviour
    {
        private static TimeToolMono instance;

        public Coroutine StartRoutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        public void StopRoutine(Coroutine routine)
        {
            StopCoroutine(routine);
        }

        public static TimeToolMono Inst
        {
            get
            {
                if (instance == null)
                {
                    GameObject ntool = new GameObject("TimeTool");
                    instance = ntool.AddComponent<TimeToolMono>();
                }
                return instance;
            }
        }
    }
}