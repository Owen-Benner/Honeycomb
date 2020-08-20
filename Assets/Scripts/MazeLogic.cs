using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeLogic : MonoBehaviour
{

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
	public Canvas playerCanvas;
	public Canvas grayCanvas;

	public HUDLogic hud;
	public LogWriter writer;
	public SimpleMovement move;

	public int[,] betas;

	public int moveCounter = 0;
	public int goalDir = 0;
	public int lastFacing;

	public int leftFacing;
	public int rightFacing;

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

	private float choiceStartTime;
	private float timerStart = 0f;

	private int mode;

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
				ForceChoice();
			}
			else if(choosing && !warning && Time.time - choiceStartTime
				>= warnTime)
			{
				hud.SetWarn();
				warning = true;
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

		// Calculate goalDir
		Vector3 toGoal = goalHex.transform.position -
			curHex.transform.position;
		toGoal.y = 0;
		goalDirExact = Vector3.SignedAngle(Vector3.forward, toGoal,
			Vector3.up);

		if(lastCorrect)
		{
			// Find choice angles
			// Currently the wrong choice is on the right if beta > 0,
			// and on the left if beta < 0
			bool holdBeta = false;

			if(betas[trial, moveCounter] > 0)
			{
				// TODO: Adjust for float rounding errors?
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

					goalDir = Mathf.FloorToInt(goalDirExact / 60);
					goalDir = (goalDir + 6) % 6;
					rightFacing = goalDir;
					leftFacing = rightFacing - betas[trial, moveCounter];

					LeftClip();

					if(!CheckEdge(leftFacing) || leftFacing == lastFacing + 3
						|| leftFacing == lastFacing - 3)
					{
						holdBeta = true;
						forceChoice = true;
					}
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
				goalDir = Mathf.FloorToInt(goalDirExact / 60);
				goalDir = (goalDir + 6) % 6;
				rightFacing = goalDir;
				leftFacing = rightFacing + betas[trial, moveCounter];

				// TODO: Fix cases near edge of maze
				rightCorrect = true;

				LeftClip();

				//roro Fix angles if wrong choice is off edge
				// Assumes correct choice is on a hex
				if(!CheckEdge(leftFacing) || leftFacing == lastFacing + 3
					|| leftFacing == lastFacing - 3)
				{
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

					goalDir = Mathf.CeilToInt(goalDirExact / 60);
					goalDir = (goalDir + 6) % 6;
					leftFacing = goalDir;
					rightFacing = leftFacing - betas[trial, moveCounter];

					RightClip();

					if(!CheckEdge(rightFacing) || rightFacing == lastFacing + 3
						|| rightFacing == lastFacing - 3)
					{
						holdBeta = true;
						forceChoice = true;
					}
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
			}
			else if(rightNextInc)
			{
				goalDir = Mathf.CeilToInt(goalDirExact / 60);
				goalDir = (goalDir + 6) % 6;
				leftFacing = goalDir;
				rightFacing = leftFacing + 1;

				rightCorrect = false;

				RightClip();

				if(!CheckEdge(rightFacing) || rightFacing == lastFacing + 3
					|| rightFacing == lastFacing - 3)
				{
					rightNextInc = !rightNextInc;

					goalDir = Mathf.FloorToInt(goalDirExact / 60);
					goalDir = (goalDir + 6) % 6;
					rightFacing = goalDir;
					leftFacing = rightFacing - 1;

					rightCorrect = true;

					LeftClip();

					if(!CheckEdge(leftFacing) || leftFacing == lastFacing + 3
						|| leftFacing == lastFacing - 3)
					{
						forceChoice = true;
					}
				}
			}
			else // Go left
			{
				goalDir = Mathf.FloorToInt(goalDirExact / 60);
				goalDir = (goalDir + 6) % 6;
				rightFacing = goalDir;
				leftFacing = rightFacing - 1;

				rightCorrect = true;

				LeftClip();

				if(!CheckEdge(leftFacing) || leftFacing == lastFacing + 3
					|| leftFacing == lastFacing - 3)
				{
					rightNextInc = !rightNextInc;

					goalDir = Mathf.CeilToInt(goalDirExact / 60);
					goalDir = (goalDir + 6) % 6;
					leftFacing = goalDir;
					rightFacing = leftFacing + 1;

					rightCorrect = false;

					RightClip();

					if(!CheckEdge(rightFacing) || rightFacing == lastFacing + 3
						|| rightFacing == lastFacing - 3)
					{
						forceChoice = true;
					}
				}
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
			if(rightCorrect)
				leftFacing = -1;
			else
				rightFacing = -1;
			forceChoice = false;
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

		// Highlight
		leftHex.BroadcastMessage("SetLeft");
		rightHex.BroadcastMessage("SetRight");

		/*
		if(forceChoice)
		{
			ForceChoice();
			forceChoice = false;
		}
		*/
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
			if(first)
			{
				first = false;
			}
			else
			{
				writer.WriteChoiceStart();
				choosing = true;
				choiceStartTime = Time.time;
			}
		
			SetChoices(curHex.GetComponent<HexLogic>().column,
				curHex.GetComponent<HexLogic>().row);
			curHex.BroadcastMessage("SetGray");
		}
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
	}

	// Call if player chooses left
	public void LeftChoice()
	{
		if(warning)
		{
			hud.ClearWarn();
			warning = false;
		}
		lastChoice = 1;
		lastCorrect = !rightCorrect;
		writer.WriteAction();
		choosing = false;
	}

	// Call if player chooses right
	public void RightChoice()
	{
		if(warning)
		{
			hud.ClearWarn();
			warning = false;
		}
		lastChoice = 2;
		lastCorrect = rightCorrect;
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
		if(trial % 2 == 1)
			rightNextInc = true;
		else
			rightNextInc = false;
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
		writer.WriteTrialStart();
		choosing = true;
		choiceStartTime = Time.time;
		move.SetCanMove(true);
		move.SetCanTurn(true);
		if(mode == 1)
			UpdateHexes();
		else
			BroadcastMessage("SetExplore");
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

	private void ForceChoice()
	{
		if(rightCorrect)
			move.RightChoice();
		else
			move.LeftChoice();
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
