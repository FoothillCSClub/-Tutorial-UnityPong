﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public static GameManager instance = null;

	public Text leftText, rightText;
	public GameObject leftPaddle, rightPaddle;
	public MenuFade menuFade;
	public GameObject ps;
	private GameObject ips;

	public int maxScore = 7;
	public bool changePaddleSize = true;
	private int lScore, rScore;
	private string logoMessage = "PONG";
	private string winMessage = " side wins!";
	private string instructionMessage = "Press SPACE to Start";

	private bool gameOver, gameStart;

	SoundManager sm;
	public AudioClip deathClip;
	public AudioClip winClip;

	private Vector3 originalPaddleScale;

	void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		sm = SoundManager.instance;
		if (!gameStart){
			DisplayMessage(logoMessage, instructionMessage);
			FreezePaddle(true);
		}
		originalPaddleScale = leftPaddle.transform.localScale;
	}

	// Update is called once per frame
	void Update () {
		if (!gameStart && Input.GetKeyDown(KeyCode.Space)){
			gameStart = true;
			menuFade.ResetDefaults();
			menuFade.gameObject.SetActive(false);
			ResetGame();
		}
		if (gameOver && Input.GetKeyDown(KeyCode.Space)){
			gameOver = false;
			menuFade.ResetDefaults();
			menuFade.gameObject.SetActive(false);
			ResetGame();
		}
		if (ips) {
			if(!ips.GetComponent<ParticleSystem>().IsAlive())
	         {
	             Destroy(ips);
	         }
	    }
	}

	public void GoalScored(Collision col){
		sm.PlayRand(deathClip);
		if (col.transform.position.x > 0){
			ips = (GameObject) Instantiate(ps, col.transform.position, Quaternion.Euler(Vector3.up));
			lScore++;
			leftText.text = lScore.ToString();
			ShrinkPaddle(leftPaddle);
			if (lScore >= maxScore){
				Debug.Log("Left win!");
				DisplayMessage("Left" + winMessage, instructionMessage);
				StartCoroutine(col.gameObject.GetComponent<BounceBall>().ResetBall(Vector3.zero));
				sm.PlayRand(winClip);
				return;
			}
			StartCoroutine(col.gameObject.GetComponent<BounceBall>().ResetBall(Vector3.left));
		}
		else {
			ips = (GameObject) Instantiate(ps, col.transform.position, Quaternion.Euler(Vector3.up));
			rScore++;
			rightText.text = rScore.ToString();
			ShrinkPaddle(rightPaddle);
			if (rScore >= maxScore){
				Debug.Log("Right win!"); 
				DisplayMessage("Right" + winMessage, instructionMessage);
				StartCoroutine(col.gameObject.GetComponent<BounceBall>().ResetBall(Vector3.zero));
				sm.PlayRand(winClip);
				return;
			}
			StartCoroutine(col.gameObject.GetComponent<BounceBall>().ResetBall(Vector3.right));
		}
	}

	private void DisplayMessage(string winner, string message){
		gameOver = true;
		menuFade.gameObject.SetActive(true);
		StartCoroutine(menuFade.Blur(winner, message));
	}

	private void ResetGame(){
		lScore = rScore = 0;
		leftText.text = rightText.text = 0.ToString();
		leftPaddle.transform.localScale = rightPaddle.transform.localScale = originalPaddleScale;
		FreezePaddle(false);
		if (UnityEngine.Random.insideUnitCircle.x < 0)
			StartCoroutine(FindObjectOfType<BounceBall>().ResetBall(Vector3.left));
		else
			StartCoroutine(FindObjectOfType<BounceBall>().ResetBall(Vector3.right));
	}

	private void FreezePaddle(bool freeze){
		leftPaddle.GetComponent<MovePaddle>().haltInput = freeze;
		rightPaddle.GetComponent<MovePaddle>().haltInput = freeze;
	}

	private void ShrinkPaddle(GameObject paddle){
		Vector3 localScale = paddle.transform.localScale;
		float yScale = localScale.y;
		yScale -= yScale/(maxScore -1);
		paddle.transform.localScale =
			new Vector3(localScale.x, yScale, localScale.z);
	}
}
