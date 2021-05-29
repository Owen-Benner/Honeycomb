using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReader : MonoBehaviour
{

	public SimpleMovement move;
	public MazeLogic maze;
	public LogWriter writer;
	public MapLogic map;
	//public EyeDots eyeDots;

	private int mode;
	private int numTrials;

	private string partStr;
	private string runStr;
	private string startDelayStr;
	private string endDelayStr;
	private string grayTimeStr;
	private string warnTimeStr;
	private string choiceTimeStr;
	private string timeLimitStr;

	private string travelTimeStr;
	private string rotRateStr;
	private string fovStr;

	private string[] betasArr;
	private string[][] betasArrArr;

	private string startHexStr;
	private string[] startHexArr;

	private string goalHexStr;

	// Awake is called when the script instance is being loaded
	void Awake()
	{
		try
		{
			StreamReader reader = new StreamReader("Config.txt");

			partStr = reader.ReadLine();
			runStr = reader.ReadLine();
			mode = int.Parse(reader.ReadLine());
			startDelayStr = reader.ReadLine();
			endDelayStr = reader.ReadLine();
			grayTimeStr = reader.ReadLine();
			warnTimeStr = reader.ReadLine();
			choiceTimeStr = reader.ReadLine();
			timeLimitStr = reader.ReadLine();

			travelTimeStr = reader.ReadLine();
			rotRateStr = reader.ReadLine();
			fovStr = reader.ReadLine();

			numTrials = int.Parse(reader.ReadLine());
			betasArr = new string[numTrials];
			for(int i = 0; i < betasArr.Length; ++i)
			{
				betasArr[i] = reader.ReadLine();
			}

			startHexStr = reader.ReadLine();
			
			goalHexStr = reader.ReadLine();

			reader.Close();
		}
		catch(Exception e)
		{
			Debug.LogError("Error reading file!");
			Debug.LogError(e);
			Application.Quit();
		}
	}

	void Start()
	{
		if(mode == 0)
		{
			writer.fileName = partStr + "_explore_" + runStr + ".xml";
		}
		else if(mode == 1)
		{
			writer.fileName = partStr + "_maze_" + runStr + ".xml";
		}
		else if(mode == 2)
		{
			map.fileName = partStr + "_explore_" + runStr + ".xml";
		}
		else if(mode == 3)
		{
			map.fileName = partStr + "_maze_" + runStr + ".xml";
		}

		move.mode = mode;

		maze.SetMode(mode);
		maze.run = int.Parse(runStr);

		if(mode == 2 || mode == 3)
			return;

		//logReader.mode = mode;
		//logReader.partCode = partStr;
		//logReader.runNum = writer.runNum;

		/*
		if(mode == 3)
		{
			logReader.enabled = true;
		}
		else
		{
			writer.enabled = true;
		}
		*/

		writer.mode = mode;
		writer.InitWriter();

		try
		{
			maze.startDelay = float.Parse(startDelayStr);
			maze.endDelay = float.Parse(endDelayStr);
			maze.grayTime = float.Parse(grayTimeStr);
			maze.warnTime = float.Parse(warnTimeStr);
			maze.choiceTime = float.Parse(choiceTimeStr);
                        if(mode == 0)
			    maze.timeLimit = float.Parse(timeLimitStr);
		}
		catch(Exception e)
		{
			Debug.LogError("Error parsing file (2)!");
			Debug.LogError(e);
			Application.Quit();
		}

		try
		{
			move.moveSpeed = 10f / float.Parse(travelTimeStr);
			move.rotateCooldown = 1f / float.Parse(rotRateStr);

			foreach(Camera c in move.gameObject.GetComponentsInChildren
				<Camera>())
			{
				c.fieldOfView = float.Parse(fovStr);
			}

			//Debug.Log("Halfway done parsing!");

			/*
			contextArr = contextStr.Split(' ');
			holdArr = holdStr.Split(' ');
			leftObjArr = leftObjStr.Split(' ');
			leftRewArr = leftRewStr.Split(' ');
			rightObjArr = rightObjStr.Split(' ');
			rightRewArr = rightRewStr.Split(' ');
			postHitArr = postHitStr.Split(' ');

			demon.contexts = new int[contextArr.Length];
			demon.holds = new float[holdArr.Length];
			demon.leftObjects = new int[leftObjArr.Length];
			demon.leftRewards = new int[leftRewArr.Length];
			demon.rightObjects = new int[rightObjArr.Length];
			demon.rightRewards = new int[rightRewArr.Length];
			demon.postHits = new float[postHitArr.Length];

			for(int i = 0; i < contextArr.Length; i++)
			{
				demon.contexts[i] = int.Parse(contextArr[i]);
			}
			for(int i = 0; i < holdArr.Length; i++)
			{
				demon.holds[i] = float.Parse(holdArr[i]);
			}
			for(int i = 0; i < leftObjArr.Length; i++)
			{
				demon.leftObjects[i] = int.Parse(leftObjArr[i]);
			}
			for(int i = 0; i < leftRewArr.Length; i++)
			{
				demon.leftRewards[i] = int.Parse(leftRewArr[i]);
			}
			for(int i = 0; i < rightObjArr.Length; i++)
			{
				demon.rightObjects[i] = int.Parse(rightObjArr[i]);
			}
			for(int i = 0; i < rightRewArr.Length; i++)
			{
				demon.rightRewards[i] = int.Parse(rightRewArr[i]);
			}
			for(int i = 0; i < postHitArr.Length; i++)
			{
				demon.postHits[i] = int.Parse(postHitArr[i]);
			}
			*/
		}
		catch(Exception e)
		{
			Debug.LogError("Error parsing file (3)!!");
			Debug.LogError(e);
			Application.Quit();
		}

		try
		{
			betasArrArr = new string[numTrials][];
			int length = 0;
			for(int i = 0; i < numTrials; ++i)
			{
				betasArrArr[i] = betasArr[i].Split(' ');
				if(betasArrArr[i].Length > length)
					length = betasArrArr[i].Length;
			}
			maze.betas = new int[numTrials, length];

			for(int i = 0; i < numTrials; ++i)
				for(int j = 0; j < betasArrArr[i].Length; ++j)
					maze.betas[i, j] = int.Parse(betasArrArr[i][j]) / 60;
		}
		catch(Exception e)
		{
			Debug.LogError("Error parsing file (4)!!");
			Debug.LogError(e);
			Application.Quit();
		}

		try
		{
			startHexArr = startHexStr.Split(' ');
			maze.startHexes = new GameObject[startHexArr.Length];
			for(int i = 0; i < startHexArr.Length; ++i)
			{
				string[] startStr = startHexArr[i].Split('-');
				int[] startInt = {int.Parse(startStr[0]),
					int.Parse(startStr[1])};
				maze.startHexes[i] = maze.maze[startInt[0], startInt[1]];
			}

			string[] goalStr = goalHexStr.Split('-');
			int[] goalInt = {int.Parse(goalStr[0]), int.Parse(goalStr[1])};
			maze.goalHex = maze.maze[goalInt[0], goalInt[1]];
		}
		catch(Exception e)
		{
			Debug.LogError("Error parsing file (5)!!");
			Debug.LogError(e);
			Application.Quit();
		}
	}

}
