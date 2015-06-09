using UnityEngine;
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

	public struct BondingPositionInfo{
		public Vector3 position;
		public bool taken;

		public BondingPositionInfo(Vector3 pos, bool taken = false){
			this.position = pos;
			this.taken = taken;
		}

	}
	public struct BondingNeighbour{
		public int bondCharge;
		public Element neighbour;
		public GameObject bond;
		//neighbours bondingpositioninfo index
		public int bpiIndex;

		public BondingNeighbour(int b, Element e, Element self, GameObject bond, int bpiIndex){
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
		
		canBondWithSameType = true;
		CHBondLength = 4f;
		//set up bonding
		relativePositions = new BondingPositionInfo[4];
		//for center to vertex distance = 1
		Vector3 pos0 = new Vector3(sqrt2/sqrt3, 0, -1/sqrt3); 
		Vector3 pos1 = new Vector3(-sqrt2/sqrt3,0, -1/sqrt3);
		Vector3 pos2 = new Vector3(0, sqrt2/sqrt3,  1/sqrt3);
		Vector3 pos3 = new Vector3(0,-sqrt2/sqrt3,  1/sqrt3);
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
		//Debug.Log("stop attraction between " + gameObject.name + " and " + other.gameObject.name );
		StopAllCoroutines();
		//other.gameObject.GetComponent<Element>().StopMyCoroutines();
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().AddForce(Vector3.zero);
		other.gameObject.GetComponent<Element>().DetachNeighbours();
		
				
	}
	void OnCollisionStay(Collision other){
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().AddForce(Vector3.zero);
	}
	void OnCollisionExit(Collision other){
		
		Element e = other.gameObject.GetComponent<Element>();
		if(this.GetInstanceID() > e.GetInstanceID())return;
		TryDisconnect(this, other.gameObject.GetComponent<Element>());

	}
	void OnMouseDown(){
		AttachShield();
		DetachNeighbours();
	}
	void OnMouseDrag(){
		PlayerControl.self.state = (int)PlayerControl.State.HoldingAtom;
		MoveWithMouse();

	}
	void OnMouseUp(){

		//check atoms within sphere
		float shieldRadius 
			= PlayerControl.sphereShield.GetComponent<MeshRenderer>().bounds.extents.x;

		Collider[] closebyAtoms 
			= Physics.OverlapSphere(
				PlayerControl.sphereShield.transform.position, 
				shieldRadius);

		foreach(Collider c in closebyAtoms){
			Element e = c.GetComponent<Element>();
			if(e.Equals(this)){
				//exclude self
				continue;
			}
			if(this.remainingCharge > 0 ){
				/*
				if(this.atomicNumber <= e.atomicNumber){
					StartCoroutine(e.Attract(this));
				}
				if(this.atomicNumber >= e.atomicNumber){
					StartCoroutine(this.Attract(e));
				}
				*/
				Debug.Log("remainingCharge > 0");
				if(this.GetType() == typeof(Carbon)){
					Debug.Log("I am carbon");
					//if(this.remainingCharge == this.maxCharge){
						//before snapping, disconnect all bonds
						e.DetachNeighbours();
						//TODO: instead of disconnecting all bonds,snap group?
						//snap e to my first bonding location
						int bpiIndex = SnapToBondingLocation(e,bondedNeighbours.Count);
						//SnapNeiboursToNewParentLocation(e);
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
						this.bondedNeighbours.Add(new BondingNeighbour(1, e, this,bond, 0));

						e.bondedNeighbours.Add(new BondingNeighbour(1,this,e, bond, bpiIndex));

						if(e.GetType() != typeof(Oxygen)){
							//TODO?
						}
					//}
				}
				else if(this.GetType() == typeof(Hydrogen)){
					//H bonds with C
					Debug.Log("I am Hydrogen");
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
		DetachShield();
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
	
	//e: element to be snapped to connect with this
	int SnapToBondingLocation(Element e, int index = 0){
		//check if I am a carbon, check my neighbours and determine 
		//my orientation and connect e
		float angle = 0f;
		Vector3 vFrom = Vector3.zero, vTo = Vector3.zero;
		Vector3 rotDir = Vector3.zero;
		Debug.Log(bondedNeighbours.Count);
		if(this.GetType() == typeof(Carbon)){
			//search for an empty bonding location
			BondingPositionInfo bpi = new BondingPositionInfo(Vector3.zero, false);
			bool hasUntakenPosition = false;
			int positionToBeTaken = -1;
			for(int i =0; i < relativePositions.Length; i++){
				if(!relativePositions[i].taken){
					bpi = relativePositions[i];
					positionToBeTaken = i;
					hasUntakenPosition = true;
					break;
				}
			}
			if(!hasUntakenPosition){
				Debug.Log("All positions taken!");
				return -1;
			}
			
			if(bondedNeighbours.Count > 0 
				&& bondedNeighbours.Count < relativePositions.Length){
				//find the taken position
				vFrom = relativePositions[0].position;
				vTo = bondedNeighbours[0].neighbour.transform.position;
				if((vTo-vFrom) == Vector3.zero){
					Debug.Log("vfrom vto in same dir");
				}
				angle = Vector3.Angle(vFrom, vTo);
				rotDir = Vector3.Cross(vFrom, vTo);
				e.transform.position = Quaternion.AngleAxis(angle, rotDir)
					* (CHBondLength/sqrt3 * bpi.position) 
					+ this.transform.position;
				relativePositions[positionToBeTaken].taken = true;
				return positionToBeTaken;
			}
		}

		//search for a bonding position that's not taken 
		for(int i=0; i < this.relativePositions.Length; i++){
			if(!this.relativePositions[i].taken){
				e.transform.position 
					= this.transform.position 
					+ (CHBondLength/sqrt3 * this.relativePositions[i].position);
		
				this.relativePositions[i].taken = true;
				Debug.Log(bondedNeighbours.Count + " " +  e.gameObject.name + " taking position of " + this.gameObject.name + ", " + i);
				return i;
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
		transform.position = newAtomPosition;
		
		
	}
}
