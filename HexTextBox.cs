using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ITLDG
{
    public partial class HexTextBox : TextBox
    {
        string beforText = "";
        private char _Separator = ' ';
        [Description("HEX分隔符")]
        [Category("HEX")]
        public char HexSeparator
        {
            get { return _Separator; }
            set
            {
                Text = Text.Replace(_Separator, value);
                _Separator = value;
            }
        }
        /// <summary>
        /// 已输入的HEX字节数,如果未输入正确的HEX,则返回0
        /// </summary>
        public int HexLength { get { return IsHex() ? HexText.Length / 2 : 0; } }
        /// <summary>
        /// 最大允许输入的HEX字节数
        /// </summary>
        [Description("最大允许输入的HEX字节数")]
        [Category("HEX")]
        public int MaxHexLength { get { return (int)Math.Ceiling((double)MaxLength / 3); } set { MaxLength = value * 3 - 1; } }

        private bool _HexMode = true;
        [Description("是否是HEX输入模式")]
        [Category("HEX")]
        public bool HexMode
        {
            get { return _HexMode; }
            set
            {
                string text = Text;
                _HexMode = value;
                Text = beforText;
                beforText = text;
            }
        }

        /// <summary>
        /// HEX字符串,不包含分隔符
        /// </summary>
        public string HexText { get { return Regex.Replace(Text, "[^0-9a-fA-F]", ""); } }
        public HexTextBox()
        {
            InitializeComponent();

            //仅net48设置该属性
#if NET48
                  this.ContextMenu = new ContextMenu();
#endif
            this.KeyDown += HexTextBox_KeyDown;
            this.TextChanged += HexTextBox_TextChanged;
            this.KeyPress += HexTextBox_KeyPress;
        }

        int lastSelectionStart = 0;
        private void HexTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!HexMode)
            {
                return;
            }
            lastSelectionStart = this.SelectionStart;
        }

        private void HexTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!HexMode)
            {
                return;
            }
            // 如果按下了删除键并且删除的是空格，则同时删除其前面的一个字符
            if (e.KeyChar == (char)Keys.Back && this.SelectionStart > 0 && this.Text[this.SelectionStart - 1] == HexSeparator)
            {
                this.SelectionStart -= 1;
                this.SelectionLength += 1;
                this.SelectedText = "";
                lastSelectionStart = this.SelectionStart;
                e.Handled = true;
            }
            else if (e.KeyChar == (char)Keys.Back)
            {
                if (this.SelectionLength == 0)
                {
                    lastSelectionStart = this.SelectionStart - 1;
                }
            }

            else if (IsValidHexChar(e.KeyChar))
            {
                if (this.SelectionStart > 0 && this.Text[this.SelectionStart - 1] != HexSeparator)
                {
                    lastSelectionStart = this.SelectionStart + 2;
                }
                else
                {
                    lastSelectionStart = this.SelectionStart + 1;
                }
            }
            //按下Ctrl+v
            else if (e.KeyChar == 22)
            {
                // 获取剪贴板文本
                string clipboardText = Clipboard.GetText();

                // 删除所有非法字符
                string hexString = string.Concat(clipboardText.Where(c => ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'))));

                // 在每两个字符之间插入一个空格
                hexString = string.Join(HexSeparator.ToString(), Enumerable.Range(0, hexString.Length / 2).Select(i => hexString.Substring(i * 2, 2)));

                // 插入到当前光标位置
                int selectionStart = SelectionStart;
                if (this.SelectionLength > 0) { this.SelectedText = ""; }

                Text = Text.Insert(SelectionStart, hexString);
                if (this.SelectionStart > 0 && this.Text[this.SelectionStart - 1] == HexSeparator)
                {
                    SelectionStart = selectionStart + hexString.Length;
                }
                else
                {
                    SelectionStart = selectionStart + hexString.Length + 1;
                }

                e.Handled = true;
            }
        }

        private void HexTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (!HexMode)
            {
                return;
            }
            string hexInput = Text;

            // 移除除了HEX字符之外的所有字符
            hexInput = Regex.Replace(hexInput, "[^0-9a-fA-F]", "");

            // 在每两个字符之间添加一个间距
            StringBuilder spacedHex = new StringBuilder();
            for (int i = 0; i < hexInput.Length; i += 2)
            {
                if (i != 0)
                {
                    spacedHex.Append(HexSeparator);
                }
                if (i + 1 < hexInput.Length)
                {
                    spacedHex.Append(hexInput.Substring(i, 2));
                }
                else
                {
                    spacedHex.Append(hexInput.Substring(i));
                }
            }

            // 更新TextBox的文本
            Text = spacedHex.ToString().ToUpper();
            if (Text.Length > MaxLength)
            {
                Text = Text.Substring(0, MaxLength);
            }
            if (lastSelectionStart >= Text.Length)
            {
                SelectionStart = Text.Length;
            }
            else if (lastSelectionStart >= 0)
            {
                SelectionStart = lastSelectionStart;
            }

        }
        /// <summary>
        /// 判断是否是HEX字符串,是否已经输入完整
        /// </summary>
        /// <returns>是否是HEX字符串</returns>
        public bool IsHex()
        {
            string hexInput = Text;
            hexInput = Regex.Replace(hexInput, HexSeparator.ToString(), "");
            return hexInput.Length % 2 == 0;
        }

        private bool IsValidHexChar(char c)
        {
            return "0123456789ABCDEFabcdef".IndexOf(c) >= 0;
        }
    }
}