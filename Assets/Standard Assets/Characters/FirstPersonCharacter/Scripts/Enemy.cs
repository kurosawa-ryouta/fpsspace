using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

	public int enemyLife = 5;
	Animator anim;
	bool flag = false;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();

	}

	// Update is called once per frame
	void Update () {
		if (enemyLife <= 0 && !flag) {
			StartCoroutine("getUp");
			anim.SetBool ("getup", false);
			flag = true;
		}

	}

	IEnumerator getUp(){
		yield return new WaitForSeconds (10f);
		anim.SetBool ("getup", true);
		enemyLife = 5;
		flag = false;
	}
}
