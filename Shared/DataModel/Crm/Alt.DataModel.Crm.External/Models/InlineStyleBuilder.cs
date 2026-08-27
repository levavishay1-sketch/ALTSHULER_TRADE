

namespace Alt.DataModel.Crm.External.Models
{
    public static class InlineStyleBuilder
    {
        public const string Style = "style";

        public static string TableStyle
        {
            get
            {
                return $"{BorderCollapse}{BorderTop}{FontFamily}{Width100Percent}";
            }
        }

        public static string HeaderStyle
        {
            get
            {
                return $"{RtlDirection}{FontFamily}";
            }
        }

        public static string ErrorLogsCommentsListStyle
        {
            get
            {
                return $"{RtlDirection}{FontFamily}{FontSize13}";
            }
        }

        public static string CrmApiCollumnStyle
        {
            get
            {
                return $"{IncomingBackground}{Padding}{TbodyBorderBottom}{LtrDirection}";
            }
        }

        public static string OutgoingCollumnStyle
        {
            get
            {
                return $"{OutgoingBackground}{Padding}{TbodyBorderBottom}{LtrDirection}";
            }
        }

        public static string PluginsCollumnStyle
        {
            get
            {
                return $"{PluginsBackground}{Padding}{TbodyBorderBottom}{LtrDirection}";
            }
        }

        public static string ClientCollumnStyle
        {
            get
            {
                return $"{ClientSideBackground}{Padding}{TbodyBorderBottom}{LtrDirection}";
            }
        }

        public static string BorderCollapse
        {
            get
            {
                return $"{CSSProperties.borderCollapse}:collapse;";
            }
        }

        public static string FontSize13
        {
            get
            {
                return $"{CSSProperties.font_size}:13px;";
            }
        }

        public static string FontSize12
        {
            get
            {
                return $"{CSSProperties.font_size}:12px;";
            }
        }

        public static string TheadRowStyle
        {
            get
            {
                return $"{CenterTextAlign}{Padding}{TheadBorderBottom}{WordWrapBreakWord}";
            }
        }

        public static string CenterTextAlign
        {
            get
            {
                return $"{CSSProperties.textAlign}:center;";
            }
        }

        public static string TbodyBorderBottom
        {
            get
            {
                return $"{CSSProperties.border_bottom}:1px solid #dddddd;";
            }
        }

        public static string BorderTop
        {
            get
            {
                return $"{CSSProperties.border_top}:1px solid black;";
            }
        }

        public static string TheadBorderBottom
        {
            get
            {
                return $"{CSSProperties.border_bottom}:1px solid black;";
            }
        }

        public static string IncomingBackground
        {
            get
            {
                return $"{CSSProperties.background}:#fff8dc;";
            }
        }

        public static string RtlDirection
        {
            get
            {
                return $"{CSSProperties.direction}:rtl;";
            }
        }

        public static string LtrDirection
        {
            get
            {
                return $"{CSSProperties.direction}:ltr;";
            }
        }

        public static string Padding
        {
            get
            {
                return $"{CSSProperties.padding}:7px;";
            }
        }

        public static string PluginsBackground
        {
            get
            {
                return $"{CSSProperties.background}:#e0ecf4;";
            }
        }

        public static string OutgoingBackground
        {
            get
            {
                return $"{CSSProperties.background}:#f0ffff;";
            }
        }

        public static string ClientSideBackground
        {
            get
            {
                return $"{CSSProperties.background}:#f5f5f5;";
            }
        }

        public static string Margin
        {
            get
            {
                return $"{CSSProperties.margin}:15px 0px 5px 0px;";
            }
        }

        public static string MarginRight
        {
            get
            {
                return $"{CSSProperties.margin_right}:20px;";
            }
        }

        public static string FontFamily
        {
            get
            {
                return $"{CSSProperties.font_family}:Tahoma;";
            }
        }

        public static string MinWidth200px
        {
            get
            {
                return $"{CSSProperties.min_width}:200px;";
            }
        }

        public static string MinWidth100px
        {
            get
            {
                return $"{CSSProperties.min_width}:100px;";
            }
        }

        public static string Width100Percent
        {
            get
            {
                return $"{CSSProperties.width}:100%;";
            }
        }

        public static string MaxWidth300px
        {
            get
            {
                return $"{CSSProperties.max_width}:300px;";
            }
        }

        public static string WordWrapBreakWord
        {
            get
            {
                return $"{CSSProperties.word_wrap}:break-word;";
            }
        }
    }
}
