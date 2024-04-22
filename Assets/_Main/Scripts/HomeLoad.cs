using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeLoad : MonoBehaviour
{
    [SerializeField] private Button playBtn;

    private bool _clicked;
    private void Awake()
    {
        playBtn.onClick.AddListener(() =>
        {
            if (!_clicked)
            {
                _clicked = false;
                SceneManager.LoadScene(1);
            }
        });
    }
}
