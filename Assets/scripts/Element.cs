using UnityEngine;
using System.Collections;

public class Element : MonoBehaviour {
	public string name;
	public int maxCharge;
	public int atomicNumber;  //number of protons
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDrag(){
		Debug.Log("dragging");
		PlayerControl.self.state = (int)PlayerControl.State.HoldingAtom;
		MoveWithMouse();
		
	}
	void OnMouseUp(){
		PlayerControl.self.state = (int)PlayerControl.State.Default;
	}
	void MoveWithMouse(){
		Vector3 mouseInWorld = Input.mousePosition;
		mouseInWorld.z = 10f;
		Vector3 newAtomPosition = Camera.main.ScreenToWorldPoint(mouseInWorld);
		transform.position = newAtomPosition;
	}
}
