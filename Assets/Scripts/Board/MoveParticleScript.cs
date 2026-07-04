using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveParticleScript : MonoBehaviour
{
    public SpriteRenderer sr;

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

    public static ParticleSprite GetParticleSpriteFromMove(Move.SpecialType st)
    {
        switch (st)
        {
            case Move.SpecialType.FireCapture:
            case Move.SpecialType.FireCaptureOnly:
                return ParticleSprite.Fire;
            case Move.SpecialType.Convert:
            case Move.SpecialType.ConvertCaptureOnly:
            case Move.SpecialType.ConvertPawn:
            case Move.SpecialType.ConvertRabbit:
                return ParticleSprite.SpinArrow;
            case Move.SpecialType.RangedPull:
            case Move.SpecialType.RangedPush:
            case Move.SpecialType.RangedPullAllyOnly:
            case Move.SpecialType.RangedPushAllyOnly:
                return ParticleSprite.Fan;
            case Move.SpecialType.InflictFreeze:
            case Move.SpecialType.InflictFreezeCaptureOnly:
                return ParticleSprite.Snowflake;
            case Move.SpecialType.InflictShift:
            case Move.SpecialType.InflictShiftCaptureOnly:
                return ParticleSprite.Fan;
            case Move.SpecialType.Inflict:
            case Move.SpecialType.InflictCaptureOnly:
                return ParticleSprite.Spiral;
            case Move.SpecialType.ChargeApplyModifier:
                return ParticleSprite.Hexagon;
            case Move.SpecialType.AimAny:
            case Move.SpecialType.AimEnemy:
            case Move.SpecialType.AimOccupied:
                return ParticleSprite.X;
            case Move.SpecialType.DepositAlly:
            case Move.SpecialType.DepositAllyPlantMove:
            case Move.SpecialType.Spawn:
                return ParticleSprite.Circle;
        }

        return ParticleSprite.Sparkle;
    }

    public void Setup(Piece.PieceAlignment pa, ParticleSprite ps)
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
    } 
}
