using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeLogic : MonoBehaviour
{
	public GameObject player;

	public GameObject curHex;
	public GameObject prevHex;
	public GameObject leftHex;
	public GameObject rightHex;

	public GameObject dummyHex;

	public GameObject goalHex;
	public GameObject[] startHexes;

	public GameObject[] col0;
	public GameObject[] col1;
	public GameObject[] col2;
	public GameObject[] col3;
	public GameObject[] col4;
	public GameObject[] col5;
	public GameObject[] col6;

	public GameObject[,] maze;

	public Camera playerCam;
	public Camera grayCam;
	public Camera skyCam;

	public Canvas playerCanvas;
	public Canvas grayCanvas;
	public Canvas mapCanvas;

	public Text plus;

	public MapLogic map;
	public HUDLogic hud;
	public LogWriter writer;
	public SimpleMovement move;

	public int[,] betas;

	public int moveCounter = 0;
	public int goalDir = 0;
	public int lastFacing;

	public int leftFacing;
	public int rightFacing;

	public int run;
	public int trial = 0;

	public int lastChoice;

	public bool lastCorrect = true;
	public bool forceChoice = false;
	public bool rightCorrect;

	public bool endTrial = false;
	public bool gray = true;

	public float goalDirExact;
	public float alpha;
	public float endTime;

	// Config variables
	public float startDelay;
	public float endDelay = 2f;
	public float grayTime = 2f;
	public float warnTime;
	public float choiceTime;
	public float timeLimit;

	private string betaStr;

	private float choiceStartTime;
	private float timerStart = 0f;

	private int mode;
	private int beta;

	private bool waiting = true;
	private bool choosing = false;
	private bool warning = false;
	private bool rightNextInc = false;
	private bool first = true;

	// Awake is called when the script instance is being loaded
	void Awake()
	{
		maze = new GameObject [7,7];
		for(int i = 0; i < 7; ++i)
			maze[0,i] = col0[i];
		for(int i = 0; i < 7; ++i)
			maze[1,i] = col1[i];
		for(int i = 0; i < 7; ++i)
			maze[2,i] = col2[i];
		for(int i = 0; i < 7; ++i)
			maze[3,i] = col3[i];
		for(int i = 0; i < 7; ++i)
			maze[4,i] = col4[i];
		for(int i = 0; i < 7; ++i)
			maze[5,i] = col5[i];
		for(int i = 0; i < 7; ++i)
			maze[6,i] = col6[i];
	}

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if(mode == 2 || mode == 3)
		{
			if(gray)
			{
				map.DrawMap();
				gray = false;
			}
			else if(Input.GetKeyDown("space"))
				map.DrawMap();
			return;
		}
		if(waiting)
		{
			if(Input.GetKey("5"))
			{
				waiting = false;
				StartRun();
			}
		}
		else if(Time.time - timerStart >= timeLimit)
		{
			Debug.Log("Out of time, quitting.");
			writer.WriteTimeout();
			Application.Quit();
		}
		else if(gray && Time.time - endTime > endDelay + grayTime)
		{
			if(trial < startHexes.Length)
			{
				StartTrial();
				if(timerStart == 0f)
					timerStart = Time.time;
			}
			else
			{
				Application.Quit();
			}
		}
		else if(mode == 1)
		{
			if(choosing && Time.time - choiceStartTime >= choiceTime)
			{
				AutoChoice();
			}
			else if(choosing && !warning && Time.time - choiceStartTime
				>= warnTime)
			{
				hud.SetWarn();
				warning = true;
				writer.WriteWarning();
			}
			else if(endTrial && Time.time - endTime > endDelay)
			{
				StartGray();
			}
		}
	}

	void StartRun()
	{
		endTime = Time.time;
		writer.SetStart(true);
		writer.StartWriting();
	}

	public void SetChoices(int column, int row)
	{
		//Debug.Log("Setting");
		
		forceChoice = false;

		// Calculate goalDir
		Vector3 toGoal = goalHex.transform.position -
			curHex.transform.position;
		toGoal.y = 0;
		goalDirExact = Vector3.SignedAngle(Vector3.forward, toGoal,
			Vector3.up);

		goalDir = Mathf.RoundToInt(goalDirExact / 60);
		bool zeroCase = false;
		if(Mathf.Approximately(goalDirExact / 60, goalDir))
		{
			Debug.Log(".0 case");
			zeroCase = true;
		}

		int originalMC = moveCounter;
		betaStr = "";
StartLoop:
		if(lastCorrect)
		{
			// Find choice angles
			// Currently the wrong choice is on the right if beta > 0,
			// and on the left if beta < 0
			bool holdBeta = false;

			if(betas[trial, moveCounter] > 0)
			{
				Debug.Log("Correct, going right");
				// TODO: Adjust for float rounding errors? Done?
				if(!zeroCase)
					goalDir = Mathf.CeilToInt(goalDirExact / 60);
				goalDir = (goalDir + 6) % 6;
				leftFacing = goalDir;
				rightFacing = leftFacing + betas[trial, moveCounter];

				// TODO: Fix cases near edge of maze
				rightCorrect = false;

				RightClip();

				// Fix angles if wrong choice is off edge
				// Assumes correct choice is on a hex
				if(!CheckEdge(rightFacing) || rightFacing == lastFacing + 3
					|| rightFacing == lastFacing - 3)
				{
					Debug.Log("Switching to left");
					//holdBeta = true;

					/*
					rightFacing = leftFacing + 1;
					RightClip();

					if(!CheckEdge(rightFacing))
					{
						rightCorrect = true;

						rightFacing = leftFacing;
						leftFacing = rightFacing - betas[trial, moveCounter];
						RightClip();
						LeftClip();

						if(!CheckEdge(leftFacing))
						{
							leftFacing = rightFacing - 1;
							LeftClip();
						}
					}
					*/

					rightCorrect = true;

					if(!zeroCase)
						goalDir = Mathf.FloorToInt(goalDirExact / 60);
					goalDir = (goalDir + 6) % 6;
					rightFacing = goalDir;
					leftFacing = rightFacing - betas[trial, moveCounter];
					beta = -betas[trial, moveCounter];

					LeftClip();

					if(!CheckEdge(leftFacing) || leftFacing == lastFacing + 3
						|| leftFacing == lastFacing - 3)
					{
						Debug.Log("Forcing");
						holdBeta = true;
						forceChoice = true;

						if(betaStr.Length > 0)
							betaStr += "_";
						betaStr += "±" + Math.Abs(beta * 60).ToString();
					}
					else
					{
						if(betaStr.Length > 0)
							betaStr += "_";
						betaStr += (beta * -60).ToString() + "_"
							+ (beta * 60).ToString();
					}
				}
				else
				{
					beta = betas[trial, moveCounter];
					if(betaStr.Length > 0)
						betaStr += "_";
					betaStr += (beta * 60).ToString();
				}

				/*
				if(rightFacing == lastFacing + 3
					|| rightFacing == lastFacing - 3)
					forceChoice = true;
				*/

				//alpha = CalcAlpha((float) leftFacing * 60, goalDirExact);
			}
			else if(betas[trial, moveCounter] < 0)
			{
				Debug.Log("Correct, going left");
				if(!zeroCase)
					goalDir = Mathf.FloorToInt(goalDirExact / 60);
				goalDir = (goalDir + 6) % 6;
				rightFacing = goalDir;
				leftFacing = rightFacing + betas[trial, moveCounter];

				// TODO: Fix cases near edge of maze
				rightCorrect = true;

				LeftClip();

				// Fix angles if wrong choice is off edge
				// Assumes correct choice is on a hex
				if(!CheckEdge(leftFacing) || leftFacing == lastFacing + 3
					|| leftFacing == lastFacing - 3)
				{
					Debug.Log("Switching to right");
					//holdBeta = true;

					/*
					leftFacing = rightFacing - 1;
					LeftClip();

					if(!CheckEdge(leftFacing))
					{
						rightCorrect = false;

						leftFacing = rightFacing;
						rightFacing = leftFacing - betas[trial, moveCounter];
						LeftClip();
						RightClip();

						if(!CheckEdge(rightFacing))
						{
							rightFacing = leftFacing + 1;
							RightClip();
						}
					}
					*/

					rightCorrect = false;

					if(!zeroCase)
						goalDir = Mathf.CeilToInt(goalDirExact / 60);
					goalDir = (goalDir + 6) % 6;
					leftFacing = goalDir;
					rightFacing = leftFacing - betas[trial, moveCounter];
					beta = -betas[trial, moveCounter];

					RightClip();

					if(!CheckEdge(rightFacing) || rightFacing == lastFacing + 3
						|| rightFacing == lastFacing - 3)
					{
						Debug.Log("Forcing");
						holdBeta = true;
						forceChoice = true;

						if(betaStr.Length > 0)
							betaStr += "_";
						betaStr += "±" + Math.Abs(beta * 60).ToString();
					}
					else
					{
						if(betaStr.Length > 0)
							betaStr += "_";
						betaStr += (beta * -60).ToString() + "_"
							+ (beta * 60).ToString();
					}
				}
				else
				{
					beta = betas[trial, moveCounter];
					if(betaStr.Length > 0)
						betaStr += "_";
					betaStr += (beta * 60).ToString();
				}

				/*
				if(leftFacing == lastFacing + 3
					|| leftFacing == lastFacing - 3)
					forceChoice = true;
				*/

				//alpha = CalcAlpha((float) rightFacing * 60, goalDirExact);
			}
			else
			{
				Debug.LogError("invalid beta: " + betas[trial, moveCounter]);
			}

			if(!holdBeta)
			{
				++moveCounter;
				if(moveCounter >= betas.GetLength(1))
					moveCounter = 0;
			}
		}
		else // Last incorrect
		{
			// 180 degree special case
			if(Mathf.Approximately(goalDirExact, (float) (lastFacing + 3) * 60)
				|| Mathf.Approximately(goalDirExact, (float) (lastFacing - 3)
				* 60))
			{
				Debug.Log("180 degree case");

				/*
				if(lastChoice == 1)
				{
					rightFacing = goalDir + 1;
					RightClip();
					forceChoice = true;
					rightCorrect = true;
				}
				else if(lastChoice == 2)
				{
					leftFacing = goalDir - 1;
					LeftClip();
					forceChoice = true;
					rightCorrect = false;
				}
				*/

				if(rightNextInc)
				{
					leftFacing = goalDir + 1;
					rightFacing = goalDir + 2;
					LeftClip();
					RightClip();
					rightCorrect = false;
					beta = 1;
				}
				else
				{
					rightFacing = goalDir - 1;
					leftFacing = goalDir - 2;
					RightClip();
					LeftClip();
					rightCorrect = true;
					beta = -1;
				}
				rightNextInc = !rightNextInc;
			}
			else if(rightNextInc)
			{
				Debug.Log("Right following incorrect");
				if(!zeroCase)
					goalDir = Mathf.CeilToInt(goalDirExact / 60);
				goalDir = (goalDir + 6) % 6;
				leftFacing = goalDir;
				rightFacing = leftFacing + 1;

				rightCorrect = false;

				RightClip();

				if(!CheckEdge(rightFacing) || rightFacing == lastFacing + 3
					|| rightFacing == lastFacing - 3 || leftFacing
					== lastFacing + 3 || leftFacing == lastFacing - 3)
				{
					Debug.Log("Switching to left");
					rightNextInc = !rightNextInc;

					if(!zeroCase)
						goalDir = Mathf.FloorToInt(goalDirExact / 60);
					goalDir = (goalDir + 6) % 6;
					rightFacing = goalDir;
					leftFacing = rightFacing - 1;
					beta = -1;

					rightCorrect = true;

					LeftClip();

					if(!CheckEdge(leftFacing) || leftFacing == lastFacing + 3
						|| leftFacing == lastFacing - 3 || rightFacing
						== lastFacing + 3 || rightFacing == lastFacing - 3)
					{
						if(betaStr.Length > 0)
							betaStr += "_";
						betaStr += "±60";
						forceChoice = true;
					}
					else
					{
						if(betaStr.Length > 0)
							betaStr += "_";
						betaStr += "60";
					}
				}
				else
					beta = 1;
				rightNextInc = !rightNextInc;
			}
			else // Go left
			{
				Debug.Log("Left following incorrect");
				if(!zeroCase)
					goalDir = Mathf.FloorToInt(goalDirExact / 60);
				goalDir = (goalDir + 6) % 6;
				rightFacing = goalDir;
				leftFacing = rightFacing - 1;

				rightCorrect = true;

				LeftClip();

				if(!CheckEdge(leftFacing) || leftFacing == lastFacing + 3
					|| leftFacing == lastFacing - 3 || rightFacing
					== lastFacing + 3 || rightFacing == lastFacing - 3)
				{
					Debug.Log("Switching to right");
					rightNextInc = !rightNextInc;

					if(!zeroCase)
						goalDir = Mathf.CeilToInt(goalDirExact / 60);
					goalDir = (goalDir + 6) % 6;
					leftFacing = goalDir;
					rightFacing = leftFacing + 1;
					beta = 1;

					rightCorrect = false;

					RightClip();

					if(!CheckEdge(rightFacing) || rightFacing == lastFacing + 3
						|| rightFacing == lastFacing - 3 || leftFacing
						== lastFacing + 3 || leftFacing == lastFacing - 3)
					{
						if(betaStr.Length > 0)
							betaStr += "_";
						betaStr += "±60";
						forceChoice = true;
					}
					else
					{
						if(betaStr.Length > 0)
							betaStr += "_";
						betaStr += "-60";
					}
				}
				else
					beta = -1;
				rightNextInc = !rightNextInc;
			}
			/*
			if(Mathf.Approximately(((float) lastFacing * 60 + 180) % 360, 
				goalDirExact))
			{
				forceChoice = true;
				if(!rightCorrect)
				{
					leftFacing = goalDir - 1;
					LeftClip();
				}
				else
				{
					rightFacing = goalDir + 1;
					RightClip();
				}
			}
			else
			*/
			/*
			{
				leftFacing = Mathf.FloorToInt(goalDirExact / 60f);
				rightFacing = leftFacing + 1; // Biased right?
				LeftClip();
				RightClip();
				rightCorrect = goalDirExact % 60f > 30f; // Biased?

				if(rightCorrect)
				{
					if(rightFacing == lastFacing + 3
						|| rightFacing == lastFacing - 3)
					{
						--rightFacing;
						--leftFacing;
						RightClip();
						LeftClip();
					}
					if(leftFacing == lastFacing + 3
						|| leftFacing == lastFacing - 3)
					{
						++rightFacing;
						++leftFacing;
						RightClip();
						LeftClip();
						rightCorrect = false;
					}
				}
				else
				{
					if(leftFacing == lastFacing + 3
						|| leftFacing == lastFacing - 3)
					{
						++leftFacing;
						++rightFacing;
						LeftClip();
						RightClip();
					}
					if(rightFacing == lastFacing + 3
						|| rightFacing == lastFacing - 3)
					{
						--leftFacing;
						--rightFacing;
						LeftClip();
						RightClip();
						rightCorrect = true;
					}
				}
				
				if(rightCorrect)
				{
					if(!CheckEdge(leftFacing))
						forceChoice = true;
				}
				else
				{
					if(!CheckEdge(rightFacing))
						forceChoice = false;
				}
			}
			*/
			/*
			// TODO: Fix cases near edge of maze
			// If direction of goal doesn't lead into previous hex
			if(goalDir + 3 != lastFacing && goalDir -3 != lastFacing)
			{
				if(rightCorrect)
				{
					rightFacing = goalDir;
					leftFacing = goalDir - 1;
					rightCorrect = true;

					alpha = CalcAlpha((float) rightFacing * 60, goalDirExact);
				}
				else
				{
					leftFacing = goalDir;
					rightFacing = goalDir + 1;
					rightCorrect = false;

					alpha = CalcAlpha((float) leftFacing * 60, goalDirExact);
				}
			}
			// If direction of goal leads into previous hex
			else
			{
				// TODO: Fix cases where alpha ~= 0
				if(alpha > 0)
				{
					rightFacing = goalDir - 1;
					leftFacing = goalDir - 2;
					rightCorrect = true;

					alpha = CalcAlpha((float) rightFacing * 60, goalDirExact);
				}
				else
				{
					leftFacing = goalDir + 1;
					rightFacing = goalDir + 2;
					rightCorrect = false;

					alpha = CalcAlpha((float) leftFacing * 60, goalDirExact);
				}
			}
			*/

			if(betaStr.Length > 0)
				betaStr += "_";
			betaStr += (beta * 60).ToString();
		}

		if(!rightCorrect)
			alpha = CalcAlpha((float) leftFacing * 60, goalDirExact);
		else
			alpha = CalcAlpha((float) rightFacing * 60, goalDirExact);

		// Assure valid facings
		LeftClip();
		RightClip();

		if(forceChoice)
		{
			Debug.Log("Force choice");
			if(rightCorrect)
				leftFacing = -1;
			else
				rightFacing = -1;
			//beta = 0;
		}

		// Find choice hexes
		switch(leftFacing)
		{
			case -1:
				// For forced choice
				leftHex = dummyHex;
				break;
			case 0:
			case 6:
				leftHex = maze[column, row + 1];
				break;
			case 1:
				leftHex = maze[column + 1, row + 1];
				break;
			case 2:
				leftHex = maze[column + 1, row];
				break;
			case 3:
				//Debug.Log("Three");
				leftHex = maze[column, row - 1];
				break;
			case 4:
				leftHex = maze[column - 1, row - 1];
				break;
			case 5:
				leftHex = maze[column - 1, row];
				break;
			default:
				Debug.LogError("Invalid facing: " + leftFacing);
				break;
		}
		switch(rightFacing)
		{
			case -1:
				// For forced choice
				rightHex = dummyHex;
				break;
			case 0:
			case 6:
				rightHex = maze[column, row + 1];
				break;
			case 1:
				rightHex = maze[column + 1, row + 1];
				break;
			case 2:
				rightHex = maze[column + 1, row];
				break;
			case 3:
				rightHex = maze[column, row - 1];
				break;
			case 4:
				rightHex = maze[column - 1, row - 1];
				break;
			case 5:
				rightHex = maze[column - 1, row];
				break;
			default:
				Debug.LogError("Invalid facing: " + rightFacing);
				break;
		}

		if(forceChoice)
		{
			// If adjacent to goal
			if(rightCorrect && rightHex == goalHex || !rightCorrect && leftHex
				== goalHex)
			{
				++moveCounter;
				if(moveCounter > betas.GetLength(1))
					moveCounter = 0;
				if(moveCounter == originalMC)
				{
					Debug.LogError("Impossible goal choice");
					Application.Quit();
				}
				Debug.Log("Looping");
				forceChoice = false;
				goto StartLoop;
			}
			else if(moveCounter == originalMC)
			{
				++moveCounter;
				if(moveCounter > betas.GetLength(1))
					moveCounter = 0;
				Debug.Log("Looping");
				forceChoice = false;
				goto StartLoop;
			}
			else
			{
				betaStr += "_0";
			}
		}
		//forceChoice = false;

		// Highlight
		leftHex.BroadcastMessage("SetLeft");
		rightHex.BroadcastMessage("SetRight");

		/*
		if(forceChoice)
		{
			AutoChoice();
			forceChoice = false;
		}
		*/

		Debug.Log("BetaComp: " + beta + "—" + betaStr);
	}

	public void UpdateHexes()
	{
		//Debug.Log("Updating");

		// Remove previous highlighting
		prevHex.BroadcastMessage("Reset");
		leftHex.BroadcastMessage("Reset");
		rightHex.BroadcastMessage("Reset");

		if(curHex.GetInstanceID() == goalHex.GetInstanceID())
		{
			endTime = Time.time;
			endTrial = true;
			move.SetCanMove(false);
			writer.WriteGoal();
			curHex.BroadcastMessage("SetGoal");
			hud.SetGoal();
			hud.AddGem();
		}
		else
		{
			SetChoices(curHex.GetComponent<HexLogic>().column,
				curHex.GetComponent<HexLogic>().row);
			curHex.BroadcastMessage("SetGray");

			if(first)
			{
				first = false;
			}
			else
			{
				writer.WriteChoiceStart(betaStr);
				//forceChoice = false;
				choosing = true;
				choiceStartTime = Time.time;
			}
		}
		//Debug.Log(betaStr);
	}

	public void Hit(GameObject hitObj)
	{
		// Check if new hex
		if(hitObj.GetInstanceID() != curHex.GetInstanceID())
		{
			//Debug.Log("switching");

			// Switch to new hex
			prevHex = curHex;
			curHex = hitObj;
		}
	}

	public bool CheckEdge(int facing)
	{
		return curHex.GetComponent<HexLogic>().edges[facing];
	}

	public void SetMode(int m)
	{
		mode = m;
		if(m == 2 || m == 3)
		{
			skyCam.enabled = true;
			mapCanvas.enabled = true;
			player.SetActive(false);
			plus.enabled = false;
			map.enabled = true;
			writer.enabled = false;
			BroadcastMessage("SetExplore");
			//map.DrawMap();
			//this.enabled = false;
		}
	}

	// Call if player chooses left
	public void LeftChoice(bool auto)
	{
		if(warning)
		{
			hud.ClearWarn();
			warning = false;
		}
		if(rightCorrect)
		{
			alpha += 60 * beta;
			if(alpha > 180)
				alpha -= 360;
			else if(alpha < -180)
				alpha += 360;
		}
		lastChoice = 1;
		lastCorrect = !rightCorrect;

		if(forceChoice)
			writer.WriteForcedAction();
		else if(auto)
			writer.WriteAutoAction();
		else
			writer.WriteAction();

		choosing = false;
	}

	// Call if player chooses right
	public void RightChoice(bool auto)
	{
		if(warning)
		{
			hud.ClearWarn();
			warning = false;
		}
		if(!rightCorrect)
		{
			alpha += 60 * beta;
			if(alpha > 180)
				alpha -= 360;
			else if(alpha < -180)
				alpha += 360;
		}
		lastChoice = 2;
		lastCorrect = rightCorrect;

		if(forceChoice)
			writer.WriteForcedAction();
		else if(auto)
			writer.WriteAutoAction();
		else
			writer.WriteAction();

		choosing = false;
	}

	public float GetDistToGoal()
	{
		Vector3 dist = goalHex.transform.position
			- move.GetPosition();
		dist.y = 0f;
		return dist.magnitude;
	}

	public float GetAngleToGoal()
	{
		if(curHex == goalHex)
			return 0f;
		Vector3 toGoal = goalHex.transform.position
			- move.GetPosition();
		toGoal.y = 0f;
		float bearing = Vector3.SignedAngle(Vector3.forward, toGoal,
			Vector3.up);
		float angle = bearing - move.GetFacing();
		if(angle < -180f)
			angle += 360f;
		return angle;
	}

	public float GetHeadingToCenter()
	{
		if(curHex == maze[3,3])
			return 0f;
		Vector3 toGoal = maze[3,3].transform.position - move.GetPosition();
		toGoal.y = 0f;
		float bearing = Vector3.SignedAngle(Vector3.forward, toGoal,
			Vector3.up);
		return bearing;
	}

	// Make private?
	public void StartGray()
	{
		endTrial = false;
		gray = true;
		writer.SetGray(true);
		writer.WriteGrayScreen();
		++trial;
		moveCounter = 0;
		playerCam.enabled = false;
		playerCanvas.enabled = false;
		grayCam.enabled = true;
		grayCanvas.enabled = true;
		move.SetCanMove(false);
		move.SetCanTurn(false);
	}

	public void StartTrial()
	{
		lastFacing = -6;

		if(trial % 2 == 1)
			rightNextInc = false;
		else
			rightNextInc = true;

		first = true;
		gray = false;
		writer.SetGray(false);
		writer.SetStart(false);
		playerCam.enabled = true;
		playerCanvas.enabled = true;
		grayCam.enabled = false;
		grayCanvas.enabled = false;
		GameObject hex = startHexes[trial];
		curHex = hex;
		move.StartTrial(hex.transform.position.x,
			hex.transform.position.z);
		choosing = true;
		choiceStartTime = Time.time;
		move.SetCanMove(true);
		move.SetCanTurn(true);

		if(mode == 1)
		{
			UpdateHexes();
			writer.WriteTrialStart(betaStr);
		}
		else
		{
			BroadcastMessage("SetExplore");
			writer.WriteTrialStart();
		}

		hud.ClearGoal();
		move.SetFacing(GetHeadingToCenter());
		move.SnapRot();
	}

	private float CalcAlpha(float heading, float goal)
	{
		float ret = heading - goal;
		if(ret > 180f)
			ret -= 360f;
		if(ret < -180f)
			ret += 360f;
		return ret;
	}

	private void AutoChoice()
	{
		if(rightCorrect)
			move.RightChoice(true);
		else
			move.LeftChoice(true);
	}

	private void LeftClip()
	{
		leftFacing = (leftFacing + 6) % 6;
	}

	private void RightClip()
	{
		rightFacing = (rightFacing + 6) % 6;
	}

}
