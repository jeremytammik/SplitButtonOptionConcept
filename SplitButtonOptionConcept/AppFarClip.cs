#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
#endregion

namespace SplitButtonOptionConcept {
    class AppFarClip : IExternalApplication {
        static string _path = typeof(Application).Assembly.Location;
        SplitButton sbFarClip;
        /// this external application class instance.
        internal static AppFarClip _app = null;
        /// Provide access to this class instance.
        public static AppFarClip Instance {
            get { return _app; }
        }
        public Result OnStartup(UIControlledApplication a) {
            _app = this;
            AddFarClipUtil_Example_Ribbon(a);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a) {
            return Result.Succeeded;
        }

        public void AddFarClipUtil_Example_Ribbon(UIControlledApplication a) {
            string ExecutingAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string ExecutingAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            // create ribbon tab 
            String thisNewTabName = "SPLITB_OPT";
            try {
                a.CreateRibbonTab(thisNewTabName);
            } catch (Autodesk.Revit.Exceptions.ArgumentException) {
                // Assume error generated is due to "WTA" already existing
            }
            PushButtonData pbSecAdjust = new PushButtonData("FarSideClip", "Far Clip", ExecutingAssemblyPath, ExecutingAssemblyName + ".SectionFarClipReset");
            PushButtonData pbSecAdjustOpt = new PushButtonData("FarSideClipOpt", "Settings", ExecutingAssemblyPath, ExecutingAssemblyName + ".SectionFarClipResetOptions");

            pbSecAdjust.LargeImage = NewBitmapImage(System.Reflection.Assembly.GetExecutingAssembly(), "SplitButtonOptionConcept.FarClip.png");
            pbSecAdjustOpt.LargeImage = NewBitmapImage(System.Reflection.Assembly.GetExecutingAssembly(), "SplitButtonOptionConcept.FarClipSetting.png");

            // add button tips (when data, must be defined prior to adding button.)
            pbSecAdjust.ToolTip = "Reset a section's far clipping boundary to a close distance.";
            pbSecAdjust.LongDescription = "Start this command and pick a section line. The far clipping distance will be reset to be close to the line.";

            //   Add new ribbon panel. 
            String thisNewPanelName = "Misc Utils";
            RibbonPanel thisNewRibbonPanel = a.CreateRibbonPanel(thisNewTabName, thisNewPanelName);
            
            // add button to ribbon panel
            SplitButtonData sbFarClipData = new SplitButtonData("splitFarClip", "FarClip");
            sbFarClip = thisNewRibbonPanel.AddItem(sbFarClipData) as SplitButton;
            sbFarClip.AddPushButton(pbSecAdjust);
            sbFarClip.AddPushButton(pbSecAdjustOpt);

        }

        /// <summary>
        /// Load a new icon bitmap from embedded resources.
        /// For the BitmapImage, make sure you reference WindowsBase
        /// and PresentationCore, and import the System.Windows.Media.Imaging namespace. 
        /// </summary>
        BitmapImage NewBitmapImage(System.Reflection.Assembly a, string imageName) {
            Stream s = a.GetManifestResourceStream(imageName);
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = s;
            img.EndInit();
            return img;
        }

        public void SetSplitButtonToThisOrTop(string _bName, SplitButton _splitButton) {
            IList<PushButton> sbList = _splitButton.GetItems();
            foreach (PushButton pb in sbList) {
                if (pb.Name.Equals(_bName)) {
                    _splitButton.CurrentButton = pb;
                    return;
                }
            }
            _splitButton.CurrentButton = sbList[0];
        }

        public void SetSplitButtonFarClipToTop() {
            IList<PushButton> sbList = sbFarClip.GetItems();
            sbFarClip.CurrentButton = sbList[0];
        }

    }
}
