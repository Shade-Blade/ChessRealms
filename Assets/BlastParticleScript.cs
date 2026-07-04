using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastParticleScript : MonoBehaviour
{
    public SpriteRenderer sr;
    public float lifetime;
    public float maxLifetime;

    public enum ParticleSprite
    {
        Circle,
        Crack,
        Crescent,
        Crown,
        Drop,
        Fan,
        Fire,
        Hexagon,
        HollowSquare,
        Money,
        Null,
        Slippery,
        Snowflake,
        Spark,
        Sparkle,
        Spike,
        SpinArrow,
        Spiral,
        Square,
        WindH,
        WindV,
        X
    }

    public List<Sprite> particleSprites;
    public ParticleSprite particle;

    public void Setup(Piece.PieceAlignment pa, ParticleSprite ps, float maxLifetime)
    {
        switch (pa)
        {
            case Piece.PieceAlignment.White:
                sr.color = new Color(1, 1, 1, 0.7f);
                break;
            case Piece.PieceAlignment.Black:
                sr.color = new Color(0, 0, 0, 0.7f);
                break;
            case Piece.PieceAlignment.Neutral:
                sr.color = new Color(1, 1, 0, 0.7f);
                break;
            case Piece.PieceAlignment.Crystal:
                sr.color = new Color(0, 1, 1, 0.7f);
                break;
        }

        particle = ps;
        sr.sprite = particleSprites[(int)ps];

        sr.transform.localScale = Vector3.one * 0;
        this.maxLifetime = maxLifetime;
    }

    public void Update()
    {
        sr.transform.localScale = Vector3.one * (1 * (lifetime / maxLifetime));
        lifetime += Time.deltaTime;
        if (lifetime > this.maxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
