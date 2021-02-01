﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main menu manager.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private LevelLoaderReference levelLoaderReference;

    /// <summary>
    /// Called when the play button was pressed.
    /// </summary>
    /// <param name="levelLoadData">The level load data.s</param>
    public void OnPlayButtonPressed(LevelLoadData levelLoadData)
    {
        Debug.Log("pressed play");
        this.levelLoaderReference.Loader?.LoadLevel(levelLoadData);
    }

    /// <summary>
    /// Called when the credits button was pressed.
    /// </summary>
    /// <param name="levelLoadData">The level load data.</param>
    public void OnCreditsButtonPressed(LevelLoadData levelLoadData)
    {
        Debug.Log("pressed credits");
        this.levelLoaderReference.Loader?.LoadLevel(levelLoadData);
    }

    /// <summary>
    /// Called when the game exited.
    /// </summary>
    public void OnExitGame()
    {
        Debug.Log("pressed quit");
        Application.Quit(0);
    }
}
