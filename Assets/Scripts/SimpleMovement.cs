using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{

	private CharacterController cc;

	public float moveSpeed;
	//public float rotSpeed;
	public float sinkSpeed;

	private float hold;

	private bool forward;

	private bool moving;
	private int facing; // 0 for +Z, 3 for -Z
	private float destX;
	private float destZ;
	private float cooldown;

	public float hexRadius;
	public float rotateInterval;
	public float rotateCooldown = 0;

	public int mode; // 0 for explore, 1 for maze

	public KeyCode left;
	private int leftFacing;
	public KeyCode right;
	private int rightFacing;

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

	// Start is called before the first frame update
	void Start()
	{
		cc = GetComponent<CharacterController>();
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

		UpdateHexes();
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 move = Vector3.zero;
		Vector3 rotate = Vector3.zero;

		// Check movement input
		if(mode == 0 && !moving)
		{
			if(Input.GetAxis("Vertical") > 0.1f)
			{
				// Find movement angle
				// TODO: Fix facing of 6?
				facing = Mathf.RoundToInt(cc.transform.eulerAngles.y / 60);
				facing = (facing + 6) % 6;

				if(curHex.GetComponent<HexLogic>().edges[facing])
				{
					StartMove();
				}
			}
		}
		else if(mode == 1 && !moving)
		{
			if(Input.GetKey(left) && !Input.GetKey(right))
			{
				facing = leftFacing;
				StartMove();
			}
			else if(Input.GetKey(right) && !Input.GetKey(left))
			{
				facing = rightFacing;
				StartMove();
			}
		}

		/*
		// Calculate movement if free to move
		if(hold <= 0f)
		{
			float vert = Input.GetAxis("Vertical");
			if(forward){vert = 1f;}
			if(vert > 0f) // Only move forward
				move += Time.deltaTime * vert * moveSpeed *
					cc.transform.forward;

			if(!forward)
				rotate += Time.deltaTime * Input.GetAxis("Horizontal") *
					rotSpeed * Vector3.up;
		}
		else
		{
			hold -= Time.deltaTime;
		}
		*/

		// Check rotation input
		if(cooldown > 0)
			cooldown -= Time.deltaTime;

		if(Input.GetAxis("Horizontal") > 0.1f && cooldown <= 0)
		{
			rotate += rotateInterval * Vector3.up;
			cooldown = rotateCooldown;
		}
		else if(Input.GetAxis("Horizontal") < -0.1f && cooldown <= 0)
		{
			rotate -= rotateInterval * Vector3.up;
			cooldown = rotateCooldown;
		}

		// Determine if at destination
		if(moving)
		{
			float angle = (float) facing * 60;
			move += Time.deltaTime * moveSpeed * (float) Math.Sin(angle *
				Mathf.Deg2Rad) * Vector3.right;
			move += Time.deltaTime * moveSpeed * (float) Math.Cos(angle *
				Mathf.Deg2Rad) * Vector3.forward;

			if(facing == 5 || facing == 6 || facing == 0 || facing == 1)
			{
				if(cc.transform.position.z > destZ)
				{
					SnapPos();
					moving = false;
					if(mode == 1)
						UpdateHexes();
				}
			}
			else if(facing == 2 || facing == 3 || facing == 4)
			{
				if(cc.transform.position.z < destZ)
				{
					SnapPos();
					moving = false;
					if(mode == 1)
						UpdateHexes();
				}
			}
			else { Debug.LogError("Invalid facing: " + facing); }
		}

		// Calculate lower to ground
		if(!cc.isGrounded)
			move += Time.deltaTime * sinkSpeed * Vector3.down;

		// Apply character translation
		cc.Move(move);

		// Apply character rotation
		cc.transform.Rotate(rotate);
	}

	public void BeginHold(float length)
	{
		hold = length;
	}

	public void EndHold()
	{
		hold = 0;
	}

	public bool IsHolding()
	{
		if(hold <= 0)
		{
			return false;
		}
		return true;
	}

	public void SetForward(bool on)
	{
		forward = on;
	}

	// Select hexagon
	public void StartMove()
	{
		moving = true;

		// Find final x coord
		switch(facing)
		{
			case 0:
			case 6:
				destZ = cc.transform.position.z + hexRadius;
				break;
			case 1:
			case 5:
				destZ = cc.transform.position.z + hexRadius / 2;
				break;
			case 2:
			case 4:
				destZ = cc.transform.position.z - hexRadius / 2;
				break;
			case 3:
				destZ = cc.transform.position.z - hexRadius;
				break;
			default:
				Debug.LogError("Invalid facing: " + facing);
				break;
		}

		// Find final z coord
		destX = cc.transform.position.x + hexRadius * (float) Math.Sin((float)
			facing * 60 * Mathf.Deg2Rad);
	}

	public void SnapPos()
	{
		Vector3 pos = new Vector3(destX, cc.transform.position.y, destZ);
		cc.transform.position = pos;
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
				Debug.LogError("Invalid facing: " + facing);
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
				Debug.LogError("Invalid facing: " + facing);
				break;
		}

		// Highlight
		leftHex.BroadcastMessage("SetLeft");
		rightHex.BroadcastMessage("SetRight");
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//Debug.Log("hit");
		if(hit.gameObject.CompareTag("HexCenter"))
		{
			//Debug.Log("center");
			GameObject hitObj = hit.transform.parent.gameObject;

			// Check if new hex
			if(hitObj.GetInstanceID() != curHex.GetInstanceID())
			{
				// Switch to new hex
				prevHex = curHex;
				curHex = hitObj;
			}
		}
	}

	private void UpdateHexes()
	{
		//Debug.Log("Updating");

		// Remove previous highlighting
		prevHex.BroadcastMessage("Reset");
		leftHex.BroadcastMessage("Reset");
		rightHex.BroadcastMessage("Reset");

		if(curHex.GetInstanceID() == goalHex.GetInstanceID() && mode == 1)
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

}
