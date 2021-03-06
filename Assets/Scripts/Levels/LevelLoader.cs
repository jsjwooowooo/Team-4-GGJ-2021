﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loading Scene.
/// </summary>
public class LoadingScene
{
    public event System.Action SceneLoadedEvent
        = delegate { };

    private float _currentLoadTime = 0f;
    private float _maxLoadTime;

    private bool _executingCoroutine = false;
    private string _sceneToLoad;

    private LevelTransitionData _transitionData;
    private StringEvent _beginEvent;

    private bool _midTransition = false, _displayLoading = false;

    public bool CanLoadScene
        => this._currentLoadTime >= this._maxLoadTime && !this._executingCoroutine;

    public LoadingScene(string sceneToLoad, StringEvent beginEvent, LevelLoadData data, float maxLoadTime)
    {
        this._transitionData = data.Transitions;
        this._sceneToLoad = sceneToLoad;
        this._maxLoadTime = maxLoadTime;
        this._beginEvent = beginEvent;
        this._displayLoading = data.ShowLoadingSceen;
    }

    public void Update()
    {
        if (this._currentLoadTime < this._maxLoadTime)
        {
            this._currentLoadTime += Time.deltaTime;
        }
    }

    public void OnMidTransition()
    {
        this._midTransition = true;
    }

    public IEnumerator LoadSceneAsync()
    {
        this._executingCoroutine = true;

        if (this._transitionData.DisplayTransition)
        {
            this._beginEvent?.Call(this._transitionData.TransitionName);

            if (this._displayLoading)
            {
                yield return new WaitUntil(() => this._midTransition);
            }
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(this._sceneToLoad);
        yield return new WaitUntil(() => operation.isDone);
        this.SceneLoadedEvent();
    }
}

/// <summary>
/// The level loader reference.
/// </summary>
[System.Serializable]
public class LevelLoaderReference
{
    [SerializeField]
    private LevelLoader levelLoader;

    public LevelLoader Loader
        => this.levelLoader;
}

/// <summary>
/// Loads the level.
/// </summary>
[CreateAssetMenu(fileName = "Level Loader", menuName = "Levels/Level Loader")]
public class LevelLoader : ScriptableObject
{
    // TODO: Implement something where we can apply the transition to after the loading screen is applied.

    [SerializeField]
    private StringEvent beginTransitionEvent;
    [SerializeField]
    private GameEvent midTransitionEvent;
    [SerializeField]
    private GameEvent switchLevelEvent;

    private LevelLoadData _transitionLoadData = null;
    private LoadingScene _currentLoadingScene = null;

    public bool IsLoading
        => this._currentLoadingScene != null;

    public bool WaitingForTransition
        => this._transitionLoadData != null;


    private void OnEnable()
    {
        this.midTransitionEvent += this.OnMidTransition;
    }

    private void OnDisable()
    {
        this.midTransitionEvent -= this.OnMidTransition;
    }

    public void Update()
    {
        this._currentLoadingScene?.Update();
    }

    public IEnumerator EnumerateLoadSceneAsync()
    {
        if (_currentLoadingScene == null)
        {
            return null;
        }

        if (_currentLoadingScene.CanLoadScene)
        {
            return _currentLoadingScene.LoadSceneAsync();
        }
        return null;
    }

    /// <summary>
    /// Loads the level.
    /// </summary>
    /// <param name="levelData">The level data.</param>
    public void LoadLevel(LevelLoadData levelData)
    {
        if (this.IsLoading)
        {

            return;
        }

        if (this.WaitingForTransition && this._transitionLoadData != levelData)
        {
            return;
        }

        if (!this.WaitingForTransition && levelData.Transitions.DisplayTransition)
        {
            this._transitionLoadData = levelData;
            this.beginTransitionEvent?.Call(levelData.Transitions.TransitionName);
            return;
        }

        if (levelData.ShowLoadingSceen)
        {
            SceneManager.LoadScene("LoadingScreen");
            this.switchLevelEvent?.Call();
            GenerateLoadingScene(levelData.RandomLoadTime, levelData);
            return;
        }

        GenerateLoadingScene(0f, levelData);
    }

    private void OnMidTransition()
    {
        if (this.IsLoading)
        {
            this._currentLoadingScene.OnMidTransition();
            return;
        }

        if (this._transitionLoadData != null)
        {
            this.LoadLevel(this._transitionLoadData);
            this._transitionLoadData = null;
        }
    }

    private void GenerateLoadingScene(float loadingTime, LevelLoadData levelData)
    {
        this._currentLoadingScene = new LoadingScene(levelData.LevelToLoad, this.beginTransitionEvent, levelData, loadingTime);
        this._currentLoadingScene.SceneLoadedEvent += OnCompleted;
    }

    /// <summary>
    /// Called when the level loader has completed.
    /// </summary>
    private void OnCompleted()
    {
        this._currentLoadingScene.SceneLoadedEvent -= OnCompleted;
        this._currentLoadingScene = null;
        this.switchLevelEvent?.Call();
    }
}
