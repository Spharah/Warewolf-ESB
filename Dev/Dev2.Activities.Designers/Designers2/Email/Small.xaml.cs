﻿
using System.Windows;

namespace Dev2.Activities.Designers2.Email
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
        }

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }
    }
}
