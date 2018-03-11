using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Zombie : MonoBehaviour {

	GameObject player;
	public float distractionTime = 3f;

    Animator anim;
    public float health = 100;
    bool dead;
	AICharacterControl charController;

    private void Start()
    {
        anim = GetComponent<Animator>();
		charController = GetComponent<AICharacterControl> ();
		player = GameObject.FindGameObjectWithTag ("Player");

    }

    // Use this for initialization
    void Update()
    {
        if (health <= 0 && !dead) Die();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }
        
    public void Die()
    {
        anim.SetTrigger("Die");
        dead = true;
		//GetComponent<AICharacterControl> ().enabled = false;
		GetComponent<NavMeshAgent>().speed = 0;
    }

	public void ChangeTarget(Transform newTarg){
		charController.target = newTarg;

		Invoke ("ResetTarget", distractionTime);
	}

	void ResetTarget(){
		charController.target = player.transform;
	}
    
}
