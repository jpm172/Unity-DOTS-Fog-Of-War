
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//update order is very important for the whole player/eye/camera relationship,
//if the update order is not correct, then there will be noticeable flickering in the vision when moving
[UpdateInGroup(typeof(TransformSystemGroup), OrderLast = true)]
public partial class CameraFollowSystem : SystemBase
{

    private EntityQuery playerQuery;
    private static float3 offset = new float3(0,0, -10);
    private Camera _camera;
    
    
    protected override void OnCreate()
    {
        playerQuery = SystemAPI.QueryBuilder().WithAll<  PlayerInputs, LocalTransform>().Build();
        RequireForUpdate( playerQuery );
    }
    
    protected override void OnStartRunning()
    {
        _camera = Camera.main;
        
    }

    protected override void OnUpdate()
    {
        //this query fetches the transform of the player
        foreach ( var (input, lt) in SystemAPI.Query<RefRO<PlayerInputs>, RefRO<LocalTransform>>() )
        {
            _camera.transform.position = lt.ValueRO.Position + offset;
        }
        

    }
}
