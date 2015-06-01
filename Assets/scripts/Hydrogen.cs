using UnityEngine;
using System.Collections;

public class Hydrogen : Element {
	public static int index;
	void Awake(){
		index = 1;
		atomicNumber = 1;
		maxCharge = 1;
		name = "H";
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
