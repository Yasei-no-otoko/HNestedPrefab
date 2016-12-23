using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using HojoSystem.Utility;
using System.Runtime.InteropServices;
using HojoSystem;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HojoSystem
{
	[System.Serializable]
	public class ObjectFieldInformation
	{
		public UnityEngine.Object objectTarget;

		/// <summary>
		/// indexがPrefabRootからの距離
		/// [0]==直下
		/// [1]が存在すれば、それは[0]の位置にあったTransformに対するSibling
		/// 以下延々繰り返す
		/// </summary>
		public List<int> SiblingIndexList = new List<int> ();
	}

	/// <summary>
	/// NestedPrefabにシリアライズしてもらって永続化して、内側で別々な意図を持ちたい対象に利用する
	/// </summary>
	public interface INestedPrefabRootSerializingContextBridge:IEventSystemHandler
	{
		void Inject (NestedPrefabRootSerializingContext receiveList);

		NestedPrefabRootSerializingContext GetExpectedDataAsContext ();
	}


}