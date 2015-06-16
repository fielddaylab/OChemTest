using UnityEngine;
using System.Collections;

public class Oxygen : Element{
	public override void Awake(){
		base.Awake();
		atomicNumber = 8;
		maxCharge = 2;
		remainingCharge = maxCharge;
		name = "O";
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
