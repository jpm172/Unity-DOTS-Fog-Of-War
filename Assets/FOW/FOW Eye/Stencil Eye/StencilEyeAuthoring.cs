using Unity.Entities;
using UnityEngine;

public class StencilEyeAuthoring : MonoBehaviour
{
    
    public float Resolution;
    public float FOV;
    public float RelativeAngle;
    public float ViewDistance;
    public float EdgeDistanceThreshold;
    public float CutAway;
    public int ResolveIterations;
    
    public class StencilEyeAuthoringBaker : Baker<StencilEyeAuthoring>
    {
        public override void Bake(StencilEyeAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent( entity, new StencilEyeComponent
            {
                Resolution = authoring.Resolution,
                FOV = authoring.FOV,
                RelativeAngle = authoring.RelativeAngle,
                ViewDistance = authoring.ViewDistance,
                EdgeDistanceThreshold = authoring.EdgeDistanceThreshold,
                ResolveIterations = authoring.ResolveIterations,
                CutAway = authoring.CutAway
            } );


            AddComponent(entity, new InitializeTag());

        }
    }
}

public struct StencilEyeComponent : IComponentData
{
    //values for making the eye mesh
    public float Resolution;
    public float FOV;
    //the RelativeAngle is used to easily turn the eye without worrying about transforms
    //ie. if you want an eye that looks behind the player, set RelativeAngle to 180
    public float RelativeAngle;
    public float ViewDistance;
    public float EdgeDistanceThreshold;
    public int ResolveIterations;
    public float CutAway;
}


