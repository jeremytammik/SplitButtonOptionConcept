
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using System.Text;


namespace SplitButtonOptionConcept {
    [Transaction(TransactionMode.Manual)]
    public class SectionFarClipReset : IExternalCommand {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements) {
            try {
                UIApplication uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
                Document thisDoc = uidoc.Document;

                if (new RevitHelpers().NotInThisView(thisDoc)) { return Result.Succeeded; }

                Selection sel = uidoc.Selection;
                Section2dPickFilter selFilter = new Section2dPickFilter(thisDoc);

                Reference pickedRef = sel.PickObject(ObjectType.Element, selFilter, "Pick the section to far clip adjust.");
                Element elem = thisDoc.GetElement(pickedRef);

                // sheesh, another crazy linq way to do this
                FilteredElementCollector viewCollector = new FilteredElementCollector(thisDoc);
                viewCollector.OfClass(typeof(Autodesk.Revit.DB.View));
                Func<Autodesk.Revit.DB.View, bool> isNamedView = v2 => (v2.Name.Equals(elem.Name));
                Autodesk.Revit.DB.View thisV = viewCollector.Cast<Autodesk.Revit.DB.View>().First<Autodesk.Revit.DB.View>(isNamedView);

                string fcs = Properties.Settings.Default.PreferFarClip.ToString();

                using (Transaction t = new Transaction(thisDoc, "FarSide Change")) {
                    t.Start();
                    Parameter vFarclip = thisV.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
                    vFarclip.SetValueString(fcs);
                    t.Commit();
                }

            } catch (Exception ex) {
                // MessageBox.Show(ex.Message, "Sorry, No Can Do.");
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    class SectionFarClipResetOptions : IExternalCommand {
        public Result Execute(ExternalCommandData commandData,
                              ref string message,
                              ElementSet elements) {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document thisDoc = uidoc.Document;

            FarClipSettingWPF settingWPF = new FarClipSettingWPF(thisDoc);
            settingWPF.ShowDialog();
            AppFarClip.Instance.SetSplitButtonFarClipToTop();
            return Result.Succeeded;
        }
    }

    public class Section2dPickFilter : ISelectionFilter {
        Document doc = null;
        public Section2dPickFilter(Document document) {
            doc = document;
        }
        public bool AllowElement(Element e) {
            if ((BuiltInCategory)e.Category.Id.IntegerValue ==
                BuiltInCategory.OST_Viewers) {
                return true;
            }
            return false;
        }
        public bool AllowReference(Reference r, XYZ p) {
            return false;
        }
    }

    class RevitHelpers {
        public void ShowBasicLinkInfo(Element elem, Document doc) {
            string s = "You Picked: " + "\n" + "\n";
            s += "Class = " + elem.GetType().Name + "\n";
            s += "Category = " + elem.Category.Name + "\n";
            s += "Workset = " + WhatWorksetNameIsThis(elem.WorksetId, doc) + "\n" + "\n";

            if (elem.IsMonitoringLinkElement()) {
                s += "Link:" + "\n";
                IList<ElementId> linkIds = elem.GetMonitoredLinkElementIds();
                s += ((RevitLinkInstance)doc.GetElement(linkIds[0])).Name + "\n";
            } else {
                s += "Element monitors nothing." + "\n";
            }
            s += "\n";
            TaskDialog thisMsgDialog = new TaskDialog("Linked Element Info");
            thisMsgDialog.TitleAutoPrefix = false;
            thisMsgDialog.MainContent = s;
            thisMsgDialog.MainInstruction = "Linked Element Info";
            TaskDialogResult tResult = thisMsgDialog.Show();
        }

        // Returns the workset name for the workset id thiswid
        public string WhatWorksetNameIsThis(WorksetId thiswid, Document doc) {
            if (thiswid == null) {
                return String.Empty;
            }
            // Find all user worksets 
            FilteredWorksetCollector worksets
                = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset);
            foreach (Workset ws in worksets) {
                if (thiswid == ws.Id) {
                    return ws.Name.ToString();
                }
            }
            return String.Empty;
        }

        // returns true if view is not of type for intended operation
        public bool NotInThisView(Document _doc) {
            if ((_doc.ActiveView.ViewType != ViewType.CeilingPlan) & (_doc.ActiveView.ViewType != ViewType.FloorPlan)
                & (_doc.ActiveView.ViewType != ViewType.Section) & (_doc.ActiveView.ViewType != ViewType.Elevation)) {
                string msg = ".... In this " + _doc.ActiveView.ViewType.ToString() + " viewtype?";
                SayMsg("Huh? Do What?", msg);
                return true;
            }
            return false;
        }

        void SayMsg(string _title, string _msg) {
            TaskDialog thisDialog = new TaskDialog(_title);
            thisDialog.TitleAutoPrefix = false;
            thisDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
            thisDialog.MainInstruction = _msg;
            thisDialog.MainContent = "";
            TaskDialogResult tResult = thisDialog.Show();
        }
    }
}
