using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Carbon : Element {
	//4 single, 1 double + 2 single, or 1 triple + 1 single
	//if an H nearby, search through the list
	public static int index;
	public override void Awake(){
		base.Awake();
		index = 0;
		atomicNumber = 6;
		maxCharge = 4;
		shieldScale = 4f;
		name = "C";
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
