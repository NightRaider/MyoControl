using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public GameObject PlayerSphere;

    private Vector3 offset;

 
	// Use this for initialization
	void Start () {
        offset = transform.position - PlayerSphere.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = PlayerSphere.transform.position + offset;
	}
}
