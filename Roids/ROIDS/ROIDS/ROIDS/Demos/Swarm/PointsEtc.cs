using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROIDS.Demos.Swarm
{
    class PointsEtc
    {
        public int Points { get { return _points; } set { _points = value; Console.WriteLine("Points: " + _points); } }
        public int Health { get { return _health; } set { _health = value; Console.WriteLine("Health: " + _health); } }

        public int _points;
        public int _health;

        public PointsEtc(int points, int health)
        {
            Points = points;
            Health = health;
        }
    }
}
