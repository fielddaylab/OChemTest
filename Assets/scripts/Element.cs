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

	public struct BondingPositionInfo{
		public Vector3 position;
		public bool taken;
		public BondingPositionInfo(Vector3 pos, bool taken = false){
			this.position = pos;
			this.taken = taken;
		}
	}
	public struct BondingNeighbour{
		public int bond;
		public Element neighbour;
		public BondingNeighbour(int b, Element e){
			this.bond = b;
			this.neighbour = e;
		}
	}
	public List<BondingNeighbour> bondedNeighbours;

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
				bond = bn.bond;
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

		Element e = other.gameObject.GetComponent<Element>();
		//avoid repetition
		if(this.GetInstanceID() > e.GetInstanceID())return;

		int bondCharge = Mathf.Min(this.remainingCharge, e.remainingCharge);
		if(bondCharge <= 0){
			Debug.Log(name + ": " + remainingCharge + ", " + e.name + ": " + e.remainingCharge);
		}

		if(bondCharge > 0 && 
			(e.GetType() != this.GetType() || e.canBondWithSameType )){

			//Debug.Log(this.name + " collides " + other.gameObject.name);
			this.remainingCharge -= bondCharge;
			e.remainingCharge -= bondCharge;
			BondingNeighbour bondee = new BondingNeighbour(bondCharge, e);
			BondingNeighbour me = new BondingNeighbour(bondCharge, this);
			bondedNeighbours.Add(bondee);
			e.bondedNeighbours.Add(me);
		}
		
				
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
			if(this.remainingCharge > 0 && e.remainingCharge > 0){
				/*
				if(this.atomicNumber <= e.atomicNumber){
					StartCoroutine(e.Attract(this));
				}
				if(this.atomicNumber >= e.atomicNumber){
					StartCoroutine(this.Attract(e));
				}
				*/
				if(this.GetType() == typeof(Carbon)){
					//if(this.remainingCharge == this.maxCharge){
						//snap e to my first bonding location
						SnapToBondingLocation(e,bondedNeighbours.Count);
					//}
				}
				else if(this.GetType() == typeof(Hydrogen)){
					if(e.GetType() == typeof(Carbon)){
						e.SnapToBondingLocation(this, e.bondedNeighbours.Count);
					}
					else if(e.GetType() == typeof(Hydrogen)){
						//do nothing or repel
					}
				}
			}
			
		}
		DetachShield();
	}
	void DetachNeighbours(){
		
		Debug.Log(gameObject.name + " detaching neighbours");
		for(int i=0; i < relativePositions.Length; i++){
			relativePositions[i].taken = false;
		}

	}
	//e: element to be snapped
	void SnapToBondingLocation(Element e, int index = 0){
		//search for a bonding position that's not taken 
		for(int i=0; i < this.relativePositions.Length; i++){
			if(!this.relativePositions[i].taken){
				e.transform.position 
					= this.transform.position 
					+ (CHBondLength/sqrt3 * this.relativePositions[i].position);
				Debug.Log(this.relativePositions[i].position);
				this.relativePositions[i].taken = true;
				Debug.Log(e.gameObject.name + " taking position of " + this.gameObject.name + ", " + i);
				break;
			}
		}
		
	}
	IEnumerator Attract(Element other){
		//v = at + v0

		//d = 1/2 a * t^2
		float distance = Vector3.Distance(transform.position, other.transform.position);
		//Debug.Log(distance + ", " + radiusSum);

		while(true){
			yield return new WaitForSeconds(0.01f);

			Vector3 forceToOther = (transform.position-other.transform.position).normalized
				*40f/Mathf.Pow(distance,2f);

			other.GetComponent<Rigidbody>().AddForce(forceToOther);
			distance = Vector3.Distance(transform.position, other.transform.position);
		}
			

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
