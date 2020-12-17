using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
public class PlayerMovementSystem : SystemBase
{
    PlayerInputActions PlayerInput;

    PlayerInputActions.PlayerActions pInput;

    protected override void OnCreate()
    {
        //Create player entity

        PlayerInput = new PlayerInputActions();
        PlayerInput.Enable();
        pInput = PlayerInput.Player;
    }

    protected override void OnUpdate()
    {
        float horizontalInput = pInput.MoveHorizontal.ReadValue<float>();
        float verticalInput = pInput.MoveVertical.ReadValue<float>();
        Dependency =  Entities.ForEach((ref Velocity velocity, in Player player) => {
            float2 value = float2.zero;
            value.x += horizontalInput;
            value.y += verticalInput;
            velocity.value = value;
        }).ScheduleParallel(Dependency);

        float dt = Time.DeltaTime;
        Dependency = Entities.ForEach((ref Translation pos, in Velocity velocity, in Speed speed) =>
        {
            pos.Value.xy += velocity.value * dt * speed.value;
        }).ScheduleParallel(Dependency);
    }
}