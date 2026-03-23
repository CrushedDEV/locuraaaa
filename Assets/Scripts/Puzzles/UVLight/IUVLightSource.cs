using UnityEngine;

namespace ScapeRoom.Puzzles.UVLight
{
    public interface IUVLightSource
    {
        Vector3 Position { get; }
        Vector3 Forward { get; }
        float Range { get; }
        float SpotAngle { get; }
        bool IsOn { get; }
    }
}
