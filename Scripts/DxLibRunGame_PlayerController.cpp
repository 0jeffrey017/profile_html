#include "PlayerController.h"
#include "Input.h"
#include "DxLib.h"
#include "System60.h"

#include <algorithm>
#include <map>

PlayerController::PlayerController(const TCHAR* filename)
    : currentPosition({3 * 64, 8 * 64}), playerAnimation(filename, 24, 4, 6, 256, 256)
{
    playerAnimation.SetState(playerCurrentState);
}

PlayerController::~PlayerController() = default;

void PlayerController::PlayerInputUpdate(Map& map)
{
    const int direction = (Input::Input_GetKeyboard(KEY_INPUT_D) > 0 ? 1 : 0)
        - (Input::Input_GetKeyboard(KEY_INPUT_A) > 0 ? 1 : 0);
    velocity[0] = direction * speed;
    if (direction != 0)
    {
        isFaceRight = direction > 0;
    }

    isGrounded = velocity[1] >= 0.0
        && CollidedGround(map, currentPosition[0], currentPosition[1] + 0.01);
    if (isGrounded && Input::Input_GetKeyboardDown(KEY_INPUT_SPACE))
    {
        velocity[1] = -jumpSpeed;
        isGrounded = false;
    }
}

void PlayerController::Update()
{
    if (!isGrounded)
    {
        playerCurrentState = isFaceRight ? JumpRight : JumpLeft;
    }
    else if (velocity[0] != 0.0)
    {
        playerCurrentState = isFaceRight ? MoveRight : MoveLeft;
    }
    else
    {
        playerCurrentState = isFaceRight ? IdleRight : IdleLeft;
    }
    playerAnimation.SetState(playerCurrentState);
    playerAnimation.Update(static_cast<int>(std::round(currentPosition[0])),
                           static_cast<int>(std::round(currentPosition[1])));
}

bool PlayerController::CollidedGround(Map& map, double x, double y) const
{
    const int left   = static_cast<int>(std::floor(x));
    const int right  = static_cast<int>(std::floor(x + playerSize - 0.0001));
    const int top    = static_cast<int>(std::floor(y));
    const int bottom = static_cast<int>(std::floor(y + playerSize - 0.0001));
    return map.CheckIsGround(left, top) || map.CheckIsGround(right, top)
        || map.CheckIsGround(left, bottom) || map.CheckIsGround(right, bottom);
}

bool PlayerController::CollidedTrap(Map& map, double x, double y) const
{
    const int left   = static_cast<int>(std::floor(x));
    const int right  = static_cast<int>(std::floor(x + playerSize - 0.0001));
    const int top    = static_cast<int>(std::floor(y));
    const int bottom = static_cast<int>(std::floor(y + playerSize - 0.0001));
    return map.CheckIsTrap(left, top) || map.CheckIsTrap(right, top)
        || map.CheckIsTrap(left, bottom) || map.CheckIsTrap(right, bottom);
}

void PlayerController::MoveAxis(Map& map, int axis, double distance)
{
    if (distance == 0.0)
    {
        return;
    }
    const int steps = (std::max)(1, static_cast<int>(std::ceil(std::abs(distance))));
    const double step = distance / steps;
    for (int i = 0; i < steps; ++i)
    {
        auto nextPosition = currentPosition;
        nextPosition[axis] += step;
        if (CollidedGround(map, nextPosition[0], nextPosition[1]))
        {
            // Tile boundaries and the collider size are integer pixel values.
            currentPosition[axis] = step > 0.0
                ? std::ceil(nextPosition[axis]) - 1.0
                : std::floor(nextPosition[axis]) + 1.0;
            velocity[axis] = 0.0;
            break;
        }
        currentPosition = nextPosition;
    }
}

int PlayerController::FixedUpdate(Map& map)
{
    const double deltaTime = System60::deltaTime;
    velocity[1] += gravity * deltaTime;
    MoveAxis(map, 0, velocity[0] * deltaTime);
    MoveAxis(map, 1, velocity[1] * deltaTime);
    if (CollidedTrap(map, currentPosition[0], currentPosition[1]))
    {
        return -1;
    }
    isGrounded = velocity[1] >= 0.0
        && CollidedGround(map, currentPosition[0], currentPosition[1] + 0.01);
    return 0;
}
