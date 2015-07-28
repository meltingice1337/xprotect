#region Imports

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel;

#endregion

//|------DO-NOT-REMOVE------|
//
// Creator: HazelDev
// Site   : HazelDev.com
// Created: 20.Sep.2014
// Changed: 24.Jan.2015
// Version: 1.2.0
//
//|------DO-NOT-REMOVE------|

namespace MonoFlat2
{

    #region  RoundRectangle

    sealed class RoundRectangle
    {
        public static GraphicsPath RoundRect(Rectangle Rectangle, int Curve)
        {
            GraphicsPath P = new GraphicsPath();
            int ArcRectangleWidth = Curve * 2;
            P.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -180, 90);
            P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -90, 90);
            P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 0, 90);
            P.AddArc(new Rectangle(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 90, 90);
            P.AddLine(new Point(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y), new Point(Rectangle.X, Curve + Rectangle.Y));
            return P;
        }
    }

    #endregion

    #region  ThemeContainer

    public class MonoFlat_ThemeContainer : ContainerControl
    {

        #region  Enums
        private void PaintParentBackground(PaintEventArgs e)
        {
            if (Parent != null)
            {
                Rectangle rect = new Rectangle(Left, Top,
                                               Width, Height);

                e.Graphics.TranslateTransform(-rect.X, -rect.Y);

                try
                {
                    using (PaintEventArgs pea =
                                new PaintEventArgs(e.Graphics, rect))
                    {
                        pea.Graphics.SetClip(rect);
                        InvokePaintBackground(Parent, pea);
                        InvokePaint(Parent, pea);
                    }
                }
                finally
                {
                    e.Graphics.TranslateTransform(rect.X, rect.Y);
                }
            }
            else
            {
                e.Graphics.FillRectangle(SystemBrushes.Control,
                                         ClientRectangle);
            }
        }
        public enum MouseState
        {
            None = 0,
            Over = 1,
            Down = 2,
            Block = 3
        }

        #endregion
        #region  Variables

        private Rectangle HeaderRect;
        protected MouseState State;
        private int MoveHeight;
        private Point MouseP = new Point(0, 0);
        private bool Cap = false;
        private bool HasShown;

        #endregion
        #region  Properties

        private bool _Sizable = true;
        public bool Sizable
        {
            get
            {
                return _Sizable;
            }
            set
            {
                _Sizable = value;
            }
        }

        private bool _SmartBounds = true;
        public bool SmartBounds
        {
            get
            {
                return _SmartBounds;
            }
            set
            {
                _SmartBounds = value;
            }
        }

        private bool _RoundCorners = true;
        public bool RoundCorners
        {
            get
            {
                return _RoundCorners;
            }
            set
            {
                _RoundCorners = value;
                Invalidate();
            }
        }

        private bool _IsParentForm;
        protected bool IsParentForm
        {
            get
            {
                return _IsParentForm;
            }
        }

        protected bool IsParentMdi
        {
            get
            {
                if (Parent == null)
                {
                    return false;
                }
                return Parent.Parent != null;
            }
        }

        private bool _ControlMode;
        protected bool ControlMode
        {
            get
            {
                return _ControlMode;
            }
            set
            {
                _ControlMode = value;
                Invalidate();
            }
        }

        private FormStartPosition _StartPosition;
        public FormStartPosition StartPosition
        {
            get
            {
                if (_IsParentForm && !_ControlMode)
                {
                    return ParentForm.StartPosition;
                }
                else
                {
                    return _StartPosition;
                }
            }
            set
            {
                _StartPosition = value;

                if (_IsParentForm && !_ControlMode)
                {
                    ParentForm.StartPosition = value;
                }
            }
        }

        #endregion
        #region  EventArgs

        protected sealed override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent == null)
            {
                return;
            }
            _IsParentForm = Parent is Form;

            if (!_ControlMode)
            {
                InitializeMessages();

                if (_IsParentForm)
                {
                    this.ParentForm.FormBorderStyle = FormBorderStyle.None;
                    this.ParentForm.TransparencyKey = Color.Fuchsia;

                    if (!DesignMode)
                    {
                        ParentForm.Shown += FormShown;
                    }
                }
                Parent.BackColor = BackColor;
                //   Parent.MinimumSize = New Size(261, 65)
            }
        }

        protected sealed override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (!_ControlMode)
            {
                HeaderRect = new Rectangle(0, 0, Width - 14, MoveHeight - 7);
            }
            Invalidate();
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button == MouseButtons.Left)
            {
                SetState(MouseState.Down);
            }
            if (!(_IsParentForm && ParentForm.WindowState == FormWindowState.Maximized || _ControlMode))
            {
                if (HeaderRect.Contains(e.Location))
                {
                    Capture = false;
                    WM_LMBUTTONDOWN = true;
                    DefWndProc(ref Messages[0]);
                }
                else if (_Sizable && !(Previous == 0))
                {
                    Capture = false;
                    WM_LMBUTTONDOWN = true;
                    DefWndProc(ref Messages[Previous]);
                }
            }
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Cap = false;
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!(_IsParentForm && ParentForm.WindowState == FormWindowState.Maximized))
            {
                if (_Sizable && !_ControlMode)
                {
                    InvalidateMouse();
                }
            }
            if (Cap)
            {
                Parent.Location = (System.Drawing.Point)((object)(System.Convert.ToDouble(MousePosition) - System.Convert.ToDouble(MouseP)));
            }
        }

        protected override void OnInvalidated(System.Windows.Forms.InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
            ParentForm.Text = Text;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        protected override void OnTextChanged(System.EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (_ControlMode || HasShown)
            {
                return;
            }

            if (_StartPosition == FormStartPosition.CenterParent || _StartPosition == FormStartPosition.CenterScreen)
            {
                Rectangle SB = Screen.PrimaryScreen.Bounds;
                Rectangle CB = ParentForm.Bounds;
                ParentForm.Location = new Point(SB.Width / 2 - CB.Width / 2, SB.Height / 2 - CB.Width / 2);
            }
            HasShown = true;
        }

        #endregion
        #region  Mouse & Size

        private void SetState(MouseState current)
        {
            State = current;
            Invalidate();
        }

        private Point GetIndexPoint;
        private bool B1x;
        private bool B2x;
        private bool B3;
        private bool B4;
        private int GetIndex()
        {
            GetIndexPoint = PointToClient(MousePosition);
            B1x = GetIndexPoint.X < 7;
            B2x = GetIndexPoint.X > Width - 7;
            B3 = GetIndexPoint.Y < 7;
            B4 = GetIndexPoint.Y > Height - 7;

            if (B1x && B3)
            {
                return 4;
            }
            if (B1x && B4)
            {
                return 7;
            }
            if (B2x && B3)
            {
                return 5;
            }
            if (B2x && B4)
            {
                return 8;
            }
            if (B1x)
            {
                return 1;
            }
            if (B2x)
            {
                return 2;
            }
            if (B3)
            {
                return 3;
            }
            if (B4)
            {
                return 6;
            }
            return 0;
        }

        private int Current;
        private int Previous;
        private void InvalidateMouse()
        {
            Current = GetIndex();
            if (Current == Previous)
            {
                return;
            }

            Previous = Current;
            switch (Previous)
            {
                case 0:
                    Cursor = Cursors.Default;
                    break;
                case 6:
                    Cursor = Cursors.SizeNS;
                    break;
                case 8:
                    Cursor = Cursors.SizeNWSE;
                    break;
                case 7:
                    Cursor = Cursors.SizeNESW;
                    break;
            }
        }

        private Message[] Messages = new Message[9];
        private void InitializeMessages()
        {
            Messages[0] = Message.Create(Parent.Handle, 161, new IntPtr(2), IntPtr.Zero);
            for (int I = 1; I <= 8; I++)
            {
                Messages[I] = Message.Create(Parent.Handle, 161, new IntPtr(I + 9), IntPtr.Zero);
            }
        }

        private void CorrectBounds(Rectangle bounds)
        {
            if (Parent.Width > bounds.Width)
            {
                Parent.Width = bounds.Width;
            }
            if (Parent.Height > bounds.Height)
            {
                Parent.Height = bounds.Height;
            }

            int X = Parent.Location.X;
            int Y = Parent.Location.Y;

            if (X < bounds.X)
            {
                X = bounds.X;
            }
            if (Y < bounds.Y)
            {
                Y = bounds.Y;
            }

            int Width = bounds.X + bounds.Width;
            int Height = bounds.Y + bounds.Height;

            if (X + Parent.Width > Width)
            {
                X = Width - Parent.Width;
            }
            if (Y + Parent.Height > Height)
            {
                Y = Height - Parent.Height;
            }

            Parent.Location = new Point(X, Y);
        }

        private bool WM_LMBUTTONDOWN;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (WM_LMBUTTONDOWN && m.Msg == 513)
            {
                WM_LMBUTTONDOWN = false;

                SetState(MouseState.Over);
                if (!_SmartBounds)
                {
                    return;
                }

                if (IsParentMdi)
                {
                    CorrectBounds(new Rectangle(Point.Empty, Parent.Parent.Size));
                }
                else
                {
                    CorrectBounds(Screen.FromControl(Parent).WorkingArea);
                }
            }
        }

        #endregion

        protected override void CreateHandle()
        {
            base.CreateHandle();
        }

        public MonoFlat_ThemeContainer()
        {
            SetStyle((ControlStyles)(139270), true);
            BackColor = Color.FromArgb(32, 41, 50);
            Padding = new Padding(10, 70, 10, 9);
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            MoveHeight = 66;
            Font = new Font("Segoe UI", 9);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics G = e.Graphics;

            G.Clear(Color.FromArgb(32, 41, 50));
            G.FillRectangle(new SolidBrush(Color.FromArgb(181, 41, 42)), new Rectangle(0, 0, Width, 60));

            if (_RoundCorners == true)
            {
                // Draw Left upper corner
                G.FillRectangle(Brushes.Fuchsia, 0, 0, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 1, 0, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 2, 0, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 3, 0, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 0, 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 0, 2, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 0, 3, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 1, 1, 1, 1);

                G.FillRectangle(new SolidBrush(Color.FromArgb(181, 41, 42)), 1, 3, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(181, 41, 42)), 1, 2, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(181, 41, 42)), 2, 1, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(181, 41, 42)), 3, 1, 1, 1);

                // Draw right upper corner
                G.FillRectangle(Brushes.Fuchsia, Width - 1, 0, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 2, 0, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 3, 0, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 4, 0, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 1, 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 1, 2, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 1, 3, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 2, 1, 1, 1);

                G.FillRectangle(new SolidBrush(Color.FromArgb(181, 41, 42)), Width - 2, 3, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(181, 41, 42)), Width - 2, 2, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(181, 41, 42)), Width - 3, 1, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(181, 41, 42)), Width - 4, 1, 1, 1);

                // Draw Left bottom corner
                G.FillRectangle(Brushes.Fuchsia, 0, Height - 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 0, Height - 2, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 0, Height - 3, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 0, Height - 4, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 1, Height - 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 2, Height - 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 3, Height - 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 1, Height - 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, 1, Height - 2, 1, 1);

                G.FillRectangle(new SolidBrush(Color.FromArgb(32, 41, 50)), 1, Height - 3, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(32, 41, 50)), 1, Height - 4, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(32, 41, 50)), 3, Height - 2, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(32, 41, 50)), 2, Height - 2, 1, 1);

                // Draw right bottom corner
                G.FillRectangle(Brushes.Fuchsia, Width - 1, Height, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 2, Height, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 3, Height, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 4, Height, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 1, Height - 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 1, Height - 2, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 1, Height - 3, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 2, Height - 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 3, Height - 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 4, Height - 1, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 1, Height - 4, 1, 1);
                G.FillRectangle(Brushes.Fuchsia, Width - 2, Height - 2, 1, 1);

                G.FillRectangle(new SolidBrush(Color.FromArgb(32, 41, 50)), Width - 2, Height - 3, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(32, 41, 50)), Width - 2, Height - 4, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(32, 41, 50)), Width - 4, Height - 2, 1, 1);
                G.FillRectangle(new SolidBrush(Color.FromArgb(32, 41, 50)), Width - 3, Height - 2, 1, 1);
            }
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;
            G.DrawString(Text, new Font("Microsoft Sans Serif", 12, FontStyle.Bold), new SolidBrush(Color.FromArgb(255, 254, 255)), new Rectangle(20, 20, Width - 1, Height),sf);
        }
    }

    #endregion
    #region ControlBox

    class MonoFlat_ControlBox : Control
    {

        #region Enums

        public enum ButtonHoverState
        {
            Minimize,
            Maximize,
            Close,
            None
        }

        #endregion
        #region Variables

        private ButtonHoverState ButtonHState = ButtonHoverState.None;

        #endregion
        #region Properties

        private bool _EnableMaximize = true;
        public bool EnableMaximizeButton
        {
            get { return _EnableMaximize; }
            set
            {
                _EnableMaximize = value;
                Invalidate();
            }
        }

        private bool _EnableMinimize = true;
        public bool EnableMinimizeButton
        {
            get { return _EnableMinimize; }
            set
            {
                _EnableMinimize = value;
                Invalidate();
            }
        }

        private bool _EnableHoverHighlight = false;
        public bool EnableHoverHighlight
        {
            get { return _EnableHoverHighlight; }
            set
            {
                _EnableHoverHighlight = value;
                Invalidate();
            }
        }

        #endregion
        #region EventArgs

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Size = new Size(100, 25);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int X = e.Location.X;
            int Y = e.Location.Y;
            if (Y > 0 && Y < (Height - 2))
            {
                if (X > 0 && X < 34)
                {
                    ButtonHState = ButtonHoverState.Minimize;
                }
                else if (X > 33 && X < 65)
                {
                    ButtonHState = ButtonHoverState.Maximize;
                }
                else if (X > 64 && X < Width)
                {
                    ButtonHState = ButtonHoverState.Close;
                }
                else
                {
                    ButtonHState = ButtonHoverState.None;
                }
            }
            else
            {
                ButtonHState = ButtonHoverState.None;
            }
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            switch (ButtonHState)
            {
                case ButtonHoverState.Close:
                    Parent.FindForm().Close();
                    break;
                case ButtonHoverState.Minimize:
                    if (_EnableMinimize == true)
                    {
                        Parent.FindForm().WindowState = FormWindowState.Minimized;
                    }
                    break;
                case ButtonHoverState.Maximize:
                    if (_EnableMaximize == true)
                    {
                        if (Parent.FindForm().WindowState == FormWindowState.Normal)
                        {
                            Parent.FindForm().WindowState = FormWindowState.Maximized;
                        }
                        else
                        {
                            Parent.FindForm().WindowState = FormWindowState.Normal;
                        }
                    }
                    break;
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            ButtonHState = ButtonHoverState.None;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }

        #endregion

        public MonoFlat_ControlBox()
            : base()
        {
            DoubleBuffered = true;
            Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            try
            {
                Location = new Point(Parent.Width - 112, 15);
            }
            catch (Exception)
            {
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics G = e.Graphics;
            G.Clear(Color.FromArgb(181, 41, 42));

            if (_EnableHoverHighlight == true)
            {
                switch (ButtonHState)
                {
                    case ButtonHoverState.None:
                        G.Clear(Color.FromArgb(181, 41, 42));
                        break;
                    case ButtonHoverState.Minimize:
                        if (_EnableMinimize == true)
                        {
                            G.FillRectangle(new SolidBrush(Color.FromArgb(156, 35, 35)), new Rectangle(3, 0, 30, Height));
                        }
                        break;

                    case ButtonHoverState.Close:
                        G.FillRectangle(new SolidBrush(Color.FromArgb(156, 35, 35)), new Rectangle(66, 0, 35, Height));
                        break;
                }
            }
            StringFormat SF = new StringFormat();
            SF.Alignment = StringAlignment.Center;
            //Close
            G.DrawString("r", new Font("Marlett", 12), new SolidBrush(Color.FromArgb(255, 254, 255)), new Point(Width - 16, 8), SF);

            //Maximize
            switch (Parent.FindForm().WindowState)
            {
                case FormWindowState.Maximized:
                    if (_EnableMaximize == true)
                    {
                        G.DrawString("2", new Font("Marlett", 12), new SolidBrush(Color.FromArgb(255, 254, 255)), new Point(51, 7), SF);
                    }
                    else
                    {
                        G.DrawString("2", new Font("Marlett", 12), new SolidBrush(Color.LightGray), new Point(51, 7), SF);
                    }
                    break;
                case FormWindowState.Normal:
                    if (_EnableMaximize == true)
                    {
                        G.DrawString("1", new Font("Marlett", 12), new SolidBrush(Color.FromArgb(255, 254, 255)), new Point(51, 7), SF);
                    }
                    else
                    {
                        G.DrawString("1", new Font("Marlett", 12), new SolidBrush(Color.LightGray), new Point(51, 7), SF);
                    }
                    break;
            }

            //Minimize
            if (_EnableMinimize == true)
            {
                G.DrawString("0", new Font("Marlett", 12), new SolidBrush(Color.FromArgb(255, 254, 255)), new Point(20, 7), SF);
            }
            else
            {
                G.DrawString("0", new Font("Marlett", 12), new SolidBrush(Color.LightGray), new Point(20, 7), SF);
            }
        }
    }

    #endregion
    #region  Button

    public class MonoFlat_Button : Control
    {

        #region  Variables

        private int MouseState;
        private GraphicsPath Shape;
        private LinearGradientBrush InactiveGB;
        private LinearGradientBrush PressedGB;
        private Rectangle R1;
        private Pen P1;
        private Pen P3;
        private Image _Image;
        private Size _ImageSize;
        private StringAlignment _TextAlignment = StringAlignment.Center;
        private Color _TextColor; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
        private ContentAlignment _ImageAlign = ContentAlignment.MiddleLeft;

        #endregion
        #region  Image Designer

        private static PointF ImageLocation(StringFormat SF, SizeF Area, SizeF ImageArea)
        {
            PointF MyPoint = new PointF();
            switch (SF.Alignment)
            {
                case StringAlignment.Center:
                    MyPoint.X = (float)((Area.Width - ImageArea.Width) / 2);
                    break;
                case StringAlignment.Near:
                    MyPoint.X = 2;
                    break;
                case StringAlignment.Far:
                    MyPoint.X = Area.Width - ImageArea.Width - 2;
                    break;

            }

            switch (SF.LineAlignment)
            {
                case StringAlignment.Center:
                    MyPoint.Y = (float)((Area.Height - ImageArea.Height) / 2);
                    break;
                case StringAlignment.Near:
                    MyPoint.Y = 2;
                    break;
                case StringAlignment.Far:
                    MyPoint.Y = Area.Height - ImageArea.Height - 2;
                    break;
            }
            return MyPoint;
        }

        private StringFormat GetStringFormat(ContentAlignment _ContentAlignment)
        {
            StringFormat SF = new StringFormat();
            switch (_ContentAlignment)
            {
                case ContentAlignment.MiddleCenter:
                    SF.LineAlignment = StringAlignment.Center;
                    SF.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleLeft:
                    SF.LineAlignment = StringAlignment.Center;
                    SF.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleRight:
                    SF.LineAlignment = StringAlignment.Center;
                    SF.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.TopCenter:
                    SF.LineAlignment = StringAlignment.Near;
                    SF.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopLeft:
                    SF.LineAlignment = StringAlignment.Near;
                    SF.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopRight:
                    SF.LineAlignment = StringAlignment.Near;
                    SF.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomCenter:
                    SF.LineAlignment = StringAlignment.Far;
                    SF.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomLeft:
                    SF.LineAlignment = StringAlignment.Far;
                    SF.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.BottomRight:
                    SF.LineAlignment = StringAlignment.Far;
                    SF.Alignment = StringAlignment.Far;
                    break;
            }
            return SF;
        }

        #endregion
        #region  Properties

        public Image Image
        {
            get
            {
                return _Image;
            }
            set
            {
                if (value == null)
                {
                    _ImageSize = Size.Empty;
                }
                else
                {
                    _ImageSize = value.Size;
                }

                _Image = value;
                Invalidate();
            }
        }

        protected Size ImageSize
        {
            get
            {
                return _ImageSize;
            }
        }

        public ContentAlignment ImageAlign
        {
            get
            {
                return _ImageAlign;
            }
            set
            {
                _ImageAlign = value;
                Invalidate();
            }
        }

        public StringAlignment TextAlignment
        {
            get
            {
                return this._TextAlignment;
            }
            set
            {
                this._TextAlignment = value;
                this.Invalidate();
            }
        }

        public override Color ForeColor
        {
            get
            {
                return this._TextColor;
            }
            set
            {
                this._TextColor = value;
                this.Invalidate();
            }
        }

        #endregion
        #region  EventArgs

        protected override void OnMouseUp(MouseEventArgs e)
        {
            MouseState = 0;
            Invalidate();
            base.OnMouseUp(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            MouseState = 1;
            Focus();
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            MouseState = 0;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnTextChanged(System.EventArgs e)
        {
            Invalidate();
            base.OnTextChanged(e);
        }

        #endregion

        public MonoFlat_Button()
        {
            SetStyle((System.Windows.Forms.ControlStyles)(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint), true);

            BackColor = Color.Transparent;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 12);
            ForeColor = Color.FromArgb(255, 255, 255);
            Size = new Size(146, 41);
            _TextAlignment = StringAlignment.Center;
            P1 = new Pen(Color.FromArgb(181, 41, 42)); // P1 = Border color
            P3 = new Pen(Color.FromArgb(165, 37, 37)); // P3 = Border color when pressed
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            if (Width > 0 && Height > 0)
            {

                Shape = new GraphicsPath();
                R1 = new Rectangle(0, 0, Width, Height);

                InactiveGB = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Color.FromArgb(181, 41, 42), Color.FromArgb(181, 41, 42), 90.0F);
                PressedGB = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Color.FromArgb(165, 37, 37), Color.FromArgb(165, 37, 37), 90.0F);
            }

            Shape.AddArc(0, 0, 10, 10, 180, 90);
            Shape.AddArc(Width - 11, 0, 10, 10, -90, 90);
            Shape.AddArc(Width - 11, Height - 11, 10, 10, 0, 90);
            Shape.AddArc(0, Height - 11, 10, 10, 90, 90);
            Shape.CloseAllFigures();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var G = e.Graphics;
            G.SmoothingMode = SmoothingMode.HighQuality;
            PointF ipt = ImageLocation(GetStringFormat(ImageAlign), Size, ImageSize);
            StringFormat sf = new StringFormat();
            sf.Alignment = _TextAlignment;
            sf.LineAlignment = StringAlignment.Center;
            switch (MouseState)
            {
                case 0:
                    //Inactive
                    G.FillPath(InactiveGB, Shape);
                    // Fill button body with InactiveGB color gradient
                    G.DrawPath(P1, Shape);
                    // Draw button border [InactiveGB]
                    if ((Image == null))
                    {
                        G.DrawString(Text, Font, new SolidBrush(ForeColor), R1, sf);
                    }
                    else
                    {
                        G.DrawImage(_Image, ipt.X, ipt.Y, ImageSize.Width, ImageSize.Height);
                        G.DrawString(Text, Font, new SolidBrush(ForeColor), R1, sf);
                    }
                    break;
                case 1:
                    //Pressed
                    G.FillPath(PressedGB, Shape);
                    // Fill button body with PressedGB color gradient
                    G.DrawPath(P3, Shape);
                    // Draw button border [PressedGB]

                    if ((Image == null))
                    {
                        G.DrawString(Text, Font, new SolidBrush(ForeColor), R1, sf);
                    }
                    else
                    {
                        G.DrawImage(_Image, ipt.X, ipt.Y, ImageSize.Width, ImageSize.Height);
                        G.DrawString(Text, Font, new SolidBrush(ForeColor), R1, sf);
                    }
                    break;
            }
            base.OnPaint(e);
        }
    }

    #endregion
    #region  Label

    public class MonoFlat_Label : Label
    {

        public MonoFlat_Label()
        {
            Font = new Font("Segoe UI", 9);
            ForeColor = Color.FromArgb(116, 125, 132);
            BackColor = Color.Transparent;
        }
    }

    #endregion
    #region  CheckBox

    [DefaultEvent("CheckedChanged")]
    public class MonoFlat_CheckBox : Control
    {

        #region  Variables

        private int X;
        private bool _Checked = false;
        private GraphicsPath Shape;

        #endregion
        #region  Properties

        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
                Invalidate();
            }
        }

        #endregion
        #region  EventArgs

        public delegate void CheckedChangedEventHandler(object sender);
        private CheckedChangedEventHandler CheckedChangedEvent;

        public event CheckedChangedEventHandler CheckedChanged
        {
            add
            {
                CheckedChangedEvent = (CheckedChangedEventHandler)System.Delegate.Combine(CheckedChangedEvent, value);
            }
            remove
            {
                CheckedChangedEvent = (CheckedChangedEventHandler)System.Delegate.Remove(CheckedChangedEvent, value);
            }
        }


        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            X = e.Location.X;
            Invalidate();
        }
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            _Checked = !_Checked;
            Focus();
            if (CheckedChangedEvent != null)
                CheckedChangedEvent(this);
            base.OnMouseDown(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            this.Height = 16;

            Shape = new GraphicsPath();
            Shape.AddArc(0, 0, 10, 10, 180, 90);
            Shape.AddArc(Width - 11, 0, 10, 10, -90, 90);
            Shape.AddArc(Width - 11, Height - 11, 10, 10, 0, 90);
            Shape.AddArc(0, Height - 11, 10, 10, 90, 90);
            Shape.CloseAllFigures();
            Invalidate();
        }

        #endregion

        public MonoFlat_CheckBox()
        {
            Width = 148;
            Height = 16;
            Font = new Font("Microsoft Sans Serif", 9);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics G = e.Graphics;
            G.Clear(Parent.BackColor);

            if (_Checked)
            {
                G.FillRectangle(new SolidBrush(Color.FromArgb(66, 76, 85)), new Rectangle(0, 0, 16, 16));
                G.FillRectangle(new SolidBrush(Color.FromArgb(66, 76, 85)), new Rectangle(1, 1, 16 - 2, 16 - 2));
            }
            else
            {
                G.FillRectangle(new SolidBrush(Color.FromArgb(66, 76, 85)), new Rectangle(0, 0, 16, 16));
                G.FillRectangle(new SolidBrush(Color.FromArgb(66, 76, 85)), new Rectangle(1, 1, 16 - 2, 16 - 2));
            }

            if (Enabled == true)
            {
                if (_Checked)
                {
                    G.DrawString("a", new Font("Marlett", 16), new SolidBrush(Color.FromArgb(181, 41, 42)), new Point(-5, -3));
                }
            }
            else
            {
                if (_Checked)
                {
                    G.DrawString("a", new Font("Marlett", 16), new SolidBrush(Color.Gray), new Point(-5, -3));
                }
            }

            G.DrawString(Text, Font, new SolidBrush(Color.FromArgb(116, 125, 132)), new Point(20, 0));
        }
    }
    #endregion
    #region  TextBox

    [DefaultEvent("TextChanged")]
    public class MonoFlat_TextBox : Control
    {

        #region  Variables

        public TextBox MonoFlatTB = new TextBox();
        private int _maxchars = 32767;
        private bool _ReadOnly;
        private bool _Multiline;
        private Image _Image;
        private Size _ImageSize;
        private HorizontalAlignment ALNType;
        private bool isPasswordMasked = false;
        private Pen P1;
        private SolidBrush B1;
        private GraphicsPath Shape;

        #endregion
        #region  Properties

        public HorizontalAlignment TextAlignment
        {
            get
            {
                return ALNType;
            }
            set
            {
                ALNType = value;
                Invalidate();
            }
        }
        public int MaxLength
        {
            get
            {
                return _maxchars;
            }
            set
            {
                _maxchars = value;
                MonoFlatTB.MaxLength = MaxLength;
                Invalidate();
            }
        }

        public bool UseSystemPasswordChar
        {
            get
            {
                return isPasswordMasked;
            }
            set
            {
                MonoFlatTB.UseSystemPasswordChar = UseSystemPasswordChar;
                isPasswordMasked = value;
                Invalidate();
            }
        }
        public bool ReadOnly
        {
            get
            {
                return _ReadOnly;
            }
            set
            {
                _ReadOnly = value;
                if (MonoFlatTB != null)
                {
                    MonoFlatTB.ReadOnly = value;
                }
            }
        }
        public bool Multiline
        {
            get
            {
                return _Multiline;
            }
            set
            {
                _Multiline = value;
                if (MonoFlatTB != null)
                {
                    MonoFlatTB.Multiline = value;

                    if (value)
                    {
                        MonoFlatTB.Height = Height - 23;
                    }
                    else
                    {
                        Height = MonoFlatTB.Height + 23;
                    }
                }
            }
        }

        public Image Image
        {
            get
            {
                return _Image;
            }
            set
            {
                if (value == null)
                {
                    _ImageSize = Size.Empty;
                }
                else
                {
                    _ImageSize = value.Size;
                }

                _Image = value;

                if (Image == null)
                {
                    MonoFlatTB.Location = new Point(8, 10);
                }
                else
                {
                    MonoFlatTB.Location = new Point(35, 11);
                }
                Invalidate();
            }
        }

        protected Size ImageSize
        {
            get
            {
                return _ImageSize;
            }
        }

        #endregion
        #region  EventArgs

        private void _Enter(object Obj, EventArgs e)
        {
            P1 = new Pen(Color.FromArgb(181, 31, 42));
            Refresh();
        }

        private void _Leave(object Obj, EventArgs e)
        {
            P1 = new Pen(Color.FromArgb(32, 31, 50));
            Refresh();
        }

        private void OnBaseTextChanged(object s, EventArgs e)
        {
            Text = MonoFlatTB.Text;
        }

        protected override void OnTextChanged(System.EventArgs e)
        {
            base.OnTextChanged(e);
            MonoFlatTB.Text = Text;
            Invalidate();
        }

        protected override void OnForeColorChanged(System.EventArgs e)
        {
            base.OnForeColorChanged(e);
            MonoFlatTB.ForeColor = ForeColor;
            Invalidate();
        }

        protected override void OnFontChanged(System.EventArgs e)
        {
            base.OnFontChanged(e);
            MonoFlatTB.Font = Font;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        private void _OnKeyDown(object Obj, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                MonoFlatTB.SelectAll();
                e.SuppressKeyPress = true;
            }
            if (e.Control && e.KeyCode == Keys.C)
            {
                MonoFlatTB.Copy();
                e.SuppressKeyPress = true;
            }
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            if (_Multiline)
            {
                MonoFlatTB.Height = Height - 23;
            }
            else
            {
                Height = MonoFlatTB.Height + 23;
            }

            Shape = new GraphicsPath();
            Shape.AddArc(0, 0, 10, 10, 180, 90);
            Shape.AddArc(Width - 11, 0, 10, 10, -90, 90);
            Shape.AddArc(Width - 11, Height - 11, 10, 10, 0, 90);
            Shape.AddArc(0, Height - 11, 10, 10, 90, 90);
            Shape.CloseAllFigures();
        }

        protected override void OnGotFocus(System.EventArgs e)
        {
            base.OnGotFocus(e);
            MonoFlatTB.Focus();
        }

        public void _TextChanged(System.Object sender, System.EventArgs e)
        {
            Text = MonoFlatTB.Text;
        }

        public void _BaseTextChanged(System.Object sender, System.EventArgs e)
        {
            MonoFlatTB.Text = Text;
        }

        #endregion

        public void AddTextBox()
        {
            MonoFlatTB.Location = new Point(8, 10);
            MonoFlatTB.Text = String.Empty;
            MonoFlatTB.BorderStyle = BorderStyle.None;
            MonoFlatTB.TextAlign = HorizontalAlignment.Left;
            MonoFlatTB.Font = new Font("Tahoma", 11);
            MonoFlatTB.UseSystemPasswordChar = UseSystemPasswordChar;
            MonoFlatTB.Multiline = false;
            MonoFlatTB.BackColor = Color.FromArgb(66, 76, 85);
            MonoFlatTB.ScrollBars = ScrollBars.None;
            MonoFlatTB.KeyDown += _OnKeyDown;
            MonoFlatTB.Enter += _Enter;
            MonoFlatTB.Leave += _Leave;
            MonoFlatTB.TextChanged += OnBaseTextChanged;
        }

        public MonoFlat_TextBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);

            AddTextBox();
            Controls.Add(MonoFlatTB);

            P1 = new Pen(Color.FromArgb(32, 31, 50));
            B1 = new SolidBrush(Color.FromArgb(66, 76, 85));
            BackColor = Color.Transparent;
            ForeColor = Color.FromArgb(176, 183, 191);

            Text = null;
            Font = new Font("Tahoma", 11);
            Size = new Size(135, 43);
            DoubleBuffered = true;
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            Bitmap B = new Bitmap(Width, Height);
            Graphics G = Graphics.FromImage(B);

            G.SmoothingMode = SmoothingMode.AntiAlias;


            if (Image == null)
            {
                MonoFlatTB.Width = Width - 18;
            }
            else
            {
                MonoFlatTB.Width = Width - 45;
            }

            MonoFlatTB.TextAlign = TextAlignment;
            MonoFlatTB.UseSystemPasswordChar = UseSystemPasswordChar;

            G.Clear(Color.Transparent);

            G.FillPath(B1, Shape);
            G.DrawPath(P1, Shape);

            if (Image != null)
            {
                G.DrawImage(_Image, 5, 8, 24, 24);
                // 24x24 is the perfect size of the image
            }

            e.Graphics.DrawImage((Image)(B.Clone()), 0, 0);
            G.Dispose();
            B.Dispose();
        }
    }

    #endregion

}