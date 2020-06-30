﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogWriter : MonoBehaviour
{

	public Transform player;
	public SimpleMovement move;
	public MazeLogic maze;

	public string fileName;

	public int frameFreq = 24;

    StreamReader lastRunReader;
	StreamWriter writer;

	private float lastFrame;
	private float frameTime;
	private float trialStart;
	private float runStart;

	private string spc = " ";

	private int frame = 0;
	private int choiceNum;

	private bool writing = false;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            lastRunReader = new StreamReader(fileName);
            Debug.LogError("Repeated log filename! Update Config.txt or remove"
                    + " last log file from directory.");
            Application.Quit();
            return;
        }
        catch {}

        writer = new StreamWriter(fileName);
		frameTime = 1f / (float) frameFreq;
    }

	public void StartWriting()
	{
		writing = true;
		WriteFrame();
		runStart = Time.time;
		lastFrame = runStart;
	}

    // Update is called once per frame
    void Update()
    {
        if(writing && Time.time - lastFrame >= frameTime)
        {
			WriteFrame();
            lastFrame += frameTime;
        }
    }

	private void WriteFrame()
	{
		writer.WriteLine("Frame " + (frame++).ToString() + ":" + spc
			+ string.Format("{0:N3}", player.position.x) + spc
			+ string.Format("{0:N3}", player.position.z) + spc
			+ string.Format("{0:N3}", player.eulerAngles.y) + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart) + spc
			+ string.Format("{0:N3}", maze.GetDistToGoal()) + spc
			+ string.Format("{0:N3}", maze.GetAngleToGoal()));
	}

	public void WriteTrialStart()
	{
		trialStart = Time.time;
		int trial = maze.trial;
		writer.WriteLine("Trial " + trial.ToString() + ":" + spc
			+ maze.startHexes[trial].GetComponent<HexLogic>()
			.column.ToString() + "-"
			+ maze.startHexes[trial].GetComponent<HexLogic>()
			.row.ToString() + spc
			+ (maze.betas[0] * 60).ToString() + spc
			+ string.Format("{0:N3}", Time.time - runStart));

		choiceNum = 0;
	}

	public void WriteAction()
	{
		writer.WriteLine("Action " + maze.trial.ToString() + "."
			+ choiceNum++.ToString() + ":" + spc
			+ maze.lastChoice.ToString() + spc
			+ BoolToString(maze.lastCorrect) + spc + maze.alpha + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteChoiceStart()
	{
		writer.WriteLine("Start_Choice " + maze.trial.ToString() + "."
			+ choiceNum.ToString() + ":" + spc
			+ maze.curHex.GetComponent<HexLogic>().column.ToString() + "-"
			+ maze.curHex.GetComponent<HexLogic>().row.ToString() + spc
			+ (maze.betas[maze.moveCounter] * 60).ToString() + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteGoal()
	{
		int trial = maze.trial;
		writer.WriteLine("Goal " + trial.ToString() + ":" + spc
			+ maze.curHex.GetComponent<HexLogic>().column.ToString() + "-"
			+ maze.curHex.GetComponent<HexLogic>().row.ToString() + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteGrayScreen()
	{
		int trial = maze.trial;
		writer.WriteLine("Gray_Screen " + trial.ToString() + ":" + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	private string BoolToString(bool b)
	{
		if(b)
			return "1";
		else
			return "0";
	}

}
