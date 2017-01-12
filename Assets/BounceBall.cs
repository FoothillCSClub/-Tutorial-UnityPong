﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class BounceBall : MonoBehaviour {

	public bool debugBounce;
	public bool useOriginalBounce = true;
	public float speedMult = 3f;
	public float randAngle = 10f;
	public float diffMult = 45f;
	public Vector3 direction, cardinal;

	Vector3 curPos, lastPos;
	float timeToRespawn = 1f;

	SoundManager sm;
	public AudioClip bounceClip;

	void Start () {
		sm = SoundManager.instance;
		//for testing we can start the ball off in a random direction
		if (debugBounce){
			direction = Vector3.left;
		}
		lastPos = Vector3.zero;
	}

	void Update () {
		curPos = this.transform.position;
		if (curPos.x < lastPos.x)
			cardinal = Vector3.left;
		else cardinal = Vector3.right;
		lastPos = curPos;

		Vector3 translation = direction.normalized * speedMult * Time.deltaTime;
		this.transform.Translate(translation, Space.World);
	}

	//As long as one of the objects has a RigidBody attached we can get collision information
	//OnCollisionEnter fires when the object's hitboxes have entered each other (only fires once)
	void OnCollisionEnter(Collision col){
		//we can get lots of information from a collision object like what the other objects name is
		//here we check the tag of the other game object to control our logic
		if (col.gameObject.tag == "Wall"){
			//contacts[] is an array of contact points given by the collision object
			//we can use the first one that triggered OnCollisionEnter to grab our normal for reflection
			direction = ReflectBall(direction, col.contacts[0].normal);
			direction = (direction*0.9f) + (cardinal*0.1f);
		}

		if (col.gameObject.tag == "Paddle"){
			//this is my custom bounce algorithm that takes into account the paddle's velocity
			if (!useOriginalBounce){
				direction = ReflectBall(direction, col.contacts[0].normal);
				MovePaddle mp = col.gameObject.GetComponent<MovePaddle>();
				Vector3 paddleVelocity = mp.getUnitVector();
				direction = (direction*0.8f) + (paddleVelocity*0.2f);
			} 
			//the original pong method of paddle bounce which takes the location of the hit on the paddle
			else {
				float hitAxis = col.collider.transform.position.y;
				float middleAxis = col.gameObject.transform.position.y;
				float diff = Mathf.Clamp01(Mathf.Abs(hitAxis) - middleAxis);
				if (hitAxis < middleAxis) diff *= -1;
				Debug.Log("Diff: " + diff + " | Angle: " + diff * diffMult);
				direction = Quaternion.Euler(0, 0, diff * diffMult) * col.contacts[0].normal;
			}
		}

		sm.PlayRand(bounceClip);
	}

	Vector3 ReflectBall(Vector3 dir, Vector3 normal){
		//Vector3 refelction is given by the formula: −(2(n · v) n − v)
		//However, we can use the built in Reflect() function to do this.
		dir = Vector3.Reflect(direction, normal);

		//We want to give our ball a little variety in movement
		//so, lets multiply our direction with a random z angle in the current direction
		dir = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-randAngle, randAngle)) * dir;
		return dir;
	}

	public IEnumerator ResetBall(Vector3 dir){
		direction = cardinal = curPos = lastPos = Vector3.zero;
		this.transform.position = Vector3.forward * 5f;
		yield return new WaitForSeconds(timeToRespawn);
		this.transform.position = Vector3.zero;
		yield return new WaitForSeconds(timeToRespawn);
		SetDir(dir);
	}

	public void SetDir(Vector3 dir){
		direction = dir;
	}

	public Vector3 GetDir(){
		return GetDir();
	}
}
