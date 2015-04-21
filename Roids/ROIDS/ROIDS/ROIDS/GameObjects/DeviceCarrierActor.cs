using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WorldCore;
using PhysicsCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;

namespace ROIDS.GameObjects
{
    public abstract class DeviceCarrierActor : Actor
    {
        List<Device> _devices { get; set; }

        public DeviceCarrierActor(Vector2 pos, float rot)
            : base(pos, rot)
        {
            _devices = new List<Device>();
        }

        public virtual void AddDevice(Device d)
        {
            d.Parent = this;
            _devices.Add(d);
        }
        public virtual void AddDeviceRange(ICollection<Device> ds)
        {
            foreach (var d in ds) this.AddDevice(d);
        }
        public void ClearDevices()
        {
            _devices.Clear();
        }
        public ReadOnlyCollection<Device> GetDevices()
        {
            return _devices.AsReadOnly();
        }

        public override void Destroy()
        {
            _devices.ForEach(d => d.Destroy());

            base.Destroy();
        }
    }
}
