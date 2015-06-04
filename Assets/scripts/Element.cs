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

			Debug.Log(this.name + " collides " + other.gameObject.name);
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

				if(this.atomicNumber <= e.atomicNumber){
					StartCoroutine(e.Attract(this));
				}
				if(this.atomicNumber >= e.atomicNumber){
					StartCoroutine(this.Attract(e));
				}
			}
			
		}
		DetachShield();
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
