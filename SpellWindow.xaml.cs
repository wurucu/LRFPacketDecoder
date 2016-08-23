using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LRFPacketDecoder
{
    /// <summary>
    /// Interaction logic for SpellWindow.xaml
    /// </summary>
    public partial class SpellWindow : MetroWindow
    {
        public SpellWindow(PacketProp pp)
        {
            InitializeComponent();

            Spell sp = null;
            if (pp.PType == EPacketPropType.UINT)
                sp = Statik.Spells.Where(x => x.HashID == (uint)pp.Value).FirstOrDefault();
            if (pp.PType == EPacketPropType.INT)
                sp = Statik.Spells.Where(x => x.HashID == (int)pp.Value).FirstOrDefault();

            if (sp == null)
                return;

            var champion = Statik.Champions.Where(x => x.ChampionID == sp.ChampionID).FirstOrDefault();
            if (champion != null)
                txtChampionName.Text = champion.Name;
            txtSpellID.Text = sp.HashID.ToString();
            txtAlternate.Text = sp.AlternateName;
            txtAnimate.Text = sp.AnimationName;
            txtMissile.Text = sp.MissileEffect;
            txtSpellDesc.Text = sp.Description;
            txtSpellDynamicTool.Text = sp.DynamicTooltip;
            txtSpellName.Text = sp.SpellName;
            txtTextFlag.Text = sp.TextFlags;
        }
    }
}
