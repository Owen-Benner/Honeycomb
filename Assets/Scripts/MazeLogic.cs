using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeLogic : MonoBehaviour
{

	public GameObject curHex;
	public GameObject prevHex;
	public GameObject leftHex;
	public GameObject rightHex;
	public GameObject goalHex;

	public GameObject [] col0;
	public GameObject [] col1;
	public GameObject [] col2;
	public GameObject [] col3;
	public GameObject [] col4;
	public GameObject [] col5;
	public GameObject [] col6;

	public GameObject [,] maze;

	public int [] alphas;
	public int [] betas;

	public int moveCounter = 0;
	public int goalDir = 0;

	public int leftFacing;
	public int rightFacing;

	private int mode;

    // Start is called before the first frame update
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetChoices(int column, int row)
	{
		// Calculate goalDir
		Vector3 toGoal = goalHex.transform.position -
			curHex.transform.position;
		toGoal.y = 0;
		goalDir = Mathf.RoundToInt(Vector3.SignedAngle(Vector3.forward,
			toGoal, Vector3.up) / 60);
		goalDir = (goalDir + 6) % 6;

		// Find choice angles
		// Currently the wrong choice is on the right if beta > 0, and on the
		// left if beta < 0
		if(betas[moveCounter] > 0)
		{
			leftFacing = goalDir + alphas[moveCounter];
			rightFacing = leftFacing + betas[moveCounter];
		}
		else if(betas[moveCounter] < 0)
		{
			rightFacing = goalDir + alphas[moveCounter];
			leftFacing = rightFacing + betas[moveCounter];
		}
		else
		{
			Debug.LogError("beta cannot be zero!");
		}
		++moveCounter;
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

		if(curHex.GetInstanceID() == goalHex.GetInstanceID())
		{
			curHex.BroadcastMessage("SetGoal");
		}
		else
		{
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
			Debug.Log("switching");

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

}
