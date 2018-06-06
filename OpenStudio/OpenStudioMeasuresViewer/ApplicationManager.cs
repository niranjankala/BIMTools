using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStudioMeasuresViewer
{
    public sealed class ApplicationManager
    {
        private static volatile ApplicationManager instance;
        private static object syncRoot = new Object();
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

        public string GetPublicFolder()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            var directory = new System.IO.DirectoryInfo(documentsPath);
            return directory.Parent.FullName;
        }
        public string GetMeasuresFolder()
        {
            return string.Format(@"{0}\Measures", GetPublicFolder());
        }

        public void CopyConfigFolder()
        {
            string measuresTarget = ApplicationManager.instance.GetMeasuresFolder();

            //Check to see if Measures Directory exists
            if (!Directory.Exists(measuresTarget))
                Directory.CreateDirectory(measuresTarget);
        }
       
    }
}
