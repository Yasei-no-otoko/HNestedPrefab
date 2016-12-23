using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HojoSystem
{
	public static class TransformExtension
	{
		public static T GetNearestParentComponent<T> (this Transform transform)
		{
			Transform checkTarget = transform;
			while (checkTarget != null) {
				var targetComponent = checkTarget.GetComponent<T> ();
				if (targetComponent == null) {
					checkTarget = transform.parent;
				} else {
					return targetComponent;
				}
			}
			return default(T);
		}

		public static void SetX (this RectTransform rectTransform, float x)
		{
			rectTransform.anchoredPosition = new Vector2 (x, rectTransform.anchoredPosition.y);
		}

		public static void SetY (this RectTransform rectTransform, float y)
		{
			rectTransform.anchoredPosition = new Vector2 (rectTransform.anchoredPosition.x, y);
		}

		public static void SetX (this Transform rectTransform, float x)
		{
			rectTransform.position = new Vector3 (x, rectTransform.position.y, rectTransform.position.z);
		}

		public static void SetY (this Transform rectTransform, float y)
		{
			rectTransform.position = new Vector3 (rectTransform.position.x, y, rectTransform.position.z);
		}

		public static void SetZ (this Transform rectTransform, float z)
		{
			rectTransform.position = new Vector3 (rectTransform.position.x, rectTransform.position.y, z);
		}

		public static void LerpX (this RectTransform rectTransform, float a, float b, float t)
		{
			rectTransform.SetX (Mathf.Lerp (a, b, t));
		}

		public static void LerpY (this RectTransform rectTransform, float a, float b, float t)
		{
			rectTransform.SetY (Mathf.Lerp (a, b, t));
		}
	}
}