using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Net;

namespace Wallboard
{
    public partial class Wallboard : Form
    {
        static TeamFoundationServer teamFoundationServer = new TeamFoundationServer(new Uri("http://tfs.interrobanginteractive.com:8080/tfs/Interrobang Interactive"), new NetworkCredential("tylermenezes", "Neodymium!"));
        static string projectName = "ROIDS";

        public Wallboard()
        {
            InitializeComponent();
            teamFoundationServer.Authenticate();

            VersionControlServer s = (VersionControlServer)teamFoundationServer.GetService(typeof(VersionControlServer));
        }

        private void Wallboard_Load(object sender, EventArgs e)
        {
            
        }

        private void updateBars()
        {
            WorkItemStore wis = (WorkItemStore)teamFoundationServer.GetService(typeof(WorkItemStore));
            Project tfsProject = wis.Projects[projectName];

            WorkItemCollection wic = wis.Query(
              " SELECT [System.Id], [System.WorkItemType]," +
              " [System.State], [System.AssignedTo], [System.Title] " +
              " FROM WorkItems " +
              " WHERE [System.TeamProject] = '" + tfsProject.Name +
              "' ORDER BY [System.WorkItemType], [System.Id]");

            foreach (WorkItem wi in wic)
            {
                Console.WriteLine(wi.Title + "[" + wi.Type.Name + "]" + wi.Description);
            }
        }
    }
}
