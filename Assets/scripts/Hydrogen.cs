using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hydrogen : Element {
	public override void Awake(){
		base.Awake();
		atomicNumber = 1;
		maxCharge = 1;
		remainingCharge = maxCharge;
		name = "H";
	}
	//snap me to e
	public override int SnapToBondingLocation(Element attractor, int index=0){
		//check if I am a carbon, check my neighbours and determine 
		//my orientation and connect e
		//check if I am a carbon, check my neighbours and determine 
		//my orientation and connect e
		
		if(remainingCharge > 0){
			
			int myBondingIndex = attractor.IndexOfClosestAvailableBondingPosition(
				this.transform.position, GetComponent<Collider>());
			if(myBondingIndex < 0){
				Debug.Log("not bonding: h myBondingIndex < 0");
				return -1;
			}
			//-----------
			Vector3 myRelativeBondingPosition = attractor.relativePositions[myBondingIndex].position;
			this.transform.position = attractor.rot 
					* (bondLength/sqrt3 * myRelativeBondingPosition) 
					+ attractor.transform.position;

			this.transform.forward = attractor.transform.position - this.transform.position;
			this.rot = this.transform.rotation;
			if(attractor.bondedNeighbours.Count == 0){
				attractor.transform.forward = this.transform.position - attractor.transform.position;
				attractor.rot = attractor.transform.rotation;
			}
			//-----------
			attractor.relativePositions[myBondingIndex].taken = true;

			this.remainingCharge -= 1;
			attractor.remainingCharge -= 1;
			//update myBondingIndex
			for(int i=0; i < attractor.relativePositions.Length; i++){
				Vector3 p = attractor.rot * (bondLength/sqrt3 * attractor.relativePositions[i].position)
					+ attractor.transform.position;
				if(Vector3.Distance(p, this.transform.position) < 0.01f){
					myBondingIndex = i;
					break;
				}
			}
			return myBondingIndex;
		}
		
		return -1;
		
	}
	public override void Bond(Element attractor){
		//snap me to attractor's chain
		int myBondingIndex = this.SnapToBondingLocation(attractor);
		//Debug.Log(closestCarbon.gameObject.name + " BondingIndex: " + closestCarbonBondingIndex);

		//if e has already bonded with other atoms
		if(myBondingIndex >= 0){
			GameObject bond = attractor.CreateBondWith(this);
			this.bondedNeighbours.Add(new BondingNeighbour(1, attractor, bond, 0));

			attractor.bondedNeighbours.Add(new BondingNeighbour(1,this, bond, myBondingIndex));
		}else{
		}
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
				){

				float dist = Vector3.Distance(this.transform.position, otherElement.transform.position);
				//Get chain mass
				if(dist > 0f){
					float chainMass = otherElement.CalculateChainMass();
					float priority = chainMass/dist;
					otherElement.connectionPriority = priority;

					if(otherElement.connectionPriority > maxPriority){
						maxPriority = otherElement.connectionPriority;
						elementWithMaxAttraction = otherElement;
					}

				}
				
			}else{
			}
		}
		if(elementWithMaxAttraction != null){
			eligibleAtoms.Add(elementWithMaxAttraction);
		}
			
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
