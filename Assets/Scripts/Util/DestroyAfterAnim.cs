using UnityEngine;
using System.Collections;

// Useful script for an object you want to destroy when it's done with an animation.
public class DestroyAfterAnim : MonoBehaviour {

	// Done gets called by an animation event. 
	public void done() {
		Destroy(gameObject);
	}
}
