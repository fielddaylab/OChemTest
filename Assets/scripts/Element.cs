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
	public void StopMyCoroutines(){
		StopAllCoroutines();
	}
	void OnCollisionEnter(Collision other){
		//Debug.Log("stop attraction between " + gameObject.name + " and " + other.gameObject.name );
		StopAllCoroutines();
		//other.gameObject.GetComponent<Element>().StopMyCoroutines();
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().AddForce(Vector3.zero);
		//other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.zero);
		//other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
	}
	void OnCollisionStay(){
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().AddForce(Vector3.zero);
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
			if(this.atomicNumber <= e.atomicNumber){
				StartCoroutine(e.Attract(this));
			}
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
