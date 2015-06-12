using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	public override void FindElegibleAtomsForConnection(ref List<Element> eligibleAtoms){
		float shieldRadius 
			= PlayerControl.sphereShield.GetComponent<MeshRenderer>().bounds.extents.x;

		Collider[] closebyAtoms 
			= Physics.OverlapSphere(
				PlayerControl.sphereShield.transform.position, 
				shieldRadius);

		float maxPriority = -1f;
		Element elementWithMaxAttraction = null;

		foreach(Collider c in closebyAtoms){
			Element otherElement = c.gameObject.GetComponent<Element>();
			if(otherElement.GetType() != typeof(Hydrogen) 
				&& otherElement.remainingCharge > 0 
				&& otherElement != this){

				float dist = Vector3.Distance(this.transform.position, otherElement.transform.position);
				//Get chain mass
				if(dist >= 0.001f){
					float chainMass = otherElement.CalculateChainMass();
					float priority = chainMass/dist;
					otherElement.connectionPriority = priority;

					if(otherElement.connectionPriority > maxPriority){
						maxPriority = otherElement.connectionPriority;
						elementWithMaxAttraction = otherElement;
					}
				}
				
			}else{
				Debug.Log(otherElement.remainingCharge);
				Debug.Log(otherElement == this);
			}
		}
		if(elementWithMaxAttraction != null)
			eligibleAtoms.Add(elementWithMaxAttraction);
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
