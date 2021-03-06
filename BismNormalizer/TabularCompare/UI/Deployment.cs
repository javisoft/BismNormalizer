﻿using BismNormalizer.TabularCompare.Core;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BismNormalizer.TabularCompare.UI
{
    public partial class Deployment : Form
    {
        private Comparison _comparison;
        private ComparisonInfo _comparisonInfo;
        private DeploymentStatus _deployStatus;
        private const string _deployRowWorkItem = "Deploy metadata";
        ProcessingErrorMessage _errorMessageForm;

        public Deployment()
        {
            InitializeComponent();
        }

        private void Deploy_Load(object sender, EventArgs e)
        {
            try
            {
                this.KeyPreview = true;
                AddRow(_deployRowWorkItem, "Deploying ...");
                _deployStatus = DeploymentStatus.Deploying;

                _comparison.PasswordPrompt += HandlePasswordPrompt;
                _comparison.DeploymentMessage += HandleDeploymentMessage;
                _comparison.DeploymentComplete += HandleDeploymentComplete;

                btnStopProcessing.Enabled = false;
                btnClose.Enabled = false;
                _errorMessageForm = new ProcessingErrorMessage();

                ProcessingTableCollection tablesToProcess = _comparison.GetTablesToProcess();
                foreach (ProcessingTable table in tablesToProcess)
                {
                    AddRow(table.Name, "Processing in progress ...");
                }
                if (tablesToProcess.Count > 0)
                {
                    btnStopProcessing.Enabled = true;
                    lblStatus.Text = "Processing ...";
                }
                _comparison.DatabaseDeployAndProcess(tablesToProcess);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "BISM Normalizer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Set status methods

        private void HandleDeploymentMessage(object sender, DeploymentMessageEventArgs e)
        {
            foreach (DataGridViewRow row in gridProcessing.Rows)
            {
                if (Convert.ToString(row.Cells[1].Value) == e.WorkItem)
                {
                    row.Cells[0].Value = DeployImageList.Images[Convert.ToInt32(e.DeploymentStatus)];
                    row.Cells[2].Value = e.Message;
                }
            }
        }

        private void HandleDeploymentComplete(object sender, DeploymentCompleteEventArgs e)
        {
            switch (e.DeploymentStatus)
            {
                case DeploymentStatus.Success:
                    picStatus.Image = Resources.ProgressSuccess;
                    lblStatus.Text = "Succes";
                    _deployStatus = DeploymentStatus.Success;
                    break;

                case DeploymentStatus.Cancel:
                    picStatus.Image = Resources.ProgressCancel;
                    lblStatus.Text = "Cancelled";
                    _deployStatus = DeploymentStatus.Cancel;
                    break;

                case DeploymentStatus.Error:
                    SetErrorStatus(e.ErrorMessage);
                    break;

                default:
                    break;
            }

            btnStopProcessing.Enabled = false;
            btnClose.Enabled = true;
            btnClose.Select();
        }

        private delegate void SetErrorStatusDelegate(string errorMessage);
        private void SetErrorStatus(string errorMessage)
        {
            //might not be on UI thread
            if (this.InvokeRequired || _errorMessageForm.InvokeRequired)
            {
                SetErrorStatusDelegate SetErrorStatusCallback = new SetErrorStatusDelegate(SetErrorStatus);
                this.Invoke(SetErrorStatusCallback, new object[] { errorMessage });
            }
            else
            {
                picStatus.Image = Resources.ProgressError;
                lblStatus.Text = "Error";
                _deployStatus = DeploymentStatus.Error;

                if (!String.IsNullOrEmpty(errorMessage) && String.IsNullOrEmpty(_errorMessageForm.ErrorMessage)) //just in case already shown
                {
                    _errorMessageForm.ErrorMessage = errorMessage;
                    _errorMessageForm.StartPosition = FormStartPosition.CenterParent;
                    _errorMessageForm.ShowDialog();
                }

                btnStopProcessing.Enabled = false;
                btnClose.Enabled = true;
                btnClose.Select();
            }
        }

        #endregion

        private void AddRow(string workItem, string status)
        {
            DataGridViewRow row = (DataGridViewRow)gridProcessing.RowTemplate.Clone();
            row.CreateCells(gridProcessing);
            row.Cells[0].Value = DeployImageList.Images[0];
            row.Cells[1].Value = workItem;
            row.Cells[2].Value = status;
            gridProcessing.Rows.Add(row);
        }

        private void btnStopProcessing_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to attempt to stop processing?", "BISM Normalizer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _comparison.StopProcessing();
            btnStopProcessing.Enabled = false;
        }

        private void HandlePasswordPrompt(object sender, PasswordPromptEventArgs e)
        {
            ImpersonationCredentials credentialsForm = new ImpersonationCredentials();
            credentialsForm.ConnectionName = e.ConnectionName;
            credentialsForm.Username = e.Username;
            credentialsForm.StartPosition = FormStartPosition.CenterParent;
            credentialsForm.ShowDialog();
            if (credentialsForm.DialogResult == DialogResult.OK)
            {
                e.Username = credentialsForm.Username;
                e.Password = credentialsForm.Password;
                e.UserCancelled = false;
            }
            else
            {
                e.Password = null;
                e.UserCancelled = true;
            }
        }

        public Comparison Comparison
        {
            get { return _comparison; }
            set { _comparison = value; }
        }

        public ComparisonInfo ComparisonInfo
        {
            get { return _comparisonInfo; }
            set { _comparisonInfo = value; }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            switch (_deployStatus)
            {
                case DeploymentStatus.Success:
                    this.DialogResult = DialogResult.OK;
                    break;
                default:
                    this.DialogResult = DialogResult.Abort;
                    break;
            }
            this.Close();
        }

        private void Deploy_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = (btnClose.Enabled == false);
        }
    }
}
