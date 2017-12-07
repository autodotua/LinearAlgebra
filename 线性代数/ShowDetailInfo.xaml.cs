using System.Text;
using System.Windows;

namespace 线性代数
{
    /// <summary>
    /// ShowDetailInfo.xaml 的交互逻辑
    /// </summary>
    public partial class ShowDetailInfo : Window
    {
        public ShowDetailInfo(StringBuilder text)
        {
            InitializeComponent();
            txt.Text = text.ToString();
        }
        public ShowDetailInfo(string text)
        {
            InitializeComponent();
            txt.Text = text;
        }
    }
}
