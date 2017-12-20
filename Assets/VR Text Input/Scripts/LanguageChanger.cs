using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageChanger : MonoBehaviour {

	List<GameObject> languages = new List<GameObject> ();
	int current;

	void Start () {
		foreach (Transform item in transform)
			languages.Add (item.gameObject);
		Change (0);
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.L)) {
			if (current == languages.Count - 1) {
				Change (0);
			} else {
				Change (current + 1);
			}
		}
	}

	void Change (int next) {
		languages [current].SetActive (false);
		languages [next].SetActive (true);
		current = next;
	}
}
