using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using CapsuleCollider = Unity.Physics.CapsuleCollider;

//update order is very important for the whole player/eye/camera relationship,
//if the update order is not correct, then there will be noticeable flickering in the vision when moving

//This system is an implementation of Collide and Slide taken from Poke Dev (https://www.youtube.com/watch?v=YR6Q7dUz2uk&t=457s)
//Adjusted to be a 2D character controller and work in ECS
[UpdateInGroup(typeof(TransformSystemGroup), OrderFirst = true)]
public partial struct PlayerMoveSystem : ISystem
{
    

    public void OnCreate( ref SystemState state )
    {
        
    }

    public void OnDestroy( ref SystemState state )
    {
        
    }

    public void OnUpdate( ref SystemState state )
    {
        
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var deltaTime = SystemAPI.Time.DeltaTime;
        new PlayerMoveJob
        {
            DeltaTime = deltaTime,
            PhysicsWorld = physicsWorld
        }.Schedule();
        
        
    }
}

[BurstCompile]
public partial struct PlayerMoveJob : IJobEntity
{
    
    public float DeltaTime;
    public PhysicsWorldSingleton PhysicsWorld;
    private static readonly float AngleAdjust = math.radians( 90 );
    private static readonly float SkinWidth = 0.0625f;
    private static readonly int MaxDepth = 5;
    
    //This collision filter will collide with everything BUT the player (the player is on layer 6)
    private static readonly CollisionFilter CastFilter = new CollisionFilter
    {
        CollidesWith = ~(uint)( 1 << 6 ),
        BelongsTo = ~(uint)( 1 << 6 )
    };

    private void Execute( ref LocalTransform transform, in PlayerInputs input, MyCharacterComponent attributes,
        PhysicsCollider col )
    {
        //rotate the character to look at the mouse
        float3 forward = input.AimPosition - transform.Position;
        quaternion rotation = quaternion.LookRotationSafe(transform.Forward(), forward );
        
        transform.Rotation = rotation;
        transform = transform.RotateZ( AngleAdjust );

        //calculate where the player is trying to move to
        float2 targetMove = input.MoveInput * attributes.MovementSpeed * DeltaTime;
        float3 vel = new float3( targetMove, 0 );

        float3 result = CollideAndSlide( col, vel, transform.Position, transform, 0, vel );

        transform.Position.xy += result.xy;
    }


    private float3 CollideAndSlide( PhysicsCollider col, float3 vel, float3 pos, LocalTransform transform, int depth, float3 velInit )
    {
        if(depth>= MaxDepth)
            return float3.zero;
        
        float dist = math.length( vel ) + SkinWidth;
        float radius = col.Value.As<CapsuleCollider>().Radius;;
        
        if ( PhysicsWorld.SphereCast( pos, radius - SkinWidth, math.normalizesafe( vel ), dist, out ColliderCastHit hit, CastFilter ) )
        {
            //get the vector that will align the player in front of whatever the cast hit
            float3 snapToSurface = math.normalizesafe( vel ) * ( math.distance( pos.xy, hit.Position.xy ) - radius - SkinWidth );
            
            //calculate the new velocity after after the collision
            float3 leftOver = vel - snapToSurface;
            
            if(math.length( snapToSurface ) <= SkinWidth)
                snapToSurface = float3.zero;
            
            float mag = math.length( leftOver );
            leftOver =  math.normalizesafe(ProjectOnPlane( leftOver, hit.SurfaceNormal ));
            leftOver *= mag;
            
            
            //slide the new velocity along a steep slope/wall
            float scale = 1 - math.dot( math.normalizesafe( hit.SurfaceNormal.xy ), -math.normalizesafe( velInit.xy ) );
            leftOver *= scale;

            //do another recursive call with the new calculated values for velocity and position
            return snapToSurface + CollideAndSlide( col, leftOver, pos + snapToSurface, transform, depth + 1, velInit );
        }
        
        return vel;
    }
    
    private static float3 ProjectOnPlane(float3 vector, float3 planeNormal)
    {
        return vector - math.project(vector, planeNormal);
    }
}
