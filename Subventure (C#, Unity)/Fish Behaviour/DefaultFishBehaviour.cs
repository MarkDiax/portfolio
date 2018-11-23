using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DefaultFishBehaviour : AbstractFishBehaviour
{
	public override void Update() {
		base.Update();

		//Checks if the fish is turning back from a collider or not
		switch (isTurningBack) {
			case true:
			//if so, grab a location behind the fish and rotate towards it
			direction = newGoal - transform.position;
			transform.rotation = Quaternion.Slerp(transform.rotation,
												  Quaternion.LookRotation(direction),
												  rotationSpeed * Time.deltaTime);
			break;
			case false:
			//make sure the fish don't move the same way all at once
			if (Random.Range(0, 5000) < 800) {
				Vector3 centreVector = Vector3.zero, avoidVector = Vector3.zero;
				//grab the waypoint that belongs to this fish' grouping channel
				Vector3 goalPosition = fishTracker.GetWaypointFromChannel(GroupingChannel);

				float distance;
				int localGroupSize = 0;

				for (int i = 0; i < fishObjects.Count; i++) {
					distance = Vector3.Distance(fishObjects[i].transform.position, transform.position);
					//if the distance is really small, avoid the neighbouring fish
					if (distance < evadeDistance) {
						avoidVector = avoidVector + (transform.position - fishObjects[i].transform.position);
					}

					//check if another fish is within groupingDistance and is in the same channel
					if (distance <= groupingDistance && fishObjects[i].GroupingChannel == this.GroupingChannel) {
						//if so, then move towards the nearby fish
						centreVector += fishObjects[i].transform.position;
						localGroupSize++;
					}
				}

				if (localGroupSize > 0) {
					//calculate the average centre vector
					centreVector = centreVector / localGroupSize + (goalPosition - transform.position);

					//update the direction with the centre of the group and to avoid any fish
					direction = (centreVector + avoidVector) - transform.position;
					transform.rotation = Quaternion.Slerp(transform.rotation,
														  Quaternion.LookRotation(direction),
														  rotationSpeed * Time.deltaTime);
				}
			}
			break;
		}
	}
}