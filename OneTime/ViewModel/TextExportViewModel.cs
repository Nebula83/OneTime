using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using Nebula.OneTime.Properties;
using Nebula.OneTime.TimesheetBookingModels;
using Nebula.OneTime.TimesheetModels;

namespace Nebula.OneTime.ViewModel
{
    public class TextExportViewModel : ViewModelBase
    {
        private AltenTimeSheet _timesheetData;
        private string _timesheet;
        private bool _parsed;
        private string _fileName;

        public string Username
        {
            get => Settings.Default.Username;
            set
            {
                Settings.Default.Username = value;
                Settings.Default.Save();
            }
        }

        public string Password
        {
            get => Settings.Default.Password;
            set
            {
                Settings.Default.Password = value;
                Settings.Default.Save();
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set { Set(() => FileName, ref _fileName, value); }
        }

        public bool Parsed
        {
            get { return _parsed; }
            set { Set(() => Parsed, ref _parsed, value); }
        }

        public String Timesheet
        {
            get { return _timesheet; }
            set { Set(() => Timesheet, ref _timesheet, value); }
        }

        public RelayCommand BrowseFiles { get; set; }
        public RelayCommand OpenFile { get; set; }
        public RelayCommand SubmitTimesheet { get; set; }
        
        public TextExportViewModel()
        {
            BrowseFiles = new RelayCommand(DoBrowseFiles);
            OpenFile = new RelayCommand(DoOpenFile);
            SubmitTimesheet = new RelayCommand(DoSubmitTimesheet);
            FileName = "";
        }

        private void DoBrowseFiles()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

            if (fileDialog.ShowDialog() == true)
            {
                FileName = fileDialog.FileName;
            }
        }

        private void DoOpenFile()
        {
            try
            {
                var fileText = GetFileText(FileName);
                _timesheetData = new AltenTimeSheet(fileText);
                Parsed = true;
                Timesheet = _timesheetData.ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error on opening");
            }
        }

        private void DoSubmitTimesheet()
        {
            var booking = new FlexposureAfas(_timesheetData, Username, Password);
            try
            {
                booking.EnterHours();
            }
            catch
            {
                MessageBox.Show("Please try again.", "afas Insertion failed", MessageBoxButton.OK);
            }
        }

        private string GetFileText(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ".txt":
                    return GetTextFileText(fileName);
                default:
                    throw new NotSupportedException($"The selected file {extension} is not supported");
            }
        }

        private string GetTextFileText(string fileName)
        {
            return File.ReadAllText(fileName);
        }
    }
}