using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Automatic : MonoBehaviour {
	public weapon Weapon;
	public GunManager gun;
	public GameManager gameManager;
	public int automaticammo = 15;
	public GameObject rightController;
	public AudioClip automaticShoot;
	public AudioClip youNeedToReload;
	public float attackDamage = 10;
	private int x = 3;

	public GameObject impactEffect;

	// Use this for initialization
	void Start () {
		rightController.GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void DoTriggerPressed (object sender, ControllerInteractionEventArgs e){
		if (!gun.selectedWeapon) {
			if (gun.assault > 0) {
				RaycastHit hit;
				Debug.Log ("TRIGGERPRESSED");
				gameManager.PlayClip (automaticShoot);

				//while (x > 0) {
					StartCoroutine (Shoot ());
				gun.assault = gun.assault - 5;
					//x--;
				//}
				//x = 3;

				//gun.assault = gun.assault - 3;

				//Shoot raycast
				if (Physics.Raycast (transform.position, transform.forward, out hit, 50) && hit.transform.tag == "Door") {
					Debug.Log ("RAYCAST HIT " + hit.point);
					Debug.Log ("Door!");
				}


			} else {
				Debug.Log ("YOU NEED TO RELOAD");
				gameManager.PlayClip (youNeedToReload);
			}
		}

}
	IEnumerator Shoot() {
		
		Ray ();
		yield return new WaitForSeconds(0.07f);
		Ray ();
		yield return new WaitForSeconds(0.07f);
		Ray ();
		yield return new WaitForSeconds(0.07f);
		Ray ();
		yield return new WaitForSeconds(0.07f);
		Ray ();
		yield return new WaitForSeconds(0.07f);
	}

	void Ray() {
		
		RaycastHit hit;
		if(Physics.Raycast(rightController.transform.position, rightController.transform.forward, out hit, 500)){
			hit.collider.gameObject.GetComponent<Zombie> ().TakeDamage (attackDamage);
			Debug.Log("RAY HIT");
			GameObject impactGO = Instantiate (impactEffect, hit.point, Quaternion.LookRotation (hit.normal));
			Destroy (impactGO, .2f);

		}
	}

}
