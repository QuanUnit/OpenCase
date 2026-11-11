using UnityEngine;

namespace Shared.Tools.DebugExtensions
{
    public class DebugColor
    {
        public static void Log(string message, Color color, Object context = default)
        {
            var hexColor = ColorUtility.ToHtmlStringRGB(color);

            var coloredMessage = $"<color=#{hexColor}>{message}</color>";

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