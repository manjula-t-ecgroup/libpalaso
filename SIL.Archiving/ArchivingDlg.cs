﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using L10NSharp;
using Palaso.UI.WindowsForms.Miscellaneous;
using Palaso.UI.WindowsForms.PortableSettingsProvider;
using SIL.Archiving.Properties;

namespace SIL.Archiving
{
	/// ----------------------------------------------------------------------------------------
	public partial class ArchivingDlg : Form
	{
		private readonly FormSettings _settings;
		private readonly ArchivingDlgViewModel _viewModel;
		private readonly Func<IDictionary<string, Tuple<IEnumerable<string>, string>>> _getFilesToArchive;

		/// ------------------------------------------------------------------------------------
		/// <summary>Caller can use this to retrieve and persist form settings (typicvally
		/// after form is closed).</summary>
		/// ------------------------------------------------------------------------------------
		public FormSettings FormSettings
		{
			get { return _settings; }
		}

		/// ------------------------------------------------------------------------------------
		/// <param name="model">View model</param>
		/// <param name="localizationManagerId">The ID of the localization manager for the
		/// calling application.</param>
		/// <param name="appSpecificArchivalProcessInfo">Application can use this to pass
		/// additional information that will be displayed to the user in the dialog to explain
		/// any application-specific details about the archival process.</param>
		/// <param name="getFilesToArchive">delegate to retrieve the lists of files of files to
		/// archive, keyed and grouped according to whatever logical grouping makes sense in the
		/// calling application. The key for each group will be supplied back to the calling app
		/// for use in "normalizing" file names. For each group, in addition to the enumerated
		/// files to include (in Item1 of the Tuple), the calling app can provide a progress
		/// message (in Item2 of the Tuple) to be displayed when that group of files is being
		/// zipped and added to the RAMP file.</param>
		/// <param name="settings">Location, size, and state where the client would like the
		/// dialog box to appear (can be null)</param>
		/// ------------------------------------------------------------------------------------
		public ArchivingDlg(ArchivingDlgViewModel model, string localizationManagerId,
			string appSpecificArchivalProcessInfo,
			Func<IDictionary<string, Tuple<IEnumerable<string>, string>>> getFilesToArchive, FormSettings settings)
		{
			_settings = settings ?? FormSettings.Create(this);

			_viewModel = model;
			_getFilesToArchive = getFilesToArchive;

			InitializeComponent();

			if (!string.IsNullOrEmpty(localizationManagerId))
				locExtender.LocalizationManagerId = localizationManagerId;

			Text = string.Format(Text, model.AppName);
			_progressBar.Visible = false;
			_buttonLaunchRamp.Enabled = false;

			// Visual Studio's designer insists on putting long strings of text in the resource
			// file, even though the dialog's Localizable property is false. So, localized
			// controls having a lot of text in their Text property have to have it set this
			// way rather than in the designer. Otherwise, the code string scanner won't find
			// the control's text.
			_linkOverview.Text = string.Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.OverviewText",
				"RAMP is a utility for entering metadata and uploading submissions to SIL's internal archive, " +
				"REAP. If you have access to this archive, this tool will help you use RAMP to archive your " +
				"{0} data. {1} When the RAMP package has been created, you can  launch RAMP and enter any additional information before doing the actual submission.",
				"The first occurance of the word 'RAMP' will be made a hyperlink to the RAMP website. " +
				"If the word 'RAMP' is not found, the text will not contain that hyperlink.",
				null, null, _linkOverview), _viewModel.AppName, appSpecificArchivalProcessInfo);

			_linkOverview.Links.Clear();
			if (model.ProgramDialogFont != null)
				_linkOverview.Font = model.ProgramDialogFont;

			int i = _linkOverview.Text.IndexOf("RAMP");
			if (i >= 0)
				_linkOverview.Links.Add(i, 4, Settings.Default.RampWebSite);

			model.LogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			model.LogBox.Margin = new Padding(0, 5, 0, 5);
			model.LogBox.ReportErrorLinkClicked += delegate { Close(); };
			_tableLayoutPanel.Controls.Add(model.LogBox, 0, 1);
			_tableLayoutPanel.SetColumnSpan(model.LogBox, 3);

			_buttonLaunchRamp.Click += (s, e) => model.CallRAMP();

			_buttonCancel.MouseLeave += delegate
			{
				if (model.IsBusy)
					WaitCursor.Show();
			};

			_buttonCancel.MouseEnter += delegate
			{
				if (model.IsBusy)
					WaitCursor.Hide();
			};

			_buttonCancel.Click += delegate
			{
				model.Cancel();
				WaitCursor.Hide();
			};

			_buttonCreatePackage.Click += delegate
			{
				Focus();
				_buttonCreatePackage.Enabled = false;
				_progressBar.Visible = true;
				WaitCursor.Show();
				_buttonLaunchRamp.Enabled = model.CreatePackage();
				_buttonCreatePackage.Enabled = false;
				_progressBar.Visible = false;
				WaitCursor.Hide();
			};
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			_settings.InitializeForm(this);
			base.OnLoad(e);
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			try
			{
				WaitCursor.Show();
				int maxProgBarValue;
				_buttonCreatePackage.Enabled = _viewModel.Initialize(_getFilesToArchive, out maxProgBarValue, () => _progressBar.Increment(1));
				_progressBar.Maximum = maxProgBarValue;
				WaitCursor.Hide();
			}
			catch
			{
				WaitCursor.Hide();
				throw;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleRampLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var tgt = e.Link.LinkData as string;

			if (!string.IsNullOrEmpty(tgt))
				System.Diagnostics.Process.Start(tgt);
		}
	}
}