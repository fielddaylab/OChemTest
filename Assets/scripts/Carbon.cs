using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Carbon : Element {
	public override void Awake(){
		base.Awake();
		atomicNumber = 6;
		maxCharge = 4;
		remainingCharge = maxCharge;
		name = "C";
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
