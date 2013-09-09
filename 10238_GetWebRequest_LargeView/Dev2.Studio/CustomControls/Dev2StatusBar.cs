﻿using System.Windows;
using System.Windows.Controls;

namespace Dev2.Studio.CustomControls
{
    /// <author>Massimo.Guerrera</author>
    /// <date>2/28/2013</date>
    [TemplatePart(Name = PART_Label, Type = typeof(Label))]
    public class Dev2StatusBar : TextBox
    {
        private const string PART_Label = "StatusBarLabel";

        private Label _label;

        public Label StatusBarLabel
        {
            get
            {
                return _label;
            }
        }


        public string StatusBarLabelText
        {
            get { return (string)GetValue(StatusBarLabelTextProperty); }
            set { SetValue(StatusBarLabelTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarLabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarLabelTextProperty =
            DependencyProperty.Register("StatusBarLabelText", typeof(string), typeof(Dev2StatusBar), new PropertyMetadata(string.Empty));



        public Visibility ProgressBarVisiblity
        {
            get { return (Visibility)GetValue(ProgressBarVisiblityProperty); }
            set { SetValue(ProgressBarVisiblityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressBarVisiblity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressBarVisiblityProperty =
            DependencyProperty.Register("ProgressBarVisiblity", typeof(Visibility), typeof(Dev2StatusBar), new PropertyMetadata(Visibility.Visible));

        public Dev2StatusBar()
        {
            DefaultStyleKey = typeof(Dev2StatusBar);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _label = GetTemplateChild(PART_Label) as Label;
        }

    }
}
