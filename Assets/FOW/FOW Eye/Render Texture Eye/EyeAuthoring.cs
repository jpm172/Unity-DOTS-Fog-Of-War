using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class EyeAuthoring : MonoBehaviour
{

    public float Resolution;
    public float FOV;
    public float RelativeAngle;
    public float ViewDistance;
    public float EdgeDistanceThreshold;
    public float CutAway;
    public float Hardness;
    public float Strength;
    public int ResolveIterations;
    
    public class EyeBaker : Baker<EyeAuthoring>
    {
        public override void Bake(EyeAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent( entity, new EyeComponent
            {
                Resolution = authoring.Resolution,
                FOV = authoring.FOV,
                Hardness = authoring.Hardness,
                Strength = authoring.Strength,
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

public struct EyeComponent : IComponentData
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

    //values for the eye material
    public float Hardness;
    public float Strength;
}
