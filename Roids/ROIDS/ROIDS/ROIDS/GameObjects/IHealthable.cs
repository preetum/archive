using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROIDS.GameObjects
{
    public interface IHealthable
    {
        float MaxHealth { get; set; }
        float CurrentHealth { get; set; }

        void Hurt(float damage);
    }
}
