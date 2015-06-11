﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Element : MonoBehaviour {

	public string name;
	public int maxCharge;
	public int remainingCharge;
	public int atomicNumber;  //number of protons
	public float shieldScale;
	public float speed;
	public float accleration;
	public bool canBondWithSameType;

	public static float sqrt2 = Mathf.Sqrt(2);
	public static float sqrt3 = Mathf.Sqrt(3);
	public float CHBondLength;
	public float CCBondLength;
	public BondingPositionInfo[] relativePositions;
	public GameObject bondPrefab;
	public Quaternion rot;

	public int visitState;
	public enum VisitState{
		unvisited,
		visited,
		visiting
	};
	public class BondingPositionInfo{
		public Vector3 position;
		public bool taken;

		public BondingPositionInfo(Vector3 pos, bool taken = false){
			this.position = pos;
			this.taken = taken;
		}

	}
	public class BondingNeighbour{
		public int bondCharge;
		public Element neighbour;
		public GameObject bond;
		//neighbours bondingpositioninfo index
		public int bpiIndex;

		public BondingNeighbour(int b, Element e, GameObject bond, int bpiIndex){
			this.bondCharge = b;
			this.neighbour = e;
			this.bond = bond;
			this.bpiIndex = bpiIndex;
		}
	}
	public List<BondingNeighbour> bondedNeighbours;

	//find the bondee in e that is the closest to this
	public Element ClosestNeighbourOf(Element e){
		Element element2Debond = null;
		float minDist = 100f;
		foreach(BondingNeighbour bn in e.bondedNeighbours){
			float d = Vector3.Distance(bn.neighbour.transform.position, this.transform.position);
			if(d < minDist){
				minDist = d;
				element2Debond = bn.neighbour;
			}
		}
		return element2Debond;
	}
	public void TryBreakClosestNeghbour(Element e){
		Element element2Debond = this.ClosestNeighbourOf(e);

		if(element2Debond != null){
			e.RemoveBondingNeighbour(element2Debond, true);
			element2Debond.RemoveBondingNeighbour(e, true);
			e.remainingCharge += 1;
			element2Debond.remainingCharge += 1;
		}
	}
	//find an element in the neighbours list, and remove it
	public void RemoveBondingNeighbour(Element e, bool destroyBond = true){
		Debug.Log("removing bonding neighbour");
		GameObject bondToDestroy = null;
		for(int i=0; i < bondedNeighbours.Count;i++){
			if(bondedNeighbours[i].neighbour == e){
				if(destroyBond){
					bondToDestroy = bondedNeighbours[i].bond;
					//bondedNeighbours[i].bond = null;
					if(bondToDestroy != null){
						Destroy(bondToDestroy);
					}
					
				}

				bondedNeighbours.RemoveAt(i);

				return;
			}
		}

	}
	//TODO
	//snaps the group eInGroup is in to connect with destinationElement
	public void SnapGroup(Element eInGroup, Element destinationElement){

	}
	public virtual void Awake(){
		speed = 0f;
		accleration = 1f;
		shieldScale = 4f;
		bondedNeighbours = new List<BondingNeighbour>();
		visitState = (int)VisitState.unvisited;

		canBondWithSameType = true;
		CHBondLength = 3f;
		rot = Quaternion.identity;
		//set up bonding
		relativePositions = new BondingPositionInfo[4];
		//for center to vertex distance = 1
		/*
		Vector3 pos0 = new Vector3(sqrt2/sqrt3, 0, -1/sqrt3); 
		Vector3 pos1 = new Vector3(-sqrt2/sqrt3,0, -1/sqrt3);
		Vector3 pos2 = new Vector3(0, sqrt2/sqrt3,  1/sqrt3);
		Vector3 pos3 = new Vector3(0,-sqrt2/sqrt3,  1/sqrt3);
		*/
		Vector3 pos0 = new Vector3(0, 0, 1); 
		Vector3 pos1 = new Vector3(0, 2*sqrt2/3f, -1/3f);
		Vector3 pos2 = new Vector3(sqrt2/sqrt3, -sqrt2/3f,  -1/3f);
		Vector3 pos3 = new Vector3(-sqrt2/sqrt3, -sqrt2/3f,  -1/3f);
		
		
		relativePositions[0] = new BondingPositionInfo(pos0);
		relativePositions[1] = new BondingPositionInfo(pos1);
		relativePositions[2] = new BondingPositionInfo(pos2);
		relativePositions[3] = new BondingPositionInfo(pos3);


	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	//returns true if e1 and e2 were conneted and now disconnected
	//else return false
	public static bool TryDisconnect(Element e1, Element e2){
		bool removedE2FromE1 = false;
		bool bothRemoved = false;
		int bond = -1;
		for(int i=0; i < e1.bondedNeighbours.Count; i++){
			BondingNeighbour bn = e1.bondedNeighbours[i];
			if(bn.neighbour == e2){
				e1.bondedNeighbours.RemoveAt(i);
				removedE2FromE1 = true;
				break;
			}
		}
		if(!removedE2FromE1)return false;
		for(int i=0; i < e2.bondedNeighbours.Count;i++){
			BondingNeighbour bn = e2.bondedNeighbours[i];
			if(bn.neighbour == e1){
				e2.bondedNeighbours.RemoveAt(i);
				bond = bn.bondCharge;
				bothRemoved = true;
				break;
			}
		}
		if(bothRemoved){
			e1.remainingCharge += bond;
			e2.remainingCharge += bond;
		}
		return bothRemoved;
	}
	public void StopMyCoroutines(){
		StopAllCoroutines();
	}
	void OnCollisionEnter(Collision other){
		//other.gameObject.GetComponent<Element>().StopMyCoroutines();
		//GetComponent<Rigidbody>().velocity = Vector3.zero;
		//GetComponent<Rigidbody>().AddForce(Vector3.zero);
		//other.gameObject.GetComponent<Element>().DetachNeighbours();
		
				
	}
	void OnCollisionStay(Collision other){
		/*
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().AddForce(Vector3.zero);
		*/
	}
	void OnCollisionExit(Collision other){
		
		Element e = other.gameObject.GetComponent<Element>();
		if(this.GetInstanceID() > e.GetInstanceID())return;
		TryDisconnect(this, other.gameObject.GetComponent<Element>());

	}
	void OnMouseDown(){
		AttachShield();
		//DetachNeighbours();
	}
	void OnMouseDrag(){
		PlayerControl.self.state = (int)PlayerControl.State.HoldingAtom;
		MoveWithMouse();

	}
	int IndexOfClosestAvailableBondingPosition(Vector3 pos, Collider coll){
		float minDist = Mathf.Infinity;
		int retIndex = -1;
		for(int i=0; i < relativePositions.Length;i++){
			Vector3 bondingPos = relativePositions[i].position;
			Vector3 potentialPosition = this.rot 
					* (CHBondLength/sqrt3 * bondingPos) 
					+ this.transform.position;

			float dist = Vector3.Distance(pos, potentialPosition);
			Collider[] hitColliders = Physics.OverlapSphere(potentialPosition, ((SphereCollider)coll).radius);
			
			if(dist < minDist && !relativePositions[i].taken){
				


				if((hitColliders.Length == 1 && hitColliders[0] == coll)
					|| hitColliders.Length == 0){
					
					minDist = dist;
					retIndex = i;
				}else{
					if(hitColliders.Length > 1){
						//Debug.Log(gameObject.name + " will hit other");
					}
					else if(hitColliders.Length == 1 && hitColliders[0] != coll){
						//Debug.Log(gameObject.name + " will hit " + hitColliders[0].gameObject.name);

					}
					//Debug.Log(i + " taken " + relativePositions[i].taken);
				}
				
			}else{
				if(relativePositions[i].taken){
					//Debug.Log(gameObject.name + " pos " + i + " taken");
				}
				Debug.Log(i + " taken " + relativePositions[i].taken);
			}
		}
		return retIndex;
	}
	void AttachAtom(Element atom){
		int atomBondingIndex = IndexOfClosestAvailableBondingPosition(atom.transform.position, atom.GetComponent<Collider>());
		Debug.Log(gameObject.name + ": " +  atomBondingIndex);
		if(atomBondingIndex < 0)return; //no bonding position available
		int myBondingIndex = this.SnapToBondingLocation(atom, atomBondingIndex);

		GameObject bond = CreateBondWith(atom);

		this.bondedNeighbours.Add(new BondingNeighbour(1, atom, bond, atomBondingIndex));

		atom.bondedNeighbours.Add(new BondingNeighbour(1,this, bond, myBondingIndex));
		
	}
	void OnMouseUp(){
		DetachShield();
		//DetachNeighbours();
		//check atoms within sphere
		float shieldRadius 
			= PlayerControl.sphereShield.GetComponent<MeshRenderer>().bounds.extents.x;

		Collider[] closebyAtoms 
			= Physics.OverlapSphere(
				PlayerControl.sphereShield.transform.position, 
				shieldRadius);

		Carbon closestCarbon = null;
		List<Element> otherAtoms = new List<Element>();
		float minDistance = Mathf.Infinity;

		if(this.GetType() == typeof(Carbon) && this.remainingCharge > 0){
			//find closest carbon and all hydrogens
			foreach(Collider collider in closebyAtoms){
				Element elementWithinRadius = collider.gameObject.GetComponent<Element>();
				if (elementWithinRadius != this
					&& elementWithinRadius.remainingCharge > 0){

					if(elementWithinRadius.GetType() == typeof(Carbon)){
						float dist = Vector3.Distance(this.transform.position, 
							elementWithinRadius.transform.position);
						if(dist < minDistance){
							minDistance = dist;
							closestCarbon = (Carbon)elementWithinRadius;
						}
					}
					otherAtoms.Add(elementWithinRadius);
				}
				
			}//end of foreach

			if(closestCarbon != null){
				int myBondingIndex = closestCarbon.IndexOfClosestAvailableBondingPosition(this.transform.position, GetComponent<Collider>());
				if(myBondingIndex < 0){
					//Debug.Log("myBondingIndex: " + myBondingIndex);
					return;
				}
				int otherBondingIndex = closestCarbon.SnapToBondingLocation(this, myBondingIndex);
				Debug.Log("myBondingIndex: " + myBondingIndex);
				//if e has already bonded with other atoms

				GameObject bond = closestCarbon.CreateBondWith(this);
			

				this.bondedNeighbours.Add(new BondingNeighbour(1, closestCarbon, bond, otherBondingIndex));

				closestCarbon.bondedNeighbours.Add(new BondingNeighbour(1,this, bond, myBondingIndex));
			}
			//attach hydrogens to me(carbon)
			Debug.Log("num of otheratoms: " + otherAtoms.Count);
			foreach(Element otherAtom in otherAtoms){
				if(otherAtom.GetType() == typeof(Carbon) && (Carbon)otherAtom == closestCarbon){
					Debug.Log("CLOSEST: " + otherAtom.gameObject.name);
				}
				if(otherAtom.GetType() == typeof(Carbon) && (Carbon)otherAtom != closestCarbon){
					AttachAtom(otherAtom);
				}
			}
		}
		/*
		foreach(Collider c in closebyAtoms){
			Element e = c.GetComponent<Element>();

			if(e.Equals(this)){
				//exclude self
				continue;
			}
			if(e.GetType() == typeof(Carbon)){
				//Debug.Log(e.gameObject.name + " chain mass: " + e.CalculateChainMass());
			}
			
			if(this.remainingCharge > 0 ){
				
				if(this.GetType() == typeof(Carbon)){
					//if(this.remainingCharge == this.maxCharge){
						//before snapping, disconnect all bonds
						//e.DetachNeighbours();
						int otherBondingIndex 
							= IndexOfClosestAvailableBondingPosition(e.transform.position);

						int myBondingIndex = SnapToBondingLocation(e,otherBondingIndex);
						Debug.Log("myBondingIndex: " + myBondingIndex);
						//if e has already bonded with other atoms

						GameObject bond = CreateBondWith(e);
						this.remainingCharge -= 1;
						if(e.remainingCharge == 0){
							//break an existing bond of e's
							//Find the closest neighbour to e and break it
							TryBreakClosestNeghbour(e);
						}
						e.remainingCharge -= 1;
						//add each other to neighbours list with bond
						

						this.bondedNeighbours.Add(new BondingNeighbour(1, e, this,bond, otherBondingIndex));

						e.bondedNeighbours.Add(new BondingNeighbour(1,this,e, bond, myBondingIndex));
						
				}
				else if(this.GetType() == typeof(Hydrogen)){
					//H bonds with C
					if(this.remainingCharge == 0){
						
						e.TryBreakClosestNeghbour(this);
					}
					if(this.remainingCharge > 0){

						if(e.GetType() == typeof(Carbon)){
							Debug.Log("that is carbon");
							this.DetachNeighbours();
							int bpiIndex = e.SnapToBondingLocation(this, e.bondedNeighbours.Count);

							GameObject bond = e.CreateBondWith(this);

							this.bondedNeighbours.Add(new BondingNeighbour(1, e, this, bond, bpiIndex));
							e.bondedNeighbours.Add(new BondingNeighbour(1,this, e,bond, 0));
							e.remainingCharge -=1;
							this.remainingCharge -=1;
						}
						else if(e.GetType() == typeof(Hydrogen)){
							//do nothing (or repel?)
						}
					}
					
				}
			}
			
		}
		*/
		
	}
	void UpdateBondTransform(Transform bondTransform, Element otherAtom){
		Vector3 bondDirection = otherAtom.transform.position - this.transform.position;
		Vector3 bondCenter = 0.5f*(otherAtom.transform.position + this.transform.position);

		bondTransform.position = bondCenter;
		bondTransform.rotation = Quaternion.identity;
		bondTransform.up = bondDirection;
		
	}
	GameObject CreateBondWith(Element e){
		Vector3 bondDirection = e.transform.position - this.transform.position;
		Vector3 bondCenter = 0.5f*(e.transform.position + this.transform.position);
		GameObject newBond = Instantiate(bondPrefab, bondCenter, Quaternion.identity) 
							as GameObject;

		newBond.transform.up = bondDirection;

		
		Vector3 defaultScale = newBond.transform.localScale;
		newBond.transform.localScale 
			= new Vector3(defaultScale.x, bondDirection.magnitude-1f, defaultScale.z);
		return newBond;
	}
	void DetachNeighbours(){
		
		Debug.Log(gameObject.name + " detaching neighbours");
		for(int i=0; i < relativePositions.Length; i++){
			relativePositions[i].taken = false;
		}
		for(int i=0; i < this.bondedNeighbours.Count; i++){
			Element neighbourElement = bondedNeighbours[i].neighbour;
			BondingNeighbour bn = bondedNeighbours[i];
			if(bn.bpiIndex >= 0){
				
				neighbourElement.relativePositions[bn.bpiIndex].taken = false;
				bn.bpiIndex = -1;
			}
			
			//remove this from neighbour elements' neighbour list
			//and destroy the bond object
			neighbourElement.RemoveBondingNeighbour(this, true);
			neighbourElement.remainingCharge += 1;
			this.remainingCharge += 1;
		}
		//clear my neighbour list
		bondedNeighbours.Clear();
	}
	//ues BFS to calcualte total mass of the chain
	int CalculateChainMass(){
		int totalMass = 0;
		Queue<Element> queue = new Queue<Element>();
		//used as a copy of the queue to clear the states
		Queue<Element> visitedPath = new Queue<Element>();

		queue.Enqueue(this);
		visitedPath.Enqueue(this);

		this.visitState = (int)VisitState.visiting;
		while(queue.Count > 0){
			//dequeue
			Element currElement = queue.Dequeue();
			totalMass += currElement.atomicNumber;

			foreach(BondingNeighbour bondingNeighbour in currElement.bondedNeighbours){
				Element neighbour = bondingNeighbour.neighbour;
				if(neighbour.visitState == (int)VisitState.unvisited){
					queue.Enqueue(neighbour);
					visitedPath.Enqueue(neighbour);
					neighbour.visitState = (int)VisitState.visiting;
				}
			}
			currElement.visitState = (int)VisitState.visited;
		}
		//reset states to unvisited
		while(visitedPath.Count > 0){
			visitedPath.Dequeue().visitState = (int)VisitState.unvisited;
		}
		return totalMass;
	}
	//BFS to attract a whole chain to this
	void AttractChain(Element e, Vector3 pos){
		
		e.transform.position = this.rot 
					* (CHBondLength/sqrt3 * pos) 
					+ this.transform.position;

		e.transform.forward = this.transform.position - e.transform.position;
		e.rot = e.transform.rotation;
	

		Queue<Element> queue = new Queue<Element>();
		Queue<Element> visitedPath = new Queue<Element>();

		queue.Enqueue(e);
		visitedPath.Enqueue(e);

		e.visitState = (int)VisitState.visiting;
		while(queue.Count > 0){
			Element currElement = queue.Dequeue();
			int j=1;
			foreach(BondingNeighbour bondingNeighbour in currElement.bondedNeighbours){
				Element neighbour = bondingNeighbour.neighbour;
				

				if(neighbour.visitState == (int)VisitState.unvisited){
					
					//int bpiIndex = bondingNeighbour.bpiIndex;
					int bpiIndex = j;
					neighbour.transform.position 
						= currElement.rot 
						* (CHBondLength/sqrt3 * currElement.relativePositions[bpiIndex%4].position)
						+ currElement.transform.position;

					neighbour.transform.forward 
						= currElement.transform.position - neighbour.transform.position;
					neighbour.rot = neighbour.transform.rotation;
					/*
					Destroy(bondingNeighbour.bond);
					GameObject bond = currElement.CreateBondWith(neighbour);
					bondingNeighbour.bond = bond;
					*/
					Vector3 bondDirection = neighbour.transform.position - this.transform.position;
					Vector3 bondCenter = 0.5f*(neighbour.transform.position + this.transform.position);

					bondingNeighbour.bond.transform.position = bondCenter;
					bondingNeighbour.bond.transform.rotation = Quaternion.identity;
					bondingNeighbour.bond.transform.up = bondDirection;

					queue.Enqueue(neighbour);
					visitedPath.Enqueue(neighbour);
					neighbour.visitState = (int)VisitState.visiting;
					j++;
				}
				else if(neighbour.visitState == (int)VisitState.visiting){
					Debug.Log(neighbour.gameObject.name + " visiting");
				}
				else{
					Debug.Log(neighbour.gameObject.name + " visited");
				}
				currElement.visitState = (int)VisitState.visited;
			}
		}

		//reset states to unvisited
		while(visitedPath.Count > 0){
			visitedPath.Dequeue().visitState = (int)VisitState.unvisited;
		}
	}
	//e: element to be snapped to connect with this
	int SnapToBondingLocation(Element e, int otherBondingIndex = 0){
		//check if I am a carbon, check my neighbours and determine 
		//my orientation and connect e
		if(this.GetType() == typeof(Carbon)){
			
			if(otherBondingIndex < 0){
				Debug.Log("All positions taken!");
				return -1;
			}
			Vector3 otherBondingPosition = relativePositions[otherBondingIndex].position;
			if(bondedNeighbours.Count >= 0 
				&& bondedNeighbours.Count < relativePositions.Length){
				
				/*
				e.transform.position = this.rot //Quaternion.AngleAxis(angle, rotDir)
					* (CHBondLength/sqrt3 * bpi.position) 
					+ this.transform.position;
					
				*/
				int myBondingIndex = e.IndexOfClosestAvailableBondingPosition(this.transform.position, GetComponent<Collider>());
				if(myBondingIndex < 0){
					return -1;
				}
				this.AttractChain(e, otherBondingPosition);
				relativePositions[otherBondingIndex].taken = true;
				e.relativePositions[myBondingIndex].taken = true;

				this.remainingCharge -= 1;
				e.remainingCharge -= 1;

				return myBondingIndex;
			}
		}

		return -1;
		
	}

	void AttachShield(){
		//attach sphere shield as child
		PlayerControl.sphereShield.transform.parent = this.transform;
		PlayerControl.sphereShield.transform.localPosition = Vector3.zero;
		PlayerControl.sphereShield.transform.localScale = Vector3.one * shieldScale;
		PlayerControl.sphereShield.SetActive(true);
	}
	void DetachShield(){
		PlayerControl.self.state = (int)PlayerControl.State.Default;
		PlayerControl.sphereShield.SetActive(false);
		PlayerControl.sphereShield.transform.parent = null;
	}

	void MoveWithMouse(){
		Vector3 mouseInWorld = Input.mousePosition;
		mouseInWorld.z = 10f;
		Vector3 newAtomPosition = Camera.main.ScreenToWorldPoint(mouseInWorld);
		//transform.position = newAtomPosition;
		Vector3 translationVector = newAtomPosition - transform.position;
		//BFS to move a group of atoms together
		Queue<Element> queue = new Queue<Element>();
		Queue<Element> visitedPath = new Queue<Element>();

		queue.Enqueue(this);
		visitedPath.Enqueue(this);
		this.visitState = (int)VisitState.visiting;
		while(queue.Count > 0){
			Element currElement = queue.Dequeue();
			currElement.transform.position += translationVector;

			foreach(BondingNeighbour bondedNeighbour in currElement.bondedNeighbours){

				Element neighbour = bondedNeighbour.neighbour;
				if(neighbour.visitState == (int)VisitState.unvisited){
					if(bondedNeighbour.bond != null){
						Vector3 bondDirection = neighbour.transform.position - currElement.transform.position;
						Vector3 bondCenter = 0.5f*(neighbour.transform.position + currElement.transform.position);

						bondedNeighbour.bond.transform.position = bondCenter;
						bondedNeighbour.bond.transform.rotation = Quaternion.identity;
						bondedNeighbour.bond.transform.up = bondDirection;
					}else{
						Debug.Log("bond is null");
					}
					
					queue.Enqueue(neighbour);
					visitedPath.Enqueue(neighbour);
					neighbour.visitState = (int)VisitState.visiting;
				}
			}
			currElement.visitState = (int)VisitState.visited;
		}
		while(visitedPath.Count > 0){
			visitedPath.Dequeue().visitState = (int)VisitState.unvisited;
		}
		
	}
}
