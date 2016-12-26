using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using HojoSystem.Utility;
using HojoSystem;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace HojoSystem
{
	public class SerializationInformation
	{
		public object Value;
		public Type Type;

		public SerializationInformation (object value, Type type)
		{
			this.Value = value;
			this.Type = type;
		}
	}

	[System.Serializable]
	public class NestedPrefabRootSerializingContext
	{
		//Unityがネイティブ側で対応しているやつは個別に対応する必要がある animationcurve以外は今んとこあるのかさえ知らない
		//[SerializeField]
		//List<AnimationCurve> animationCurveList = new List<AnimationCurve> ();

		[SerializeField]
		List<string> valueFieldList = new List<string> ();

		public List<string> ValueFieldList {
			get {
				return valueFieldList;
			}
		}

		/// <summary>
		/// ObjectFieldの参照がNestedPrefab内の物体だった場合はSiblingIndexが格納される
		/// </summary>
		[SerializeField]
		List<ObjectFieldInformation> objectFieldList = new List<ObjectFieldInformation> ();

		public List<ObjectFieldInformation> ObjectFieldList {
			get {
				return objectFieldList;
			}
		}


		public NestedPrefabRootSerializingContext (GameObject injectedObject, params SerializationInformation[] injectionList)
		{
			#if UNITY_EDITOR
			HNestedPrefabRoot root = injectedObject.transform.GetNearestParentComponent<HNestedPrefabRoot> ();
			if (root == null) {
				throw new NullReferenceException ("HNestedPrefabRoot doesn't exist.");
			}

			if (valueFieldList == null) {
				valueFieldList = new List<string> ();
			}
			if (objectFieldList == null) {
				objectFieldList = new List<ObjectFieldInformation> ();
			}
			for (int i = 0; i < injectionList.Length; i++) {
				if (typeof(UnityEngine.Object).IsAssignableFrom (injectionList [i].Type)) {
					objectFieldList.Add (new ObjectFieldInformation ());

					//Unityの型としてnullを扱わせる必要があるっぽい 多分equal overrideしている
					if (injectionList [i].Value == null || (injectionList [i].Value is Component && ((Component)injectionList [i].Value) == null) || (injectionList [i].Value is GameObject && ((GameObject)injectionList [i].Value) == null)) {
						continue;
					}

					bool isPrefabOriginal = PrefabUtility.GetPrefabParent ((UnityEngine.Object)injectionList [i].Value) == null && PrefabUtility.GetPrefabObject ((UnityEngine.Object)injectionList [i].Value) != null;


					var last = objectFieldList.Last ();

					if (isPrefabOriginal) {
						last.SiblingIndexList.Add (-1);
						last.objectTarget = (UnityEngine.Object)injectionList [i].Value;
					} else {
						Transform searchTransform = injectedObject.transform;
						if (injectionList [i].Type == typeof(GameObject)) {
							searchTransform = ((GameObject)injectionList [i].Value).transform;
						} else {
							searchTransform = ((Component)injectionList [i].Value).transform;
						}
						last.objectTarget = (UnityEngine.Object)PrefabUtility.GetPrefabParent ((UnityEngine.Object)injectionList [i].Value);

						while (searchTransform.GetComponent<HNestedPrefabRoot> () != root) {
							last.SiblingIndexList.Add (searchTransform.transform.GetSiblingIndex ());
							searchTransform = searchTransform.transform.parent;
							if (searchTransform == null) {
								throw new NullReferenceException ("SceneObject needs to be children of HNestedPrefabRoot for serialization");
							}

						}
					}

				} else {
					var type = injectionList [i].Type;
					if (type.IsPrimitive || type == typeof(string)) {
						valueFieldList.Add (injectionList [i].Value.ToString ());
					} else {
						throw new NullReferenceException ("Currently Primitive type or string type are only allowed to be serialized.Your original class or structure or something that Unity natively serializing(ex.AnimationCurve) cannnot be serialized.If your original something, Before serialization you can decompose to primitive and you can serialize them.");
					}
				}
			}
			#endif

		}
	}
}