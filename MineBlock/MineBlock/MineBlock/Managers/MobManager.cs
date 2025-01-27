﻿using MineBlock.Mobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MineBlock
{
    public class MobManager
    {
        public List<Mob> mobs = new List<Mob>();
        public Hoverbot bot;
        public MobManager()
        {
            addBot();
        }

        public void AddMob(Mob mob)
        {
            mobs.Add(mob);

        }
        public void addBot()
        {
            bot = new Hoverbot();
        }
        public void RemoveMobs()
        {
            mobs.Clear();
        }
        public void update(Microsoft.Xna.Framework.GameTime time)
        {
            foreach (Mob mob in mobs)
                mob.update(time);
            bot.update(time);

        }
        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
        {
            foreach (Mob mob in mobs)
                mob.Draw(batch);
            bot.draw(batch);
        }
    }
}
