using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using HojoSystem.Utility;
using UnityEngine.SceneManagement;

namespace HojoSystem
{


	/// <summary>
	/// エディター外で残り続けるかどうか確認
	/// </summary>
	[ExecuteInEditMode ()]
	public class HNestedPrefabRoot : MonoBehaviour
	{

		private bool isInitialized;

		[SerializeField,Header ("Do not input manually. Only for seeing.")]
		private List<PrefabInitializeData> prefabInitializeDataList = new List<PrefabInitializeData> ();

		void OnEnable ()
		{

			#if UNITY_EDITOR
			RegisterDeleteChildrenEvent ();
			#endif

			if (!isInitialized) {
				InstantiateNestedPrefabs ();
			}

			#if UNITY_EDITOR
			if (EditorApplication.isPlaying) 
			#endif
			Destroy (this);
		}


		public void InstantiateNestedPrefabs ()
		{
			#if UNITY_EDITOR
			//Unity5.5はビルド前に勝手にOnWillSaveAssetsを呼び出さない形でシーンを生成しセーブする気配がある　少なくともOnEnableが呼ばれる
			//ビルド前に勝手にInstantiatePrefabで生成された状態になってしまうので、ビルド中はInstantiateNestedPrefabsを回避
			if (BuildPipeline.isBuildingPlayer) {
				return;
			}
			#endif

			#if UNITY_EDITOR
			DeleteChildren ();
			#endif

			List<INestedPrefabRootSerializingContextBridge[]> contextBridgeList = new List<INestedPrefabRootSerializingContextBridge[]> ();

			prefabInitializeDataList.ForEach (m => {
				#if UNITY_EDITOR
				if (!EditorApplication.isPlayingOrWillChangePlaymode) {
					var instance = (GameObject)PrefabUtility.InstantiatePrefab (m.assetPrefab);
					instance.transform.SetParent (this.transform);
					instance.transform.localPosition = m.localPosition;
					instance.transform.localScale = m.LocalScale;
					instance.transform.localRotation = Quaternion.Euler (m.EulerAngleRotation);

					var contextListt = instance.GetComponents<INestedPrefabRootSerializingContextBridge> ();

					contextBridgeList.Add (contextListt);
					return;
				}
				#endif
				var instancee = (GameObject)Instantiate (m.assetPrefab);

				instancee.transform.SetParent (this.transform);
				instancee.transform.localPosition = m.localPosition;
				instancee.transform.localScale = m.LocalScale;
				instancee.transform.localRotation = Quaternion.Euler (m.EulerAngleRotation);
				var contextList = instancee.GetComponents<INestedPrefabRootSerializingContextBridge> ();
				contextBridgeList.Add (contextList);
			});
			isInitialized = true;

			for (int j = 0; j < contextBridgeList.Count; j++) {
				for (int i = 0; i < contextBridgeList [j].Length && i < prefabInitializeDataList [j].injectionContextList.Count; i++) {
					GrabSceneObjectToContext (prefabInitializeDataList [j].injectionContextList [i]);
					contextBridgeList [j] [i].Inject (prefabInitializeDataList [j].injectionContextList [i]);
				}
			}

		}

		/// <summary>
		/// 注入コンテクストの要求がSceneObjectであるときによしなにやって取得する必要がある
		/// </summary>
		private void GrabSceneObjectToContext (NestedPrefabRootSerializingContext context)
		{
			for (int i = 0; i < context.ObjectFieldList.Count; i++) {
				var siblingIndexList = context.ObjectFieldList [i].SiblingIndexList;
				if (siblingIndexList == null || siblingIndexList.Count == 0 || siblingIndexList [0] == -1) {
					return;
				}
				Transform searchTransform = transform;
				for (int j = siblingIndexList.Count - 1; j >= 0; j--) {
					int siblingIndex = siblingIndexList [j];
					searchTransform = searchTransform.GetChild (siblingIndex);
				}
				if (!context.ObjectFieldList [i].objectTarget is GameObject) {
					context.ObjectFieldList [i].objectTarget = searchTransform.GetComponent (context.ObjectFieldList [i].objectTarget.GetType ());
				} else {
					context.ObjectFieldList [i].objectTarget = searchTransform.gameObject;
				}
			}
		}


		#if UNITY_EDITOR

		private static bool isRegisteredDeleteEvent;

		public void RegisterDeleteChildrenEvent ()
		{
			if (isRegisteredDeleteEvent) {
				return;
			}

			//意味分からんがunity5.5時点でInstantiatePrefabで作ってたプレハブは存在をコピーされてシーンルートにコピーされてて実行瞬間には既に削除できなくなっているので、実行直前イベントに滑り込ませる
			//#if UNITY_EDITORになっているが、シーンセーブ時にはrootのオブジェクトしかセーブされていないため、ビルド後はそもそもどうでもいい
			EditorApplication.playmodeStateChanged = () => {
				if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying) {

					var currentScene = SceneManager.GetActiveScene ();
					var gameObjects = currentScene.GetRootGameObjects ().ToList ();

					gameObjects.SelectMany (m => m.transform.GetComponentsInChildren<HNestedPrefabRoot> ()).Where (m => m != null).ToList ().ForEach (m => {
						if (m != null) {
							m.DeleteChildren ();
						}
					});
				}
			};

			isRegisteredDeleteEvent = true;


		}

		void LateUpdate ()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode) {
				if (!isInitialized) {
					//PrefabUtility.prefabInstanceUpdatedがmacではapply時によばれないというか、なにがあってもよばれん
					var position = gameObject.transform.localPosition;
					var preRotation = gameObject.transform.localRotation;
					var localScale = transform.localScale;
					PrefabUtility.RevertPrefabInstance (gameObject);
					gameObject.transform.localPosition = position;
					gameObject.transform.localRotation = preRotation;
					transform.localScale = localScale;
					InstantiateNestedPrefabs ();
				}
			}
		}

		public void GenerateNestedPrefabData ()
		{
			prefabInitializeDataList.Clear ();
			HojoLogger.Log ("GenerateNestedPrefabData invoked " + gameObject.name);
			var recognizedTransformList = gameObject.GetComponentsInChildren<Transform> ().Where (m => m.parent == this.transform).OrderByDescending (m => m.childCount).ToList ();
			GameObject originalOfThisObject = null;
			try {
				originalOfThisObject = ((Transform)PrefabUtility.GetPrefabParent (transform)).gameObject;
			} catch {
				return;
			}
			recognizedTransformList.ForEach (m => {
				GameObject original = null;
				try {
					original = ((Transform)PrefabUtility.GetPrefabParent (m)).gameObject;
				} catch {
					HojoLogger.LogWarning ("Nested Prefab Contents need to be Unity Prefab.(Register to Project View as a persistent file.)", HojoLogger.LoggerColor.WarningOrange);
					return;
				}
				if (original == originalOfThisObject) {
					HojoLogger.LogWarning ("You cannot nest same prefab.You need infinitely long something? Give me a break!", HojoLogger.LoggerColor.WarningOrange);
					return;
				}

				var newInitializeData = new PrefabInitializeData ();
				newInitializeData.assetPrefab = original;
				newInitializeData.localPosition = m.localPosition;
				newInitializeData.EulerAngleRotation = m.localRotation.eulerAngles;
				newInitializeData.LocalScale = m.localScale;

				var contextList = m.GetComponents<INestedPrefabRootSerializingContextBridge> ();
				for (int i = 0; i < contextList.Length; i++) {
					newInitializeData.injectionContextList.Add (contextList [i].GetExpectedDataAsContext ());
				}

				prefabInitializeDataList.Add (newInitializeData);


			});


			isInitialized = false;
		}

		/// <summary>
		/// exeptionTargetは削除に対する例外（削除しないもの）
		/// </summary>
		public void DeleteChildren (GameObject exeptionTarget = null)
		{
			gameObject.GetComponentsInChildren<Transform> ().Where (mm => mm.parent == this.transform).ToList ().ForEach (mm => {
				if (mm != null && mm.gameObject != exeptionTarget) {
					DestroyImmediate (mm.gameObject);
				}
			});

			isInitialized = false;
		}
		#endif


	}

	[System.Serializable]
	public class PrefabInitializeData
	{
		
		public Object assetPrefab;
		public Vector3 localPosition;
		public Vector3 EulerAngleRotation;
		public Vector3 LocalScale;
		public List<NestedPrefabRootSerializingContext> injectionContextList = new List<NestedPrefabRootSerializingContext> ();
	}




}