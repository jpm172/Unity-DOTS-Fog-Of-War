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
//Since registering meshes/materials is a structural change, we only want to do this once per eye
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class InitializeEyeSystem : SystemBase
{

    private Material _stencilMat;
    private Material _debugMat;

    protected override void OnCreate()
    {//

        //_stencilMat = new Material( Shader.Find( "Universal Render Pipeline/Custom/DotsCutOutFade" ) );
        _debugMat = new Material(  Shader.Find( "Universal Render Pipeline/Unlit" ) );
        RequireForUpdate<InitializeTag>();
        
    }
    protected override void OnUpdate()
    {
        
        Entities.WithStructuralChanges().ForEach(
                (ref EyeComponent eye, ref InitializeTag init, in Entity entity, in LocalTransform transform, in LocalToWorld ltw ) =>
                {
                    
                    // Load a mesh from assets
                    Mesh eyeMesh = new Mesh();
                    eyeMesh.MarkDynamic();
                    eyeMesh.name = "Eye Stencil Mesh";

                        
                    // Create a RenderMeshDescription with the layer set to the Fog of War layer
                    RenderMeshDescription renderMeshDescription = new RenderMeshDescription(ShadowCastingMode.Off, false, MotionVectorGenerationMode.Camera, 7);
                    //RenderMeshDescription renderMeshDescription = new RenderMeshDescription(ShadowCastingMode.Off, false);


                    //create a new material with the eye's values
                    //we want a unique material for each eye since they can all have different values,
                    //and if we dont make a new material then the different eyes will interfere with each other
                    Material newMat = new Material( _debugMat );
                    
                    /*
                    newMat.SetFloat("_Radius", eye.ViewDistance);
                    newMat.SetFloat("_Hardness", eye.Hardness);
                    newMat.SetFloat("_Strength", eye.Strength);
                    newMat.SetVector("_Center", new Vector4(ltw.Position.x,ltw.Position.y ));
                    */
                    
                    // Create a RenderMeshArray with the required mesh and material
                    RenderMeshArray renderMeshArray = new RenderMeshArray(new[] { newMat  }, new[] { eyeMesh });

                    // Create a MaterialMeshInfo instance which maps the first material and mesh from RenderMeshArray
                    MaterialMeshInfo materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0);

                    // Add the rendering Sharedcomponents using the helper class RenderMeshUtility
                    RenderMeshUtility.AddComponents(entity, EntityManager, renderMeshDescription, renderMeshArray, materialMeshInfo);

                    // Add RenderBounds
                    EntityManager.AddComponentData(entity, new RenderBounds() { Value = { Center = eyeMesh.bounds.center, Extents = eyeMesh.bounds.extents } });
                    
                    //remove the Initialize tag from the eye so that this system will go back to waiting for a new eye
                    EntityManager.RemoveComponent<InitializeTag>( entity );
                    
                }
            )
            .Run();
    }
}
