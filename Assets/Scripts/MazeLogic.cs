using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeLogic : MonoBehaviour
{

	public GameObject curHex;
	public GameObject prevHex;
	public GameObject leftHex;
	public GameObject rightHex;

	public GameObject [] goalHexes;
	public GameObject [] startHexes;

	public GameObject [] col0;
	public GameObject [] col1;
	public GameObject [] col2;
	public GameObject [] col3;
	public GameObject [] col4;
	public GameObject [] col5;
	public GameObject [] col6;

	public GameObject [,] maze;

	public Camera playerCam;
	public Camera grayCam;
	public Canvas playerCanvas;
	public Canvas grayCanvas;

	public HUDLogic hud;
	public LogWriter writer;
	public SimpleMovement move;

	// New array for each trial?
	public int [] betas;

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

	private int mode;

	private bool waiting = true;

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
		else
		{
			if(endTrial && Time.time - endTime > 2f)
			{
				StartGray();
				endTrial = false;
			}
			if(gray && Time.time - endTime > 4f)
			{
				StartTrial();
			}
		}
    }

	void StartRun()
	{
		endTime = Time.time;
		writer.StartWriting();
	}

	public void SetChoices(int column, int row)
	{
		//Debug.Log("Setting");

		// Calculate goalDir
		Vector3 toGoal = goalHexes[trial].transform.position -
			curHex.transform.position;
		toGoal.y = 0;
		goalDirExact = Vector3.SignedAngle(Vector3.forward, toGoal,
			Vector3.up);
		goalDir = Mathf.RoundToInt(goalDirExact / 60);
		goalDir = (goalDir + 6) % 6;

		if(lastCorrect)
		{
			// TODO: Refactor alpha calculation
			// Find choice angles
			// Currently the wrong choice is on the right if beta > 0,
			// and on the left if beta < 0
			if(betas[moveCounter] > 0)
			{
				leftFacing = goalDir;
				rightFacing = leftFacing + betas[moveCounter];

				// TODO: Fix cases near edge of maze
				rightCorrect = false;

				leftFacing = (leftFacing + 6) % 6;
				rightFacing = (rightFacing + 6) % 6;

				// Fix angles if wrong choice is off edge
				// Assumes correct choice is on a hex
				if(betas[moveCounter] == 3)
				{
					forceChoice = CheckEdge(rightFacing);
				}
				else if(betas[moveCounter] == 1 || betas[moveCounter] == 2)
				{
					while(!CheckEdge(rightFacing))
					{
						--leftFacing;
						--rightFacing;
					}
				}
				else
				{
					Debug.LogError("invalid facing: " + betas[moveCounter]);
				}

				//TODO: Refactor
				alpha = (float) leftFacing * 60 - goalDirExact;
				if(alpha < -180f)
					alpha += 360f;
			}
			else if(betas[moveCounter] < 0)
			{
				rightFacing = goalDir;
				leftFacing = rightFacing + betas[moveCounter];

				// TODO: Fix cases near edge of maze
				rightCorrect = true;

				leftFacing = (leftFacing + 6) % 6;
				rightFacing = (rightFacing + 6) % 6;

				if(betas[moveCounter] == -3)
				{
					forceChoice = CheckEdge(rightFacing);
				}
				else if(betas[moveCounter] == -1 || betas[moveCounter] == -2)
				{
					while(!CheckEdge(rightFacing))
					{
						++leftFacing;
						++rightFacing;
					}
				}
				else
				{
					Debug.LogError("invalid facing: " + betas[moveCounter]);
				}

				alpha = (float) rightFacing * 60 - goalDirExact;
				if(alpha < -180f)
					alpha += 360f;
			}
			else
			{
				Debug.LogError("invalid facing: " + betas[moveCounter]);
			}

			++moveCounter;
		}
		else
		{
			// TODO: Fix cases near edge of maze

			// If direction of goal doesn't lead into previous hex.
			if(goalDir + 3 != lastFacing && goalDir -3 != lastFacing)
			{
				if(rightCorrect)
				{
					rightFacing = goalDir;
					leftFacing = goalDir - 1;
					rightCorrect = true;
					alpha = (float) rightFacing * 60 - goalDirExact;
					if(alpha < -180f)
						alpha += 360f;
				}
				else
				{
					leftFacing = goalDir;
					rightFacing = goalDir + 1;
					rightCorrect = false;
					alpha = (float) leftFacing * 60 - goalDirExact;
					if(alpha < -180f)
						alpha += 360f;
				}
			}
			// If direction of goal leads into previous hex.
			else
			{
				// TODO: Fix cases where alpha ~= 0
				if(alpha > 0)
				{
					rightFacing = goalDir - 1;
					leftFacing = goalDir - 2;
					rightCorrect = true;
					alpha = (float) rightFacing * 60 - goalDirExact;
					if(alpha < -180f)
						alpha += 360f;
				}
				else
				{
					leftFacing = goalDir + 1;
					rightFacing = goalDir + 2;
					rightCorrect = false;
					alpha = (float) leftFacing * 60 - goalDirExact;
					if(alpha < -180f)
						alpha += 360f;
				}
			}
		}

		leftFacing = (leftFacing + 6) % 6;
		rightFacing = (rightFacing + 6) % 6;

		// Find choice hexes
		switch(leftFacing)
		{
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
	}

	public void UpdateHexes()
	{
		//Debug.Log("Updating");

		// Remove previous highlighting
		prevHex.BroadcastMessage("Reset");
		leftHex.BroadcastMessage("Reset");
		rightHex.BroadcastMessage("Reset");

		if(curHex.GetInstanceID() == goalHexes[trial].GetInstanceID())
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
			if(moveCounter > 0)
				writer.WriteChoiceStart();
		
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
		lastChoice = 1;
		lastCorrect = !rightCorrect;
		writer.WriteAction();
	}

	// Call if player chooses right
	public void RightChoice()
	{
		lastChoice = 2;
		lastCorrect = rightCorrect;
		writer.WriteAction();
	}

	public float GetDistToGoal()
	{
		Vector3 dist = goalHexes[trial].transform.position
			- move.GetPosition();
		dist.y = 0;
		return dist.magnitude;
	}

	public float GetAngleToGoal()
	{
		Vector3 toGoal = goalHexes[trial].transform.position
			- move.GetPosition();
		toGoal.y = 0;
		float bearing = Vector3.SignedAngle(Vector3.forward, toGoal,
			Vector3.up);
		float angle = bearing - move.GetFacing();
		if(angle < -180f)
			angle += 360f;
		return angle;
	}

	// Make private?
	public void StartGray()
	{
		gray = true;
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
		gray = false;
		playerCam.enabled = true;
		playerCanvas.enabled = true;
		grayCam.enabled = false;
		grayCanvas.enabled = false;
		GameObject hex = startHexes[trial];
		curHex = hex;
		move.StartTrial(hex.transform.position.x,
			hex.transform.position.z);
		writer.WriteTrialStart();
		move.SetCanMove(true);
		move.SetCanTurn(true);
		if(mode == 1)
			UpdateHexes();
		hud.ClearGoal();
	}

}
