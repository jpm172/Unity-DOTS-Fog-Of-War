using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


//this system stores the players inputs to be used in PlayerMoveSystem
[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial class GetPlayerInputSystem : SystemBase
{
    private DemoInputActions _inputActions;
    private Camera _camera;
    private static float3 xy = new float3(1,1,0);


    protected override void OnCreate()
    {
        _inputActions = new DemoInputActions();
    }

    protected override void OnStartRunning()
    {
        _camera = Camera.main;
        _inputActions.Enable();
        _inputActions.DemoMap.Interact.performed += OnPlayerInteract;
    }

    protected override void OnUpdate()
    {
        Vector2 moveInput = _inputActions.DemoMap.PlayerMovement.ReadValue<Vector2>();
        float3 mousePosition = _camera.ScreenToWorldPoint( Input.mousePosition ) * xy;
        
        foreach (var playerInputs in SystemAPI.Query<RefRW<PlayerInputs>>())
        {
            playerInputs.ValueRW.MoveInput = moveInput;
            playerInputs.ValueRW.AimPosition = mousePosition;
        }

    }

    protected override void OnStopRunning()
    {
        _inputActions.DemoMap.Interact.performed -= OnPlayerInteract;
        _inputActions.Disable();
    }

    private void OnPlayerInteract(InputAction.CallbackContext context)
    {
        Debug.Log( "interact" );
    }
}
