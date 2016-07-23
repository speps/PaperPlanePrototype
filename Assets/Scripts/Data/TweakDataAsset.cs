using UnityEngine;

public class TweakDataAsset : ScriptableObject
{
    public int MaxPopUps = 0;
    public float RollMax = 45.0f;
    public float TriggersThreshold = 0.1f;

    public float RespawnDelay = 1.0f;

    public float DirectionForceMin = 0.2f;
    public float DirectionForceMax = 0.8f;

    public float LiftVelocityMin = 0.0f;
    public float LiftVelocityMax = 1.8f;

    public float LiftForceMin = 1.0f;
    public float LiftForceMinAction = -0.5f;
    public float LiftForceMax = 3.0f;
    public float LiftForceMaxAction = 1.0f;

    public float CombineReleasedFactor = 0.6f;
    public float CombinePressedFactor = 1.0f;
    public float CombineMaximumTime = 3.0f;

    public float BoostDelay = 4.0f;
    public float BoostingDelay = 2.0f;
    public float BoostingForce = 4.0f;

    public float FeedbackTimer = 10.0f;
    public float FlySurfaceParticleVelocity = 0.3f;
    public float TubeParticleVelocity = 0.3f;
    public float FlyThroughParticleVelocity = 0.3f;
    public float TubeParticleAngularVelocity = 180f;
    public float ApproachForce = 3.0f;

    public float ActionFactorFlySurface = 1.0f;

    public float PadTresholdTubeH = 0.8f;

    public bool DrawPopUpGraph = false;
}
