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
}
