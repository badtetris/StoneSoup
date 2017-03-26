using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContributorList : MonoBehaviour {


	public string[] allContributorIDs;

	protected List<string> _activeContributorIDs = new List<string>();
	public string[] activeContributorIDs {
		get { return _activeContributorIDs.ToArray(); }
	}
	public bool idIsActive(string id) {
		return _activeContributorIDs.Contains(id);
	}

	protected static ContributorList _instance = null;
	public static ContributorList instance {
		get { return _instance; }
	}

	void Awake() {
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
		// Initially, we only have the first id in our active list.
		_activeContributorIDs.Clear();
		_activeContributorIDs.Add(allContributorIDs[0]);
	}

	public void activateContributorID(string id) {
		if (!_activeContributorIDs.Contains(id)) {
			_activeContributorIDs.Add(id);
		}
	}

	public void deactivateContributorID(string id) {
		if (_activeContributorIDs.Contains(id)) {
			_activeContributorIDs.Remove(id);
		}
	}

}
