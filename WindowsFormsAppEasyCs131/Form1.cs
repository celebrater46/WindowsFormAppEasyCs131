using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsAppEasyCs131
{
    public partial class Form1 : Form
    {
        private MenuStrip ms;
        private ToolStripMenuItem[] mi = new ToolStripMenuItem[11];
        private List<Shape> shapeList;
        private int currentShape;
        private Color currentColor;

        private static PrintDocument pd;
        
        [STAThread]
        public Form1()
        {
            InitializeComponent();
            this.Text = "Happy Drawing!";
            this.Width = 600;
            this.Height = 400;

            ms = new MenuStrip();
            mi[0] = new ToolStripMenuItem("ファイル");
            mi[1] = new ToolStripMenuItem("設定");
            mi[2] = new ToolStripMenuItem("図形");
            mi[3] = new ToolStripMenuItem("開く");
            mi[4] = new ToolStripMenuItem("保存");
            mi[5] = new ToolStripMenuItem("印刷プレビュー");
            mi[6] = new ToolStripMenuItem("印刷");
            mi[7] = new ToolStripMenuItem("四角形");
            mi[8] = new ToolStripMenuItem("楕円");
            mi[9] = new ToolStripMenuItem("直線");
            mi[10] = new ToolStripMenuItem("色");

            mi[0].DropDownItems.Add(mi[3]);
            mi[0].DropDownItems.Add(mi[4]);
            mi[0].DropDownItems.Add(new ToolStripSeparator());
            mi[0].DropDownItems.Add(mi[5]);
            mi[0].DropDownItems.Add(mi[6]);
            
            mi[1].DropDownItems.Add(mi[2]);
            mi[1].DropDownItems.Add(mi[10]);
            
            mi[2].DropDownItems.Add(mi[7]);
            mi[2].DropDownItems.Add(mi[8]);
            mi[2].DropDownItems.Add(mi[9]);

            ms.Items.Add(mi[0]);
            ms.Items.Add(mi[1]);

            this.MainMenuStrip = ms;
            ms.Parent = this;

            pd = new PrintDocument();
            shapeList = new List<Shape>();
            currentShape = Shape.RECT;
            currentColor = Color.Blue;

            for (int i = 0; i < mi.Length; i++)
            {
                mi[i].Click += new EventHandler(MiClick);
            }

            this.MouseDown += new MouseEventHandler(FmMouseDown);
            this.MouseUp += new MouseEventHandler(FmMouseUp);
            this.Paint += new PaintEventHandler(FmPaint);
            pd.PrintPage += new PrintPageEventHandler(FmPaint);
        }

        public void MiClick(Object sender, EventArgs e)
        {
            if (sender == mi[3])
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "画像ファイル | *.g";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                    shapeList.Clear();
                    shapeList = (List<Shape>)bf.Deserialize(fs);
                    fs.Close();
                    this.Invalidate();
                }
            }
            else if (sender == mi[4])
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "画像ファイル | *.g";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate, FileAccess.Write);
                    bf.Serialize(fs, shapeList);
                    fs.Close();
                }
            }
            else if (sender == mi[5])
            {
                PrintPreviewDialog pp = new PrintPreviewDialog();
                pp.Document = pd;
                pp.ShowDialog();
            }
            else if (sender == mi[6])
            {
                pd.Print();
            }
            else if (sender == mi[7])
            {
                currentShape = Shape.RECT;
            }
            else if (sender == mi[8])
            {
                currentShape = Shape.OVAL;
            }
            else if (sender == mi[9])
            {
                currentShape = Shape.LINE;
            }
            else if (sender == mi[10])
            {
                ColorDialog cd = new ColorDialog();
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    currentColor = cd.Color;
                }
            }
        }

        public void FmMouseDown(Object sender, MouseEventArgs e)
        {
            // Create new shape object
            Shape sh;
            if (currentShape == Shape.RECT)
            {
                sh = new Rect();
            }
            else if (currentShape == Shape.OVAL)
            {
                sh = new Oval();
            }
            else
            {
                sh = new Line();
            }
            
            // Set the shape object's color
            sh.SetColor(currentColor);
            
            // Set the shape object's location
            sh.SetStartPoint(e.X, e.Y);
            sh.SetEndPoint(e.X, e.Y);
            
            shapeList.Add(sh);
        }

        public void FmMouseUp(Object sender, MouseEventArgs e)
        {
            // Get the shape object from the list
            Shape sh = (Shape)(shapeList[shapeList.Count - 1] as Shape);
            sh.SetEndPoint(e.X, e.Y);
            
            // Redraw the form
            this.Invalidate();
        }

        public void FmPaint(Object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (Shape sh in shapeList)
            {
                sh.Draw(g);
            }
        }

        public void PdPrintPage(Object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (Shape sh in shapeList)
            {
                sh.Draw(g);
            }
        }
    }

    [Serializable]
    abstract class Shape
    {
        public static int RECT = 0;
        public static int OVAL = 1;
        public static int LINE = 2;
        protected int x1, y1, x2, y2;
        protected Color c;

        abstract public void Draw(Graphics g);

        public void SetColor(Color c)
        {
            this.c = c;
        }

        public void SetStartPoint(int x, int y)
        {
            x1 = x;
            y1 = y;
        }

        public void SetEndPoint(int x, int y)
        {
            x2 = x;
            y2 = y;
        }
    }

    [Serializable]
    class Rect : Shape
    {
        override public void Draw(Graphics g)
        {
            SolidBrush sb = new SolidBrush(c);
            g.FillRectangle(sb, x1, y1, x2-x1, x2-y1);
        }
    }

    [Serializable]
    class Oval : Shape
    {
        override public void Draw(Graphics g)
        {
            SolidBrush sb = new SolidBrush(c);
            g.FillEllipse(sb, x1, y1, x2-x1, y2-y1);
        }
    }

    [Serializable]
    class Line : Shape
    {
        override public void Draw(Graphics g)
        {
            SolidBrush sb = new SolidBrush(c);
            Pen p = new Pen(sb);
            g.DrawLine(p, x1, y1, x2, y2);
        }
    }
}