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
	[System.Serializable]
	public class NestedPrefabRootSerializingContext
	{

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


		public NestedPrefabRootSerializingContext (GameObject injectedObject, params object[] injectionList)
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
				if (injectionList [i] is UnityEngine.Object || injectionList [i] == null) {
					objectFieldList.Add (new ObjectFieldInformation ());

					if (injectionList [i] == null || (injectionList [i] is Component && ((Component)injectionList [i]) == null) || (injectionList [i] is GameObject && ((GameObject)injectionList [i]) == null)) {
						continue;
					}

					bool isPrefabOriginal = PrefabUtility.GetPrefabParent ((UnityEngine.Object)injectionList [i]) == null && PrefabUtility.GetPrefabObject ((UnityEngine.Object)injectionList [i]) != null;


					var last = objectFieldList.Last ();

					if (isPrefabOriginal) {
						last.SiblingIndexList.Add (-1);
						last.objectTarget = (UnityEngine.Object)injectionList [i];
					} else {
						Transform searchTransform = injectedObject.transform;
						if (injectionList [i] is GameObject) {
							searchTransform = ((GameObject)injectionList [i]).transform;
						} else {
							searchTransform = ((Component)injectionList [i]).transform;
						}
						last.objectTarget = (UnityEngine.Object)PrefabUtility.GetPrefabParent ((UnityEngine.Object)injectionList [i]);

						while (searchTransform.GetComponent<HNestedPrefabRoot> () != root) {
							last.SiblingIndexList.Add (searchTransform.transform.GetSiblingIndex ());
							searchTransform = searchTransform.transform.parent;
							if (searchTransform == null) {
								throw new NullReferenceException ("SceneObject needs to be children of HNestedPrefabRoot for serialization");
							}
						}
					}

				} else {
					valueFieldList.Add (injectionList [i].ToString ());
				}
			}
			#endif

		}
	}
}