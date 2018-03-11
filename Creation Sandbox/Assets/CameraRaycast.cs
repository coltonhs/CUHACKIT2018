using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRaycast : MonoBehaviour {



	//Public Variables
	private float speed = 0f;
	public float maxSpeed;
	public float acceleration;
	public float deceleration;

	public float range = 500f;
	public bool isGrappling = false;

	//Private Variables
	private RaycastHit hit;
	private bool hasRayCasted = false;

	//GameObjects
	public GameObject grapple;
	public GameObject playerCollider;
	private GameObject grappledObject;
	public GameObject player;


	public float minGrappleDistance;

	public float rotateSpeed;
	public bool enableControls = true;

	private bool isMovingPlatform = false;




	private float heightY;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			enableControls = true;
		}

		if (Input.GetButtonDown("Fire1")) {
			RaycastFromCamera ();
		}

		if (isGrappling) {
			MoveGrappleToPosition ();
			if (MoveGrappleToPosition ()) {
				grappledObject = hit.collider.gameObject;
				if (isMovingPlatform == true) {
					Debug.Log ("Moving Platform");
				
	

					//GetHeight ();
					heightY = grappledObject.transform.position.y + (grappledObject.transform.lossyScale.y) / 2;

					Debug.Log ("height is " + heightY);

					//grapple.transform.parent = grappledObject.transform;
					player.transform.parent = grappledObject.transform;

				}

				MovePlayerToGrapple ();
				if (MovePlayerToGrapple ()) {
					//EndGrapple ();
				}
				//EndGrapple ();
			}
		}
	}

	#region Raycast
	void RaycastFromCamera () {

		int layerMask = 1 << 8;
		if (Physics.Raycast (this.transform.position, this.transform.forward, out hit, range, layerMask)) {
			
			hit.point.Normalize ();

			if (hit.transform.tag == "MovingPlatform") {
				isMovingPlatform = true;
			} else {
				Debug.Log ("FALSE");

				isMovingPlatform = false;
				grapple.transform.parent = null;
				player.transform.parent = null;
			}

			//Verify grapple distance
			if (CheckDistance ()) {
				isGrappling = true;
			}
		}
	}
	#endregion

	#region Grapple

	//function used to compare player's current grappled position to desired grappled position
	bool CheckDistance () {
		
		float distance = Vector3.Distance (grapple.transform.position, hit.point);
		Debug.Log ("The distance is" + distance);


		if (distance < minGrappleDistance) {
			isGrappling = false;
			return false;
		} else {
			return true;
		}
	}

	//function used to move the grapple towards coordinates from raycast hit
	bool MoveGrappleToPosition () {
		float step = maxSpeed + acceleration * Time.deltaTime;

			if (grapple.transform.position != (hit.point) && hit.point.z != 0) {
				grapple.transform.position = Vector3.MoveTowards (grapple.transform.position, hit.point, step);
			}


		if (grapple.transform.position == (hit.point)) {



			if (isMovingPlatform) {
				grapple.transform.parent = hit.collider.gameObject.transform;
			}




			return true;
		} else {
			return false;
		}
	}

	//function used to move player to grappled position
	bool MovePlayerToGrapple () {

		float step = maxSpeed + acceleration * Time.deltaTime;

		if ((playerCollider.transform.position != grapple.transform.position + hit.normal)) {
			playerCollider.transform.position = Vector3.MoveTowards (playerCollider.transform.position, grapple.transform.position + hit.normal, step);
			return false;
		} else {
			
			isGrappling = false;
			return true;
		}
	}
	#region EndGrapple
	void EndGrapple () {

		StartCoroutine (CameraRotate ());
	}

	IEnumerator CameraRotate() {
		enableControls = false;
		Vector3 targetDir = hit.normal - this.transform.position;
		float step = rotateSpeed * Time.deltaTime;
		Vector3 newDir = Vector3.RotateTowards (transform.forward, targetDir, step, 0.0F);
		Debug.DrawRay (transform.position, newDir, Color.red);
		transform.rotation = Quaternion.LookRotation (newDir);



		//this.transform.rotation.y = playerCollider.GetComponent<CameraController> ().rotationY;
		yield return new WaitForSeconds (0.15f);
		//playerCollider.GetComponent<CameraController> ().rotationX = this.transform.eulerAngles.x * (-1);
		//playerCollider.GetComponent<CameraController> ().rotationY = this.transform.eulerAngles.y;

		isGrappling = false;
		enableControls = true;
	}
	#endregion
	#endregion 

	void GetHeight () {


	}

	#region TODO
/*
 * 
 * 
 * 
 * 
 * 
 * 
 */
	#endregion

}
