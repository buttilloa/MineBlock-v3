﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace MineBlock.Blocks
{
    class CobbleStone : Block
    {
        public CobbleStone(int XPos, int yPos)
        {
            x = XPos;
            y = yPos;
            index = 16;
        }
        public override Block Place(int x, int y)
        {

            return new CobbleStone(x, y);
        }

        public override Block Mine(int x, int y)
        {

            return new CobbleStone(x, y);
        }
        public override Block Reset(int X, int Y)
        {
            return new CobbleStone(X, Y);
        }
    }
}
