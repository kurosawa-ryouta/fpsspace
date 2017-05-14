
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class snipe : MonoBehaviour {

	public Image image;
	bool flag = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(1))
		{
			if (!flag) {
				image.gameObject.SetActive (true);
				flag = true;
			} else {
				image.gameObject.SetActive (false);
				flag = false;
			}
		}
	}
}
