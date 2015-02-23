using LinqEditor.Core.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Forms
{
    public class ToolTip2 : Form
    {
        Color _borderColor = ColorTranslator.FromHtml("#CCCEDB");
        Color _backColor = ColorTranslator.FromHtml("#F6F6F6");
        Label _text;
        Timer _timer;

        // current data
        bool _showIntent; // if the tooltip should be visible or not
        int _top;
        int _left;
        Point _start;
        Point _end;

        const int _paddingHorizontal = 10;
        const int _paddingTopVertical = 7;
        const int _paddingBottomVertical = 13;
        const int _maxWidth = 500;
        const int _textOffset = 2;
        
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ff700543%28v=vs.85%29.aspx
        const long WS_EX_NOACTIVATE = 0x08000000L;
        const long WS_EX_TOOLWINDOW = 0x00000080L;
        const long WS_EX_TOPMOST = 0x00000008L;

        const long WS_CAPTION = 0x00C00000L;
        const long WS_THICKFRAME = 0x00040000L;
        const long WS_MINIMIZE = 0x20000000L;
        const long WS_MAXIMIZE = 0x01000000L;
        const long WS_SYSMENU = 0x00080000L;
        
        // pretty crude
        public int CurrentWordStart { get; set; }
        public UserControl CurrentOwner { get; set; }
        public bool ClientIsActive { get; set; }

        public void KillTip()
        {
            _showIntent = false;
        }

        public void EnableTimer(bool enable)
        {
            if (enable)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                Visible = false;
            }
            Debug.WriteLine("EnableTimer: " + enable);
        }

        // start/end is the upper bounding edge of the word box
        public void PlaceTip(Point start, Point end, int lineHeight, ToolTipData data)
        {
            if (lineHeight < 1) throw new ArgumentException("lineHeight must be positive");
            Debug.WriteLineIf(start == _start && end == _end, "PlaceTip same pos");
            
            if (start == _start && end == _end) // assumes same data/lineheight
            {
                _showIntent = true;
                return;
            }

            Debug.WriteLine("PlaceTip new pos");

            _start = start;
            _end = end;

            var tooltipText = string.Format("{0}\n{1}", data.TypeAndName, data.Description);
            var textSize = TextRenderer.MeasureText(tooltipText, _text.Font);

            if (textSize.Width + _paddingHorizontal * 2 > _maxWidth)
            {
                textSize = TextRenderer.MeasureText(tooltipText, _text.Font, proposedSize: new Size(_maxWidth, 10000));
            }

            var newWidth = textSize.Width + _paddingHorizontal * 2;
            var newHeight = textSize.Height + _paddingTopVertical + _paddingBottomVertical;
            Width = newWidth;
            Height = newHeight;
            
            _left = end.X - Width;
            _top = end.Y + lineHeight + _textOffset;
            _text.Size = textSize;
            _text.Text = tooltipText;

            _showIntent = true;
        }

        public ToolTip2()
        {
            BackColor = _backColor;
            var font = new Font("Segoe UI", 9f);
            _text = new Label();
            _text.Font = font;
            _text.Location = new Point(_paddingHorizontal, _paddingTopVertical);
            Controls.Add(_text);

            _timer = new Timer();
            _timer.Interval = 50;
            _timer.Tick += delegate
            {
                Visible = _showIntent;
                Top = _top;
                Left = _left;
            };
            _timer.Start();

            MouseEnter += delegate
            {
                Visible = false;
            };

            Height = 100;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, _borderColor, ButtonBorderStyle.Solid);
        }

        // http://stackoverflow.com/questions/156046/show-a-form-without-stealing-focus
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var baseParams = base.CreateParams;
                baseParams.Style &= ~(int)(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZE | WS_MAXIMIZE | WS_SYSMENU);
                // sets toolwindow styling, which prevents window from appearing in alt-tab list
                // and noactivate which prevents stealing focus on click events
                baseParams.ExStyle |= (int)(WS_EX_TOOLWINDOW | WS_EX_TOPMOST | WS_EX_NOACTIVATE);
                return baseParams;
            }
        }
    }
}
