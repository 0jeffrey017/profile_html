#include "BlockMovement.h"
#include <DxLib.h>

void BlockMovement::Update(Board& board)
{
    KeyBoard.Update();

    if (KeyBoard.Input_GetKeyboardDown(KEY_INPUT_SPACE))
    {
        TurnClockWise(board);
    }
    else if (KeyBoard.Input_GetKeyboardDown(KEY_INPUT_D))
    {
        Move(board, 1, 0);
    }
    else if (KeyBoard.Input_GetKeyboardDown(KEY_INPUT_A))
    {
        Move(board, -1, 0);
    }
    else if (KeyBoard.Input_GetKeyboardDown(KEY_INPUT_S) || KeyBoard.Input_GetKeyboard(KEY_INPUT_S) >= 60)
    {
        MoveDown(board);
    }
        
}

void BlockMovement::SetPivot(int x, int y, bool isO)
{
    this->pivotX = x;
    this->pivotY = y;
    this->isBlockO = isO;
}

std::array<int, 2> BlockMovement::GetPivot() const
{
    return {pivotX, pivotY};
}

bool BlockMovement::UpdateBlockOnBoardDown(Board& board)
{
    return MoveDown(board);
}

bool BlockMovement::TurnClockWise(Board& board)
{
    if (isBlockO)
    {
        return false;
    }
    const auto before = board.GetTypeInCell(Board::Falling);
    if (before.empty())
    {
        return false;
    }

    std::vector<std::array<int, 2>> after;
    for (const auto& pos : before)
    {
        int x = pivotX - (pos[1] - pivotY);
        int y = pivotY + (pos[0] - pivotX);
        if (!board.IsInside(x, y) || board.ReadAt(x, y) == Board::Fixed)
        {
            return false;
        }
        after.push_back({x, y});
    }

    for (const auto& pos : before)
    {
        board.WriteAt(pos[0], pos[1]) = Board::Empty;
    }
    for (const auto& pos : after)
    {
        board.WriteAt(pos[0], pos[1]) = Board::Falling;
    }
    return true;
}

bool BlockMovement::Move(Board& board, int dx, int dy)
{
    const auto positions = board.GetTypeInCell(Board::Falling);
    if (positions.empty())
    {
        return false;
    }

    for (const auto& pos : positions)
    {
        int x = pos[0] + dx;
        int y = pos[1] + dy;
        if (!board.IsInside(x, y) || board.ReadAt(x, y) == Board::Fixed)
        {
            return false;
        }
    }

    for (const auto& pos : positions)
    {
        board.WriteAt(pos[0], pos[1]) = Board::Empty;
    }
    for (const auto& pos : positions)
    {
        board.WriteAt(pos[0] + dx, pos[1] + dy) = Board::Falling;
    }

    if (!isBlockO){
        pivotX += dx;
        pivotY += dy;
    }
    return true;
}

bool BlockMovement::MoveDown(Board& board)
{
    if (Move(board, 0, 1))
    {
        return true;
    }
    board.FixFallingBlock();
    return false;
}
