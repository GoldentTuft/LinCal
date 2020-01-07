using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LinCal
{
    public partial class Form1 : Form
    {
        Point mouseDownPoint;
        ContextMenu menu1 = new ContextMenu();
        const int yMargin = 4;

        public Form1()
        {
            InitializeComponent();
            sikiGrid1.SetMinimumSize();
            this.MinimumSize = sikiGrid1.MinimumSize;
            int bh = this.Size.Height - this.ClientSize.Height;
            int bw = this.Size.Width - this.ClientSize.Width;
            this.MinimumSize = new Size(0, sikiGrid1.MinimumSize.Height + bh + yMargin);

            setupMenu1();
            this.ContextMenu = menu1;

            Size s = sikiGrid1.GetViewSize();
            this.ClientSize = new Size(this.ClientSize.Width, s.Height + yMargin);

            try
            {
                Settings.LoadFromXmlFile();
            }
            catch (System.IO.FileNotFoundException)
            {
                Settings.SaveToXmlFile();
                Settings.LoadFromXmlFile();
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            if (Settings.Instance.Pos != default(Point))
            {
                if (Settings.Instance.Pos.X >= 0 && Settings.Instance.Pos.Y >= 0)
                this.Location = Settings.Instance.Pos;
            }
            if (Settings.Instance.Size != default(Size))
                this.Size = Settings.Instance.Size;
            if (Settings.Instance.LineSize > 1)
                sikiGrid1.ExpandLine(Settings.Instance.LineSize - sikiGrid1.LineSize);
            if (Settings.Instance.RowSize > 1)
                sikiGrid1.ExpandRow(Settings.Instance.RowSize - sikiGrid1.RowSize);
            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.Location.X >= 0 && this.Location.Y >= 0)
            {
                Settings.Instance.Pos = this.Location;
                Settings.Instance.Size = this.Size;
                Settings.Instance.LineSize = sikiGrid1.LineSize;
                Settings.Instance.RowSize = sikiGrid1.RowSize;
                Settings.SaveToXmlFile();
            }
            base.OnClosing(e);
        }

        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
 
        //    if ((keyData & Keys.Alt) != 0)
        //    {
        //            MessageBox.Show(this.ActiveControl.ToString());
        //            return true;
        //    }
        //    return base.ProcessCmdKey(ref msg, keyData);
        //}

        private void setupMenu1()
        {
            menu1.MenuItems.Clear();

            MenuItem menuItem1 = new MenuItem("&Exit");
            menuItem1.Click += new EventHandler(miExit_Click);
            menu1.MenuItems.Add(menuItem1);

            MenuItem menuItem2 = new MenuItem("行追加(&L)");
            menuItem2.Click += new EventHandler(miAddLine_Click);
            menuItem2.Shortcut = Shortcut.CtrlN;
            menu1.MenuItems.Add(menuItem2);

            MenuItem menuItem3 = new MenuItem("列追加(&R)");
            menuItem3.Click += new EventHandler(miAddRow_Click);
            menuItem3.Shortcut = Shortcut.CtrlShiftN;
            menu1.MenuItems.Add(menuItem3);

            MenuItem menuItem4 = new MenuItem("行削除(&D)");
            menuItem4.Click += new EventHandler(miDeleteLine_Click);
            menu1.MenuItems.Add(menuItem4);

            MenuItem menuItem5 = new MenuItem("列削除(&D)");
            menuItem5.Click += new EventHandler(miDeleteRow_Click);
            menu1.MenuItems.Add(menuItem5);
            //MessageBox.Show(menuItem5.ToString());
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                mouseDownPoint = new Point(e.X, e.Y);
            }
            base.OnMouseDown(e);
        }

        //protected override void OnMouseUp(MouseEventArgs e)
        //{
        //    if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
        //    {
        //        setupMenu1();
        //        menu1.Show(this, new Point(e.X, e.Y));
        //    }
        //    base.OnMouseUp(e);
        //}

        void miExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        void miAddLine_Click(object sender, EventArgs e)
        {
            sikiGrid1.ExpandRow(1); //memo なんかわかりづらい。line内でrowとか。
            Size s = sikiGrid1.GetViewSize();
            this.ClientSize = new Size(this.ClientSize.Width, s.Height + yMargin);
        }

        void miAddRow_Click(object sender, EventArgs e)
        {
            sikiGrid1.ExpandLine(1);
        }

        void miDeleteLine_Click(object sender, EventArgs e)
        {
            sikiGrid1.ChopRow(1);
            Size s = sikiGrid1.GetViewSize();
            this.ClientSize = new Size(this.ClientSize.Width, s.Height + yMargin);
        }

        void miDeleteRow_Click(object sender, EventArgs e)
        {
            sikiGrid1.ChopLine(1);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Left += e.X - mouseDownPoint.X;
                this.Top += e.Y - mouseDownPoint.Y;
            }
            base.OnMouseMove(e);
        }
    }
}
