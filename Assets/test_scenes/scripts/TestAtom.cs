using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestAtom : MonoBehaviour {
	public GameObject[] hAtoms; //h atoms to bond 

	public static Vector3[] relativePositions;
	public List<Vector3> neighbours;

	public static float sqrt2 = Mathf.Sqrt(2);
	public static float sqrt3 = Mathf.Sqrt(3);
	public float CHBondLength;
	public float CCBondLength = 2f;
	void Awake(){
		CHBondLength = 4f;
		neighbours = new List<Vector3>();
		relativePositions = new Vector3[4];
		//for center to vertex distance = 1
		Vector3 pos0 = new Vector3(sqrt2/sqrt3, 0, -1/sqrt3); 
		Vector3 pos1 = new Vector3(-sqrt2/sqrt3,0, -1/sqrt3);
		Vector3 pos2 = new Vector3(0, sqrt2/sqrt3,  1/sqrt3);
		Vector3 pos3 = new Vector3(0,-sqrt2/sqrt3,  1/sqrt3);
		relativePositions[0] = pos0;
		relativePositions[1] = pos1;
		relativePositions[2] = pos2;
		relativePositions[3] = pos3;
	}
	// Use this for initialization
	void Start () {
		
		if(gameObject.name == "CTest 1"){return;}
		for(int i=0; i < hAtoms.Length; i++){
			GameObject g = hAtoms[i];
			g.transform.position = CHBondLength/sqrt3 * relativePositions[i];
			//g.transform.position 
			//	= Quaternion.AngleAxis(Mathf.Acos(-1/3f)*Mathf.Rad2Deg/2f, Vector3.right) * g.transform.position;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(gameObject.name == "CTest 1"){
			//fix C's positions
			GameObject g = hAtoms[0];
			Vector3 vFrom = relativePositions[0];
			Vector3 vTo = (g.transform.position - this.transform.position).normalized;
			//Vector3 v0 = transform.position + vTo * (-CHBondLength/sqrt3) ;//+ Vector3.up * Mathf.Sin(Mathf.Acos(-1/3f));
			Vector3 v0 = Vector3.MoveTowards(transform.position, g.transform.position, -CHBondLength/sqrt3);
			Vector3 v1 = Quaternion.AngleAxis(120f, vTo) * v0;
			//hAtoms[1].transform.position = v0;
			//hAtoms[2].transform.position = v1;    
			
			float angle = Vector3.Angle(vFrom, vTo);
			Debug.Log(angle);
			Vector3 rotDir = Vector3.Cross(vFrom, vTo);
			for(int j=1; j < hAtoms.Length; j++){
				hAtoms[j].transform.position
					= Quaternion.AngleAxis(angle, rotDir) 
					* (CHBondLength/sqrt3 * relativePositions[j]) + this.transform.position;
			}
			
			return;
		}
	}
}
