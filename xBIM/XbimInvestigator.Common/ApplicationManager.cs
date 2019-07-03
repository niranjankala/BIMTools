using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xbim.Ifc;
using BIMTools.Shared.Extensions;

namespace XbimInvestigator.Common
{
    public sealed class ApplicationManager
    {
        private static volatile ApplicationManager instance;
        private static object syncRoot = new Object();
        private string currnetModelFile = string.Empty;
        private string currentLibraryFileName = string.Empty;
        private string previousLibraryFileName = string.Empty;
        private ApplicationState applicationState = ApplicationState.Ready;
        public string InstallPath = System.Windows.Forms.Application.StartupPath;        

        private ApplicationManager()
        {            
        }

        public static ApplicationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ApplicationManager();
                    }
                }
                return instance;
            }
        }

        public ApplicationState ApplicationState
        {
            get { return applicationState; }
            set
            {
                if (value != applicationState)
                {
                    applicationState = value;
                    //GlobalEvents.RaiseApplicationStatusChangedEvent(applicationState);
                }
            }
        }
        public string CurrentModelFile
        {
            get { return currnetModelFile; }
            set { currnetModelFile = value; }
        }        
        public string SimXmlFileName { get; set; }

        public IfcStore CurrentModel { get; set; }        
        
        
        private bool isProjectSaved = true;
        public bool IsProjectSaved { get => isProjectSaved; set => isProjectSaved = value; }
 

        public bool IsApplicationClose { get; set; }        
        /// <summary>
        /// INdicates that the current model is being closed
        /// </summary>
        public bool ModelIsClosing { get; set; }

        private ProjectFileType projectFileType = ProjectFileType.Ifc4;

        public ProjectFileType ProjectFileType
        {
            get { return projectFileType; }
            set { projectFileType = value; }
        }
        

        
        /// <summary>
        /// Allows javascript to run on a windows form web browser control
        /// Ref from :https://weblog.west-wind.com/posts/2011/May/21/Web-Browser-Control-Specifying-the-IE-Version
        /// </summary>
        /// <param name="emulationmode">https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/general-info/ee330730(v=vs.85)#binary-behavior-security</param>
        public void EnableBrowserEmulation(uint emulationmode)
        {
            try
            {
                FileInfo exeInfo = new FileInfo(System.Windows.Forms.Application.ExecutablePath);
                EnsureBrowserEmulationEnabled(exeInfo.Name, emulationmode);
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        private void EnsureBrowserEmulationEnabled(string exename, uint emulationmode, bool uninstall = false)
        {
            try
            {
                string key = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
                if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
                {
                    key = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
                }
                using (
                    var rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, true)
                )
                {
                    if (!uninstall)
                    {
                        object value = rk.GetValue(exename);
                        if (value == null)
                            rk.SetValue(exename, emulationmode, Microsoft.Win32.RegistryValueKind.DWord);
                    }
                    else
                        rk.DeleteValue(exename);
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        public string GetFullPath(string fileName)
        {
            return String.Format(@"{0}\{1}", InstallPath, fileName);
        }

        #region "get C:\users\public\App Path"      
        public string GetPublicFolder()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            var directory = new System.IO.DirectoryInfo(documentsPath);
            return directory.Parent.FullName;
        }

        public string GetDataAccessResourcesFolder()
        {
            return System.Windows.Forms.Application.StartupPath + @"\DataAccess\Resources\";
        }

      

        public string GetConfigurationFolder()
        {
            string programDataPath = string.Empty;          

            programDataPath = GetPublicFolder() + @"\ProgramData";

            return programDataPath;
        }
        #endregion




        public bool IsCubeOperationInProgress { get; set; }

       
       

        #region "Public Methods"
        public string GetActualPressedKey(System.Windows.Forms.Keys key)
        {
            try
            {
                if (key.ToString().Contains("NumPad"))
                {
                    return key.ToString().Substring(key.ToString().Length - 1, 1);
                }
                else
                {
                    return ((Char)key).ToString();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        private ProjectVariables common = null;

        public ProjectVariables Common
        {
            get { return common; }
            set { common = value; }
        }

        XbimEditorCredentials editor = new XbimEditorCredentials
        {
            ApplicationDevelopersName = "DA",
            ApplicationFullName = "Xbim Investigator",
            ApplicationIdentifier = "XbimInvestigator",
            ApplicationVersion = "1.0"            
        };
        public XbimEditorCredentials ApplicationEditorCredentials { get ; set; }
    }
}
