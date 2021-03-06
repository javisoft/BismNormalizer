﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BismNormalizer.TabularCompare.UI
{
    public partial class ValidationOutputButtons : UserControl
    {
        //complete fudge to look like the VS Error List window (tabcontrol doesn't support button multiselection)

        private ValidationOutputButton _informationalMessageButton = new ValidationOutputButton();
        private ValidationOutputButton _warningsButton = new ValidationOutputButton();
        private Panel _fillerPanel = new Panel();
        private int _informationalMessageCount = 0;
        private int _warningCount = 0;

        public event EventHandler InformationalMessageValueChangedHandler;
        public delegate void InformationalMessageValueChangedDelegate(object sender, EventArgs e);
        public event EventHandler WarningValueChangedHandler;
        public delegate void WarningValueChangedDelegate(object sender, EventArgs e);
        
        public ValidationOutputButtons()
        {
            InitializeComponent();
        }

        private void ValidationOutput_Load(object sender, EventArgs e)
        {
            _warningsButton.Height = 25;
            _warningsButton.WarningImage = true;
            _warningsButton.ValueChangedHandler += new EventHandler(WarningsButton_ValueChangedHandler);
            this.Controls.Add(_warningsButton);

            _informationalMessageButton.Height = 25;
            _informationalMessageButton.WarningImage = false;
            _informationalMessageButton.ValueChangedHandler += new EventHandler(InformationalMessageButton_ValueChangedHandler);
            this.Controls.Add(_informationalMessageButton);

            _informationalMessageButton.BringToFront();

            this.Controls.Add(_fillerPanel);

            _fillerPanel.BringToFront();

            SetTextAndWidths();
        }

        void InformationalMessageButton_ValueChangedHandler(object sender, EventArgs e)
        {
            if (InformationalMessageValueChangedHandler != null)
            {
                InformationalMessageValueChangedHandler(this, e);
            }
        }

        void WarningsButton_ValueChangedHandler(object sender, EventArgs e)
        {
            if (WarningValueChangedHandler != null)
            {
                WarningValueChangedHandler(this, null);
            }
        }

        public int InformationalMessageCount
        {
            get
            {
                return _informationalMessageCount;
            }
            set
            {
                _informationalMessageCount = value;
                SetTextAndWidths();
            }
        }

        public int WarningCount
        {
            get
            {
                return _warningCount;
            }
            set
            {
                _warningCount = value;
                SetTextAndWidths();
            }
        }

        public bool InformationalMessagesVisible
        {
            get
            {
                return _informationalMessageButton.Selected;
            }
            set
            {
                _informationalMessageButton.Selected = value;
            }
        }

        public bool WarningsVisible
        {
            get
            {
                return _warningsButton.Selected;
            }
            set
            {
                _warningsButton.Selected = value;
            }
        }

        private void SetTextAndWidths()
        {
            int pixelsPerDigit = 6;

            string warningCount = Convert.ToString(_warningCount);
            _warningsButton.Text = warningCount + " Warnings";

            string informationalMessageCount = Convert.ToString(_informationalMessageCount);
            _informationalMessageButton.Text = informationalMessageCount + " Informational Messages";
            _informationalMessageButton.Left = 92 + pixelsPerDigit * warningCount.Length;

            _fillerPanel.Left = 253 + (pixelsPerDigit * warningCount.Length) + (pixelsPerDigit * informationalMessageCount.Length);
        }
    }
}
