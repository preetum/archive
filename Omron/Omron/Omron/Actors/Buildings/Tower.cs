using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitsAndBuilduings;
using Microsoft.Xna.Framework;

using Omron.Framework;
using UnitsAndBuilduings;
using Omron.AI;

namespace Omron.Actors
{
    class Tower : FatherBuilding
    {
        public Tower(BuilduingTypeInfo info, string type, Vector2 pos)
            : base(info, type, pos)
        {
            string[] coms = new string[info.Attacks.Length + info.SpawnAttacks.Length];
            

            meleAtks = new ArealAttack[info.Attacks.Length];
            int count = 0;
            foreach (AttackTypeInfo atk in info.Attacks)
            {
                coms[count] = "Mele " + count;
                meleAtks[count] = new ArealAttack(atk, this);
                count++;
            }

            spawnAtks = new SpawnAttack[info.SpawnAttacks.Length];
            count = 0;
            foreach (SpawnAttackTypeInfo atk in info.SpawnAttacks)
            {
                coms[count + meleAtks.Length] = atk.SpawnActor + "-" + atk.Number;
                spawnAtks[count] = new SpawnAttack(atk, this);
                count++;
            }

            menu = new ActorMenu(this, coms);
            menu.CommandInvoked += new MenuItemPressed(menu_CommandInvoked);
            curSpawn = 0;
            curMele = 0;
            updateMenuInfo();

            AI = new TowerAI(this);
        }

        public override ActorMenu Menu
        {
            get
            {
                if (IsComplete)
                    return menu;
                return base.Menu;
            }
        }

        public override ArealAttack MeleeAttack
        {
            get
            {
                return meleAtks[curMele];
            }
        }

        public override SpawnAttack RangedAttack
        {
            get
            {
                return spawnAtks[curSpawn];
            }
        }

        public override void IssueMeleeAttack(Vector2 loc)
        {
            if (MeleeAttack != null)
            {
                ArealAttack atk = MeleeAttack.GetInstance(loc);
                stage.InflictAttack(atk);
            }
        }
        public override void IssueRangedAttack(Vector2 loc)
        {
            if (RangedAttack != null)
            {
                int NUM = RangedAttack.Number; //number of arrows to fire
                float span = this.MaxRadius; //span arrow which to distribute arrows (perpendicular to the direction to the target -- essentially space out the arrows across this span)

                Vector2 targDir = Vector2.Normalize(loc - this.Position);
                Vector2 launchPos = this.Position + 0.5f * this.MaxRadius * targDir;

                if (NUM == 1)
                {
                    Actor shot = UnitConverter.CreateActor(RangedAttack.Type, launchPos, this.Faction);
                    ((FatherUnit)shot).Track(loc);
                    stage.AddActor(shot);
                }
                else
                {
                    for (int i = 0; i < NUM; i++)
                    {

                        float x = (float)i * (span / (NUM - 1)) - span / 2f;

                        Vector2 perpDisp = x * MathHelper.Perpen(targDir);

                        Actor shot = UnitConverter.CreateActor(RangedAttack.Type, launchPos + perpDisp, this.Faction);
                        ((FatherUnit)shot).Track(loc);
                        stage.AddActor(shot);
                    }
                }

            }
        }

        void menu_CommandInvoked(int itemKey)
        {
            if (itemKey < meleAtks.Length + spawnAtks.Length)//error checking
            {
                if (itemKey < meleAtks.Length)
                {//set mele attack
                    curMele = itemKey;
                }
                else
                {//set spawn attack
                    curSpawn = itemKey - meleAtks.Length;
                }
                updateMenuInfo();
            }
        }

        

        int curMele, curSpawn;
        ArealAttack[] meleAtks;
        SpawnAttack[] spawnAtks;

        void updateMenuInfo()
        {
            menu.Info = "";
            if (meleAtks.Length > 0)
                menu.Info = "Current mele attack: attack " + curMele;
            if (spawnAtks.Length > 0)
                menu.Info += "\nCurrent spawn attack: " + spawnAtks[curSpawn].Type + "-" + spawnAtks[curSpawn].Number;
        }

        ActorMenu menu;

        public override void UpdateFast(GameTime gameTime)
        {
            base.UpdateFast(gameTime);
        }
    }
}


