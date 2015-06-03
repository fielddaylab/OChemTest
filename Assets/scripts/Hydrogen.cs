using UnityEngine;
using System.Collections;

public class Hydrogen : Element {
	public static int index;
	public override void Awake(){
		base.Awake();
		index = 1;
		atomicNumber = 1;
		maxCharge = 1;
		remainingCharge = maxCharge;
		name = "H";
		canBondWithSameType = false;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
