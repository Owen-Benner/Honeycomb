using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapLogic : MonoBehaviour
{

	public GameObject redArrow;
	public GameObject greenArrow;
	public GameObject blueArrow;
	public GameObject number;

	public MazeLogic maze;
	public SimpleMovement move;

	public Text trialInfo;
	public Text betas;

	public string fileName;

	private int curTrial = 0;
	//private int maxTrial;

    // Start is called before the first frame update
    void Start()
    {
        maze = gameObject.GetComponent<MazeLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void DrawMap()
	{
		StreamReader reader = new StreamReader(fileName);
		Debug.Log("Drawing");

		maze.BroadcastMessage("SetExplore");

		trialInfo.text = fileName;
		trialInfo.enabled = true;

		try
		{
			GameObject hex;
			while(true)
			{
				Debug.Log("Loop 0");
				string line = reader.ReadLine();
				string [] lineArr = line.Split(' ');
				Debug.Log(lineArr[0]);
				if(lineArr[0] == "Trial" && int.Parse(lineArr[1].Trim(':'))
					== curTrial)
				{
					trialInfo.text += "\n" + "Trial " + curTrial;
					string [] coords = lineArr[2].Split('-');
					Debug.Log(int.Parse(coords[0]) + "-"
						+ int.Parse(coords[1]));
					hex = maze.maze[int.Parse(coords[0]),
						int.Parse(coords[1])];
					hex.BroadcastMessage("SetRight");
					betas.enabled = true;
					betas.text = "0: " + lineArr[3] + "\n";
					Debug.Log("Breaking");
					break;
				}
				if(reader.EndOfStream)
				{
					curTrial = 0;
					reader.Close();
					reader = new StreamReader(fileName);
				}
			}
			++curTrial;

			float angle = 0;
			while(true)
			{
				Debug.Log("Loop 1");
				string line = reader.ReadLine();
				string [] lineArr = line.Split(' ');
				Debug.Log(lineArr[0]);
				if(lineArr[0] == "Action" || lineArr[0] == "Auto_Action")
				{
					GameObject arrow;
					float dir = float.Parse(lineArr[4]) + angle;
					if(lineArr[3] == "0")
					{
						Debug.Log("Red");
						arrow = Instantiate(redArrow,
							hex.transform);
					}
					else if(lineArr[0] == "Action")
					{
						Debug.Log("Green");
						arrow = Instantiate(greenArrow,
							hex.transform);
					}
					else
					{
						Debug.Log("Blue");
						arrow = Instantiate(blueArrow,
							hex.transform);
					}
					arrow.transform.position += move.hexRadius / 2
						* Mathf.Sin(dir * Mathf.Deg2Rad) * Vector3.right;
					arrow.transform.position += move.hexRadius / 2
						* Mathf.Cos(dir * Mathf.Deg2Rad) * Vector3.forward;
					arrow.transform.position += 5 * Vector3.up;
					arrow.transform.eulerAngles += Vector3.up * (dir - 60);
					arrow.SetActive(true);

					GameObject num = Instantiate(number, hex.transform);
					string numText = lineArr[1].Split('.')[1].Split(':')[0];
					num.GetComponent<TextMesh>().text = numText;
					num.transform.position += move.hexRadius / 2
						* Mathf.Sin(dir * Mathf.Deg2Rad) * Vector3.right;
					num.transform.position += move.hexRadius / 2
						* Mathf.Cos(dir * Mathf.Deg2Rad) * Vector3.forward;
					num.transform.position += 5 * Vector3.up;
					num.SetActive(true);
				}
				else if(lineArr[0] == "Start_Choice")
				{
					string [] coords = lineArr[2].Split('-');
					hex = maze.maze[int.Parse(coords[0]),
						int.Parse(coords[1])];
					betas.text = betas.text + lineArr[1].Split('.')[1]
						+ " " + lineArr[3] + "\n";
				}
				// TODO: Make this better.
				else if(lineArr[0] == "Frame")
				{
					angle = float.Parse(lineArr[4]) + float.Parse(lineArr[8]);
				}
				else if(lineArr[0] == "Goal")
				{
					string [] coords = lineArr[2].Split('-');
					hex = maze.maze[int.Parse(coords[0]),
						int.Parse(coords[1])];
					hex.BroadcastMessage("SetGoal");
				}
				else if(lineArr[0] == "Gray_Screen")
					break;
			}
		}
		catch(Exception e)
		{
			Debug.LogError("Error generating map!");
			Debug.LogError(e.ToString());
		}
	}

}
