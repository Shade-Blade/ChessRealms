using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableTypeScript : MonoBehaviour
{
    public ConsumableScript cs;
    public TMPro.TMP_InputField typeField;

    Move.ConsumableMoveType oldCMT = Move.ConsumableMoveType.Bottle;
    public void Update()
    {
        Enum.TryParse(typeField.text, out Move.ConsumableMoveType newCMT);

        if (newCMT != oldCMT)
        {
            cs.Setup(newCMT);
            oldCMT = newCMT;
        }
    }
}
