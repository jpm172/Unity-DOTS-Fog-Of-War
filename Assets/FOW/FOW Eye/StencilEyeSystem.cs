
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

using Material = UnityEngine.Material;
using RaycastHit = Unity.Physics.RaycastHit;



//update order is very important for the whole player/eye/camera relationship,
//if the update order is not correct, then there will be noticeable flickering in the vision when moving

//The vision cones are created using a procedural mesh, and the algorithm is taken from Sebastian Lague (https://www.youtube.com/watch?v=73Dc5JTCmKI&t=1s)
//And slightly changed to work with ECS
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct StencilEyeSystem : ISystem
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


        foreach (
            var (transformComp, ltwComp, eyeComp, info, entity)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRO<LocalToWorld>, RefRO<StencilEyeComponent>, RefRO<MaterialMeshInfo>>()
                .WithEntityAccess()
        )
        {
            
            StencilEyeComponent eye = eyeComp.ValueRO;
            LocalTransform transform = transformComp.ValueRO;
            LocalToWorld ltw = ltwComp.ValueRO;
            

            int stepCount =  (int) math.round(eye.Resolution * eye.FOV);

            //calculate the transform of the eye relative to its parent
            LocalTransform t = transform.WithPosition( ltw.Position ).WithRotation( ltw.Rotation );
            
            //calculate the maximum size of the mesh, where the worst case is every raycast
            //will call the FindEdge function, and where every FindEdge adds 2 more vertices
            int vertexCount = ( stepCount + 2 ) + ( stepCount * 2);

            NativeArray<Vector3> vertices = new NativeArray<Vector3>(vertexCount, Allocator.TempJob);
            NativeArray<int> triangles = new NativeArray<int>((vertexCount)*3, Allocator.TempJob);
            NativeReference<int> newLength = new NativeReference<int>(Allocator.TempJob);
            
            
           
            new StencilEyePhyicsQueryJob()
            {
                PhysicsWorld = physicsWorld,
                eye = eye,
                transform = t,
                Vertices = vertices,
                Triangles = triangles,
                NewLength = newLength
            }.Run();
            
            
            //fetch the mesh from the render mesh array
            RenderMeshArray arr = state.EntityManager.GetSharedComponentManaged<RenderMeshArray>(entity);
            Mesh curMesh = arr.GetMesh( info.ValueRO );

            //update the mesh
            curMesh.Clear();
            curMesh.vertices = vertices.Slice(0, newLength.Value).ToArray();
            curMesh.triangles = triangles.Slice(0, newLength.Value*3).ToArray();

            newLength.Dispose();
            vertices.Dispose();
            triangles.Dispose();
            

        }
    }

}

[BurstCompile]
public struct StencilEyePhyicsQueryJob : IJob
{
    private static readonly float3 float3Forward = new float3( 1, 0, 0 );
    
    //This collision filter will collide with everything BUT the player (the player is on layer 6)
    private static readonly CollisionFilter RayFilter = new CollisionFilter
    {
        CollidesWith = ~(uint)( 1 << 6 ),
        BelongsTo = ~(uint)( 1 << 6 )
    };
    
    public StencilEyeComponent eye;
    public LocalTransform transform;
    public PhysicsWorldSingleton PhysicsWorld;

    public NativeArray<Vector3> Vertices;
    public NativeArray<int> Triangles;
    public NativeReference<int> NewLength;

    public void Execute( )
    {
        int stepCount =  (int) math.round(eye.Resolution * eye.FOV);
        float degreesPerStep = eye.FOV / stepCount;

        NativeList<float3> viewPoints = new NativeList<float3>(stepCount+1, Allocator.Temp);
        ViewCastInfo oldViewCast = new ViewCastInfo();
        //cast a ray every (degreesPerStep) and store the result in the viewPoints list
        for ( int i = 0; i <= stepCount; i++ )
        {
            float angle = -( eye.FOV / 2 ) + eye.RelativeAngle +  degreesPerStep * i;
            
            ViewCastInfo viewCast = CastRay(  angle );

            if ( i > 0 )
            {
                bool threshold = math.abs( oldViewCast.Distance - viewCast.Distance ) > eye.EdgeDistanceThreshold;
                //if the previous cast and the current cast have different results, or the two results are too far away from each other
                //try and find the points in between the two casts to get a more accurate result
                if ( oldViewCast.Hit != viewCast.Hit || (oldViewCast.Hit && viewCast.Hit && threshold) )
                {
                    EdgeInfo edge = FindEdge( oldViewCast, viewCast );
                    
                    if ( edge.PointA != Vector3.zero )
                    {
                        viewPoints.Add( edge.PointA );
                    }
                    if ( edge.PointB != Vector3.zero )
                    {
                        viewPoints.Add( edge.PointB );
                    }
                    
                }
            }
            
            
            oldViewCast = viewCast;
            viewPoints.Add( viewCast.Position ); ;
        }

        
        int vertexCount = viewPoints.Length + 1;
        NewLength.Value = vertexCount;

        Vertices[0] = Vector3.zero;
        for ( int i = 0; i < vertexCount -1; i++ )
        {
            Vertices[i + 1] = transform.InverseTransformPoint( viewPoints[i] )+ float3Forward *eye.CutAway;

            if ( i < vertexCount - 2 )
            {
                Triangles[i * 3] = i + 2;
                Triangles[i * 3 + 1] = i + 1;
                Triangles[i * 3 + 2] = 0;
            }
        }
        
    }

    
    private ViewCastInfo CastRay( float angle)
    {
        float3 rayEnd = transform.RotateZ( angle * math.TORADIANS ).Right() * eye.ViewDistance;

        RaycastInput rayInput = new RaycastInput
        {
            Start = transform.Position,
            End = transform.Position + rayEnd,
            Filter = RayFilter
        };

        
        if ( PhysicsWorld.CastRay( rayInput, out RaycastHit rayHit ) )
        {
            return new ViewCastInfo(true, rayHit.Position, math.distance( rayHit.Position, transform.Position ), angle );
        } 
        
        
        return new ViewCastInfo(false, rayInput.End, eye.ViewDistance, angle );
    }
    
    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.Angle;
        float maxAngle = maxViewCast.Angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for ( int i = 0; i < eye.ResolveIterations; i++ )
        {
            float angle = ( minAngle + maxAngle ) / 2;
            
            ViewCastInfo viewCast = CastRay( angle );

            bool threshold = math.abs( minViewCast.Distance - viewCast.Distance ) > eye.EdgeDistanceThreshold;
            if ( viewCast.Hit == minViewCast.Hit && !threshold )
            {
                minPoint = viewCast.Position;
                minAngle = angle;
            }
            else
            {
                maxPoint = viewCast.Position;
                maxAngle = angle;
            }
            
        }
        
        return new EdgeInfo(minPoint, maxPoint);
    } 
    
}


