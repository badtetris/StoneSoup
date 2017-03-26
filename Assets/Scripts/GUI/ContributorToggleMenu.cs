using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContributorToggleMenu : MonoBehaviour {

	public GameObject contributorTogglePrefab;

	public int toggleYDistance = -12;
	public int toggleXDistance = 64;
	public int maxTogglesPerColumn = 8;

	void Start () {
		// Make a toggle for everyone in our contributor list.

		int currentX = 0;
		int currentY = 0;
		for (int i = 0; i < ContributorList.instance.allContributorIDs.Length; i++) {
			string id = ContributorList.instance.allContributorIDs[i];

			GameObject toggleObj = Instantiate(contributorTogglePrefab);
			(toggleObj.transform as RectTransform).SetParent(transform, false);
			int toggleX = currentX*toggleXDistance;
			int toggleY = currentY*toggleYDistance;
			(toggleObj.transform as RectTransform).anchoredPosition = new Vector2(toggleX, toggleY);
			currentY++;
			if (currentY >= maxTogglesPerColumn) {
				currentY = 0;
				currentX++;
			}

			bool toggleIsOn = ContributorList.instance.idIsActive(id);
			toggleObj.GetComponent<Toggle>().isOn = toggleIsOn;
			toggleObj.GetComponent<ContributorToggle>().contributorID = id;
		}
	}
}
