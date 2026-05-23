using System.Collections.Generic;
using UnityEngine;

public class BootManager : MonoBehaviour
{
    public List<GameObject> persistentSytems;

    private void Awake()
    {
        Application.targetFrameRate = 120;

        foreach (GameObject system in persistentSytems)
        {
            Instantiate(system);
        }
    }

    private void Start()
    {
        SceneLoader.Instance.LoadMainMenu();
    }
}
