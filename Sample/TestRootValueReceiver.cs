using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HojoSystem;

public class TestRootValueReceiver : MonoBehaviour,INestedPrefabRootSerializingContextBridge
{
	[SerializeField]
	private int testParameter1;
	[SerializeField]
	private float testParameter2;

	[SerializeField]
	private GameObject testObjectField1;
	[SerializeField]
	private Transform testObjectField2;

	#region INestedPrefabRootSerializingContextBridge implementation

	public void Inject (NestedPrefabRootSerializingContext receiveList)
	{
		testParameter1 = int.Parse (receiveList.ValueFieldList [0]);
		testParameter2 = float.Parse (receiveList.ValueFieldList [1]);
		testObjectField1 = NestedPrefabContextInjectionHelper.CastToGameObject (receiveList.ObjectFieldList [0].objectTarget);
		testObjectField2 = NestedPrefabContextInjectionHelper.CastTo<Transform> (receiveList.ObjectFieldList [1].objectTarget);
	}

	public NestedPrefabRootSerializingContext GetExpectedDataAsContext ()
	{
		return new NestedPrefabRootSerializingContext (this.gameObject, testParameter1, testParameter2, testObjectField1, testObjectField2);
	}

	#endregion

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
