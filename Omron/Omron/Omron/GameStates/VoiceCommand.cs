using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Recognition;

using Omron.Framework;
using UnitsAndBuilduings;

namespace Omron.GameStates
{   
    class VoiceCommand
    {
        public VoiceCommand(World w, Faction f)
        {
            genVerbs();
            Grammar grammar = new Grammar(new GrammarBuilder(genChoices()));
            recognizer = new SpeechRecognizer();
            recognizer.LoadGrammar(grammar);
            recognizer.Enabled = true;
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

            world = w;
            fac = f;
        }

        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (Enabled)
            {
                int spInd = e.Result.Text.IndexOf(' ');
                if (spInd != -1)
                {
                    string verb = e.Result.Text.Substring(0, spInd);
                    string arg = e.Result.Text.Substring(spInd + 1);
                    coms[verb](arg);
                }
                else
                {
                    coms[e.Result.Text]("");
                }
            }
        }

        public bool Enabled { get; set; }

        List<string> units, buildings;
        delegate void SpeechActionDelegate(string arg);
        Dictionary<string, SpeechActionDelegate> coms;
        SpeechRecognizer recognizer;
        World world;
        Faction fac;

        List<Actor> currentActive;
        int curInd;

        public delegate void SelectDelegate(List<Actor> units);
        public event SelectDelegate NewUnitsSelected;

        void genVerbs()
        {
            units = new List<string>();
            buildings = new List<string>();
            foreach (string str in ResourceManager.Resources.Keys)
            {
                if (ResourceManager.Resources[str] is UnitTypeInfo)
                {
                    units.Add(str);
                }
                else if (ResourceManager.Resources[str] is BuilduingTypeInfo)
                {
                    buildings.Add(str);
                }
            }

            //generate coms
            coms = new Dictionary<string, SpeechActionDelegate>();
            coms.Add("Select", new SpeechActionDelegate(SelectUnit));
            coms.Add("Next", new SpeechActionDelegate(NextUnit));
        }

        Choices genChoices()
        {
            Choices chs = new Choices();
            foreach (string str in units.Union(buildings))
            {
                chs.Add("Select " + str);
                chs.Add("Select " + str + "s");
            }
            chs.Add("Next ");
            for (int i = 2; i < 100; i++)
            {
                chs.Add("Next " + i.ToString());
            }
            return chs;
        }

        void NextUnit(string arg)
        {
            if (currentActive != null)
            {
                if (arg.Length == 0)
                {
                    curInd = (curInd + 1) % currentActive.Count;
                    List<Actor> temp = new List<Actor>();
                    temp.Add(currentActive[curInd]);
                    NewUnitsSelected(temp);
                }
            }
        }

        void SelectUnit(string unit)
        {
            if (units.Contains(unit))
            {//select one unit
                List<Actor> sel = new List<Actor>();
                foreach (Actor act in world.GetActors().Where(act => act.Faction == fac))
                {
                    if (act.Type == unit)
                    {
                        sel.Add(act);
                    }
                }
                if (sel.Count == 0)
                {
                    //synthesizer.SpeakAsync("You don't have any " + unit);
                    return;
                }
                currentActive = sel;
                curInd = 0;
                List<Actor> temp = new List<Actor>();
                temp.Add(sel[0]);
                NewUnitsSelected(temp);
            }
            else if (buildings.Contains(unit))
            {//select one building
                List<Actor> sel = new List<Actor>();
                foreach (Actor act in world.GetActors().Where(act => act.Faction == fac))
                {
                    if (act.Type == unit)
                    {
                        sel.Add(act);
                    }
                }
                if (sel.Count == 0)
                    return;
                currentActive = sel;
                curInd = 0;
                List<Actor> temp = new List<Actor>();
                temp.Add(sel[0]);
                NewUnitsSelected(temp);
            }
            else
            {
                unit = unit.TrimEnd('s');
                if (units.Contains(unit))
                {//select all units
                    List<Actor> sel = new List<Actor>();
                    foreach (Actor act in world.GetActors().Where(act => act.Faction == fac))
                    {
                        if (act.Type == unit)
                        {
                            sel.Add(act);
                        }
                    }
                    currentActive = sel;
                    curInd = 0;
                    NewUnitsSelected(sel);
                }
                else if (buildings.Contains(unit))
                {//select all buildings
                    List<Actor> sel = new List<Actor>();
                    foreach (Actor act in world.GetActors().Where(act => act.Faction == fac))
                    {
                        if (act.Type == unit)
                        {
                            sel.Add(act);
                        }
                    }
                    currentActive = sel;
                    curInd = 0;
                    NewUnitsSelected(sel);
                }
            }
        }
    }
}
