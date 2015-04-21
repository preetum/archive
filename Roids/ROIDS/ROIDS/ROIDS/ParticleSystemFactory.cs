using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using PhysicsCore;
using Utilities;

namespace ROIDS
{
    public static class ParticleSystemFactory
    {
        public static ParticleSystem[] GetDirtyBomb(Vector2 pos)
        {
            var pfire = new ParticleSystem(pos, 20, 25, 1, 50, 1, 3f, ContentRepository.Repository["fire"]);
            pfire.InitVelocities(0, 200);

            var psmoke = new ParticleSystem(pos, 20, 10, 2f, 100, 1, 1f, ContentRepository.Repository["smoke"]);
            psmoke.InitVelocities(0, 300);

            return new ParticleSystem[] { pfire, psmoke };
        }

        /*public static ParticleSystem[] GetDirtyBomb(Vector2 pos, float rad)
        {
            var pfire = new ParticleSystem(pos, rad * 2 / 5, 25, 1, rad, 1, 3f, ContentRepository.Repository["fire"]);
            pfire.InitVelocities(0, rad*3);

            var psmoke = new ParticleSystem(pos, rad * 2 / 5, 10, 2f, rad * 2, 1, 1f, ContentRepository.Repository["smoke"]);
            psmoke.InitVelocities(0, rad*5);

            return new ParticleSystem[] { pfire, psmoke };
        }*/
        public static ParticleSystem[] GetDirtyBomb(Vector2 pos, float rad)
        {
            var pfire = new ParticleSystem(pos, rad * 2 / 5, 25, 1, rad, 1, 3f, ContentRepository.Repository["fire"]);
            pfire.InitVelocities(0, rad * 3);

            var psmoke = new ParticleSystem(pos, rad * 2 / 5, 10, 2f, rad * 2, 1, 1f, ContentRepository.Repository["smoke"]);
            psmoke.InitVelocities(0, rad * 3);

            return new ParticleSystem[] { pfire, psmoke };
        }
    }
}
