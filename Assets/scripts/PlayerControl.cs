using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {
	[HideInInspector]public Camera camera;
	public static Vector3 center;
	public float horizontalSpeed = 2.0f;
	public float verticalSpeed = 2.0f; 
	void Awake(){
		center = Vector3.zero;
		camera = GetComponent<Camera>();
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
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButton(0)){
			RotateCameraAroundCenter();
		}
		
		
		
	}
}
