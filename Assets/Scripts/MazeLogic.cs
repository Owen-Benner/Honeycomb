using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeLogic : MonoBehaviour
{

	public GameObject leftRing;
	public int leftFacing;
	public GameObject rightRing;
	public int rightFacing;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

	void Enter(GameObject collision)
	{
		Debug.Log("Enter");
		collision.GetComponent<SimpleMovement>().
			SetChoices(leftFacing, rightFacing);
		leftRing.GetComponent<Highlighter>().SetLeft();
		rightRing.GetComponent<Highlighter>().SetRight();
	}

	void Exit(GameObject collision)
	{
		leftRing.GetComponent<Highlighter>().Reset();
		rightRing.GetComponent<Highlighter>().Reset();
	}

}
