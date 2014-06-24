using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSLab3
{
    public partial class Form1 : Form
    {
        private String path;
        private List<String> solutions;
        private List<String> sources;
        private String projectName;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxPath.Text = folderBrowserDialog1.SelectedPath;
                buttonLoad.Enabled = true;
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            textBoxConsole.Visible = true;
            Height = 425;
            path = textBoxPath.Text;
            solutions = new List<String>();
            sources = new List<String>();
            Directory.CreateDirectory(path + "\\Sources");

            textBoxConsole.Text += "Copying:" + "\r\n";

            foreach (var file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == ".sln")
                {
                    solutions.Add(file);
                    textBoxConsole.Text += Path.GetFileName(file) + "\r\n";
                    File.Copy(path + "\\" + Path.GetFileName(file), path + "\\Sources\\" + Path.GetFileName(file), true);
                }
            }

            char[] trimChar = { '\"', ',' };

            foreach (var sln in solutions)
            {
                String[] fileTextLines = File.ReadAllLines(sln);
                List<String[]> shreddedString = new List<String[]>();

                foreach (var line in fileTextLines)
                {
                    if (line.Contains("Project(\""))
                    {
                        shreddedString.Add(line.Split(' '));
                    }
                }

                projectName = shreddedString[0][2].Trim(trimChar);

                foreach (var proj in shreddedString)
                {
                    File.Copy(path + "\\" + proj[3].Trim(trimChar), path + "\\Sources\\" + Path.GetFileName(proj[3].Trim(trimChar)), true);
                    textBoxConsole.Text += proj[3].Trim(trimChar) + "\r\n";
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path + "\\" + proj[3].Trim(trimChar));
                    var nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("Project", doc.DocumentElement.NamespaceURI);
                    XmlNodeList itemCompileNodes = doc.SelectNodes("//Project:Compile", nsmgr);

                    foreach (XmlNode node in itemCompileNodes)
                    {
                        sources.Add(node.Attributes["Include"].Value);
                        textBoxConsole.Text += node.Attributes["Include"].Value + "\r\n";
                    }

                    /*XmlNodeList noneNodes = doc.SelectNodes("//Project:None", nsmgr);
                    textBoxConsole.Text += noneNodes[1].Attributes["Include"].Value + "\r\n";
                    sources.Add(noneNodes[1].Attributes["Include"].Value);*/
                }

                foreach (var file in sources)
                {
                    File.Copy(path + "\\" + projectName + "\\" + file, path + "\\Sources\\" + Path.GetFileName(file), true);
                }

                textBoxConsole.Text += "\r\n";
            }

            ZipFile.CreateFromDirectory(path + "\\Sources", path + "\\Sources.zip");
        }
    }
}
