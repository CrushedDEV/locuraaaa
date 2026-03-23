namespace ScapeRoom.Player
{
    public interface IPlayerSettings
    {
        float WalkSpeed { get; }
        float RunSpeed { get; }
        float CrouchSpeed { get; }
        float Acceleration { get; }
        float Deceleration { get; }
        float JumpForce { get; }
        float Gravity { get; }
        float MouseSensitivity { get; }
        float MaxLookAngle { get; }
        float StandingHeight { get; }
        float CrouchingHeight { get; }
        float AirControl { get; }
        float BobFrequency { get; }
        float BobAmplitude { get; }
        float SwayAmount { get; }
        float TiltAmount { get; }
        float SwaySmoothness { get; }
        float JumpBobAmount { get; }
        float LandBobAmount { get; }
        float JumpBobSmoothness { get; }
        float CoyoteTime { get; }
        float JumpBufferTime { get; }
    }
}
