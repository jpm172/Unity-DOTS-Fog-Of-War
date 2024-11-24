using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
//Registers the meshes and materials needed for the EyeSystem when a new eye is added
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class InitializeEyeSystem : SystemBase
{

    private Material _revealMat;
    private Material _stencilMat;

    private static readonly int Radius = Shader.PropertyToID( "_Radius" );
    private static readonly int Hardness = Shader.PropertyToID( "_Hardness" );
    private static readonly int Strength = Shader.PropertyToID( "_Strength" );
    private static readonly int Center = Shader.PropertyToID( "_Center" );
    
    private static readonly int FogOfWarLayer = 7;
    
    

    protected override void OnCreate()
    {

        _revealMat = new Material( Shader.Find( "Universal Render Pipeline/Custom/DotsRevealShader" ) );
        _stencilMat =  new Material( Shader.Find( "Universal Render Pipeline/Custom/DotsStencilMask" ) );
        //_stencilMat =  new Material( Shader.Find( "Universal Render Pipeline/Unlit" ) );
        
        RequireForUpdate<InitializeTag>();

    }
    protected override void OnUpdate()
    {
        
        Entities.WithStructuralChanges().ForEach(
                (ref EyeComponent eye, ref InitializeTag init, in Entity entity, in LocalTransform transform, in LocalToWorld ltw ) =>
                {
                    InitializeRTEye( eye, ltw, entity );
                }
            )
            .Run();
        
        Entities.WithStructuralChanges().ForEach(
                (ref StencilEyeComponent eye, ref InitializeTag init, in Entity entity, in LocalTransform transform, in LocalToWorld ltw ) =>
                {
                    InitializeStencilEye( eye, ltw, entity );
                }
            )
            .Run();
        
    }

    private void InitializeRTEye(EyeComponent eye, LocalToWorld ltw, Entity entity)
    {
        // Load a mesh from assets
        Mesh eyeMesh = new Mesh();
        eyeMesh.MarkDynamic();
        eyeMesh.name = "Eye Stencil Mesh";

            
        // Create a RenderMeshDescription with the layer set to the Fog of War layer
        RenderMeshDescription renderMeshDescription = 
            new RenderMeshDescription(ShadowCastingMode.Off, false, MotionVectorGenerationMode.Camera, FogOfWarLayer);


        //create a new material with the eye's values
        //we want a unique material for each eye since they can all have different values,
        //and if we dont make a new material then the different eyes will interfere with each other
        Material newMat = new Material( _revealMat );
        
        
        newMat.SetFloat(Radius, eye.ViewDistance);
        newMat.SetFloat(Hardness, eye.Hardness);
        newMat.SetFloat(Strength, eye.Strength);
        newMat.SetVector(Center, new Vector4(ltw.Position.x,ltw.Position.y ));
        
        
        // Create a RenderMeshArray with the required mesh and material
        RenderMeshArray renderMeshArray = new RenderMeshArray(new[] { newMat  }, new[] { eyeMesh });

        // Create a MaterialMeshInfo instance which maps the first material and mesh from RenderMeshArray
        MaterialMeshInfo materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0);

        // Add the rendering Sharedcomponents using the helper class RenderMeshUtility
        RenderMeshUtility.AddComponents(entity, EntityManager, renderMeshDescription, renderMeshArray, materialMeshInfo);

        // Add RenderBounds
        EntityManager.AddComponentData(entity, new RenderBounds() { Value = { Center = eyeMesh.bounds.center, Extents = eyeMesh.bounds.extents } });
        
        //remove the Initialize tag from the eye so that this system will go back to waiting for a new eye
        //If you are wondering why i am not using an enableable component, disabled components still register
        //in the context of RequireForUpdate, so to make the system completely pause, I need to remove the component entirely
        EntityManager.RemoveComponent<InitializeTag>( entity );
    }

    private void InitializeStencilEye(StencilEyeComponent eye, LocalToWorld ltw, Entity entity)
    {
        // Load a mesh from assets
        Mesh eyeMesh = new Mesh();
        eyeMesh.MarkDynamic();
        eyeMesh.name = "Eye Stencil Mesh";

            
        // Create a RenderMeshDescription with the layer set to the Fog of War layer
        //RenderMeshDescription renderMeshDescription = new RenderMeshDescription(ShadowCastingMode.Off, false, MotionVectorGenerationMode.Camera, FogOfWarLayer);
        RenderMeshDescription renderMeshDescription = new RenderMeshDescription(ShadowCastingMode.Off, false);


        //create a new material of the stencil material
        //we want a unique material for each eye since they can all have different values,
        //and if we dont make a new material then the different eyes will interfere with each other
        Material newMat = new Material( _stencilMat );


        // Create a RenderMeshArray with the required mesh and material
        RenderMeshArray renderMeshArray = new RenderMeshArray(new[] { newMat  }, new[] { eyeMesh });

        // Create a MaterialMeshInfo instance which maps the first material and mesh from RenderMeshArray
        MaterialMeshInfo materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0);

        // Add the rendering Sharedcomponents using the helper class RenderMeshUtility
        RenderMeshUtility.AddComponents(entity, EntityManager, renderMeshDescription, renderMeshArray, materialMeshInfo);

        // Add RenderBounds
        EntityManager.AddComponentData(entity, new RenderBounds() { Value = { Center = eyeMesh.bounds.center, Extents = eyeMesh.bounds.extents } });
        
        //remove the Initialize tag from the eye so that this system will go back to waiting for a new eye
        //If you are wondering why i am not using an enableable component, disabled components still register
        //in the context of RequireForUpdate, so to make the system completely pause, I need to remove the component entirely
        EntityManager.RemoveComponent<InitializeTag>( entity );
    }
}
