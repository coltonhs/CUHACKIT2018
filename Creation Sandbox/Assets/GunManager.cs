using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour {

	public GameObject shotgun;
	public GameObject automatic;


	public int shot = 5;
	public int assault = 15;


	public bool selectedWeapon = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (selectedWeapon) {
			shotgun.SetActive(true);
			automatic.SetActive(false);
		} else {
			shotgun.SetActive(false);
			automatic.SetActive(true);
		}
	}

		void Shotgun() {

	}
		void Automatic() {

	}
}
