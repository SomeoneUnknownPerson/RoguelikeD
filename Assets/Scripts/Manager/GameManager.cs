﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rogue;

public class GameManager : Singleton<GameManager>
{
   	public int level = 1;
    public int gold = 0;

   	public GameObject Canvas;
   	public GameObject GameOverPanel;
   	public GameObject NextLevelPanel;
   	public GameObject PausePanel;
   	public GameObject SettingsPanel;
   	public GameObject InventoryPlayerPanel;

   	public Text levelCount;
   	public Text goldCount;
   	public Text healthCount;

	public AudioClip BackgroundMusic;
	public AudioClip DeathSound;
	public AudioClip soundOfGold;

	public bool onPause = false;

	private Animation TransitionAnim;

	private void Start()
	{
		Application.targetFrameRate = 60;
		AudioManager.Instance.PlayMusic(BackgroundMusic);
		healthCount.text = Player.Instance.currentLifes.ToString() + 
            "/" + Player.Instance.maxLifes.ToString();
	}

   	private void Awake()
   	{
   		Time.timeScale = 1;
   		AudioManager.Instance.RestoreMusic();
        Generator.Instance.setupScene(level);
    }

	public void NewLevelMessage()
	{
		NextLevelPanel.SetActive(true);
		onPause = true;
		Time.timeScale = 0;
	}

	public void PauseGame()
	{
		if(SettingsPanel.activeSelf || PausePanel.activeSelf)
		{
			SettingsPanel.SetActive(false);
			PausePanel.SetActive(false);
			onPause = false;
			Time.timeScale = 1;
		}
		else
		{
			SettingsPanel.SetActive(true);
			PausePanel.SetActive(true);
			onPause = true;
			Time.timeScale = 0;
		}
	}

	public void NextGame()
	{
		PausePanel.SetActive(false);
		SettingsPanel.SetActive(false);

		onPause = false;
		Time.timeScale = 1;
	}

	public void OpenInventory()
	{
		onPause = true;
		InventoryPlayerPanel.SetActive(!InventoryPlayerPanel.activeSelf);
	}

	public void ToNewLevel()
	{
		onPause = false;
		Time.timeScale = 1;

		TransitionAnim = GameObject.Find("Panel").GetComponent<Animation>();
		TransitionAnim.Play("Transition");

		NextLevelPanel.SetActive(false);

		DestroyAllObjects();

		level++;
		levelCount.text = level.ToString();

		Generator.Instance.setupScene(level);
		
		Player.Instance.transform.position = new Vector3(100, 100, 0);
		Player.Instance.stepPoint = Vector3Int.FloorToInt(Player.Instance.transform.position);
		Camera.main.transform.position = new Vector3(100, 100, -5);	
	}

	public void DestroyAllObjects()
	{
		foreach (Transform child in transform) Destroy(child.gameObject);
	}

	public void GameOver()
	{
		AudioManager.Instance.PauseMusic();
		AudioManager.Instance.PlayEffects(DeathSound);
		onPause = true;
		GameOverPanel.SetActive(true);
	}

	public void addGold(int GoldCount)
	{
        gold += GoldCount;
		goldCount.text = gold.ToString();
		AudioManager.Instance.PlayEffects(soundOfGold);
	}

	public void LoadData(Save.GameManagerSaveData save)
    {
        gold = save.goldCount;
        level = save.levelCount;
        goldCount.text = gold.ToString();
        levelCount.text = level.ToString();
        healthCount.text = Player.Instance.currentLifes.ToString() + "/" + Player.Instance.maxLifes.ToString();
    }
}
