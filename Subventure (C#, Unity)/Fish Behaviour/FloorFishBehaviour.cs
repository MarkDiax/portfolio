using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorFishBehaviour : AbstractFishBehaviour
{

	public override void Update() {
		switch (isTurningBack) {

			case true:
			direction = newGoal - transform.position;
			Quaternion temp = Quaternion.Slerp(transform.rotation,
												  Quaternion.LookRotation(direction),
												  rotationSpeed * Time.deltaTime);
			transform.rotation = new Quaternion(0, temp.y, 0, temp.w);
			break;
			case false:
			//make sure the fish don't move the same way all at once
			if (Random.Range(0, 5000) < 800) {
				Vector3 centreVector = Vector3.zero, avoidVector = Vector3.zero;
				Vector3 goalPosition = fishTracker.GetWaypointFromChannel(GroupingChannel);

				Vector3 goalCentre = Vector3.zero;

				float distance;

				int localGroupSize = 0;

				for (int i = 0; i < fishObjects.Count; i++) {
					distance = Vector3.Distance(fishObjects[i].transform.position, transform.position);
					//if the distance is really small, avoid the neighbouring fish
					if (distance < evadeDistance) {
						Vector3 delta = transform.position - fishObjects[i].transform.position;
						avoidVector = avoidVector + new Vector3(delta.x, delta.y, delta.z);       //without rotation along y axis
					}
					if (distance <= groupingDistance && fishObjects[i].GroupingChannel == this.GroupingChannel) {
						centreVector += fishObjects[i].transform.position;
						localGroupSize++;
					}
				}

				if (localGroupSize > 0) {
					Vector3 delta = goalPosition - transform.position;
					centreVector = centreVector / localGroupSize + new Vector3(delta.x, delta.y, delta.z);     //calculate the average centre
																											   //without rotation along y axis
																											   //update the direction with the centre of the group and to avoid any fish
					direction = (centreVector + avoidVector) - transform.position;
					Quaternion tempQ = Quaternion.Slerp(transform.rotation,
														  Quaternion.LookRotation(direction),
														  rotationSpeed * Time.deltaTime);
					transform.rotation = new Quaternion(0, tempQ.y, 0, tempQ.w);
				}
			}
			break;
		}

		transform.Translate(0, 0, movementSpeed * Time.deltaTime);
	}
}

