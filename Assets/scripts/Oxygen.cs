/*
	Class: Oxygen
	Desc: Extends Element.cs. TO BE IMPLEMENTED.
	An oxygen atom should attract hydrogen atoms, 
	be attracted to carbon atoms, not attract other
	oxygen atoms, and may repel other oxygen atoms.
*/

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
