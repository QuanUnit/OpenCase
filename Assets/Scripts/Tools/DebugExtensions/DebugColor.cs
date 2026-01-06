using UnityEngine;

namespace Tools.DebugExtensions
{
    public class DebugColor
    {
        public static void Log(string message, Color color, Object context = default)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(color);

            string coloredMessage = $"<color=#{hexColor}>{message}</color>";

            if (context == default)
            {
                Debug.Log(coloredMessage);
            }
            else
            {
                Debug.Log(coloredMessage, context);
            }
        }
    }
}