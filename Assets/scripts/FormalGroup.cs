using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FormalGroup : MonoBehaviour {
	public string name;
	public int maxBondCharge; //CH(3, or 1,2)
	public int remainingCharge;
	public int[] elements; //[0]: C, [1]: H, [2] O
	public List<FormalGroup> possibleConnections;
	public List<FormalGroup> connectedGroups;
	void Awake(){
		remainingCharge = maxBondCharge;
		connectedGroups = new List<FormalGroup>();
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static void ConnectGroups(FormalGroup fg0, FormalGroup fg1, int charge = 1){
		if(fg0.connectedGroups.Contains(fg1)){
			return;
		}
		fg0.connectedGroups.Add(fg1);
		fg1.connectedGroups.Add(fg0);
		//change remaining charges
		fg0.remainingCharge -= charge;
		fg1.remainingCharge -= charge;

	}

}
