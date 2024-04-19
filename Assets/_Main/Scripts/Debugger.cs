using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Spiderman
{
    public class Debugger : MonoBehaviour
    {
        public static Debugger Instance;

        [SerializeField] private TextMeshProUGUI mainState;
        [SerializeField] private TextMeshProUGUI substate;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void UpdateCurrentStateDebugger(MainStates main, SubStates sub)
        {
            mainState.text = $"Main state {main.ToString()}";
            substate.text = $"Sub state {sub.ToString()}";
        }
    }
}