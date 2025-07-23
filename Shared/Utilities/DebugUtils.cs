using System;

namespace Graphics
{
    internal class DebugUtils
    {
        public static void LogWithDotsWarning(string message, string name)
        {
            //string message = "LOADING PRESET";
            int messageLength = message.Length;
            int nameLength = name.Length;
            int dotsNeeded = 40 - messageLength - nameLength;

            string dots = new string('.', Math.Max(0, dotsNeeded));

            Graphics.Instance.Log.LogWarning($"{message}: {dots} {name}");
        }


        public static void LogWithDots(string message, string name)
        {
            //string message = "LOADING PRESET";
            int messageLength = message.Length;
            int nameLength = name.Length;
            int dotsNeeded = 40 - messageLength - nameLength - 2;

            string dots = new string('.', Math.Max(0, dotsNeeded));

            Graphics.Instance.Log.LogInfo($"{message}: {dots} [{name}]");
        }

        public static void LogWithDotsMessage(string message, string name)
        {
            //string message = "LOADING PRESET";
            int messageLength = message.Length;
            int nameLength = name.Length;
            int dotsNeeded = 40 - messageLength - nameLength;

            string dots = new string('.', Math.Max(0, dotsNeeded));

            Graphics.Instance.Log.LogMessage($"{message}: {dots} {name}");
        }

        //public static void LogWithDotsSkip(string message)
        //{
        //    int messageLength = message.Length;
        //    int dotsNeeded = 40 - messageLength - 6;
        //    string dots = new string('.', Math.Max(0, dotsNeeded));

        //    Graphics.Instance.Log.LogInfo($"{message}: {dots} [SKIP]");
        //}
    }
}
