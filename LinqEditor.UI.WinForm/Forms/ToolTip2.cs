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
        
        
        // when the tooltip is hidden, we store the mouse cords, 
        // and don't show again until the user moves the mouse again
        Point _mouseLoc;
        bool _mouseMoved = true;
        // other placement data
        bool _showing;
        Rectangle _capture = Rectangle.Empty;
        int _top;
        int _left;
        Point _start;
        Point _end;

        // text data
        string _type;
        string _desc;
        IEnumerable<string> _spec;

        const int _paddingHorizontal = 10;
        const int _paddingTopVertical = 9;
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
        public UserControl CurrentOwner { get; set; }
        public bool Showing 
        { 
            get 
            { 
                // if we're not showing, but want to appear to (for the ui to stop spamming us)
                return _showing || !_showing && !_mouseMoved; 
            } 
        }
        
        public void EnableTimer(bool enable)
        {
            ;
        }

        public void HideTip()
        {
            _showing = false;
            _mouseMoved = false;
            _mouseLoc = MousePosition;
        }

        // start/end is the upper bounding edge of the word box
        public void PlaceTip(Point start, Point end, int lineHeight, ToolTipData data)
        {
            if (lineHeight < 1) throw new ArgumentException("lineHeight must be positive");
            Debug.Assert(start.X < end.X && start.Y == end.Y);

            // if we haven't detected a mouse move since last manual kill, dont do anything
            if (!_mouseMoved)
            {
                return;
            }

            var incomingCapture = new Rectangle(start, new Size(end.X - start.X, lineHeight));

            // check if the desired location is actually still relevant (mouse may have moved already)
            if (!incomingCapture.Contains(MousePosition))
            {
                return;
            }

            if (start == _start && end == _end && _type == data.TypeAndName && _desc == data.Description &&
                data.Specializations.All(x => _spec.Contains(x)) && _mouseMoved) // assumes same lineheight
            {
                _showing = true;
                return;
            }

            _start = start;
            _end = end;
            _type = data.TypeAndName;
            _desc = data.Description;
            _spec = data.Specializations ?? new List<string>();

            var tooltipText = data.ToString();
            var textSize = TextRenderer.MeasureText(tooltipText, _text.Font);

            if (textSize.Width + _paddingHorizontal * 2 > _maxWidth)
            {
                textSize = TextRenderer.MeasureText(tooltipText, _text.Font, proposedSize: new Size(_maxWidth, 10000));
            }

            var newWidth = textSize.Width + _paddingHorizontal * 2;
            var newHeight = textSize.Height + _paddingTopVertical + _paddingBottomVertical;
            Width = newWidth;
            Height = newHeight;

            // capture is the text area (rect) that the tip is associated with
            _capture = incomingCapture;
            _left = end.X - Width;
            _top = end.Y + lineHeight + _textOffset;
            _text.Size = textSize;
            _text.Text = tooltipText;

            _showing = true;
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
                _timer.Stop();
                var pos = MousePosition;
                if (Visible && !_capture.Contains(pos))
                {
                    // user moved beyond text box
                    _showing = false;
                }

                // this is kinda ugly, alternative is to install global keyboard hooks.
                // http://stackoverflow.com/questions/3312752/capturing-mouse-keyboard-events-outside-of-form-app-running-in-background
                if (!_mouseMoved && _mouseLoc != pos)
                {
                    _mouseMoved = true;
                }

                Visible = _showing;
                Top = _top;
                Left = _left;
                _timer.Start();
            };
            _timer.Start();
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

        protected override void Dispose(bool disposing)
        {
            _timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
