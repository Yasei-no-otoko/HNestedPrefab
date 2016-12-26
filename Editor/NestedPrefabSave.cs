using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using HojoSystem.Utility;
using UnityEngine.SceneManagement;

namespace HojoSystem
{
	/// <summary>
	/// シーンセーブされるときは正常に最上位ルートのみが保存されているが、ビルドされるとどうも全部保存されている感じがする
	/// このままでは毎回DeleteChilrenを走らせなければならない　要調査
	/// </summary>
	public class NestedPrefabSave:AssetModificationProcessor
	{
		static string[] OnWillSaveAssets (string[] paths)
		{
			bool isSceneSave = false;
			if (paths.Length == 0) {
				Selection.activeGameObject = null;
				isSceneSave = true;
			}

			PrefabUtility.prefabInstanceUpdated = null;
			EditorApplication.playmodeStateChanged = null;

			var currentScene = SceneManager.GetActiveScene ();
			var gameObjects = currentScene.GetRootGameObjects ().ToList ();

			//現状applyしたターゲットの認識をSelection.activeGameObjectでやってるが、俺はやらないがインスペクタ複数出して使う時とかどうなるかわかんない
			//もっと適切な方法くれ
			var targetObject = Selection.activeGameObject;
			if (!isSceneSave && (targetObject == (GameObject)null || targetObject.GetComponent<HNestedPrefabRoot> () == null)) {
				//SceneSaveでなく、しかもHNestedPrefabRootでなかったら処理を切り上げる
				return paths;
			}

			gameObjects.SelectMany (m => m.transform.GetComponentsInChildren<HNestedPrefabRoot> ())
				.Where (m => m != null && m.gameObject == targetObject)
				.ToList ()
				.ForEach (m => {
				m.GenerateNestedPrefabData ();
			});

			//上のGenerateNestedPrefabData ()の段階ではシーン全体の構成が保たれてないと厳しい場面が結構出てくるので終わってからHNestedPrefabRootの子は全部削除
			//これによってシーンにセーブされるとルートしか存在しない感じになるが、どうせシーンに於いたらまとめてロードすることには変わらないので速度は変わらない（同一オブジェクトとして認識されるものが増えるのでパースの時間は減ると思うが）
			//セーブ後には何事もなかったかのように生成されているので、利用者は何も気付かない
			gameObjects.SelectMany (m => m.transform.GetComponentsInChildren<HNestedPrefabRoot> ())
				.OrderBy (m => m.transform.childCount)
				.Where (m => m != null)
				.ToList ()
				.ForEach (m => {
				if (m != null) {
					//もしApplyした対象がネストされた子だった場合、保存するために削除回避
					m.DeleteChildren (targetObject);
				}
				return;
			});

			string savedObjectString = string.Empty;
			GameObject.FindObjectsOfType<GameObject> ().ToList ().ForEach (m => savedObjectString += m.name + "\n");
			return paths;
		}
	}

}