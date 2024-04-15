using UnityEngine;
using System;

namespace KillItMyself.Runtime
{
    public class DisableObjectGameSettings : MonoBehaviour
    {
        [SerializeField] private string VariableName;

        private void Start()
        {
            Type t = typeof(GameSettings);
            if (!(bool)t.GetField(VariableName).GetValue(t))
            {
                gameObject.SetActive(false);
            }
        }
    }
}