using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadgeTypeScript : MonoBehaviour
{
    public BadgeScript cs;
    public TMPro.TMP_InputField typeField;

    [NonSerialized]
    public Board.PlayerModifier oldPM = 0;
    [NonSerialized]
    public Board.PlayerModifierB oldPMB = 0;
    public void Update()
    {
        Enum.TryParse(typeField.text, out Board.PlayerModifier newPM);
        Enum.TryParse(typeField.text, out Board.PlayerModifierB newPMB);

        if (newPM != oldPM || newPMB != oldPMB)
        {
            cs.Setup(new Board.BadgeType(newPM, newPMB));
            oldPM = newPM;
            oldPMB = newPMB;
        }
    }
}
