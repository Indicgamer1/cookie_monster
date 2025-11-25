using UnityEngine;

namespace CookieGame.Utils
{
    /// <summary>
    /// Debug helper utilities
    /// Provides enhanced debugging features
    /// </summary>
    public static class DebugHelper
    {
        private static bool _isDebugEnabled = true;

        /// <summary>
        /// Logs a message with color
        /// </summary>
        public static void Log(string message, Color color)
        {
            if (!_isDebugEnabled) return;
            Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
        }

        /// <summary>
        /// Logs a success message (green)
        /// </summary>
        public static void LogSuccess(string message)
        {
            Log(message, Color.green);
        }

        /// <summary>
        /// Logs an info message (cyan)
        /// </summary>
        public static void LogInfo(string message)
        {
            Log(message, Color.cyan);
        }

        /// <summary>
        /// Logs a warning message (yellow)
        /// </summary>
        public static void LogWarning(string message)
        {
            if (!_isDebugEnabled) return;
            Debug.LogWarning(message);
        }

        /// <summary>
        /// Logs an error message (red)
        /// </summary>
        public static void LogError(string message)
        {
            Debug.LogError(message);
        }

        /// <summary>
        /// Draws a debug line in 3D space
        /// </summary>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f)
        {
            if (!_isDebugEnabled) return;
            Debug.DrawLine(start, end, color, duration);
        }

        /// <summary>
        /// Draws a debug sphere in 3D space
        /// </summary>
        public static void DrawSphere(Vector3 center, float radius, Color color, float duration = 0f)
        {
            if (!_isDebugEnabled) return;

            // Draw sphere using multiple circles
            int segments = 16;
            float angleStep = 360f / segments;

            // Draw circles on each axis
            for (int i = 0; i < segments; i++)
            {
                float angle1 = Mathf.Deg2Rad * (i * angleStep);
                float angle2 = Mathf.Deg2Rad * ((i + 1) * angleStep);

                // XY plane
                Vector3 p1XY = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0f);
                Vector3 p2XY = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0f);
                Debug.DrawLine(p1XY, p2XY, color, duration);

                // XZ plane
                Vector3 p1XZ = center + new Vector3(Mathf.Cos(angle1) * radius, 0f, Mathf.Sin(angle1) * radius);
                Vector3 p2XZ = center + new Vector3(Mathf.Cos(angle2) * radius, 0f, Mathf.Sin(angle2) * radius);
                Debug.DrawLine(p1XZ, p2XZ, color, duration);

                // YZ plane
                Vector3 p1YZ = center + new Vector3(0f, Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius);
                Vector3 p2YZ = center + new Vector3(0f, Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius);
                Debug.DrawLine(p1YZ, p2YZ, color, duration);
            }
        }

        /// <summary>
        /// Draws a debug box in 3D space
        /// </summary>
        public static void DrawBox(Vector3 center, Vector3 size, Color color, float duration = 0f)
        {
            if (!_isDebugEnabled) return;

            Vector3 halfSize = size * 0.5f;

            // Define corners
            Vector3[] corners = new Vector3[8];
            corners[0] = center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            corners[1] = center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            corners[2] = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            corners[3] = center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            corners[4] = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            corners[5] = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            corners[6] = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
            corners[7] = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            // Draw bottom face
            Debug.DrawLine(corners[0], corners[1], color, duration);
            Debug.DrawLine(corners[1], corners[2], color, duration);
            Debug.DrawLine(corners[2], corners[3], color, duration);
            Debug.DrawLine(corners[3], corners[0], color, duration);

            // Draw top face
            Debug.DrawLine(corners[4], corners[5], color, duration);
            Debug.DrawLine(corners[5], corners[6], color, duration);
            Debug.DrawLine(corners[6], corners[7], color, duration);
            Debug.DrawLine(corners[7], corners[4], color, duration);

            // Draw vertical lines
            Debug.DrawLine(corners[0], corners[4], color, duration);
            Debug.DrawLine(corners[1], corners[5], color, duration);
            Debug.DrawLine(corners[2], corners[6], color, duration);
            Debug.DrawLine(corners[3], corners[7], color, duration);
        }

        /// <summary>
        /// Enables or disables debug logging
        /// </summary>
        public static void SetDebugEnabled(bool enabled)
        {
            _isDebugEnabled = enabled;
        }

        /// <summary>
        /// Logs memory usage
        /// </summary>
        public static void LogMemoryUsage()
        {
            if (!_isDebugEnabled) return;

            long totalMemory = System.GC.GetTotalMemory(false);
            float memoryMB = totalMemory / (1024f * 1024f);

            LogInfo($"Memory Usage: {memoryMB:F2} MB");
        }
    }
}
