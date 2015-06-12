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
		remainingCharge = maxCharge;
		name = "C";
		canBondWithSameType = true;
	}
	public override void FindElegibleAtomsForConnection(ref List<Element> eligibleAtoms){
		float shieldRadius 
			= PlayerControl.sphereShield.GetComponent<MeshRenderer>().bounds.extents.x;

		Collider[] closebyAtoms 
			= Physics.OverlapSphere(
				PlayerControl.sphereShield.transform.position, 
				shieldRadius);

		foreach(Collider c in closebyAtoms){
			Element otherElement = c.gameObject.GetComponent<Element>();
			if(otherElement.remainingCharge > 0 && otherElement != this){
				float dist = Vector3.Distance(this.transform.position, otherElement.transform.position);
				//Get chain mass
				if(dist >= 0.001f){
					float chainMass = otherElement.CalculateChainMass();
					float priority = chainMass/dist;
					otherElement.connectionPriority = priority;
					eligibleAtoms.Add(otherElement);
				}
				
			}else{
				Debug.Log(otherElement.remainingCharge);
				Debug.Log(otherElement == this);
			}
		}
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
