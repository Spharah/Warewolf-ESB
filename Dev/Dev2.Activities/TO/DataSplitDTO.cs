﻿using System.Collections.Generic;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;

// ReSharper disable CheckNamespace

namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    // ReSharper disable InconsistentNaming
    public class DataSplitDTO : ValidatedObject, IDev2TOFn, IOutputTOConvert
    // ReSharper restore InconsistentNaming
    {
        public const string SplitTypeIndex = "Index";
        public const string SplitTypeChars = "Chars";
        public const string SplitTypeNone = "None";

        string _outputVariable;
        string _splitType;
        string _at;
        int _indexNum;
        bool _enableAt;
        bool _include;
        string _escapeChar;
        bool _isEscapeCharFocused;
        bool _isOutputVariableFocused;
        bool _isAtFocused;

        public DataSplitDTO()
        {
            SplitType = SplitTypeIndex;
            _enableAt = true;
        }

        public DataSplitDTO(string outputVariable, string splitType, string at, int indexNum, bool include = false, bool inserted = false)
        {
            Inserted = inserted;
            OutputVariable = outputVariable;
            SplitType = string.IsNullOrEmpty(splitType) ? SplitTypeIndex : splitType;
            At = string.IsNullOrEmpty(at) ? string.Empty : at;
            IndexNumber = indexNum;
            Include = include;
            _enableAt = true;
            OutList = new List<string>();
        }

        public string WatermarkTextVariable { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public bool EnableAt { get { return _enableAt; } set { OnPropertyChanged(ref _enableAt, value); } }

        public int IndexNumber { get { return _indexNum; } set { OnPropertyChanged(ref _indexNum, value); } }

        public List<string> OutList { get; set; }

        public bool Include { get { return _include; } set { OnPropertyChanged(ref _include, value); } }

        [FindMissing]
        public string EscapeChar { get { return _escapeChar; } set { OnPropertyChanged(ref _escapeChar, value); } }

        public bool IsEscapeCharFocused { get { return _isEscapeCharFocused; } set { OnPropertyChanged(ref _isEscapeCharFocused, value); } }

        [FindMissing]
        public string OutputVariable
        {
            get { return _outputVariable; }
            set
            {
                OnPropertyChanged(ref _outputVariable, value);
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsOutputVariableFocused { get { return _isOutputVariableFocused; } set { OnPropertyChanged(ref _isOutputVariableFocused, value); } }

        public string SplitType
        {
            get { return _splitType; }
            set
            {
                if(value != null)
                {
                    OnPropertyChanged(ref _splitType, value);
                    RaiseCanAddRemoveChanged();
                }
            }
        }

        [FindMissing]
        public string At
        {
            get { return _at; }
            set
            {
                OnPropertyChanged(ref _at, value);
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsAtFocused { get { return _isAtFocused; } set { OnPropertyChanged(ref _isAtFocused, value); } }

        public bool CanRemove()
        {
            if(SplitType == SplitTypeIndex || SplitType == SplitTypeChars)
            {
                if(string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        public bool CanAdd()
        {
            bool result = true;
            if(SplitType == SplitTypeIndex || SplitType == SplitTypeChars)
            {
                if(string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
                {
                    result = false;
                }
            }
            return result;
        }

        public void ClearRow()
        {
            OutputVariable = string.Empty;
            SplitType = SplitTypeChars;
            At = string.Empty;
            Include = false;
            EscapeChar = string.Empty;
        }

        public bool Inserted { get; set; }

        public OutputTO ConvertToOutputTO()
        {
            return DataListFactory.CreateOutputTO(OutputVariable, OutList);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(OutputVariable) && SplitType == SplitTypeIndex && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(OutputVariable) && SplitType == SplitTypeChars && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(OutputVariable) && SplitType == SplitTypeNone && string.IsNullOrEmpty(At);
        }

        public override RuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();
            if(IsEmpty())
            {
                return ruleSet;
            }

            switch(propertyName)
            {
                case "OutputVariable":
                    var outputExprRule = new IsValidExpressionRule(() => OutputVariable, "1");
                    ruleSet.Add(outputExprRule);
                    ruleSet.Add(new IsStringNullOrEmptyRule(() => outputExprRule.ExpressionValue));
                    break;

                case "At":
                    if(SplitType == SplitTypeIndex)
                    {
                        var atExprRule = new IsValidExpressionRule(() => At, "1");
                        ruleSet.Add(atExprRule);
                        ruleSet.Add(new IsNumericRule(() => atExprRule.ExpressionValue));
                    }
                    break;

                //case "EscapeChar":
                //    break;
            }
            return ruleSet;
        }
    }
}
