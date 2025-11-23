using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadgeTypeScript : MonoBehaviour
{
    public BadgeScript cs;
    public TMPro.TMP_InputField typeField;

    public Board.PlayerModifier oldPM = 0;
    public void Update()
    {
        Enum.TryParse(typeField.text, out Board.PlayerModifier newPM);

        if (newPM != oldPM)
        {
            cs.Setup(newPM);
            oldPM = newPM;
        }
    }
}
