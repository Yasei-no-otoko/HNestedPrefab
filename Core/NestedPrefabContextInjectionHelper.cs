using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using HojoSystem.Utility;
using HojoSystem;
using System;
using System.Security.Cryptography.X509Certificates;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HojoSystem
{

	public static class NestedPrefabContextInjectionHelper
	{
		public static SerializationInformation GetSerializationInformation <T> (T value)
		{
			//nullからtypeは取れないが、型にnullが入っているものをジェネリックメソッドが受け取ればtypeをとれる
			return new SerializationInformation (value, typeof(T));
		}

		public static GameObject CastToGameObject (object target)
		{
			if (target is GameObject) {
				return ((GameObject)target);
			} else if (target is Component) {
				return ((Component)target).gameObject;
			}
			return null;
		}

		public static T CastTo<T> (object target)
		{
			GameObject gameObject = null;
			if ((gameObject = CastToGameObject (target)) == null) {
				return default(T);
			}
			return gameObject.GetComponent<T> ();
		}
	}
}