using System;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;

namespace SplitButtonOptionConcept
{
  /// <summary>
  /// Interaction logic for FarClipSettingWPF_B.xaml
  /// </summary>
  public partial class FarClipSettingWPF : Window
  {
    Document _doc;
    public FarClipSettingWPF( Document doc )
    {
      InitializeComponent();
      _doc = doc;
      this.Top = Properties.Settings.Default.FormFarClip_Top;
      this.Left = Properties.Settings.Default.FormFarClip_Left;
      this.Height = Properties.Settings.Default.FormFarClip_HT;
      this.Width = Properties.Settings.Default.FormFarClip_WD;
      btn_Close.IsCancel = true;
    }

    private void Window_Loaded( object sender, RoutedEventArgs e )
    {
      this.pfClip.Text = UnitFormatUtils.Format(
          _doc.GetUnits(),
          UnitType.UT_Length,
          Properties.Settings.Default.PreferFarClip,
          false,
          false );
    }

    private void Window_Closing( object sender,
        System.ComponentModel.CancelEventArgs e )
    {
      String pfc = this.pfClip.Text;
      try
      {
        UnitType clipUnit = UnitType.UT_Length;
        Units thisDocUnits = _doc.GetUnits();
        double userFarClipSetting;
        UnitFormatUtils.TryParse( thisDocUnits,
            clipUnit, pfc, out userFarClipSetting );
        Properties.Settings.Default.PreferFarClip = userFarClipSetting;

      }
      catch( Exception )
      {
        MessageBox.Show( "For some unknown reason.\n\nNo change was made.",
            "Settings Error" );
      }
      Properties.Settings.Default.FormFarClip_Top = this.Top;
      Properties.Settings.Default.FormFarClip_Left = this.Left;
      Properties.Settings.Default.FormFarClip_HT = this.Height;
      Properties.Settings.Default.FormFarClip_WD = this.Width;
      Properties.Settings.Default.Save();
    }

    private void pfClip_KeyUp( object sender, KeyEventArgs e )
    {
      if( e.Key == Key.Return )
      {
        String pfc = this.pfClip.Text;
        UnitType clipUnit = UnitType.UT_Length;
        Units thisDocUnits = _doc.GetUnits();
        double userFarClipSetting;
        UnitFormatUtils.TryParse( thisDocUnits, clipUnit, pfc,
            out userFarClipSetting );
        if( userFarClipSetting > 0 )
        {
          this.pfClip.Text = UnitFormatUtils.Format( thisDocUnits,
              clipUnit, userFarClipSetting, false, false );
        }
        else
        {
          this.pfClip.Text = "8' - 0\"";
        }
      }
    }

    private void btn_Close_Click( object sender, RoutedEventArgs e )
    {
      this.Close();
    }
  }
}
