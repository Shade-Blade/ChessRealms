using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoverArmyPopupScript : HoverPopupScript
{
    public Piece.PieceType[] army;

    public GameObject hoverPiecePrototype;
    public GameObject squarePrototype;
    public GameObject armyHolder;
    public GameObject squareHolder;

    public List<HoverPopupPieceScript> hppsList;

    public void SetArmy(Piece.PieceType[] army)
    {
        int rows = 2;
        for (int i = 0; i < army.Length; i++)
        {
            if (army[i] == Piece.PieceType.Null)
            {
                continue;
            }

            rows = 1 + (i >> 3);
        }

        baseBox.rectTransform.sizeDelta = new Vector2(260, 30 + 30 * rows);
        borderBox.rectTransform.sizeDelta = new Vector2(260, 30 + 30 * rows);
        textMesh.rectTransform.sizeDelta = new Vector2(240, 10 + 30 * rows);
        armyHolder.GetComponent<RectTransform>().localPosition = Vector3.down * (5 + 15 * rows);

        this.army = army;

        for (int i = 0; i < hppsList.Count; i++)
        {
            Destroy(hppsList[i].gameObject);
        }
        hppsList = new List<HoverPopupPieceScript>();

        for (int i = 0; i < army.Length; i++)
        {
            if (i < rows << 3)
            {
                GameObject newSquare = Instantiate(squarePrototype, squareHolder.transform);
                Image squareImage = newSquare.GetComponent<Image>();
                if (((i + (i >> 3)) & 1) == 0)
                {
                    squareImage.color = new Color(0.75f, 0.75f, 0.75f);
                }
                else
                {
                    squareImage.color = new Color(0.9f, 0.9f, 0.9f);
                }
                squareImage.rectTransform.localPosition = new Vector3(-105 + 30 * (i & 7), 15 + 30 * (i >> 3), 0);
            }

            if (army[i] == Piece.PieceType.Null)
            {
                continue;
            }

            GameObject newPiece = Instantiate(hoverPiecePrototype, armyHolder.transform);
            HoverPopupPieceScript hpps = newPiece.GetComponent<HoverPopupPieceScript>();
            RectTransform hppsrt = hpps.GetComponent<RectTransform>();
            hppsrt.localPosition = new Vector3(-105 + 30 * (i & 7), 15 + 30 * (i >> 3), 0);
            hpps.Setup(army[i]);
            hppsList.Add(hpps);
        }
    }

    public override void PositionUpdate()
    {
        rectTransform.anchoredPosition = MainManager.Instance.RealMousePos() + (baseBox.rectTransform.sizeDelta.x * Vector2.right * 0.5f) + (baseBox.rectTransform.sizeDelta.y * Vector2.down * 0.5f) + new Vector2(-5, -10);

        if (baseBox.rectTransform.sizeDelta.x + rectTransform.anchoredPosition.x > MainManager.CanvasWidth())
        {
            rectTransform.anchoredPosition = MainManager.Instance.RealMousePos() - (baseBox.rectTransform.sizeDelta.x * Vector2.right * 0.5f) + (baseBox.rectTransform.sizeDelta.y * Vector2.down * 0.5f) + new Vector2(5, -10);
        }
    }


    public override void RecalculateBoxSize()
    {
        //remove it completely, force box to stay same size
    }
}
