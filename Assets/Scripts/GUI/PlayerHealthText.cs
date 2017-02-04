using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthText : MonoBehaviour {

	protected Text _text;

	// Use this for initialization
	void Start () {
		_text = GetComponent<Text>();
	}
		
	// Update is called once per frame
	void Update () {
		if (Player.instance != null) {
			_text.text = string.Format("Health: {0}", Player.instance.health);
		}
	}
}
