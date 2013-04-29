﻿#region

using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Debugger;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Parsing.Intellisense;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Data.Decision;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;
using Dev2.Enums;
using Dev2.Factories;
using Dev2.Interfaces;
using Dev2.Studio.ActivityDesigners;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Wizards;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Enums;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Wizards;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views;
using Dev2.Studio.Views.Workflow;
using Dev2.Util;
using Dev2.Utilities;
using Microsoft.VisualBasic.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework;

#endregion

namespace Dev2.Studio.ViewModels.Workflow
{
    public class WorkflowDesignerViewModel : BaseWorkSurfaceViewModel,
        IWorkflowDesignerViewModel, IDisposable,
        IHandle<UpdateResourceMessage>, 
        IHandle<AddStringListToDataListMessage>, 
        IHandle<AddMissingAndFindUnusedDataListItemsMessage>,
                                             IHandle<AddRemoveDataListItemsMessage>,
                                             IHandle<FindMissingDataListItemsMessage>,
                                             IHandle<ShowActivityWizardMessage>,
                                             IHandle<ShowActivitySettingsWizardMessage>,
        IHandle<EditActivityMessage>
    {
        #region Fields

        private readonly IDesignerManagementService _designerManagementService;

        private RelayCommand _collapseAllCommand;

        private RelayCommand _expandAllCommand;
        private IList<IDataListVerifyPart> _filteredDataListParts;
        public delegate void SourceLocationEventHandler(SourceLocation src);

        dynamic _dataObject;
        private Point _lastDroppedPoint;
        private ModelItem _lastDroppedModelItem;
        private UserControl _popupContent;
        private IContextualResourceModel _resourceModel;
        private Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts;
        private ViewStateService _viewstateService;
        private WorkflowDesigner _wd;
        private DesignerMetadata _wdMeta;
        private DsfActivityDropViewModel _vm;
        private ModelService _modelService;

        #endregion

        #region Constructor

        public WorkflowDesignerViewModel(IContextualResourceModel resource, bool createDesigner = true)
        {
            SecurityContext = ImportService.GetExportValue<IFrameworkSecurityContext>();
            PopUp = ImportService.GetExportValue<IPopupController>();
            WizardEngine = ImportService.GetExportValue<IWizardEngine>();
            _resourceModel = resource;
            _designerManagementService = new DesignerManagementService(_resourceModel.Environment.ResourceRepository);
            if (createDesigner) ActivityDesignerHelper.AddDesignerAttributes(this);
        }

        #endregion

        #region Properties

        public IFrameworkSecurityContext SecurityContext { get; set; }

        //2012.10.01: massimo.guerrera - Add Remove buttons made into one:)
        public IPopupController PopUp { get; set; }

        public IWizardEngine WizardEngine { get; set; }

        public IList<IDataListVerifyPart> WorkflowVerifiedDataParts
        {
            get { return _filteredDataListParts; }
                }

        public string DesignerText
        {
            get { return _wd.Text; }
        }

        public UserControl PopupContent
        {
            get { return _popupContent; }
            set
            {
                _popupContent = value;
                NotifyOfPropertyChange(() => PopupContent);
            }
        }

        public object SelectedModelItem
        {
            get
            {
                if (_wd != null)
                {
                    return _wd.Context.Items.GetValue<Selection>().SelectedObjects.FirstOrDefault();
            }
                return null;
            }
        }

        public bool HasErrors { get; set; }

        public IContextualResourceModel ResourceModel
        {
            get { return _resourceModel; }
            set { _resourceModel = value; }
        }

        public string WorkflowName
        {
            get { return _resourceModel.ResourceName; }
            }

        public string ServiceDefinition
        {
            get
            {
                if (_wd != null)
                {
                _wd.Flush();
                return _wd.Text;
            }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (_wd != null)
                {
                _wd.Flush();
                _resourceModel.WorkflowXaml = _wd.Text;
            }
        }
        }

        public bool RequiredSignOff
        {
            get { return _resourceModel.RequiresSignOff; }
            }

        public string AuthorRoles
        {
            get { return _resourceModel.AuthorRoles; }
            set { _resourceModel.AuthorRoles = value; }
            }

        public WorkflowDesigner Designer
        {
            get { return _wd; }
            }

        public UIElement DesignerView
        {
            get { return _wd.View; }
            }

        #endregion

        #region Commands

        public ICommand CollapseAllCommand
        {
            get
            {
                if (_collapseAllCommand == null)
                {
                    _collapseAllCommand = new RelayCommand(param =>
                    {
                        bool val = Convert.ToBoolean(param);
                        if (val)
                        {
                            _designerManagementService.RequestCollapseAll();
                        }
                        else
                        {
                            _designerManagementService.RequestRestoreAll();
                        }
                    }, param => true);
                }
                return _collapseAllCommand;
            }
        }

        public ICommand ExpandAllCommand
        {
            get
            {
                if (_expandAllCommand == null)
                {
                    _expandAllCommand = new RelayCommand(param =>
                    {
                        bool val = Convert.ToBoolean(param);
                        if (val)
                        {
                            _designerManagementService.RequestExpandAll();
                        }
                        else
                        {
                            _designerManagementService.RequestRestoreAll();
                        }
                    }, param => true);
                }
                return _expandAllCommand;
            }
        }

        #endregion

        #region Private Methods

        void PerformAddItems(List<ModelItem> addedItems)
        {
            for (int i = 0; i < addedItems.Count(); i++)
            {
                ModelItem mi = addedItems.ToList()[i];

                if (mi != null && (mi.Parent.Parent.Parent != null) && mi.Parent.Parent.Parent.ItemType == typeof(FlowSwitch<string>))
                {
                    #region Extract the Switch Expression ;)

                    ModelProperty activityExpression = mi.Parent.Parent.Parent.Properties["Expression"];

                    var tmpModelItem = activityExpression.Value;

                    var switchExpressionValue = string.Empty;

                    if (tmpModelItem != null)
                    {
                        var tmpProperty = tmpModelItem.Properties["ExpressionText"];

                        if (tmpProperty != null)
                        {
                            var tmp = tmpProperty.Value.ToString();
                            // Dev2DecisionHandler.Instance.FetchSwitchData("[[res]]",AmbientDataList)

                            if (!string.IsNullOrEmpty(tmp))
                            {
                                int start = tmp.IndexOf("(");
                                int end = tmp.IndexOf(",");

                                if (start < end && start >= 0)
                                {
                                    start += 2;
                                    end -= 1;
                                    switchExpressionValue = tmp.Substring(start, (end - start));
                                }
                            }
                        }
                    }

                    #endregion

                    if ((mi.Properties["Key"].Value != null) && mi.Properties["Key"].Value.ToString().Contains("Case"))
                    {
                        Tuple<ConfigureCaseExpressionTO, IEnvironmentModel> wrapper = new Tuple<ConfigureCaseExpressionTO, IEnvironmentModel>(new ConfigureCaseExpressionTO() { TheItem = mi, ExpressionText = switchExpressionValue }, _resourceModel.Environment);
                        EventAggregator.Publish(new ConfigureCaseExpressionMessage(wrapper));
                    }
                }

                if (mi.ItemType == typeof(FlowSwitch<string>))
                {
                    //This line is necessary to fix the issue were decisions and switches didn't have the correct positioning when dragged on
                    SetLastDroppedModelItem(mi);

                    // Travis.Frisinger : 28.01.2013 - Switch Amendments
                    Tuple<ModelItem, IEnvironmentModel> wrapper = new Tuple<ModelItem, IEnvironmentModel>(mi, _resourceModel.Environment);
                    EventAggregator.Publish(new ConfigureSwitchExpressionMessage(wrapper));
                }

                if (mi.ItemType == typeof(FlowDecision))
                {
                    //This line is necessary to fix the issue were decisions and switches didn't have the correct positioning when dragged on
                    SetLastDroppedModelItem(mi);

                    Tuple<ModelItem, IEnvironmentModel> wrapper = new Tuple<ModelItem, IEnvironmentModel>(mi, _resourceModel.Environment);
                    EventAggregator.Publish(new ConfigureDecisionExpressionMessage(wrapper));
                }

                if (mi.ItemType == typeof(FlowStep))
                {
                    if (mi.Properties["Action"].ComputedValue is DsfWebPageActivity)
                    {
                        var modelService = Designer.Context.Services.GetService<ModelService>();
                        var items = modelService.Find(modelService.Root, typeof(DsfWebPageActivity));

                        int totalActivities = items.Count();

                        items.Last().Properties["DisplayName"].SetValue(string.Format("Webpage {0}", totalActivities.ToString()));
                    }

                    if (mi.Properties["Action"].ComputedValue is DsfActivity)
                    {
                        DsfActivity droppedActivity = mi.Properties["Action"].ComputedValue as DsfActivity;

                        if (!string.IsNullOrEmpty(droppedActivity.ServiceName))
                        {
                            //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                            IContextualResourceModel resource = _resourceModel.Environment.ResourceRepository.FindSingle(
                                c => c.ResourceName == droppedActivity.ServiceName) as IContextualResourceModel;
                            droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false);
                            mi.Properties["Action"].SetValue(droppedActivity);
                        }
                        else
                        {
                            if (_dataObject != null)
                            {
                                var navigationItemViewModel = _dataObject as ResourceTreeViewModel;

                                if (navigationItemViewModel != null)
                                {
                                    var resource = navigationItemViewModel.DataContext;

                                    if (resource != null)
                                    {
                                        //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                                        DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, true);

                                        d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.ResourceName;
                                        d.IconPath = resource.IconPath;
                                        mi.Properties["Action"].SetValue(d);
                                    }
                                }

                                _dataObject = null;
                            }
                            else
                            {
                                //Massimo.Guerrera:17-04-2012 - PBI 9000                               
                                if (_vm != null)
                                {
                                    IContextualResourceModel resource = _vm.SelectedResourceModel;
                                    if (resource != null)
                                    {
                                        droppedActivity.ServiceName = droppedActivity.DisplayName = droppedActivity.ToolboxFriendlyName = resource.ResourceName;
                                        droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false);
                                        mi.Properties["Action"].SetValue(droppedActivity);
                                    }
                                    _vm = null;
                                }
                            }
                        }
                    }
                }
                i++;
            }
        }       

        /// <summary>
        ///     Updates the location of last dropped model item.
        ///     !!!!!!!!!!!!!!!!!!This method is called alot, please ensure all logic stays in the if statement which checks for nulls AND avoid doing any heavy work in it!
        /// </summary>
        private void UpdateLocationOfLastDroppedModelItem()
        {
            if (_lastDroppedModelItem != null && _lastDroppedModelItem.View != null)
            {
                AutomationProperties.SetAutomationId(_lastDroppedModelItem.View,
                                                     _lastDroppedModelItem.ItemType.ToString());
                _viewstateService.StoreViewState(_lastDroppedModelItem, "ShapeLocation", _lastDroppedPoint);
                _lastDroppedModelItem = null;
            }
        }

        private void EditActivity(ModelItem modelItem)
        {
            if (Designer == null) return;
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root && modelItem.ItemType == typeof (DsfActivity))
            {
                var modelProperty = modelItem.Properties["ServiceName"];
                if (modelProperty != null)
                {
                    var res = modelProperty.ComputedValue;

                    var resource =
                        _resourceModel.Environment.ResourceRepository.FindSingle(c => c.ResourceName == res.ToString());

                    if (resource != null)
                    {
                        switch (resource.ResourceType)
                        {
                            case ResourceType.WorkflowService:
                                EventAggregator.Publish(new AddWorkSurfaceMessage(resource));
                                break;

                            case ResourceType.Service:
                                EventAggregator.Publish(new ShowEditResourceWizardMessage(resource));
                                break;
                        }
                    }
                }
            }
        }

        private void ShowActivitySettingsWizard(ModelItem modelItem)
        {
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root)
            {
                if (WizardEngine == null)
                {
                    return;
                }
                WizardInvocationTO activityWizardTO = null;
                try
                {
                    activityWizardTO = WizardEngine.GetActivitySettingsWizardInvocationTO(modelItem, _resourceModel);
                }
                catch (Exception e)
                {
                    PopUp.Show(e.Message, "Error");
                }
                ShowWizard(activityWizardTO);
            }
        }

        private void ShowActivityWizard(ModelItem modelItem)
        {
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root)
            {
                if (WizardEngine == null)
                {
                    return;
                }
                WizardInvocationTO activityWizardTO = null;


                if (!WizardEngine.HasWizard(modelItem, _resourceModel.Environment))
                {
                    PopUp.Show("Wizard not found.", "Missing Wizard");
                    return;
                }
                try
                {
                    activityWizardTO = WizardEngine.GetActivityWizardInvocationTO(modelItem, _resourceModel);
                }
                catch (Exception e)
                {
                    PopUp.Show(e.Message, "Error");
                }
                ShowWizard(activityWizardTO);
            }
        }

        private void ShowWizard(WizardInvocationTO activityWizardTO)
        {
            if (activityWizardTO != null)
            {
                var activitySettingsView = new ActivitySettingsView();
                var activitySettingsViewModel =
                    new ActivitySettingsViewModel(activityWizardTO, _resourceModel);

                activitySettingsViewModel.Close += delegate
                {
                    activitySettingsView.Dispose();
                    activitySettingsViewModel.Dispose();
                    activitySettingsView.DataContext = null;
                    PopupContent = null;
                };

                activitySettingsView.DataContext = activitySettingsViewModel;
                bool showPopUp = activitySettingsViewModel.InvokeWizard();

                if (showPopUp)
                {
                    PopupContent = activitySettingsView;
                }
            }
            else
            {
                PopupContent = null;
            }
        }

        private ModelItem RecursiveForEachCheck(dynamic activity)
        {
            var innerAct = activity.DataFunc.Handler as ModelItem;
            if (innerAct != null)
            {
                if (innerAct.ItemType == typeof (DsfForEachActivity))
                {
                    innerAct = RecursiveForEachCheck(innerAct);
                }
            }
            return innerAct;
        }

        /// <summary>
        ///     Prevents the delete from being executed if it is a FlowChart.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void PreventDeleteFromBeingExecuted(CanExecuteRoutedEventArgs e)
        {
            if (Designer != null && Designer.Context != null)
            {
                var selection = Designer.Context.Items.GetValue<Selection>();

                if (selection == null || selection.PrimarySelection == null) return;

                if (selection.PrimarySelection.ItemType != typeof (Flowchart) &&
                    selection.SelectedObjects.All(modelItem => modelItem.ItemType != typeof (Flowchart))) return;
            }

            e.CanExecute = false;
            e.Handled = true;
        }

        /// <summary>
        ///     Sets the last dropped point.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="DragEventArgs" /> instance containing the event data.
        /// </param>
        private void SetLastDroppedPoint(DragEventArgs e)
        {
            var senderAsFrameworkElement = _modelService.Root.View as FrameworkElement;
            if (senderAsFrameworkElement != null)
            {
                UIElement freePormPanel = senderAsFrameworkElement.FindNameAcrossNamescopes("flowchartPanel");
                if (freePormPanel != null)
                {
                    _lastDroppedPoint = e.GetPosition(freePormPanel);
                }
            }
        }

        /// <summary>
        ///     Sets the last dropped model item.
        /// </summary>
        /// <param name="modelItem">The model item.</param>
        private void SetLastDroppedModelItem(ModelItem modelItem)
        {
            _lastDroppedModelItem = modelItem;
        }

        #region DataList Workflow Specific Methods

        // We will be assuming that a workflow field is a recordset based on 2 criteria:
        // 1. If the field contains a set of parenthesis
        // 2. If the field contains a period, it is a recordset with a field.
        private IList<IDataListVerifyPart> BuildWorkflowFields()
        {
            var dataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
            _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(dataPartVerifyDuplicates);
            if (Designer != null)
            {
                var modelService = Designer.Context.Services.GetService<ModelService>();
                var flowNodes = modelService.Find(modelService.Root, typeof (FlowNode));

                foreach (var flowNode in flowNodes)
                {
                    var workflowFields = GetWorkflowFieldsFromModelItem(flowNode);
                    foreach (var field in workflowFields)
                    {
                        BuildDataPart(field);
                        }
                    }
            }
            var flattenedList = _uniqueWorkflowParts.Keys.ToList();
            return flattenedList;
        }

        private List<string> GetWorkflowFieldsFromModelItem(ModelItem flowNode)
                    {
            var workflowFields = new List<string>();

            var modelProperty = flowNode.Properties["Action"];
            if (modelProperty != null)
                        {
                var activity = modelProperty.ComputedValue;
                workflowFields = GetActivityElements(activity);
            }
            else
            {
                            string propertyName = string.Empty;
                switch (flowNode.ItemType.Name)
                            {
                    case "FlowDecision":
                                propertyName = "Condition";
                        break;
                    case "FlowSwitch`1":
                                propertyName = "Expression";
                        break;
                            }
                var property = flowNode.Properties[propertyName];
                if (property != null)
                {
                    var activity = property.ComputedValue;
                            if (activity != null)
                            {
                                workflowFields = GetDecisionElements(activity);
                            }
                        }
                else
                        {
                    return workflowFields;
                        }
                    }
            return workflowFields;
                    }

        private List<String> GetDecisionElements(dynamic decision)
        {
            var DecisionFields = new List<string>();
            string expression = decision.ExpressionText;
            int startIndex = expression.IndexOf('"');
            startIndex = startIndex + 1;
            int endindex = expression.IndexOf('"', startIndex);
            string decisionValue = expression.Substring(startIndex, endindex - startIndex);

            // Travis.Frisinger - 25.01.2013 
            // We now need to parse this data for regions ;)

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            // NEED - DataList for active workflow
            IList<IIntellisenseResult> parts = parser.ParseDataLanguageForIntellisense(decisionValue,
                                                                                       DataListSingleton.ActiveDataList
                                                                                                        .WriteToResourceModel
                                                                                           (), true);

            // push them into the list
            foreach (var p in parts)
            {
                DecisionFields.Add(DataListUtil.StripBracketsFromValue(p.Option.DisplayValue));
            }

            return DecisionFields;
        }

        private void BuildDataPart(string DataPartFieldData)
        {
            DataPartFieldData = DataListUtil.StripBracketsFromValue(DataPartFieldData);
            IDataListVerifyPart verifyPart;
            string fullyFormattedStringValue;
            string[] fieldList = DataPartFieldData.Split('.');
            if (fieldList.Count() > 1 && !String.IsNullOrEmpty(fieldList[0]))
            {
                // If it's a RecordSet Containing a field
                foreach (var item in fieldList)
                {
                    if (item.EndsWith(")") && item == fieldList[0])
                    {
                        if (item.Contains("("))
                        {
                            fullyFormattedStringValue = RemoveRecordSetBrace(item);
                            verifyPart =
                                IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue,
                                                                                          String.Empty);
                            AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                        }
                        else
                        {
                            // If it's a field containing a single brace
                            continue;
                        }
                    }
                    else if (item == fieldList[1] && !(item.EndsWith(")") && item.Contains(")")))
                    {
                        // If it's a field to a record set
                        verifyPart =
                            IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                RemoveRecordSetBrace(fieldList.ElementAt(0)), item);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (fieldList.Count() == 1 && !String.IsNullOrEmpty(fieldList[0]))
            {
                // If the workflow field is simply a scalar or a record set without a child
                if (DataPartFieldData.EndsWith(")") && DataPartFieldData == fieldList[0])
                {
                    if (DataPartFieldData.Contains("("))
                    {
                        fullyFormattedStringValue = RemoveRecordSetBrace(fieldList[0]);
                        verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(
                            fullyFormattedStringValue, String.Empty);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                }
                else
                {
                    verifyPart =
                        IntellisenseFactory.CreateDataListValidationScalarPart(RemoveRecordSetBrace(DataPartFieldData));
                    AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                }
            }
        }

        private List<String> GetActivityElements(object activity)
        {
            var assign = activity as DsfActivityAbstract<string>;
            var other = activity as DsfActivityAbstract<bool>;
            enFindMissingType findMissingType;

            if (assign != null)
            {
                findMissingType = assign.GetFindMissingType();
            }

            else if (other != null)
            {
                findMissingType = other.GetFindMissingType();
            }
            else
            {
                return new List<String>();
            }

            var activityFields = new List<string>();

            var stratFac = new Dev2FindMissingStrategyFactory();

            IFindMissingStrategy strategy = stratFac.CreateFindMissingStrategy(findMissingType);

            foreach (var activityField in strategy.GetActivityFields(activity))
                    {
                if (!string.IsNullOrEmpty(activityField))
                        {
                    activityFields.AddRange(
                        (FormatDsfActivityField(activityField)).Where(item => !item.Contains("xpath(")));
                        }
                    }
            return activityFields;
                        }

        private List<IDataListVerifyPart> MissingDataListParts(IList<IDataListVerifyPart> partsToVerify)
        {
            var MissingDataParts = new List<IDataListVerifyPart>();
            foreach (var part in partsToVerify)
            {
                if (DataListSingleton.ActiveDataList != null)
                {
                if (!(part.IsScalar))
                {
                        var recset =
                            DataListSingleton.ActiveDataList.DataList.Where(
                                c => c.Name == part.Recordset && c.IsRecordset).ToList();
                    if (!recset.Any())
                    {
                        MissingDataParts.Add(part);
                    }
                    else
                    {
                            if (!string.IsNullOrEmpty(part.Field) &&
                                recset[0].Children.Count(c => c.Name == part.Field) == 0)
                        {
                            MissingDataParts.Add(part);
                        }
                    }
                }
                    else if (DataListSingleton.ActiveDataList.DataList
                        .Count(c => c.Name == part.Field && !c.IsRecordset) == 0)
                {
                    MissingDataParts.Add(part);
                }
            }
            }
            return MissingDataParts;
        }


        private bool Equals(IDataListVerifyPart part, IDataListVerifyPart part2)
        {
            if (part.DisplayValue == part2.DisplayValue)
            {
                return true;
            }
            return false;
        }

        private List<IDataListVerifyPart> MissingWorkflowItems(IList<IDataListVerifyPart> PartsToVerify)
        {
            var MissingWorkflowParts = new List<IDataListVerifyPart>();
            if (DataListSingleton.ActiveDataList != null && DataListSingleton.ActiveDataList.DataList != null)
                foreach (var dataListItem in DataListSingleton.ActiveDataList.DataList)
            {
                if (String.IsNullOrEmpty(dataListItem.Name))
                {
                    continue;
                }
                if ((dataListItem.Children.Count > 0))
                {
                    if (PartsToVerify.Count(part => part.Recordset == dataListItem.Name) == 0)
                    {
                        //19.09.2012: massimo.guerrera - Added in the description to creating the part
                        if (dataListItem.IsEditable)
                        {
                                MissingWorkflowParts.Add(
                                    IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.Name,
                                                                                              String.Empty,
                                                                                              dataListItem.Description));
                            foreach (var child in dataListItem.Children)
                                if (!(String.IsNullOrEmpty(child.Name)))
                                    //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                    if (dataListItem.IsEditable)
                                    {
                                            MissingWorkflowParts.Add(
                                                IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                                    dataListItem.Name, child.Name, child.Description));
                                    }
                        }
                    }
                        else
                            foreach (var child in dataListItem.Children)
                                if (
                                    PartsToVerify.Count(
                                        part => part.Field == child.Name && part.Recordset == child.Parent.Name) == 0)
                            {
                                //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                if (child.IsEditable)
                                {
                                        MissingWorkflowParts.Add(
                                            IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                                dataListItem.Name, child.Name, child.Description));
                                }
                            }
                }
                else if (PartsToVerify.Count(part => part.Field == dataListItem.Name) == 0)
                {
                    {
                        if (dataListItem.IsEditable)
                        {
                            //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                MissingWorkflowParts.Add(
                                    IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.Name,
                                                                                           dataListItem.Description));
                        }
                    }
                }
            }
            return MissingWorkflowParts;
        }


        private IList<String> FormatDsfActivityField(string activityField)
        {
            // Sashen: 09-10-2012 : Using the new parser
            var intellisenseParser = new SyntaxTreeBuilder();

            IList<string> result = new List<string>();
            Node[] nodes = intellisenseParser.Build(activityField);
            if (intellisenseParser.EventLog.HasEventLogs)
            {
                //2013.01.23: Ashley Lewis - Removed this condition for Bug 6413
                //if (intellisenseParser.EventLog.GetEventLogs().First().ErrorStart.Contents == "{{")
                //{
                // Sashen: 09-10-2012: If the new parser is unable to parse a region, use the old one.
                //                     this is only to cater for the parser being unable to parse "{{" and literals - TEMPORARY
                // TO DO: Remove this when new parser caters for all cases.

                IDev2StudioDataLanguageParser languageParser = DataListFactory.CreateStudioLanguageParser();

                try
                {
                    result = languageParser.ParseForActivityDataItems(activityField);
                }
                catch (Dev2DataLanguageParseError)
                {
                    return new List<String>();
                }
                catch (NullReferenceException)
                {
                    return new List<String>();
                }
                //}
            }
            var allNodes = new List<Node>();


            if (nodes.Count() > 0 && !(intellisenseParser.EventLog.HasEventLogs))
            {
                nodes[0].CollectNodes(allNodes);

                for (int i = 0; i < allNodes.Count; i++)
                {
                    if (allNodes[i] is DatalistRecordSetNode)
                    {
                        var refNode = allNodes[i] as DatalistRecordSetNode;
                        string nodeName = refNode.GetRepresentationForEvaluation();
                        nodeName = nodeName.Substring(2, nodeName.Length - 4);
                        result.Add(nodeName);
                    }
                    else if (allNodes[i] is DatalistReferenceNode)
                    {
                        var refNode = allNodes[i] as DatalistReferenceNode;
                        string nodeName = refNode.GetRepresentationForEvaluation();
                        nodeName = nodeName.Substring(2, nodeName.Length - 4);
                        result.Add(nodeName);
                    }
                    else if (allNodes[i] is DatalistRecordSetFieldNode)
                    {
                        var refNode = allNodes[i] as DatalistRecordSetFieldNode;
                        string nodeName = refNode.GetRepresentationForEvaluation();
                        nodeName = nodeName.Substring(2, nodeName.Length - 4);
                        result.Add(nodeName);
                    }
                }
            }
            return result;
        }

        public string GetValueFromUnlimitedObject(UnlimitedObject activityField, string valueToGet)
        {
            List<UnlimitedObject> enumerableResultSet = activityField.GetAllElements(valueToGet);
            string result = string.Empty;
            foreach (var item in enumerableResultSet)
            {
                if (!string.IsNullOrEmpty(item.GetValue(valueToGet)))
                {
                    result = item.GetValue(valueToGet);
                }
            }

            return result;
        }

        private string RemoveRecordSetBrace(string RecordSet)
        {
            string fullyFormattedStringValue;
            if (RecordSet.Contains("(") && RecordSet.Contains(")"))
            {
                fullyFormattedStringValue = RecordSet.Remove(RecordSet.IndexOf("("));
            }
            else
                return RecordSet;
            return fullyFormattedStringValue;
        }

        private string RemoveSquareBraces(string field)
        {
            return (field.Contains("[[") && field.Contains("]]")) ? field.Replace("[[", "").Replace("]]", "") : field;
        }

        private void AddDataVerifyPart(IDataListVerifyPart part, string nameOfPart)
        {
            if (!_uniqueWorkflowParts.ContainsValue(nameOfPart))
            {
                _uniqueWorkflowParts.Add(part, nameOfPart);
            }
        }

        #endregion

        #endregion

        #region Internal Methods

        internal void OnItemSelected(Selection item)
        {
            NotifyItemSelected(item.PrimarySelection);
        }

        #endregion Internal Methods

        #region Public Methods

        /// <summary>
        ///     Handles the adding of the missing variables to the datalist and also sets the unused datalist items
        /// </summary>
        /// <param name="message">The message.</param>
        /// <author>Massimo.Guerrera</author>
        /// <date>2013/02/06</date>
        public void Handle(AddMissingAndFindUnusedDataListItemsMessage message)
        {
            if (ResourceModel == message.CurrentResourceModel)
            {
                AddMissingWithNoPopUpAndFindUnusedDataListItems();
            }
            }

        /// <summary>
        ///     Handels the list of strings to be added to the data list without a pop up message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <author>Massimo.Guerrera</author>
        /// <date>2013/02/06</date>
        public void Handle(AddStringListToDataListMessage message)
        {
            IDataListViewModel dlvm = DataListSingleton.ActiveDataList;
            if (dlvm != null)
            {
                var dataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
                _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(dataPartVerifyDuplicates);
                foreach (var s in message.ListToAdd)
            {
                    BuildDataPart(s);
                }
                IList<IDataListVerifyPart> partsToAdd = _uniqueWorkflowParts.Keys.ToList();
                List<IDataListVerifyPart> uniqueDataListPartsToAdd = MissingDataListParts(partsToAdd);
                dlvm.AddMissingDataListItems(uniqueDataListPartsToAdd);
            }
            }

        public void Handle(UpdateResourceMessage message)
            {
            if (
                ContexttualResourceModelEqualityComparer.Current.Equals(
                    message.ResourceModel as IContextualResourceModel, _resourceModel))
            {
                _resourceModel.Update(message.ResourceModel);
        }
        }

        public bool NotifyItemSelected(object primarySelection)
        {
            var selectedItem = primarySelection as ModelItem;

            bool isItemSelected = false;
            if (selectedItem != null)
            {
                if (selectedItem.ItemType == typeof (DsfForEachActivity))
                {
                    dynamic test = selectedItem;
                    ModelItem innerActivity = RecursiveForEachCheck(test);
                    if (innerActivity != null)
                    {
                        selectedItem = innerActivity;
                    }
                }
                if (selectedItem.Properties["DisplayName"] != null)
                {
                    var modelProperty = selectedItem.Properties["DisplayName"];

                    if (modelProperty != null)
                    {
                        string displayName = modelProperty.ComputedValue.ToString();
                        var resourceModel = _resourceModel.Environment.ResourceRepository.All().FirstOrDefault(resource => resource.ResourceName.Equals(displayName, StringComparison.InvariantCultureIgnoreCase));

                        if (resourceModel != null || selectedItem.ItemType == typeof (DsfWebPageActivity))
                        {
                            IWebActivity webActivity = WebActivityFactory
                                .CreateWebActivity(selectedItem, resourceModel as IContextualResourceModel, displayName);
                            isItemSelected = true;
                        }
                    }
                }
            }
            return isItemSelected;
        }

        public void BindToModel()
        {
            if (_wd == null)
            {
                return;
            }

            _wd.Flush();
            _resourceModel.WorkflowXaml = _wd.Text;
            if (string.IsNullOrEmpty(_resourceModel.ServiceDefinition))
            {
                _resourceModel.ServiceDefinition = _resourceModel.ToServiceDefinition();
            }
        }

        public ActivityBuilder GetBaseUnlimitedFlowchartActivity()
        {
            var flowchart = new Flowchart
                {
                    DisplayName = _resourceModel.ResourceName
                };

            EnsureVariables(flowchart);
            AddDev2ImportsToActivity(flowchart);

            var emptyWorkflow = new ActivityBuilder
                {
                    Name = _resourceModel.ResourceName,
                    Properties =
                        {
                            new DynamicActivityProperty
                                {
                                    Name = "AmbientDataList",
                                    Type = typeof (InOutArgument<List<string>>)
                                }
                            ,
                            new DynamicActivityProperty
                                {
                                    Name = "ParentWorkflowInstanceId",
                                    Type = typeof (InOutArgument<Guid>)
                                }
                            ,
                            new DynamicActivityProperty
                                {
                                    Name = "ParentServiceName",
                                    Type = typeof (InOutArgument<string>)
                                }
                        },
                    Implementation = flowchart
                };

            return emptyWorkflow;
        }

        public void InitializeDesigner(IDictionary<Type, Type> designerAttributes)
        {
            _wd = new WorkflowDesigner();

            _wdMeta = new DesignerMetadata();
            _wdMeta.Register();
            var builder = new AttributeTableBuilder();
            foreach (var designerAttribute in designerAttributes)
            {
                builder.AddCustomAttributes(designerAttribute.Key, new DesignerAttribute(designerAttribute.Value));
            }

            MetadataStore.AddAttributeTable(builder.CreateTable());
            if (string.IsNullOrEmpty(_resourceModel.WorkflowXaml))
            {
                _wd.Load(GetBaseUnlimitedFlowchartActivity());
                BindToModel();
            }
            else
            {
                _wd.Text = _resourceModel.WorkflowXaml;
                _wd.Load();
            }

            _wdMeta.Register();

            _wd.Context.Services.Subscribe<ModelService>(instance =>
                {
                    _modelService = instance;
                    _modelService.ModelChanged += ModelServiceModelChanged;
                });

            //_modelService = _wd.Context.Services.GetService<ModelService>();
            //_modelService.ModelChanged += new EventHandler<ModelChangedEventArgs>(ModelServiceModelChanged);

            _wd.Context.Services.Subscribe<ViewStateService>(instance => { _viewstateService = instance; });

            //_viewstateService = _wd.Context.Services.GetService<ViewStateService>();

            _wd.View.PreviewDrop += ViewPreviewDrop;
            _wd.View.PreviewMouseDown += ViewPreviewMouseDown;
            _wd.View.LayoutUpdated += ViewOnLayoutUpdated;

            _wd.Context.Services.Subscribe<DesignerView>(
                instance => { instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom; });

            //_wd.Context.Services.GetService<DesignerView>().WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom;
            _wd.Context.Items.Subscribe<Selection>(OnItemSelected);

            _wd.Context.Services.Publish(_designerManagementService);

            //Jurie.Smit 2013/01/03 - Added to disable the deleting of the root flowchart
            CommandManager.AddPreviewCanExecuteHandler(_wd.View, CanExecuteRoutedEventHandler);

            _wd.ModelChanged += WdOnModelChanged;

            // Ensure all the Dev2 namespaces on the current workflow are there
            EnsureWorkflowState();
        }

        private void WdOnModelChanged(object sender, EventArgs eventArgs)
        {
            //This will auto save the workflow if anything changes on the design surface.Comment back in if we want the  workflows to auto save.
            //BindToModel();
        }

        /// <summary>
        ///     Clears all imports from an activity
        /// </summary>
        private void ClearActivityImports(Activity activity)
        {
            VisualBasicSettings vbsettings = VisualBasic.GetSettings(activity);
            if (vbsettings == null)
            {
                vbsettings = new VisualBasicSettings();
            }

            vbsettings.ImportReferences.Clear();

            VisualBasic.SetSettings(activity, vbsettings);
        }

        /// <summary>
        ///     Adds an import to an activity
        /// </summary>
        private void AddImportToActivity(Activity activity, string assemblyName, string ns)
        {
            VisualBasicSettings vbsettings = VisualBasic.GetSettings(activity);
            if (vbsettings == null)
            {
                vbsettings = new VisualBasicSettings();
            }

            vbsettings.ImportReferences.Add(new VisualBasicImportReference
            {
                Assembly = assemblyName,
                Import = ns
            });

            VisualBasic.SetSettings(activity, vbsettings);
        }

        private void AddDev2ImportsToActivity(Activity activity)
        {
            //
            // Add the namespaces that are used by any objects which are added as variables to the workflow.
            // If this isn't done, when an activity is added to the design surface that uses one of these variables
            // the workflow shows an error.
            //
            Assembly dev2DataAssembly = typeof (Dev2DataListDecisionHandler).Assembly;
            Assembly dev2CommonAssembly = typeof (GlobalConstants).Assembly;

            AddImportToActivity(activity, dev2CommonAssembly.GetName().Name, "Dev2.Common");
            AddImportToActivity(activity, dev2DataAssembly.GetName().Name, "Dev2.Data.Decisions.Operations");
            AddImportToActivity(activity, dev2DataAssembly.GetName().Name, "Dev2.Data.SystemTemplates.Models");
            AddImportToActivity(activity, dev2DataAssembly.GetName().Name, "Dev2.DataList.Contract");
            AddImportToActivity(activity, dev2DataAssembly.GetName().Name, "Dev2.DataList.Contract.Binary_Objects");
        }

        /// <summary>
        ///     Ensures the the workflow has the correct imports and variables
        /// </summary>
        private void EnsureWorkflowState()
        {
            if (_modelService == null || _modelService.Root == null)
            {
                return;
            }

            var activityBuilder = _modelService.Root.GetCurrentValue() as ActivityBuilder;

            if (activityBuilder == null)
            {
                return;
            }

            var chart = activityBuilder.Implementation as Flowchart;

            if (chart == null)
            {
                return;
            }

            EnsureVariables(chart);
            ClearActivityImports(chart);
            AddDev2ImportsToActivity(chart);
        }

        /// <summary>
        ///     Ensures that the only the correct variables exist on the workflow
        /// </summary>
        private void EnsureVariables(Flowchart flowchart)
        {
            flowchart.Variables.Clear();
            flowchart.Variables.Add(new Variable<List<string>> {Name = "InstructionList"});
            flowchart.Variables.Add(new Variable<string> {Name = "LastResult"});
            flowchart.Variables.Add(new Variable<bool> {Name = "HasError"});
            flowchart.Variables.Add(new Variable<string> {Name = "ExplicitDataList"});
            flowchart.Variables.Add(new Variable<bool> {Name = "IsValid"});
            flowchart.Variables.Add(new Variable<UnlimitedObject> {Name = "d"});
            flowchart.Variables.Add(new Variable<Unlimited.Applications.BusinessDesignStudio.Activities.Util>
                {
                    Name = "t"
                });
            flowchart.Variables.Add(new Variable<Dev2DataListDecisionHandler> {Name = "Dev2DecisionHandler"});
        }

        // Travis.Frisinger : 23.01.2013 - Added A Dev2DataListDecisionHandler Variable

        public void AddMissingWithNoPopUpAndFindUnusedDataListItems()
        {
            IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
            IList<IDataListVerifyPart> _removeParts = MissingWorkflowItems(workflowFields);
            _filteredDataListParts = MissingDataListParts(workflowFields);
            var eventAggregator = ImportService.GetExportValue<IEventAggregator>();

            if (eventAggregator != null)
            {
                eventAggregator.Publish(new ShowUnusedDataListVariablesMessage(_removeParts, ResourceModel));
            }

            if (eventAggregator != null)
            {
                eventAggregator.Publish(new AddMissingDataListItems(_filteredDataListParts, ResourceModel));
            }
        }

        public void FindUnusedDataListItems()
        {
            IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
            IList<IDataListVerifyPart> _removeParts = MissingWorkflowItems(workflowFields);

            var eventAggregator = ImportService.GetExportValue<IEventAggregator>();

            if (eventAggregator != null)
            {
                eventAggregator.Publish(new ShowUnusedDataListVariablesMessage(_removeParts, ResourceModel));
            }
        }

        public void RemoveAllUnusedDataListItems(IDataListViewModel dataListViewModel)
        {
            if (dataListViewModel != null && ResourceModel == dataListViewModel.Resource)
            {
                dataListViewModel.RemoveUnusedDataListItems();
            }
        }

        public void AddMissingOnlyWithNoPopUp(IDataListViewModel dataListViewModel)
        {
            IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
            _filteredDataListParts = MissingDataListParts(workflowFields);
            if (dataListViewModel != null && ResourceModel == dataListViewModel.Resource)
            {
                dataListViewModel.AddMissingDataListItems(_filteredDataListParts);
            }
            else
            {
                var eventAggregator = ImportService.GetExportValue<IEventAggregator>();

                if (eventAggregator != null)
                {
                    eventAggregator.Publish(new AddMissingDataListItems(_filteredDataListParts, ResourceModel));
                }
            }
        }

        //2013.02.11: Ashley Lewis - Bug 8553

        #endregion

        #region Event Handlers

        private void ViewOnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            UpdateLocationOfLastDroppedModelItem();
        }

        private void ViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dcontxt = (e.Source as dynamic).DataContext;
            var vm = dcontxt as WorkflowDesignerViewModel;

            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                var designerView = e.Source as DesignerView;
                if (designerView != null && designerView.FocusedViewElement == null)
                {
                    e.Handled = true;
                    return;
                }

                var item = SelectedModelItem as ModelItem;

                // Travis.Frisinger - 28.01.2013 : Case Amendments
                if (item != null)
                {
                    var dp = e.OriginalSource as DependencyObject;
                    string itemFn = item.ItemType.FullName;

                    //2013.03.20: Ashley Lewis - Bug 9202 Don't open any wizards if the source is a 'Microsoft.Windows.Themes.ScrollChrome' object
                    if (dp != null && string.Equals(dp.ToString(), "Microsoft.Windows.Themes.ScrollChrome", StringComparison.InvariantCulture))
                    {
                        WizardEngineAttachedProperties.SetDontOpenWizard(dp, true);
                    }

                    // Handle Case Edits
                    if (item != null &&
                        itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchCaseLink",
                                          StringComparison.Ordinal)
                        &&
                        !itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchDefaultLink",
                                           StringComparison.Ordinal))
                    {
                        ModelProperty tmp = item.Properties["Case"];

                        if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp))
                        {
                            //Mediator.SendMessage(MediatorMessages.EditCaseExpression, new Tuple<ModelProperty, IEnvironmentModel>(tmp, _resourceModel.Environment));
                            EventAggregator.Publish(
                                new EditCaseExpressionMessage(new Tuple<ModelProperty, IEnvironmentModel>(tmp,
                                                                                                          _resourceModel
                                                                                                              .Environment)));
                        }
                    }

                    // Handle Switch Edits
                    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) &&
                        item.ItemType == typeof (FlowSwitch<string>))
                    {
                        //Mediator.SendMessage(MediatorMessages.ConfigureSwitchExpression, new Tuple<ModelItem, IEnvironmentModel>(item, _resourceModel.Environment));
                        EventAggregator.Publish(
                            new ConfigureSwitchExpressionMessage(new Tuple<ModelItem, IEnvironmentModel>(item,
                                                                                                         _resourceModel
                                                                                                             .Environment)));
                    }

                    // Handle Decision Edits
                    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) &&
                        item.ItemType == typeof (FlowDecision))
                    {
                        //Mediator.SendMessage(MediatorMessages.ConfigureDecisionExpression, new Tuple<ModelItem, IEnvironmentModel>(item, _resourceModel.Environment));
                        EventAggregator.Publish(
                            new ConfigureDecisionExpressionMessage(new Tuple<ModelItem, IEnvironmentModel>(item,
                                                                                                           _resourceModel
                                                                                                               .Environment)));
                    }
                }


                if (designerView != null && designerView.FocusedViewElement != null &&
                    designerView.FocusedViewElement.ModelItem != null)
                {
                    ModelItem modelItem = designerView.FocusedViewElement.ModelItem;

                    if (modelItem.ItemType == typeof (DsfWebPageActivity) ||
                        modelItem.ItemType == typeof (DsfWebSiteActivity))
                    {
                        IWebActivity webpageActivity = WebActivityFactory.CreateWebActivity(modelItem, _resourceModel,
                                                                                            modelItem.Properties[
                                                                                                "DisplayName"]
                                                                                                .ComputedValue.ToString());
                        //Mediator.SendMessage(MediatorMessages.AddWebpageDesigner, webpageActivity);
                        EventAggregator.Publish(new AddWorkSurfaceMessage(webpageActivity));
                        e.Handled = true;
                    }
                }
            }
        }


        private void ViewPreviewDrop(object sender, DragEventArgs e)
        {

            SetLastDroppedPoint(e);
            _dataObject = e.Data.GetData(typeof(ResourceTreeViewModel));
            var isWorkflow = e.Data.GetData("WorkflowItemTypeNameFormat") as string;
            if (isWorkflow != null)
            _dataObject = e.Data.GetData(typeof (ResourceTreeViewModel));
        }

        private void ModelServiceModelChanged(object sender, ModelChangedEventArgs e)
        {
            if (e.ItemsAdded != null)
            {
                PerformAddItems(e.ItemsAdded.ToList());
            }
            else if (e.PropertiesChanged != null)
            {
                if (e.PropertiesChanged.Any(mp => mp.Name == "Handler"))
                {
                    if (_dataObject != null)
                    {
                        var navigationItemViewModel = _dataObject as ResourceTreeViewModel;
                        ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");

                        if (navigationItemViewModel != null && modelProperty != null)
                        {
                            var resource = navigationItemViewModel.DataContext as IContextualResourceModel;

                            if (resource != null)
                            {
                                //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                                DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true);
                                d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.ResourceName;
                                d.IconPath = resource.IconPath;

                                modelProperty.SetValue(d);
                            }
                        }

                        _dataObject = null;
                    }
                    else
                    {
                        ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");

                        if (modelProperty != null)
                        {
                            if (_vm != null)
                            {
                                IContextualResourceModel resource = _vm.SelectedResourceModel;
                                if (resource != null)
                                {
                                    DsfActivity droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, null, true);

                                    droppedActivity.ServiceName = droppedActivity.DisplayName = droppedActivity.ToolboxFriendlyName = resource.ResourceName;
                                    droppedActivity.IconPath = resource.IconPath;

                                    modelProperty.SetValue(droppedActivity);
                                }
                                _vm.Dispose();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Handler attached to intercept checks for executing the delete command
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete) //triggered from deleting an activity
            {
                PreventDeleteFromBeingExecuted(e);
            }
            else if (e.Command == EditingCommands.Delete) //triggered from editing displayname, expressions, etc
            {
                PreventDeleteFromBeingExecuted(e);
            }
        }

        #endregion

        #region Dispose

        public new void Dispose()
        {
            //MediatorRepo.deregisterAllItemMessages(this.GetHashCode());
            _wd = null;
            _designerManagementService.Dispose();
            EventAggregator.Unsubscribe(this);
            base.Dispose();
        }

        #endregion Dispose

        public override WorkSurfaceContext WorkSurfaceContext
        {
            get
            {
                return (ResourceModel == null)
                           ? WorkSurfaceContext.Unknown
                           : ResourceModel.ResourceType.ToWorkSurfaceContext();
            }
        }

        #region Overrides of ViewAware

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            AddMissingWithNoPopUpAndFindUnusedDataListItems();
        }

        #endregion

        public IEnvironmentModel EnvironmentModel
        {
            get { throw new NotImplementedException(); }
        }


        #region Implementation of IHandle<AddRemoveDataListItemsMessage>

        public void Handle(AddRemoveDataListItemsMessage message)
        {
            RemoveAllUnusedDataListItems(message.DataListViewModel);
        }

        #endregion

        #region Implementation of IHandle<FindMissingDataListItemsMessage>

        public void Handle(FindMissingDataListItemsMessage message)
        {
            AddMissingOnlyWithNoPopUp(message.DataListViewModel);
        }

        #endregion

        #region Implementation of IHandle<ShowActivityWizardMessage>

        public void Handle(ShowActivityWizardMessage message)
        {
            ShowActivityWizard(message.ModelItem);
        }

        #endregion

        #region Implementation of IHandle<ShowActivitySettingsWizardMessage>

        public void Handle(ShowActivitySettingsWizardMessage message)
        {
            ShowActivitySettingsWizard(message.ModelItem);
        }

        #endregion

        #region Implementation of IHandle<EditActivityMessage>

        public void Handle(EditActivityMessage message)
        {
            EditActivity(message.ModelItem);
        }

        #endregion
    }
}
