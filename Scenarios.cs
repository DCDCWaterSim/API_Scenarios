 using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;                        // for BinaryReader
using System.Data;
using WaterSimDCDC;                     // Model proper (constructor)
using UniDB;
using System.Runtime.InteropServices;   // for DllImport
using System.Diagnostics;   // 

namespace WaterSim
{
    class Scenarios : WaterSimU
    {
        /// <remarks>   David A Sampson 04/28/2015. </remarks>
        const string _APIVersion = "8.0";  // latest version of API
        string _ModelBuild = "";
        internal StreamWriter sw;
        DateTime now = DateTime.Now;
        //
        ModelParameterClass SVTraceParm;
        ModelParameterClass COTraceParm;
        ModelParameterClass WBParm;
        ModelParameterClass WAugParm;
        ModelParameterClass WReuseParm;
        ModelParameterClass GPCDParm;
        ModelParameterClass PopParm;
        ModelParameterClass DroughtParmCO;
         ModelParameterClass DroughtParmSV;
        //

        string[] ScnNames = new string[1] { "BASE" };
        int RecCount=1;
        //
       // const int FCORTraceN = 40;
       // int[] COtrace = new int[FCORTraceN] { 1956, 1944, 1934, 1964, 1979, 1948, 1926, 1915, 1940, 1915, 1949, 1936, 1925, 1934, 1955, 1906, 1964, 1923, 1950, 1911, 1908, 1974, 1957, 1958, 1956, 1961, 1939, 1965, 1933, 1951, 1923, 1953, 1925, 1932, 1906, 1953, 1979, 1922, 1928, 1931};
       // //  
       //const int FSVRTraceN = 40;
       //int[] SVtrace = new int[FCORTraceN] { 1978, 1970, 1975, 1969, 1970, 1977, 1951, 1973, 1949, 1957, 1971, 1947, 1979, 1946, 1975, 1969, 1961, 1976, 1965, 1973, 1968, 1973, 1979, 1963, 1949, 1947, 1956, 1979, 1971, 1948, 1945, 1969, 1951, 1976, 1952, 1974, 1967, 1953, 1948, 1967 };
        //
        const int FCORTraceN = 3;
        int[] COtrace = new int[FCORTraceN] { 1956, 1944, 1934 };
        //  
        const int FSVRTraceN = 3;
        int[] SVtrace = new int[FCORTraceN] { 1978, 1970, 1975 };

        // opposite of normal logic- this is the UI variable
        const int FGPCDN = 2;
        //int[] GPCDValues = new int[FGPCDN] {50,67,84,100};
        int[] GPCDValues = new int[FGPCDN] {50,100};
        int GPCD_pct = 0;
        //
        // 10.04.15 das check-verified: NOTE: cannot use a value of zero for PopValues: it has special significance
        // in the FORTRAN code that CANNOT be overridden
         const int FpopN = 2;
        //int[] PopValues = new int[FpopN] {1,25,50,75,100,125};
        int[] PopValues = new int[FpopN] {75,100 };
        //
        // Fortran code modified on 10.04.15 to permit both default water banking and added water banking
        // New subroutine written
         const int FWBN = 2;
        //int[] WaterBanking = new int[FWBN] {0,25,50,100 };
        //int[] WaterBanking = new int[FWBN] {0,100};
        int[] WaterBanking = new int[FWBN] { 0,100 };

        //
        // Impt:  epWaterAugmentationUsed
        const int FAUGN = 2;
        //int[] WaterAug = new int[FAUGN] { 0, 7, 14,21};
        int[] WaterAug = new int[FAUGN] {0,21 };
        
        //
        const int FREUSEN = 2;
        //int[] WaterReuse = new int[FREUSEN] {17,50,75,100};
        //int[] WaterReuse = new int[FREUSEN] {17,100};
        int[] WaterReuse = new int[FREUSEN] { 17,100 };
         //
        const int FDroughtN = 1;
        //int[] Drought = new int[FDroughtN] {2015,2030,2060};
        int[] Drought = new int[FDroughtN] {2060 };
        //
        WaterSimManager_SIO ws;
         WaterSimDCDC.Processes.AlterGPCDFeedbackProcess dGPCD;
        //
        const int OutPutParamN = 25;
           int[] eModelParametersForOutput = new int[OutPutParamN]
          {
            eModelParam.epColorado_Historical_Extraction_Start_Year,
            eModelParam.epSaltVerde_Historical_Extraction_Start_Year,
            eModelParam.epColorado_User_Adjustment_Stop_Year,
            eModelParam.epSaltVerde_User_Adjustment_Stop_Year,
            eModelParam.epColorado_River_Flow,
            eModelParam.epSaltVerde_River_Flow,
            eModelParam.epSaltVerde_Annual_Deliveries_SRP,
            eModelParam.epColorado_Annual_Deliveries,
           //
           eModelParam.epWebWaterBank_PCT,
           eModelParam.epWebReclaimedWater_PCT,
           eModelParam.epWebUIPersonal_PCT,
           eModelParam.epWebAugmentation_PCT,
           eModelParam.epWebPop_GrowthRateAdj_PCT,
            //
            eModelParam.epGroundwater_Bank_Used,
            eModelParam.epGroundwater_Bank_Balance,
            eModelParam.epRO_Reclaimed_Water_Used,
            eModelParam.epWaterAugmentationUsed,
            eModelParam.epPopulation_Used,
            eModelParam.epGPCD_Used,
            eModelParam.epGroundwater_Pumped_Municipal,
            //
            eModelParam.epRegionalGWBalance,
            eModelParam.epGroundwater_Balance,
            eModelParam.epRegPctGWDemand,
            eModelParam.epRegGWYrsSustain,
            eModelParam.epTotal_Demand
          };
        // ============================================================
          public Scenarios(string DataDirectoryName, string TempDirectoryName)
        {
            WaterSimManager.CreateModelOutputFiles = false;
            bool run=true;
            bool runDefault=false;
            ws = new WaterSimManager_SIO(DataDirectoryName, TempDirectoryName);
            dGPCD = new WaterSimDCDC.Processes.AlterGPCDFeedbackProcess(ws, true, "Test");
            //
            ScnGenForm();
            if (run) {Run();}
            if(runDefault) {defaultRuns();}
         }
          // ---------------------------------------------------------------------
          #region Initialize
          internal int[] aOne = new int[ProviderClass.NumberOfProviders];
          internal int[] OneHundred = new int[ProviderClass.NumberOfProviders];
          ProviderIntArray One = new ProviderIntArray(0);
          internal void Init()
          {
             //
              ws.Simulation_Start_Year = 2000;
              ws.Simulation_End_Year = 2060;
              ws.BaseYear = 2016;
              //
             ws.Assured_Water_Supply_Annual_Groundwater_Pumping_Limit = 1;
             ws.SaltVerde_User_Adjustment_Start_Year = 2015;
             ws.Colorado_User_Adjustment_StartYear = 2015;
              ws.SaltVerde_User_Adjustment_Stop_Year = 2060;
              ws.Colorado_User_Adjustment_Stop_Year = 2060;
              ws.Provider_Demand_Option = 3;
              ws.SaltVerde_User_Adjustment_Percent = 81;
              ws.Colorado_User_Adjustment_Percent = 88;
             //
              //set_ColoradoTrace = 48;
              //set_SaltVerdeTrace = 48;
              ws.Colorado_Historical_Data_Source = 1;
              ws.SaltVerde_Historical_Data_Source = 1;
              //
                for (int i = 0; i < ProviderClass.NumberOfProviders; i++)
              {
                  aOne[i] = 1;
               }
               One.Values = aOne;
              ws.WaterBank_Source_Option.setvalues(One);
           }
          internal void InitDefault()
          {
              ws.Simulation_Start_Year = 2000;
              ws.Simulation_End_Year = 2060;
              ws.Assured_Water_Supply_Annual_Groundwater_Pumping_Limit = 1;
              ws.Colorado_Historical_Data_Source = 2;

          }
         //
         internal void InitDefaultDrought()
          {
              ws.Simulation_Start_Year = 2000;
              ws.Simulation_End_Year = 2061;
              ws.Assured_Water_Supply_Annual_Groundwater_Pumping_Limit = 1;
              ws.SaltVerde_User_Adjustment_Start_Year = 2015;
              ws.Colorado_User_Adjustment_StartYear = 2015;
              ws.SaltVerde_User_Adjustment_Stop_Year = 2061;
              ws.Colorado_User_Adjustment_Stop_Year = 2061;

              ws.SaltVerde_User_Adjustment_Percent = 81;
              ws.Colorado_User_Adjustment_Percent = 88;
          }
          //
          internal void InitSpecial(int gpcdReduce, int reuse, double count)
          {
               if (gpcdReduce < 100)
               {
                   ws.ParamManager.Model_Parameter(eModelParam.epProvider_Demand_Option).Value = 4;  // 7/29  WSim.ParamManager.BaseModel_ParameterBaseClass(eModelParam.epProvider_Demand_Option).Value = 4;
                   ws.Provider_Demand_Option = 4;
                   ws.ProcessManager.AddProcess(dGPCD);
               }
             
              if (25 < reuse)
              {
                  // where is effluent to reclaimed WWTP?
                  for (int i = 0; i < ProviderClass.NumberOfProviders; i++) { OneHundred[i] = 100; }
                  One.Values = OneHundred;
                  ws.PCT_Reclaimed_to_RO.setvalues(One);
                  ws.PCT_RO_to_Water_Supply.setvalues(One);
                  //ws.PCT_Max_Demand_Reclaim is (default) set at 45%;
              }
          }
          #endregion
          //
          public void Run()
          {
              DateTime value = new DateTime(2017,10,27);
              String Filename= "E:\\WaterSim\\CSharp\\WaterSim_5\\WS5_AWS_API_11_.txt";
             // String Filename = "WS5_AWS_API_11_slow.txt";
            int ScnCnt = 1;
              double Count = 1;
              double traces = FCORTraceN * FSVRTraceN;
                  double total = traces*FGPCDN*FWBN*FAUGN*FREUSEN*FpopN* FDroughtN;
              System.IO.StreamWriter MySW = new System.IO.StreamWriter(Filename);
            //
             foreach (int st in SVtrace)
              {
                  foreach (int co in COtrace)
                  {
                      foreach (int wb in WaterBanking)
                      {
                          foreach (int reuse in WaterReuse)
                          {
                              foreach (int Aug in WaterAug)
                              {
                                  foreach (int pop in PopValues)
                                  {
                                      foreach (int GPCD in GPCDValues)
                                      {
                                          //
                                          GPCD_pct = GPCD;
                                          ws.Simulation_Initialize();
                                          Init();
                                          InitSpecial(GPCD,reuse, Count);
                                          //
                                          string ScnName = GPCD_pct.ToString();
                                          //
                                          if (SetParms(st, co, wb, reuse, GPCD, Aug, pop))
                                          {
                                            //
                                            int scaler = 100;
                                            int outPrint = Convert.ToInt32(Count)*2*2;
                                              string Cstring = DateTime.Now.ToString("h:mm:ss tt") + " In step: " + "--" + Count + " Of: " + total + " > " + (Count / total) * 100 + " %";
                                                if(outPrint % scaler == 1) Console.WriteLine(Cstring);

                                              for (int year = ws.Simulation_Start_Year; year < ws.Simulation_End_Year + 1; ++year)
                                              {
                                                  ws.Simulation_NextYear();
                                              }

                                              Count += 1;
                                              ws.Simulation_Stop();
                                              if (WaterSimManager.CreateModelOutputFiles)
                                              {
                                                  CloseFiles();
                                              }
                                              if (ScnCnt == 1)
                                              {
                                                  WriteHeader(ws.SimulationRunResults, MySW);
                                              }
                                              if (WriteResults(ws.SimulationRunResults, ScnName, MySW))
                                              {
                                                  ScnCnt++;

                                              }
                                              else
                                              {
                                              }
                                          }
                                      }
                                  }
                              }
                          }
                      }
                  }
              }

         
          }
        public void defaultRuns()
       {
              int ScnCnt = 1;
              double Count = 1;
              String Filename = "OutputDefault.txt";
              double traces = FCORTraceN * FSVRTraceN;
            //  traces = 35 * 74;
              double total = traces ;

              System.IO.StreamWriter MySW = new System.IO.StreamWriter(Filename);

              foreach (int st in SVtrace)
              {
                  foreach (int co in COtrace)
                  {
                     string ScnName =
                         "default";
                     //
                     ws.Simulation_Initialize();
                     InitDefault();
                     //
                     if (SetParmsDefault(st, co))
                   {
                       //
                       string Cstring = DateTime.Now.ToString("h:mm:ss tt") + " In step: " + "--" + Count + " Of: " + total + " > " + (Count / total) * 100 + " %";
                       Console.WriteLine(Cstring);

                       for (int year = ws.Simulation_Start_Year; year < ws.Simulation_End_Year + 1; ++year)
                       {
                           ws.Simulation_NextYear();
                       }

                       Count += 1;
                       ws.Simulation_Stop();
                       if (ScnCnt == 1)
                       {
                           WriteHeader(ws.SimulationRunResults, MySW);
                       }
                       if (WriteResults(ws.SimulationRunResults, ScnName, MySW))
                       {
                           ScnCnt++;

                       }
                       else
                       {
                       }
                   }



               }
           }

        }
          public bool SetParms(int SVTraceYr, int COTraceYr, int wb, int reuse, int GPCD, int Aug, int Pop)
          {
              //(svTraceYr, coTraceYr, wb, reuse, GPCD, Aug, pop)
              bool result = true;
           
              SVTraceParm.Value = SVTraceYr;
             // ws.SaltVerde_Historical_Extraction_Start_Year = SVTraceYr;
              COTraceParm.Value = COTraceYr;
              //ws.Colorado_Historical_Extraction_Start_Year = COTraceYr;
              WBParm.Value = wb;
              //ws.Web_WaterBankPercent = wb;
              WReuseParm.Value = reuse;
              
              GPCDParm.Value = GPCD;
              //ws.Web_Personal_PCT = GPCD;
              WAugParm.Value = Aug;
             
              PopParm.Value = Pop;
             // ws.Web_PopulationGrowthRate_PCT = Pop;
              return result;
          }
      
          public bool SetParmsDefault(int SVTraceYr, int COTraceYr)
          {
              bool result = true;

              SVTraceParm.Value = SVTraceYr;
              COTraceParm.Value = COTraceYr;
                return result;
          }
          public bool SetScenarioParameters()
          {
              bool result = false;       
              result = true;
              return result;
          }
         public void ScnGenForm()
        {
            
            ws.IncludeAggregates = true;
            DroughtParmCO = ws.ParamManager.Model_Parameter(eModelParam.epColorado_User_Adjustment_Stop_Year);
            DroughtParmSV = ws.ParamManager.Model_Parameter(eModelParam.epSaltVerde_User_Adjustment_Stop_Year);

            SVTraceParm = ws.ParamManager.Model_Parameter(eModelParam.epSaltVerde_Historical_Extraction_Start_Year);
            COTraceParm = ws.ParamManager.Model_Parameter(eModelParam.epColorado_Historical_Extraction_Start_Year);
            WBParm = ws.ParamManager.Model_Parameter(eModelParam.epWebWaterBank_PCT);
            WReuseParm = ws.ParamManager.Model_Parameter(eModelParam.epWebReclaimedWater_PCT);
            GPCDParm = ws.ParamManager.Model_Parameter(eModelParam.epWebUIPersonal_PCT);
            WAugParm = ws.ParamManager.Model_Parameter(eModelParam.epWebAugmentation_PCT);
            PopParm = ws.ParamManager.Model_Parameter(eModelParam.epWebPop_GrowthRateAdj_PCT);
        }
         public class ScenarioData
          {
              List<ParmData> ParmDatas = new List<ParmData>();
              string FScnName = "";
              public ScenarioData(string aName)
              {
                  FScnName = aName;
              }

              public void AddParmData(ParmData PD)
              {
                  ParmDatas.Add(PD);
              }

              public List<ParmData> Data
              {
                  get { return ParmDatas; }
              }

              public string Name
              {
                  get { return FScnName; }
              }
          }
          public class ParmData
          {
              string Fieldname = "";
              int[] Data = new int[33];
              ModelParameterClass TheMP;
              public ParmData(string Field, int[] PData, ParameterManagerClass PM)
              {
                  Data = PData;
                  Fieldname = Field;
                  TheMP = PM.Model_Parameter(Field);
              }

              public void SetData()
              {
                  switch (TheMP.ParamType)
                  {
                      case modelParamtype.mptInputBase:
                          TheMP.Value = Data[0];
                          break;
                      case modelParamtype.mptInputProvider:
                          ProviderIntArray PData = new ProviderIntArray();
                          PData.Values = Data;
                          TheMP.ProviderProperty.setvalues(PData);
                          break;
                  }
              }
          }
          public bool WriteHeader(SimulationResults SR, System.IO.StreamWriter SW)
          {
              bool result = false;
              int FirstYear = SR.StartYear;
              AnnualSimulationResults ASR = SR.ByYear(FirstYear);
              string BaseStr = "ID,SCN_NAME,SIMYEAR";
              // now loop through base outputs and set those
              try
              {
                  int index = 0;
                  foreach (int emp in ASR.Outputs.BaseOutputModelParam)
                  {
                      // check if this is one of the output fields
                      if (isOutPutParam(emp))
                      {
                          ModelParameterClass MP = ws.ParamManager.Model_Parameter(emp);

                          BaseStr += "," + MP.Fieldname;
                      }
                      index++;
                  }
                  // lopp through base inputs
                  index = 0;
                  foreach (int emp in ASR.Inputs.BaseInputModelParam)
                  {
                      if (isOutPutParam(emp))
                      {
                          ModelParameterClass MP = ws.ParamManager.Model_Parameter(emp);

                          BaseStr += "," + MP.Fieldname;
                      }
                      index++;
                  }
                  BaseStr += ",PRVDCODE";
                  // loop through provider outputs
                  index = 0;
                  foreach (int emp in ASR.Outputs.ProviderOutputModelParam)
                  {
                      // check if for output
                      if (isOutPutParam(emp))
                      {
                          ModelParameterClass MP = ws.ParamManager.Model_Parameter(emp);

                          BaseStr += "," + MP.Fieldname;
                      }
                      index++;
                  }
                  // loop through provider inputs
                  index = 0;
                  foreach (int emp in ASR.Inputs.ProviderInputModelParam)
                  {
                      // check if for output
                      if (isOutPutParam(emp))
                      {
                          ModelParameterClass MP = ws.ParamManager.Model_Parameter(emp);

                          BaseStr += "," + MP.Fieldname;
                      }
                      index++;
                  }

                  SW.WriteLine(BaseStr);
                  result = true;
              }
              finally
              {
                  SW.Flush();
              }
              return result;
          }

          public bool WriteResults(SimulationResults SR, string ScenarioName, System.IO.StreamWriter SW)
          {

              bool result = false;
              string IDS = "";

              // get the start year            
              int FirstYear = SR.StartYear;
              // loop through all years in SR
              for (int yeari = 0; yeari < SR.Length; yeari++)
              {
                  // Set the calender year
                  int ThisYear = FirstYear + yeari;

                  // get results for this year
                  AnnualSimulationResults ASR = SR.ByYear(ThisYear);

                  // set the key, the scenario name and the year
                  string BaseStr = RecCount.ToString() + "," + '"' + ScenarioName + '"' + "," + ThisYear.ToString();
                  // now loop through base outputs and set those
                  int index = 0;
                  foreach (int emp in ASR.Outputs.BaseOutputModelParam)
                  {
                      // check if this is one of the output fields
                      if (isOutPutParam(emp))
                      {
                          BaseStr += "," + ASR.Outputs.BaseOutput.Values[index].ToString();
                      }
                      index++;
                  }
                  // lopp through base inputs
                  index = 0;
                  foreach (int emp in ASR.Inputs.BaseInputModelParam)
                  {
                      if (isOutPutParam(emp))
                      {
                          BaseStr += "," + ASR.Inputs.BaseInput.Values[index].ToString();
                      }
                      index++;
                  }
                  // OK, have all base stuff, now loop through providers
                  eProvider ep = eProvider.Regional;
                  //foreach (eProvider ep in ProviderClass.providersAll())
                  {
                      // Increment the rec count
                      RecCount++;
                      // set the base string
                      IDS = BaseStr + "," + '"' + ProviderClass.FieldName(ep) + '"';
                      // loop through provider outputs
                      index = 0;
                      foreach (int emp in ASR.Outputs.ProviderOutputModelParam)
                      {
                          // check if for output
                          if (isOutPutParam(emp))
                          {
                              if ((ep < eProvider.Regional) || (ASR.Outputs.ProviderOutput[index].IncludesAggregates))
                              {
                                  IDS += "," + ASR.Outputs.ProviderOutput[index].Values[ProviderClass.index(ep, true)].ToString();
                              }
                              else
                              {
                                  IDS += "," + SpecialValues.MissingIntValue.ToString();
                              }
                          }
                          index++;
                      }
                      // loop through provider inputs
                      index = 0;
                      foreach (int emp in ASR.Inputs.ProviderInputModelParam)
                      {
                          // check if for output
                          if (isOutPutParam(emp))
                          {
                              if ((ep < eProvider.Regional) || (ASR.Inputs.ProviderInput[index].IncludesAggregates))
                              {
                                  IDS += "," + ASR.Inputs.ProviderInput[index].Values[ProviderClass.index(ep, true)].ToString();
                              }
                              else
                              {
                                  IDS += "," + SpecialValues.MissingIntValue.ToString();
                              }
                          }
                          index++;
                      }
                      // ok write it out
                      SW.WriteLine(IDS);
                  } // provider

              } // year
              SW.Flush();
              result = true;
              return result;
          }
          public bool isOutPutParam(int theEmp)
          {
              bool found = false;
              foreach (int emp in eModelParametersForOutput)
              {
                  if (emp == theEmp)
                  {
                      found = true;
                      break;
                  }
              }
              return found;
          }

         internal void StreamW(string TempDirectoryName)
          {
              string filename = string.Concat(TempDirectoryName + "Output" + now.Month.ToString()
                  + now.Day.ToString() + now.Minute.ToString() + now.Second.ToString()
                  + "_" + ".csv");
              sw = File.AppendText(filename);
          }
          public string APiVersion { get { return _APIVersion; } }
          /// <summary>
          /// Verson of the Fortran Model
          /// </summary>
          public string ModelBuild { get { return _ModelBuild; } }
    }
}
