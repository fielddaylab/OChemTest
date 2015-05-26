using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConnectionRules : MonoBehaviour {
	public static ConnectionRules self;
	public FormalGroup formalGroupPrefab;
	public Dictionary<string,FormalGroup> allFormalGroups;
	void Awake(){
		self = this;
		if(formalGroupPrefab  == null){
			Debug.Log("No formal group template!");
		}
		allFormalGroups = new Dictionary<string, FormalGroup>();
		//FormalGroup formalGroup = Instantiate(formalGroupPrefab) as FormalGroup;
		GameObject[] dicFormalGroups = GameObject.FindGameObjectsWithTag("DictionaryFormalGroup");
		foreach(GameObject g in dicFormalGroups){
			FormalGroup fg = g.GetComponent<FormalGroup>();
			Debug.Log(fg.name);
			allFormalGroups.Add(fg.name, fg);
		}
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
