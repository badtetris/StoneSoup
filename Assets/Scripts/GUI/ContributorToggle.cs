using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContributorToggle : MonoBehaviour {



	public Text labelText;
	public string contributorID;

	protected Toggle _toggle;


	void Start() {
		// First, find the room object in the resources folder.
		//string roomFilename = string.Format("{0}/room", contributorID);
		//Room room = Resources.Load<GameObject>(roomFilename).GetComponent<Room>();
		labelText.text = contributorID;
		_toggle = GetComponent<Toggle>();
	}

	void Update() {
		if (ContributorList.instance.activeContributorIDs.Length <= 1
			&& _toggle.isOn) {
			_toggle.enabled = false;
		}
		else {
			_toggle.enabled = true;
		}
	}

	public void onToggle(bool toggleActive) {
		if (toggleActive) {
			ContributorList.instance.activateContributorID(contributorID);
		}
		else {
			ContributorList.instance.deactivateContributorID(contributorID);
		}
	}
}
