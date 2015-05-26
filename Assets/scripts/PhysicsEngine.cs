using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsEngine : MonoBehaviour {
	public List<FormalGroup> formalGroups;
	//a reference to connection rules' dictionary
	public Dictionary<string, FormalGroup> connectionMap;

	void Awake(){
		GameObject[] formalGroupObjects = GameObject.FindGameObjectsWithTag("ActiveFormalGroup");
		foreach(GameObject g in formalGroupObjects){
			FormalGroup fg = g.GetComponent<FormalGroup>();
			if(fg != null){
				formalGroups.Add(fg);
			}
		}
		connectionMap = ConnectionRules.self.allFormalGroups;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		for(int i=0; i < formalGroups.Count; i++){
			//do a sphere cast
			FormalGroup currentFormalGroup = formalGroups[i];
			float cutoffRadius = 10.0f;
			Collider[] closeFormalGroups 
				= Physics.OverlapSphere(
					currentFormalGroup.transform.position, 
					cutoffRadius,
					LayerMask.GetMask("ActiveFormalGroup")
					);
			if(closeFormalGroups != null && closeFormalGroups.Length > 0){
				//Debug.Log("has hit");
				foreach(Collider coll in closeFormalGroups){
					FormalGroup closeFormalGroup = coll.gameObject.GetComponent<FormalGroup>();
					//Debug.Log(currentFormalGroup.name);
					if(connectionMap.ContainsKey(currentFormalGroup.name)
						&& connectionMap.ContainsKey(closeFormalGroup.name)){

						FormalGroup formalGroupInDic
							= connectionMap[currentFormalGroup.name];
						FormalGroup closeFormalGroupInDic 
							= connectionMap[closeFormalGroup.name];
						/* //Debug
						Debug.Log(formalGroupInDic != null);
						Debug.Log(closeFormalGroupInDic != null);
						Debug.Log(currentFormalGroup.remainingCharge > 0);
						Debug.Log(closeFormalGroup.remainingCharge > 0);
						Debug.Log("-----------------------------------");
						*/
						if(formalGroupInDic != null && closeFormalGroupInDic != null
							&& currentFormalGroup.remainingCharge > 0
							&& closeFormalGroup.remainingCharge > 0){

							if(formalGroupInDic.possibleConnections
								.Contains(closeFormalGroupInDic)){
								Debug.Log("can connect " + currentFormalGroup.name + ", " + closeFormalGroup.name);
								FormalGroup.ConnectGroups(currentFormalGroup, closeFormalGroup);
							}
						}
					}
				}
			}
		}
	}
}
