using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ROIDS.GameObjects.Devices;
using Microsoft.Xna.Framework;
namespace ROIDS
{
    class PlayerProfile
    {
        public float Health { get; set; }
        public int RoidsToBlast { get; set; }
        public int RoidsToNotBlast { get; set; }
        public int CurrentGoodNumber { get; set; }
        
        private List<Sensor> _sensors;
        private List<Charge> _charges;

        public TimeSpan TimePlayed;

        public void AddSensor(Sensor sensor)
        {
            _sensors.Add(sensor);
        }

        public void AddCharge(Charge charge)
        {
            _charges.Add(charge);
        }

        public Sensor ReleaseSensor(int index)
        {
            if (index < 0 || index >= _sensors.Count)
                return null;
            var sensor = _sensors[index];
            _sensors.RemoveAt(index);
            return sensor;
        }
        public Charge ReleaseCharge(int index)
        {
            if (index < 0 || index >= _charges.Count)
                return null;

            var charge = _charges[index];
            _charges.RemoveAt(index);
            return charge;
        }

        public void UpdateTime(TimeSpan timeElapsed)
        {
            TimePlayed += timeElapsed;
        }

        public PlayerProfile(float health, int roidsToBlast, int roidsToNotBlast)
        {
            Health = health;
            RoidsToBlast = roidsToBlast;
            CurrentGoodNumber = roidsToNotBlast;
            RoidsToNotBlast = roidsToNotBlast;
            TimePlayed = new TimeSpan();

            _sensors = new List<Sensor>();
            _charges = new List<Charge>();
        }

        public int ChargesLeft { get { return _charges.Count; } }

        public int SensorsLeft { get { return _sensors.Count; } }
    }
}
