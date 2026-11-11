#include "ShadowSystem.h"

#include <algorithm>

void ShadowSystem::Update(Board& board)
{
    Clear(board);
    auto fallingBlock = board.GetTypeInCell(Board::Falling);
    
    if (fallingBlock.empty())
    {
        return;
    }
    
    int max_y = 0;
    
    for (auto block : fallingBlock)
    {
        max_y = std::max(max_y,block.at(1));
    }

    int dy = 0;
    
    while (true)
    {
        bool canFall = true;

        for (const auto& block : fallingBlock)
        {
            int x = block[0];
            int y = block[1] + dy + 1;

            if (!board.IsInside(x, y) ||
                board.ReadAt(x, y) == Board::Fixed)
            {
                canFall = false;
                break;
            }
        }

        if (!canFall)
        {
            break;
        }

        ++dy;
    }
    for (const auto& block : fallingBlock)
    {
        int x = block[0];
        int y = block[1] + dy;

        if (board.ReadAt(x, y) == Board::Empty)
        {
            board.WriteAt(x, y) = Board::Shadow;
        }
    }
}

void ShadowSystem::Clear(Board& board)
{
    auto fallingBlock = board.GetTypeInCell(Board::Shadow);
    if (fallingBlock.empty())
    {
        return;
    }
    
    for (auto block : fallingBlock)
    {
        board.WriteAt(block.at(0),block.at(1)) = Board::Empty;
    }
}
