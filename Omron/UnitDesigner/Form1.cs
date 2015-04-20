using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UnitsAndBuilduings;
using System.Xml.Serialization;
using System.IO;

namespace UnitDesigner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            UnitTypeInfo info = new UnitTypeInfo();
            XmlSerializer ser = new XmlSerializer(typeof(UnitTypeInfo));
            ser.Serialize(File.OpenWrite(@"ui.xml"), info);
        }

        private void labelDragDrop_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            AttackTypeInfo inf = new AttackTypeInfo();
            Serializer.Deserialize(File.ReadAllText(fileNames[0]), ref inf); 
            XmlSerializer ser = new XmlSerializer(typeof(UnitTypeInfo));
            UnitTypeInfo info = null;
            try
            {
                info = (UnitTypeInfo)ser.Deserialize(File.OpenRead(fileNames[0]));
            }
            catch
            {
                MessageBox.Show("This ain't a valid unit");
                return;
            }
            textBoxUName.Text = Path.GetFileNameWithoutExtension(fileNames[0]);
            textBoxUType.Text = info.Type;
        }

        

        private void labelDragDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;
        }
    }
}
