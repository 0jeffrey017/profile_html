#include "PlayerMoveSystem.h"

#include <DxLib.h>
#include <cassert>
#include <iterator>

#include "Map.h"
#include "Input.h"
#include "MoveResult.h"

void PlayerMoveSystem::InputUpdate()
{
    if (Input::Input_GetKeyboardDown(KEY_INPUT_W) && direction != std::array{0,1})
    {
        directionBuffer.at(0) = 0;
        directionBuffer.at(1) = -1;
    }
    if (Input::Input_GetKeyboardDown(KEY_INPUT_S) && direction != std::array{0,-1})
    {
        directionBuffer.at(0) = 0;
        directionBuffer.at(1) = 1;
    }
    if (Input::Input_GetKeyboardDown(KEY_INPUT_D) && direction != std::array{-1,0})
    {
        directionBuffer.at(0) = 1;
        directionBuffer.at(1) = 0;
    }
    if (Input::Input_GetKeyboardDown(KEY_INPUT_A) && direction != std::array{1,0})
    {
        directionBuffer.at(0) = -1;
        directionBuffer.at(1) = 0;
    }
}

MoveResult PlayerMoveSystem::Update(Map& map)
{
    auto& board = map.mapArray;
    auto& directions = map.directionArray;
    auto& turnArrays = map.turnAngleArray;
    const auto isInside = [&](long long x, long long y)
    {
        return y >= 0 && y < std::ssize(board) && y < std::ssize(directions)
            && x >= 0 && x < std::ssize(board[y]) && x < std::ssize(directions[y]);
    };

    const auto [headX, headY] = map.GetChar('h');
    const auto [tailX, tailY] = map.GetChar('t');
    if (!isInside(headX, headY) || !isInside(tailX, tailY))
    {
        return MoveResult::InvalidMap;
    }

    const int nextHeadX = headX + directionBuffer[0];
    const int nextHeadY = headY + directionBuffer[1];
    if (!isInside(nextHeadX, nextHeadY))
    {
        return MoveResult::HitWall;
    }

    const char target = board[nextHeadY][nextHeadX];
    if (target == 'b')
    {
        return MoveResult::HitBody;
    }

    const bool ateFood = target == 'f';
    if (!ateFood)
    {
        const auto [dx, dy] = directions[tailY][tailX];
        const auto nextTailX = static_cast<long long>(tailX) + dx;
        const auto nextTailY = static_cast<long long>(tailY) + dy;
        if (!isInside(nextTailX, nextTailY) || board[nextTailY][nextTailX] != 'b')
        {
            return MoveResult::InvalidMap;
        }

        board[tailY][tailX] = '.';
        board[nextTailY][nextTailX] = 't';
    }

    turnArrays[headY][headX] = {
        directionBuffer[0] - direction[0],
        directionBuffer[1] - direction[1]
    };
    direction = directionBuffer;
    directions[headY][headX] = direction;
    directions[nextHeadY][nextHeadX] = direction;
    board[headY][headX] = 'b';
    assert(board[nextHeadY][nextHeadX] == '.' || board[nextHeadY][nextHeadX] == 'f');
    board[nextHeadY][nextHeadX] = 'h';
    return ateFood ? MoveResult::AteFood : MoveResult::Moved;
}
