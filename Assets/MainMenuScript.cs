using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    //background stuff
    public SpriteRenderer backgroundA;
    public SpriteRenderer backgroundB;

    public List<SpriteRenderer> pieceSprites;
    public float spawnTimer;

    public Scrollbar difficultySlider;
    public TMPro.TMP_Text difficultyText;

    public GameObject mainMenuSpriteTemplate;

    //if you go back to main menu then generate another seed
    private void Start()
    {
        MainManager.Instance.playerData.GenerateSeed();
    }

    // Update is called once per frame
    void Update()
    {
        MainManager.Instance.playerData.difficulty = (int)((difficultySlider.value) * 2 + 0.5f) + 3;
        switch (MainManager.Instance.playerData.difficulty)
        {
            case 1:
                difficultyText.text = "Difficulty\n<size=75%><color=#00ffff>Very Very Easy</color></size>";
                break;
            case 2:
                difficultyText.text = "Difficulty\n<size=75%><color=#40ffff>Very Easy</color></size>";
                break;
            case 3:
                difficultyText.text = "Difficulty\n<size=75%><color=#80ffff>Easy</color></size>";
                break;
            case 4:
                difficultyText.text = "Difficulty\n<size=75%><color=#ffffff>Normal</color></size>";
                break;
            case 5:
                difficultyText.text = "Difficulty\n<size=75%><color=#ff8080>Hard</color></size>";
                break;
        }


        float timeValue = Time.time * 0.2f;
        backgroundB.transform.localPosition = Vector3.up * 0.25f * (timeValue - Mathf.Ceil(timeValue));

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            spawnTimer = 0.1f;

            GameObject go = Instantiate(mainMenuSpriteTemplate);
            SpriteRenderer s = go.GetComponent<SpriteRenderer>();
            Piece.PieceType pieceType = (Piece.PieceType)(Random.Range((int)(Piece.PieceType.Null + 1), (int)Piece.PieceType.EndOfTable));
            while ((GlobalPieceManager.GetPieceTableEntry(pieceType).pieceValueX2 == 0))
            {
                pieceType = (Piece.PieceType)(Random.Range((int)(Piece.PieceType.Null + 1), (int)Piece.PieceType.EndOfTable));
            }
            s.material = Text_PieceSprite.GetMaterial(Piece.PieceModifier.None);
            s.SetPropertyBlock(new MaterialPropertyBlock());
            s.sprite = Text_PieceSprite.GetPieceSprite(pieceType);

            if ((GlobalPieceManager.GetPieceTableEntry(pieceType).piecePropertyB & Piece.PiecePropertyB.Giant) != 0)
            {
                go.transform.localScale = Vector3.one * 2;
            }

            if (Random.Range(0, 1f) < 0.5f)
            {
                s.color = Piece.GetPieceColor(Piece.PieceAlignment.Black);
            }
            else
            {
                s.color = Piece.GetPieceColor(Piece.PieceAlignment.White);
            }

            go.transform.localPosition = Vector3.down * 6f + Vector3.right * Random.Range(-10f, 10f) + Vector3.forward * Random.Range(-1, 0);

            pieceSprites.Add(s);
        }

        for (int i = 0; i < pieceSprites.Count; i++)
        {
            pieceSprites[i].transform.localPosition += Time.deltaTime * (Vector3.up + Vector3.right * (pieceSprites[i].transform.localPosition.x * 0.1f));

            if (pieceSprites[i].transform.localPosition.y > 6f || pieceSprites[i].transform.localPosition.x > 12f || pieceSprites[i].transform.localPosition.x < -12f)
            {
                Destroy(pieceSprites[i].gameObject);
                pieceSprites.RemoveAt(i);
                i--;
                continue;
            }
        }
    }

    public void PlayButton()
    {
        MainManager.Instance.StartRun();
    }

    public void ExitButton()
    {
        Application.Quit();
    }
}
