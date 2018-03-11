using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon : MonoBehaviour {

	public int ammo = 5;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.S)) {
			Shoot();
			Debug.Log("SHOOT");

		}

		if(Input.GetKeyDown(KeyCode.R)) {
				Reload();
				Debug.Log("RELOAD");
			}
			
	}

	void Shoot() {
		if (ammo > 0) {
						ammo = ammo - 1;
		}
	}

	void Reload() {
		if (ammo < 5) {
			ammo = 5;
		}
	}
}
