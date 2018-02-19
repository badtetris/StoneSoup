using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283DoubleItem : Tile {

	public apt283ProxyTile firstProxy, secondProxy;

	public Tile firstItem, secondItem;

	public float secondAimOffset = 90f;

	public override void init() {
		base.init();
		firstItem.init();
		firstItem.mainCollider.enabled = false;
		secondItem.init();
		secondItem.mainCollider.enabled = false;
	}

	public override void pickUp(Tile tilePickingUsUp) {
		base.pickUp(tilePickingUsUp);
		if (_tileHoldingUs == tilePickingUsUp) {
			firstProxy.bodyParent = _tileHoldingUs;
			secondProxy.bodyParent = _tileHoldingUs;


			firstItem.mainCollider.enabled = true;
			secondItem.mainCollider.enabled = true;

			firstItem.pickUp(firstProxy);
			secondItem.pickUp(secondProxy);

			mainCollider.enabled = false;
		}
	}

	public override void dropped(Tile tileDroppingUs) {
		base.dropped(tileDroppingUs);

		// WATCH OUT: If one of the items can't be properly dropped, we shouldn't drop either of them. 
		if (_tileHoldingUs == null) {
			firstItem.dropped(firstProxy);
			secondItem.dropped(secondProxy);

			firstItem.mainCollider.enabled = false;
			secondItem.mainCollider.enabled = false;

			firstItem.transform.parent = transform;
			secondItem.transform.parent = transform;

			mainCollider.enabled = true;
		}
	}

	void Update() {
		if (_tileHoldingUs != null) {
			firstProxy.aimDirection = _tileHoldingUs.aimDirection;
			float aimAngle = Mathf.Atan2(_tileHoldingUs.aimDirection.y, _tileHoldingUs.aimDirection.x)*Mathf.Rad2Deg;
			aimAngle += secondAimOffset;
			Vector2 secondAimDirection = new Vector2(Mathf.Cos(aimAngle*Mathf.Deg2Rad), Mathf.Sin(aimAngle*Mathf.Deg2Rad));
			secondProxy.aimDirection = secondAimDirection;
		}
	}

	public override void useAsItem(Tile tileUsingUs) {
		firstItem.useAsItem(firstProxy);
	}

	
}
