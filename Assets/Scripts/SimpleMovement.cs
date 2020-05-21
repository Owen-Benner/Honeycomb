using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{

	private CharacterController cc;

	public float moveSpeed;
	public float rotSpeed;
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

	public GameObject prevHex;
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

	public int moveCounter;

	public int column;
	public int row;

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
				if(prevHex.GetComponent<HexLogic>().edges[facing % 6])
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
				}
			}
			else if(facing == 2 || facing == 3 || facing == 4)
			{
				if(cc.transform.position.z < destZ)
				{
					SnapPos();
					moving = false;
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

	public void SetChoices()
	{
		leftFacing = alphas[moveCounter];
		rightFacing = leftFacing + betas[moveCounter];
		if(rightFacing < leftFacing)
		{
			int temp = leftFacing;
			leftFacing = rightFacing;
			rightFacing = temp;
		}
		leftFacing %= 6;
		rightFacing %= 6;

		switch(leftFacing)
		{
			case 0:
			case 6:
				maze[column, row + 1].BroadcastMessage("SetLeft");
				break;
			case 1:
				maze[column + 1, row + 1].BroadcastMessage("SetLeft");
				break;
			case 2:
				maze[column + 1, row].BroadcastMessage("SetLeft");
				break;
			case 3:
				maze[column, row - 1].BroadcastMessage("SetLeft");
				break;
			case 4:
				maze[column - 1, row - 1].BroadcastMessage("SetLeft");
				break;
			case 5:
				maze[column - 1, row].BroadcastMessage("SetLeft");
				break;
			default:
				Debug.LogError("Invalid facing: " + facing);
				break;
		}
		switch(rightFacing)
		{
			case 0:
			case 6:
				maze[column, row + 1].BroadcastMessage("SetRight");
				break;
			case 1:
				maze[column + 1, row + 1].BroadcastMessage("SetRight");
				break;
			case 2:
				maze[column + 1, row].BroadcastMessage("SetRight");
				break;
			case 3:
				maze[column, row - 1].BroadcastMessage("SetRight");
				break;
			case 4:
				maze[column - 1, row - 1].BroadcastMessage("SetRight");
				break;
			case 5:
				maze[column - 1, row].BroadcastMessage("SetRight");
				break;
			default:
				Debug.LogError("Invalid facing: " + facing);
				break;
		}
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//Debug.Log("hit");
		if(hit.gameObject.CompareTag("HexCenter"))
		{
			//Debug.Log("center");
			GameObject hitObj = hit.transform.parent.parent.gameObject;
			if(hitObj.GetInstanceID() != prevHex.GetInstanceID())
			{
				prevHex = hitObj;
				if(hitObj.GetInstanceID() == goalHex.GetInstanceID() &&
					mode == 1)
				{
					hitObj.BroadcastMessage("SetGoal");
				}
				else
				{
					SetChoices();
				}
				//hit.gameObject.SendMessage("Enter", gameObject);
				//prevHex.SendMessage("Exit", gameObject);
			}
		}
	}

}
