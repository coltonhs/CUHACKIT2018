using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class weapon : MonoBehaviour {
	public GameManager gameManager;
	public GunManager gun;

	public GameObject rightController;
	public AudioClip shotgunShoot;
	public AudioClip automaticShoot;
	public AudioClip youNeedToReload;
	public int shotgunammo = 5;
	public float attackDamage = 50;
	public GameObject impactEffect1;
	public GameObject impactEffect2;


	// Use this for initialization
	void Start () {
		rightController.GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
	}
	
	// Update is called once per frame
	void Update () {


		RayCast ();

				}



	public void RayCast() {
		Vector3 forward = transform.TransformDirection (Vector3.forward) * 10;
		Debug.DrawRay (transform.position, forward, Color.blue);




		}

//Shooting
	void DoTriggerPressed (object sender, ControllerInteractionEventArgs e){
		//shotgun
		if(gun.selectedWeapon == true) {
			if(gun.shot > 0) {
		RaycastHit hit;
		Debug.Log ("TRIGGERPRESSED");
				gun.shot--;
		gameManager.PlayClip (shotgunShoot);
				StartCoroutine (Shoot ());
		//Shoot raycast
		if (Physics.Raycast (transform.position, transform.forward, out hit, 50) && hit.transform.tag == "Door") {
			Debug.Log ("RAYCAST HIT " + hit.point);
			Debug.Log ("Door!");
		}
	

	}
		else{
			Debug.Log ("YOU NEED TO RELOAD");
			gameManager.PlayClip (youNeedToReload);
		}
}
		//automatic
		else if(!gun.selectedWeapon)  {
			//gameManager.PlayClip (automaticShoot);
		}
}
	IEnumerator Shoot() {

		Ray ();
		yield return new WaitForSeconds(0.1f);
	}

	void Ray() {
		//gun.assault--;
		RaycastHit hit;
		if(Physics.Raycast(rightController.transform.position, rightController.transform.forward, out hit, 500)){
			Debug.Log("RAY HIT");
			hit.collider.gameObject.GetComponent<Zombie> ().TakeDamage (attackDamage);
			GameObject impactGO = Instantiate (impactEffect1, hit.point, Quaternion.LookRotation (hit.normal));
			GameObject impactGO2 = Instantiate (impactEffect2, hit.point, Quaternion.LookRotation (hit.normal));
			Destroy (impactGO, .1f);
			Destroy (impactGO2, .2f);

		}
	}
}
