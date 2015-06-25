using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Element : MonoBehaviour {

	public string name;
	public int maxCharge;
	public int remainingCharge;
	public int atomicNumber;  //number of protons
	public float shieldScale;

	public static float sqrt2 = Mathf.Sqrt(2);
	public static float sqrt3 = Mathf.Sqrt(3);

	public float bondLength;
	public BondingPositionInfo[] relativePositions;
	public GameObject bondPrefab;
	public Quaternion rot;
	public float connectionPriority;
	private float zBeforeMoving;
	public GameObject helperSphere;

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
		public int bpiIndex; //index of relative position e taken at me

		public BondingNeighbour(int b, ref Element e, GameObject bond, int bpiIndex){
			this.bondCharge = b;
			this.neighbour = e;
			this.bond = bond;
			this.bpiIndex = bpiIndex;
			
		}
	}
	public List<BondingNeighbour> bondedNeighbours;

	
	//find an element in the neighbours list, and remove it
	public void RemoveBondingNeighbour(Element e){
		GameObject bondToDestroy = null;
		int itemToRemoveIndex = -1;
		for(int i=0; i < bondedNeighbours.Count;i++){
			if(bondedNeighbours[i].neighbour == e){
				//destroy bond
				bondToDestroy = bondedNeighbours[i].bond;
				//bondedNeighbours[i].bond = null;
				if(bondToDestroy != null){
					Destroy(bondToDestroy);
				}
					
				
				int bpiIndex = bondedNeighbours[i].bpiIndex;
				if(bpiIndex < 0){
					Debug.Log("BPI INDEX < 0");
				}else{
					relativePositions[bpiIndex].taken = false;
				}
				

				//bondedNeighbours[i] = null;
				itemToRemoveIndex = i;
				break;
			}
		}
		if(itemToRemoveIndex >= 0){
			Debug.Log("removing " + e.gameObject.name + " from " + gameObject.name);
			bondedNeighbours.RemoveAt(itemToRemoveIndex);
			if(bondedNeighbours.Count == 0){
				this.transform.forward = Vector3.forward;
				this.rot = this.transform.rotation;
			}
		}


	}
	public virtual void Awake(){
		shieldScale = 4f;
		bondedNeighbours = new List<BondingNeighbour>();
		visitState = (int)VisitState.unvisited;
	
		bondLength = 2.3f;
		rot = Quaternion.identity;
		//set up bonding
		relativePositions = new BondingPositionInfo[4];
		//for center to vertex distance = 1
		helperSphere = GameObject.Find("helperSphere");

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
	
	public void StopMyCoroutines(){
		StopAllCoroutines();
	}
	void OnCollisionEnter(Collision other){
		//Debug.Log("stop attraction between " + gameObject.name + " and " + other.gameObject.name );
		StopAllCoroutines();
		//other.gameObject.GetComponent<Element>().StopMyCoroutines();
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().AddForce(Vector3.zero);
		other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
		other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.zero);
		//other.gameObject.GetComponent<Element>().DetachNeighbours();
		
				
	}
	void OnCollisionStay(Collision other){
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().AddForce(Vector3.zero);
		other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
		other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.zero);
	}
	void OnCollisionExit(Collision other){
		

	}
	void OnMouseDown(){
		if(PlayerControl.self.state != (int)(PlayerControl.State.Default))return;
		AttachShield();
		if(!PlayerControl.moveAtomsAsGroup){
			DetachNeighbours();
		}
		//get z
		zBeforeMoving = Vector3.Distance(transform.position, Camera.main.transform.position);
	}
	void OnMouseDrag(){
		PlayerControl.self.state = (int)PlayerControl.State.HoldingAtom;
		MoveWithMouse();

	}
	//order does not matter
	static bool HasPathBetween(Element e1, Element e2){
		Queue<Element> queue = new Queue<Element>();
		Queue<Element> visitedPath = new Queue<Element>();
		queue.Enqueue(e1);
		visitedPath.Enqueue(e1);

		e1.visitState = (int)VisitState.visiting;

		bool pathFound = false;
		while(queue.Count > 0 && !pathFound){
			Element currElement = queue.Dequeue();
			foreach(BondingNeighbour bondedNeighbour in currElement.bondedNeighbours){
				Element neighbour = bondedNeighbour.neighbour;
				if(neighbour.visitState == (int)VisitState.unvisited){
					if(neighbour == e2){
						pathFound = true;
						break; //break foreach
					}
					else{
						queue.Enqueue(neighbour);
						visitedPath.Enqueue(neighbour);
						neighbour.visitState = (int)VisitState.visiting;
					}
				}

			}//end foreach
			currElement.visitState = (int)VisitState.visited;
		}

		//reset visit states
		while(visitedPath.Count > 0){
			visitedPath.Dequeue().visitState = (int)VisitState.unvisited;
		}
		return pathFound;
	}
	//this: bonder
	public virtual void Bond(Element bondee){
		int bondeeBondingIndex = IndexOfClosestAvailableBondingPosition(
			bondee.transform.position, 
			bondee.gameObject.GetComponent<Collider>()
		);
		if(bondeeBondingIndex < 0){
			//Debug.Log(closestCarbon.gameObject.name + " BondingIndex: " + closestCarbonBondingIndex);
			return;
		}
		//snap e's chain to me
		int myBondingIndex = SnapToBondingLocation(bondee, bondeeBondingIndex);
		//update bondeeindex at this
		for(int i=0; i < this.relativePositions.Length; i++){
			Vector3 p 
				= this.rot * (bondLength/sqrt3 * this.relativePositions[i].position) + this.transform.position;
			if(Vector3.Distance(p,bondee.transform.position) < 0.01f){
				bondeeBondingIndex = i;
				break;
			}
		}
		//Debug.Log(closestCarbon.gameObject.name + " BondingIndex: " + closestCarbonBondingIndex);

		//if e has already bonded with other atoms
		if(myBondingIndex >= 0){
			GameObject bond = bondee.CreateBondWith(this);
			foreach(BondingNeighbour neighbour in this.bondedNeighbours){
				if(neighbour.bpiIndex == bondeeBondingIndex && neighbour.neighbour != bondee){
					Debug.Log(this.gameObject.name + " already has neighbour " 
						+ neighbour.neighbour.gameObject.name + " at pos " + bondeeBondingIndex
						+ ", new neighbour " + bondee.gameObject.name);
				}
			}
			this.bondedNeighbours.Add(new BondingNeighbour(1, ref bondee, bond, bondeeBondingIndex));


			foreach(BondingNeighbour neighbour in bondee.bondedNeighbours){
				if(neighbour.bpiIndex == myBondingIndex && neighbour.neighbour != this){
					Debug.Log(bondee.gameObject.name + " already has neighbour " 
						+ neighbour.neighbour.gameObject.name + " at pos " + myBondingIndex
						+ ", new neighbour " + this.gameObject.name);
				}
			}
			Element thisRef = this;
			bondee.bondedNeighbours.Add(new BondingNeighbour(1,ref thisRef, bond, myBondingIndex));
		}
	}
	public virtual void FindElegibleAtomsForConnection(ref List<Element> eligibleAtoms){}

	void OnMouseUp(){
		if(PlayerControl.self.state != (int)(PlayerControl.State.HoldingAtom))return;
		DetachShield();
		if(PlayerControl.moveAtomsAsGroup){
			DetachNeighbours();
		}
		
		if(this.remainingCharge <= 0)return;
		//check atoms within sphere
		List<Element> eligibleAtoms = new List<Element>();
		
		
		this.FindElegibleAtomsForConnection(ref eligibleAtoms);
		if(this.GetType() == typeof(Carbon)){
			//sort eligible atoms by priority
			List<Element> atomsOrderedByPriority = eligibleAtoms.OrderByDescending(e=>e.connectionPriority).ToList();
			//do we need to clear priority values in eligible atoms list?
			//only need the first four 
			for(int i=0; i < Mathf.Min(this.maxCharge,atomsOrderedByPriority.Count) && this.remainingCharge>0; i++){
				Element currOtherElement = atomsOrderedByPriority[i];
				if(currOtherElement.remainingCharge > 0 && !HasPathBetween(this, currOtherElement)){
					this.Bond(currOtherElement);
				}
			}
		}
		else if(this.GetType() == typeof(Hydrogen)){
			//attach this(hydrogen) to the C-Chain with largest priority
			if(eligibleAtoms.Count == 0)return;
			Element attractor = eligibleAtoms[0];
			if(this.remainingCharge > 0 && !HasPathBetween(this, attractor)){

				this.Bond(attractor);
			}

		}

	}
	public GameObject CreateBondWith(Element e){
		Vector3 bondDirection = e.transform.position - this.transform.position;
		Vector3 bondCenter = 0.5f*(e.transform.position + this.transform.position);
		GameObject newBond = Instantiate(bondPrefab, bondCenter, Quaternion.identity) 
							as GameObject;

		newBond.transform.up = bondDirection;

		
		Vector3 defaultScale = newBond.transform.localScale;
		newBond.transform.localScale 
			= new Vector3(defaultScale.x, 
				bondDirection.magnitude - this.GetComponent<SphereCollider>().radius - e.GetComponent<SphereCollider>().radius, 
				defaultScale.z);
		return newBond;
	}
	void DetachNeighbours(){
		
		//Debug.Log(gameObject.name + " detaching neighbours");
		for(int i=0; i < relativePositions.Length; i++){
			relativePositions[i].taken = false;
		}
		for(int i=0; i < this.bondedNeighbours.Count; i++){
			Element neighbourElement = bondedNeighbours[i].neighbour;
			BondingNeighbour bn = bondedNeighbours[i];
			/*
			if(bn.bpiIndex >= 0){
				
				neighbourElement.relativePositions[bn.bpiIndex].taken = false;
				bn.bpiIndex = -1;
			}
			*/
			//remove this from neighbour elements' neighbour list
			//and destroy the bond object
			neighbourElement.RemoveBondingNeighbour(this);

			neighbourElement.remainingCharge += 1;
			this.remainingCharge += 1;
			Debug.Log("this: " + this.gameObject.name + " remainingCharge: " +  this.remainingCharge
				+ " and e: " + neighbourElement.gameObject.name + " remainingCharge +1");
		}
		//clear my neighbour list
		bondedNeighbours.Clear();
		this.transform.forward = Vector3.forward;
		this.rot = this.transform.rotation;
	}
	//ues BFS to calcualte total mass of the chain
	public virtual int CalculateChainMass(){
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
	public void UpdateBondTransform(GameObject bond, Element neighbour){
		//update bond transformation
		Vector3 bondDirection = neighbour.transform.position - this.transform.position;
		Vector3 bondCenter = 0.5f*(neighbour.transform.position + this.transform.position);

		bond.transform.position = bondCenter;
		bond.transform.rotation = Quaternion.identity;
		bond.transform.up = bondDirection;
	}
	//BFS to attract a whole chain to this
	//return this bpiIndex at e
	int AttractChain(Element e, Vector3 pos){
		
		int thisBPIatE = -1;

		e.transform.position = this.rot 
					* (bondLength/sqrt3 * pos) 
					+ this.transform.position;
		e.transform.forward = this.transform.position - e.transform.position;
		e.rot = e.transform.rotation;
		
		
		if(this.bondedNeighbours.Count == 0){
			this.transform.forward = e.transform.position - this.transform.position;
			this.rot = this.transform.rotation;
		}
		//calculate this' bpi at e
		if(e.GetType() == typeof(Carbon)){
			for(int i=0; i < e.relativePositions.Length; i++){
				Vector3 p = e.rot * (bondLength/sqrt3 * e.relativePositions[i].position) + e.transform.position;
				if(Vector3.Distance(p, this.transform.position) < 0.01f){
					thisBPIatE = i;
					break;
				}
			}
		}
		else if(e.GetType() == typeof(Hydrogen)){
			thisBPIatE = 0;
		}
		if(thisBPIatE < 0){
			Debug.Log(this.gameObject.name + " updated BPI at " + e.gameObject.name + " < 0" );
		}
		
		Queue<Element> queue = new Queue<Element>();
		Queue<Element> visitedPath = new Queue<Element>();

		queue.Enqueue(e);
		visitedPath.Enqueue(e);

		e.visitState = (int)VisitState.visiting;
		while(queue.Count > 0){
			Element currElement = queue.Dequeue();
			
			foreach(BondingNeighbour bondingNeighbour in currElement.bondedNeighbours){
				Element neighbour = bondingNeighbour.neighbour;
				

				if(neighbour.visitState == (int)VisitState.unvisited){
					
					int bpiIndex = bondingNeighbour.bpiIndex;

					if(bpiIndex < 0){
						Debug.Log(neighbour.gameObject.name + " bpiIndex out of range of " 
							+ currElement.gameObject.name + bpiIndex);
						bpiIndex = currElement.IndexOfClosestAvailableBondingPosition(
							neighbour.transform.position, neighbour.GetComponent<Collider>());
					}
					Vector3 newPosition = currElement.rot 
						* (bondLength/sqrt3 * currElement.relativePositions[bpiIndex].position)
						+ currElement.transform.position;

					//find neighbour in currElement's neighbour list and change its bpiindex to 0
					
					for(int i=0; i < neighbour.bondedNeighbours.Count; i++){
						BondingNeighbour bn = neighbour.bondedNeighbours[i];
						if(bn.neighbour == currElement){
							bn.bpiIndex = 0;
							break;
						}
					}
					
					int oldBPI = bpiIndex;
					int newNeighbourBPI = currElement.IndexOfClosestAvailableBondingPosition(
						newPosition, neighbour.GetComponent<Collider>());
					bpiIndex = newNeighbourBPI;
					bondingNeighbour.bpiIndex = bpiIndex;

					//TODO: check if new bpiIndex < 0
					//this should NOT happen!
					if(bpiIndex < 0){
						Debug.Log("new bpi: " + neighbour.gameObject.name + " at " + currElement.gameObject.name
						+ ": " + bpiIndex + ", old bpi: " + oldBPI 
						+ ", remainingCharge: " + currElement.remainingCharge);
						bpiIndex = oldBPI;
						//return -1;
					}
					neighbour.transform.position 
						= currElement.rot 
						* (bondLength/sqrt3 * currElement.relativePositions[bpiIndex].position)
						+ currElement.transform.position;
					
					neighbour.transform.forward 
						= currElement.transform.position - neighbour.transform.position;
					neighbour.rot = neighbour.transform.rotation;
					//update bond transformation
					currElement.UpdateBondTransform(bondingNeighbour.bond, neighbour);
					

					queue.Enqueue(neighbour);
					visitedPath.Enqueue(neighbour);
					neighbour.visitState = (int)VisitState.visiting;
				
				}
				currElement.visitState = (int)VisitState.visited;
			}
		}

		//reset states to unvisited
		while(visitedPath.Count > 0){
			visitedPath.Dequeue().visitState = (int)VisitState.unvisited;
		}
		return thisBPIatE;
	}
	public int IndexOfClosestAvailableBondingPosition(Vector3 pos, Collider coll){
		float minDist = Mathf.Infinity;
		int retIndex = -1;
		string msg = "; ";
		bool collidersContainOther = false;
		for(int i=0; i < relativePositions.Length ; i++){
			Vector3 bondingPos = relativePositions[i].position;
			Vector3 potentialPosition = this.rot 
					* (bondLength/sqrt3 * bondingPos) 
					+ this.transform.position;
			//Instantiate(helperSphere, potentialPosition, Quaternion.identity);

			float dist = Vector3.Distance(pos, potentialPosition);
			//Debug.Log("dist " + dist);
			Collider[] hitColliders = Physics.OverlapSphere(potentialPosition, ((SphereCollider)coll).radius);
			
			if(this.GetType() == typeof(Carbon)){
				if(hitColliders.Length == 1 
					&& hitColliders[0].gameObject != coll.gameObject){
				//this position has been taken
					
					
					
				}
				else if(hitColliders.Length == 1 
					&& hitColliders[0].gameObject == coll.gameObject
					|| hitColliders.Length == 0){
					if(hitColliders.Length == 1 && 
					hitColliders[0].gameObject == coll.gameObject){
						msg += "self";
					}
					if(dist < minDist){
						minDist = dist;
						retIndex = i;
					}
				}
				else{
					msg += " multi colls: ";
					//check if other is one of them
					//collidersContainOther = false;
					/*
					foreach(Collider c in hitColliders){
						if(c.gameObject == coll.gameObject){
							collidersContainOther = true;
							break;
						}
					}
					*/ 
					
				}
			}
			else if(this.GetType() == typeof(Hydrogen)){
				if((hitColliders.Length == 1 && hitColliders[0] == coll)
					|| hitColliders.Length == 0){
					
					minDist = dist;
					retIndex = i;
				}
			}
			
			
		}
		if(collidersContainOther){
			for(int i=0; i < relativePositions.Length;i++){
				Vector3 v = relativePositions[i].position;
				Vector3 p = this.rot 
					* (bondLength/sqrt3 * v) 
					+ this.transform.position;
				if(Vector3.Distance(p, coll.gameObject.transform.position) < 0.01f){
					retIndex = i;
					for(int j=0; j < this.bondedNeighbours.Count; j++){
						Element neighbour = bondedNeighbours[j].neighbour;
						if(neighbour.gameObject == coll.gameObject){
							bondedNeighbours[j].bpiIndex = retIndex;
							break;
						}
					}
					break;
				}
			}
		}
		if(retIndex == -1){
			foreach(BondingNeighbour bn in this.bondedNeighbours){
				msg += bn.neighbour.gameObject.name + ", ";
				GameObject helperSphereClone = 
				Instantiate(helperSphere, bn.neighbour.transform.position, Quaternion.identity) as GameObject;
				helperSphere.name = bn.neighbour.gameObject.name + " _help";
			}
			Debug.Log(coll.gameObject.name + " pos taken at " + gameObject.name + msg);

		}
		return retIndex;
	}
	//e: element to be snapped to connect with this
	//other bonding index: 
	public virtual int SnapToBondingLocation(Element e, int otherBondingIndex = 0){
		//check if I am a carbon, check my neighbours and determine 
		//my orientation and connect e
		//check if I am a carbon, check my neighbours and determine 
		//my orientation and connect e
		if(this.GetType() == typeof(Carbon)){
			
			if(otherBondingIndex < 0){
				Debug.Log("All positions taken!");
				return -1;
			}
			Vector3 otherBondingPosition = relativePositions[otherBondingIndex].position;
			if(remainingCharge > 0 && e.remainingCharge > 0){
				
				int myBondingIndex = -1;
				
				
				myBondingIndex = this.AttractChain(e, otherBondingPosition);
				relativePositions[otherBondingIndex].taken = true;

				if(e.GetType() != typeof(Hydrogen)){
					e.relativePositions[myBondingIndex].taken = true;
				}
				

				this.remainingCharge -= 1;
				e.remainingCharge -= 1;
				/*
				Debug.Log("this: " + gameObject.name + " remainingCharge: " + this.remainingCharge 
					+ " and e: " + e.gameObject.name + " remainingCharge -1");
					*/
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
	public virtual void MoveWithMouse(){
		Vector3 mouseInWorld = Input.mousePosition;
		mouseInWorld.z = zBeforeMoving; 
		Vector3 newAtomPosition = Camera.main.ScreenToWorldPoint(mouseInWorld);

		Vector3 positionOffset = newAtomPosition-this.transform.position;
		if(!PlayerControl.moveAtomsAsGroup){
			transform.position = newAtomPosition;
			return;
		}
		//BFS to move as a group
		Queue<Element> queue = new Queue<Element>();
		Queue<Element> visitedPath = new Queue<Element>();

		queue.Enqueue(this);
		visitedPath.Enqueue(this);
		this.visitState = (int)VisitState.visiting;

		while(queue.Count > 0){
			Element currElement = queue.Dequeue();
			//move 
			currElement.transform.position += positionOffset;
			foreach(BondingNeighbour bondedNeighbour in currElement.bondedNeighbours){
				Element neighbour = bondedNeighbour.neighbour;
				if(neighbour.visitState == (int)VisitState.unvisited){
					//move bond
					// bondedNeighbour.bond
					currElement.UpdateBondTransform(bondedNeighbour.bond, neighbour);
					// find currElement in bondedneighbours of neighbour
					foreach(BondingNeighbour neighboursBondedNeighbour in neighbour.bondedNeighbours){
						if(neighboursBondedNeighbour.neighbour == currElement){
							//neighboursBondedNeighbour.bond
							neighbour.UpdateBondTransform(neighboursBondedNeighbour.bond, currElement);
						}
					}
					queue.Enqueue(neighbour);
					visitedPath.Enqueue(neighbour);
					neighbour.visitState = (int)VisitState.visiting;
				}
			}//end foreach
			currElement.visitState = (int)VisitState.visited;
		}

		while(visitedPath.Count > 0){
			visitedPath.Dequeue().visitState = (int)VisitState.unvisited;
		}
		
	}
}
