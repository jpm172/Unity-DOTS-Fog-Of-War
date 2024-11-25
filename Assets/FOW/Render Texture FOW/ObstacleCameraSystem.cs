using Unity.Entities;
using Unity.Rendering;
using UnityEngine;


//this system captures what the Obstacle Camera sees for use in the FOW shader, then disables the camera and this system
[UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
public partial class ObstacleCameraSystem : SystemBase
{
    private Camera _camera;
    private int _frameCount = 0;
    
    protected override void OnCreate()
    {
        EntityQuery query = GetEntityQuery(typeof(RenderMeshArray), typeof(EyeComponent));
        RequireForUpdate(query);
    }
    
    
    protected override void OnUpdate()
    {
        //wait until after the first frame to capture the camera
        //pretty hacky, but seems to be the best way to ensure every entity is rendered and can be captured by the camera
        if(_frameCount > 0)
            RenderAndDisable();
        
        _frameCount++;
    }

    private void RenderAndDisable()
    {
        GameObject cameraObj = GameObject.FindWithTag( "ObstacleCamera" );
        if ( cameraObj == null )
        {
            World.GetExistingSystemManaged<ObstacleCameraSystem>().Enabled = false;
            return;
        }

        _camera = cameraObj.GetComponent<Camera>();
        _camera.Render();
        _camera.gameObject.SetActive(false);
        
        World.GetExistingSystemManaged<ObstacleCameraSystem>().Enabled = false;
        

    }
}