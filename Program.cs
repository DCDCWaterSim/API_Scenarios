using System;
using System.Collections.Generic;
using WaterSim;
using WaterSimDCDC;

namespace WaterSim_Tester
{
    static class Program
    {
         /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            WaterSim.Scenarios sc;
              try{
                 CreateDirectory(TempDirectoryName);
                 sc = new WaterSim.Scenarios(DataDirectoryName, TempDirectoryName);
              }
             catch (Exception e)
            {
                  throw new Exception(e .Message);
            }
        }
        #region Website directory faking
        private static string DataDirectoryName
        {
            get
            {
                return @"App_Data\WaterSim_5_0\";
            }
        }

        private static string TempDirectoryName
        {
            set
            {
                string dir = value;
                string.Concat(@"WaterSim_Output\", dir);
            }
            get
            {
                // Make a common for testing
                return @"WaterSim_Output\";
                // Make the temp directory name unique for each access to avoid client clashes
                //return +System.Guid.NewGuid().ToString() + @"\";
            }
        }
        private static void CreateDirectory(string directoryName)
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(directoryName);
            if (!dir.Exists)
            {
                dir.Create();
            }
        }
        #endregion

    }
}

