using UnityEngine;
using System.Collections;

public class Element : MonoBehaviour {
	public string name;
	public int maxCharge;
	public int atomicNumber;  //number of protons
	public float shieldScale;
	public float speed;
	public float accleration;
	public virtual void Awake(){
		speed = 0f;
		accleration = 1f;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
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
		//find nearby atom with the max atomic number
		Collider atomWithMaxAtomicNumber = null;
		Collider atomWithMinAtomicNumber = null;
		int minAtomicNumber = 1000;
		int maxAtomicNumber = 0;
		foreach(Collider c in closebyAtoms){
			Element e = c.GetComponent<Element>();
			if(e.Equals(this)){
				//exclude self
				continue;
			}
			if(e.atomicNumber > maxAtomicNumber){
				maxAtomicNumber = e.atomicNumber;
				atomWithMaxAtomicNumber = c;
			}
			if(e.atomicNumber < minAtomicNumber){
				minAtomicNumber = e.atomicNumber;
				atomWithMinAtomicNumber = c;
			}
		}
		if(atomWithMaxAtomicNumber != null){
			Element e = atomWithMaxAtomicNumber.GetComponent<Element>();
			if(this.atomicNumber <= e.atomicNumber){
				StartCoroutine(e.Attract(this));
			}
		}
		if(atomWithMinAtomicNumber != null){
			Element e = atomWithMinAtomicNumber.GetComponent<Element>();
			if(this.atomicNumber >= e.atomicNumber){
				StartCoroutine(this.Attract(e));
			}
		}
		DetachShield();
	}
	IEnumerator Attract(Element other){
		//v = at + v0

		//d = 1/2 a * t^2
		float distance = Vector3.Distance(transform.position, other.transform.position);
		float myRadius = GetComponent<SphereCollider>().radius;
		float otherRadius = other.GetComponent<SphereCollider>().radius;
		float radiusSum = myRadius + otherRadius;
		//Debug.Log(distance + ", " + radiusSum);

		while(distance > radiusSum){
			yield return new WaitForSeconds(0.01f);
			float vCurrent = other.speed + this.accleration * 0.01f;
			float deltaDistance = (other.speed + vCurrent) * 0.01f / 2f;
			other.transform.position 
				= Vector3.MoveTowards(
					other.transform.position, 
					this.transform.position, 
					deltaDistance);

			other.speed = vCurrent;

			distance = Vector3.Distance(transform.position, other.transform.position);
			myRadius = GetComponent<SphereCollider>().radius;
			otherRadius = other.GetComponent<SphereCollider>().radius;
			radiusSum = myRadius + otherRadius;
			//Debug.Log(distance + ", " + radiusSum);
		}
		other.speed = 0f;
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
