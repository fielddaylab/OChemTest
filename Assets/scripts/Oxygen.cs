using UnityEngine;
using System.Collections;

public class Oxygen : Element{
	public static int index;
	public override void Awake(){
		base.Awake();
		index = 2;
		atomicNumber = 8;
		maxCharge = 2;
		name = "O";
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
