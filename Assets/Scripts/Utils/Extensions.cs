using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CookieGame.Utils
{
    /// <summary>
    /// Extension methods for common Unity types
    /// Provides convenient helper methods following DRY principle
    /// </summary>
    public static class Extensions
    {
        #region Transform Extensions

        /// <summary>
        /// Destroys all children of a transform
        /// </summary>
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Destroys all children immediately (useful in editor)
        /// </summary>
        public static void DestroyAllChildrenImmediate(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Resets transform to default values
        /// </summary>
        public static void ResetTransform(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Gets all children of a transform
        /// </summary>
        public static List<Transform> GetAllChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
            }
            return children;
        }

        #endregion

        #region Vector Extensions

        /// <summary>
        /// Returns a vector with only the X and Y components
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        /// <summary>
        /// Returns a vector with Z set to 0
        /// </summary>
        public static Vector3 FlattenZ(this Vector3 vector)
        {
            return new Vector3(vector.x, vector.y, 0f);
        }

        /// <summary>
        /// Returns a random point within bounds
        /// </summary>
        public static Vector3 RandomPoint(this Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        #endregion

        #region Color Extensions

        /// <summary>
        /// Returns a color with modified alpha
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Converts hex string to Color
        /// </summary>
        public static Color ToColor(this string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }

        #endregion

        #region UI Extensions

        /// <summary>
        /// Sets alpha of a CanvasGroup
        /// </summary>
        public static void SetAlpha(this CanvasGroup canvasGroup, float alpha)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        /// <summary>
        /// Shows a CanvasGroup
        /// </summary>
        public static void Show(this CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Hides a CanvasGroup
        /// </summary>
        public static void Hide(this CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Sets the text color of a Text component
        /// </summary>
        public static void SetTextColor(this Text text, Color color)
        {
            text.color = color;
        }

        #endregion

        #region Collection Extensions

        /// <summary>
        /// Shuffles a list using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Gets a random element from a list
        /// </summary>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);

            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Checks if list is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        #endregion

        #region GameObject Extensions

        /// <summary>
        /// Gets or adds a component
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Checks if GameObject has component
        /// </summary>
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }

        /// <summary>
        /// Sets layer recursively
        /// </summary>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        #endregion
    }
}
