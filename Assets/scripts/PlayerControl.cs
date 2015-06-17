using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {
	[HideInInspector]public Camera camera;
	public static PlayerControl self;
	public static GameObject sphereShield;
	public static Vector3 center;
	public float horizontalSpeed = 2.0f;
	public float verticalSpeed = 2.0f; 
	public static bool moveAtomsAsGroup;

	private Element atomBeingHeld;

	public int state;
	public enum State{
		HoldingAtom,
		AddingAtom,
		Default
	};
	void Awake(){
		self = this;
		center = Vector3.zero;
		moveAtomsAsGroup = false;
		camera = GetComponent<Camera>();
		atomBeingHeld = null;
		state = (int)State.Default;
		sphereShield = GameObject.Find("SphereShield");
		
		sphereShield.SetActive(false);
	}
	// Use this for initialization
	void Start () {
	
	}
	void RotateCameraAroundCenter(){
		float yRotation = Input.GetAxis("Mouse X") * horizontalSpeed;
		float rightRotation = Input.GetAxis("Mouse Y") * verticalSpeed;
		yRotation *= Time.deltaTime;
		rightRotation *= Time.deltaTime;

		transform.RotateAround(center, transform.rotation*Vector3.up, yRotation * Mathf.Rad2Deg);
		transform.RotateAround(center, transform.rotation*Vector3.left, rightRotation * Mathf.Rad2Deg);
	}
	public void OnAddAtom(Element e){
		Vector3 spawnPosition;
		Vector3 mousePosition = Input.mousePosition;
		//add depth
		mousePosition.z = 10f;
		spawnPosition = camera.ScreenToWorldPoint(mousePosition);
		GameObject newAtom = GameObject.Instantiate(e.gameObject, spawnPosition, Quaternion.identity) as GameObject;
		atomBeingHeld = newAtom.GetComponent<Element>();
		//find bondPrefab
		atomBeingHeld.bondPrefab = GameObject.Find("BondPrefab");
		//change state
		state = (int)State.AddingAtom;
	}
	void MoveAtomWithMouse(){
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 10f;
		Vector3 newAtomPosition = camera.ScreenToWorldPoint(mousePosition);
		atomBeingHeld.transform.position = newAtomPosition;
	}
	// Update is called once per frame
	void Update () {
		if(Input.GetKey("g")){
			moveAtomsAsGroup = true;	
		}
		else if(Input.GetKey("s")){
			moveAtomsAsGroup = false;
		}
		if(state == (int)State.Default){
			if(Input.GetMouseButton(0)){
				RotateCameraAroundCenter();
			}	
		}else if(state == (int)State.HoldingAtom){

		} 
		else if(state == (int)State.AddingAtom){
			MoveAtomWithMouse();
			//if click, release atom, changing state to default
			if(Input.GetMouseButtonDown(0)){
				state = (int)State.Default;
			}
		}
		
	}
}
