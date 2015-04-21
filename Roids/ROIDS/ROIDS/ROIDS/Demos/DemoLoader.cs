using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using GameCore;
using UICore;
using UICore.Controls;
using Utilities;

namespace ROIDS.Demos
{
    class DemoLoader : GameState
    {
        UIEngine _ui;
        Type[] _demos;
        Assembly _demoAssembly;

        public DemoLoader()
        {
            _demoAssembly = Assembly.GetExecutingAssembly();
        }
        public override void Load()
        {
            _ui = new UIEngine();

            _demos = GetAvailableDemos();
            string[] demoNames = new string[_demos.Length];
            for (int i = 0; i < _demos.Length; i++)
                demoNames[i] = _demos[i].Name;

            // Setup Frame
            var frame = new Frame();
            var listBox = new ListBox(Vector2.Zero,
                demoNames,
                (SpriteFont)Utilities.ContentRepository.Repository["BasicFont"],
                Color.Blue,
                frame);

            listBox.MaximimumElementsDisplayed = 20;
            listBox.SelectionMade += new ListBoxEventHandler(SelectionMade);


            frame.KeyUp += new KeyEventHandler(KeyUp);
            _ui.AddAndLoad(frame);
            // End Frame Setup

            Game currentGame = (Game)Utilities.Globals.Data["Game"];
            currentGame.IsMouseVisible = true;

            
        }

        private Type[] GetAvailableDemos()
        {
            return _demoAssembly.GetTypes().Where<Type>(
                x => Attribute.GetCustomAttribute(x, typeof(DemoAttribute)) != null).ToArray<Type>();
        }


        void KeyUp(Element sender, KeyEventArgs e)
        {
            if (e.InterestingKeys.Contains<Keys>(Keys.Escape))
                this.Exit();
        }

        void SelectionMade(ListBox sender, string selection)
        {
            var loadState = (GameState)Activator.CreateInstance(
                _demos[sender.SelectedItem]);
            GameEngine.Singleton.AddAndLoad(loadState);
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            _ui.Draw(time, spriteBatch);
        }

        public override void Update(GameTime time)
        {
            if (!_ui.Update(time))
                this.Exit();
        }
    }
}
