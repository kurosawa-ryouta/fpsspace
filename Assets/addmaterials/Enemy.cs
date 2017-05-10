using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

	public int enemyLife = 5;
	Animator anim;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();

	}

	// Update is called once per frame
	void Update () {
		if (enemyLife <= 0) {
			getUp ();
		}

	}

	IEnumerator getUp(){
		yield return new WaitForSeconds (10f);
		anim.SetBool ("getup", false);
		enemyLife = 5;
	}
}
