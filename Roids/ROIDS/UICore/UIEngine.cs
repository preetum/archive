using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace UICore
{
    public class UIEngine
    {
        List<Frame> _frames;
        public Frame ActiveFrame
        {
            get
            {
                return (_frames.Count > 0) ? _frames[_frames.Count - 1] : null;
            }
        }

        public UIEngine()
        {
            _frames = new List<Frame>();
        }

        public void AddFrame(Frame frame)
        {
            if (ActiveFrame != null)
                ActiveFrame.OnLoseControl(ActiveFrame);
            _frames.Add(frame);
            frame.BindToGUIEngine(this);
            ActiveFrame.OnGainControl(ActiveFrame);
        }
        public void AddAndLoad(Frame frame)
        {
            if (ActiveFrame != null)
               ActiveFrame.OnLoseControl(ActiveFrame);
            _frames.Add(frame);
            frame.BindToGUIEngine(this);
            frame.Load();
            ActiveFrame.OnGainControl(ActiveFrame);
        }

        public bool Update(GameTime time)
        {
            for (int i = _frames.Count - 1; i >= 0; i--)
            {
                if (!ActiveFrame.PleaseDestroy)
                    break;

                ActiveFrame.Destroy();
                _frames.RemoveAt(_frames.Count - 1);

                if (ActiveFrame != null && !ActiveFrame.PleaseDestroy) // This is a new active frame
                    ActiveFrame.OnGainControl(ActiveFrame);                
            }

            if (_frames.Count > 0)
                ActiveFrame.Update(time);
            else
                return false;
            return true;
        }
        public bool Draw(GameTime time, SpriteBatch spriteBatch)
        {
            if (_frames.Count > 0)
            {
                var startDrawingAt = _frames.Count - 1;
                for (int i = startDrawingAt; i >= 0; i--)
                {
                    if (!_frames[i].Transparent)
                    {
                        startDrawingAt = i;
                        break;
                    }
                }
                for (int i = startDrawingAt; i < _frames.Count; i++)
                    _frames[i].Draw(time, spriteBatch);
            }
            else
                return false;
            return true;
        }

        public void Destroy()
        {
            _frames.ForEach(x => x.Destroy());
        }
    }

}
