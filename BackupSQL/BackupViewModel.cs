using DevExpress.Mvvm;
using DophinMSSQLConnector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace BackupSQL {
    public class BackupViewModel {
        public BackupViewModel() {
            GetSettings();

        }
        void GetSettings() {
            // MsSqlConnector.GetSettingsFromFile(pathToSettingsFile);

            StreamReader sr = new StreamReader(pathToSettingsFile);
            string st = sr.ReadToEnd();
            sr.Close();

            MsSqlConnector.GetSettingsFromTxt(st);
            XElement xl = XElement.Parse(st);
            pathDropbox = xl.Element("Dropbox").Value;
            winRarPath = xl.Element("BackupSetting").Element("WinRar").Value;
            foreach (XElement x in xl.Element("BackupSetting").Element("Bases").Elements()) {
                string stBaseName = x.Value;
                switch (stBaseName) {
                    case "EngBase":
                        IsEngBaseBackup = true;
                        break;
                    case "Budget":
                        IsBudgetBackup = true;
                        break;
                    case "DXTicketsBase":
                        IsTicketsBackup = true;
                        break;
                    case "ListOfDealBase":
                        IsListOfDealBackup = true;
                        break;

                }
            }
            backupPath = pathDropbox + "\\BackupMSSQL";
        }

        string pathToSettingsFile = @"C:\MSSQLSettings.ini";
        string pathDropbox;
        string backupPath;
        string winRarPath;

        bool _isEngBaseBackup;
        bool _isBudgetBackup;
        bool _isTicketsBackup;
        bool _isListOfDealBackup;

        public bool IsListOfDealBackup {
            get { return _isListOfDealBackup; }
            set { _isListOfDealBackup = value; }
        }
        public bool IsEngBaseBackup {
            get { return _isEngBaseBackup; }
            set { _isEngBaseBackup = value; }
        }


        public bool IsBudgetBackup {
            get { return _isBudgetBackup; }
            set { _isBudgetBackup = value; }
        }


        public bool IsTicketsBackup {
            get { return _isTicketsBackup; }
            set { _isTicketsBackup = value; }
        }


        ICommand _backupCommand;
        ICommand _restoreCommand;


        public ICommand BackupCommand {
            get {
                if (_backupCommand == null)
                    _backupCommand = new DelegateCommand(Backup);

                return _backupCommand;
            }
        }

        public ICommand RestoreCommand {
            get {
                if (_restoreCommand == null)
                    _restoreCommand = new DelegateCommand(RestoreEngBase);
                return _restoreCommand;
            }
        }

       public void Backup() {
            Debug.Print("ba");
            //0  delete old backup
           

            string[] bakFiles = Directory.GetFiles(backupPath, "*.bak");
            foreach (string st in bakFiles) {
                File.Delete(st);
                Debug.Print(String.Format("File {0} was deleted", st));
            }


            //1 BackupFile
            MsSqlConnector.Open();
            if (IsEngBaseBackup)
                BackupBase("EngBase");
            if (IsBudgetBackup)
                BackupBase("Budget");
            if (IsTicketsBackup)
                BackupBase("DXTicketsBase");
            if (IsListOfDealBackup)
                BackupBase("ListOfDealBase");

            MsSqlConnector.Close();

            //2 compress files
            string backupDate = DateTime.Now.ToString("yyyy.MM.dd");

            ProcessStartInfo winRarInfo = new ProcessStartInfo();
            winRarInfo.FileName = winRarPath;
           string machineName = System.Environment.MachineName;
           string mName = "";
           if (machineName == "KOZHEVNIKOV-NB")
               mName = "work";
           else
               mName = "home";

           winRarInfo.Arguments = string.Format(@" a -ep ""{0}\BackupMSSQL\MyBases_backup_{1}_{2}.rar""  ""{0}\BackupMSSQL\*.bak""", pathDropbox, backupDate, mName);
            Process.Start(winRarInfo);
            //"c:\Program Files (x86)\WinRAR\Rar.exe" a -ep "F:\dropbox\english\backup\MyBases_backup_%custDate%.rar"  "F:\temp\backup\*.bak"
        }

       public void RestoreEngBase() {
            MsSqlConnector.MsDataBase = "master";
            string backupName = Directory.GetFiles(backupPath, "[EngBase]*.bak")[0];
         //   string query = string.Format(@"RESTORE DATABASE [EngBase] FROM  DISK = N'{0}' WITH  FILE = 1,  NOUNLOAD,  REPLACE,  STATS = 5",backupName);
            string query = "ALTER DATABASE [EngBase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            query+=string.Format(@" RESTORE DATABASE [EngBase] FROM  DISK = N'{0}' WITH  FILE = 1,  MOVE N'EngData' TO N'C:\Temp\EngBase.mdf',  MOVE N'EngData_log' TO N'C:\temp\EngBase.ldf',  NOUNLOAD,  REPLACE,  STATS = 5",backupName);
            query+=" ALTER DATABASE [EngBase] SET MULTI_USER";
            MsSqlConnector.MakeNonQuery(query);
            //          USE [master]
            //ALTER DATABASE [EngBase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
            //RESTORE DATABASE [EngBase] FROM  DISK = N'F:\Dropbox\English\backup\[EngBase]-20130313_18-05wrk.bak' WITH  FILE = 1,  MOVE N'EngData' TO N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\EngBase.mdf',  MOVE N'EngData_log' TO N'C:\temp\EngBase.ldf',  NOUNLOAD,  REPLACE,  STATS = 5
            //ALTER DATABASE [EngBase] SET MULTI_USER
        }

        void BackupBase(string baseName) {
            string backupDate = DateTime.Now.ToString("yyyyMMdd_HH-mm");
            string backupName = string.Format("{0}\\[{1}]{2}.bak", backupPath, baseName, backupDate);
            string sqlQuery = String.Format("BACKUP DATABASE [{0}] TO  DISK = '{1}' WITH NOFORMAT, NOINIT,  NAME = N'{0}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10", baseName, backupName);
  
       

            
            MsSqlConnector.MakeNonQuery(sqlQuery);
            //BACKUP DATABASE [EngBase] TO  DISK = @BackupName WITH NOFORMAT, NOINIT,  NAME = N'EngBase-Полная База данных Резервное копирование', SKIP, NOREWIND, NOUNLOAD,  STATS = 10

        }

    }
}
