//ApproachAndCorrelation


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ApproachAndCorrelation
{

    class histBin
    {
        public double binFreq = 0;
        public double min = 0;
        public double max = 0;
    }

    public class Position
    {
        public double x = 0;
        public double y = 0;
        public DateTime t = new DateTime();

    }
    class PairInfo

    {
        public string PairA = "";
        public string PairOrdered = "";
        public Position PosPers1T0 = new Position();
        public Position PosPers2T0 = new Position();
        public Position PosPers1T1 = new Position();
        public Position PosPers2T1 = new Position();
        public double GetApproach()
        {

            double dist1 = Math.Sqrt(calcSquaredDist(PosPers1T1, PosPers2T0));
            double dist0 = Math.Sqrt(calcSquaredDist(PosPers1T0, PosPers2T0));
            double approachMeters = dist0 - dist1;
            return approachMeters;
        }
        public double calcSquaredDist(Position a, Position b)
        {
            Double x1 = a.x;
            Double y1 = a.y;
            Double x2 = b.x;
            Double y2 = b.y;
            return Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2);
        }
    }

    class ApproachInfo
    {
        public double Mean = 0;
        public double Median = 0;
        public double Mode = 0;
        public double Range = 0;
        public double Variance = 0;
        public double StdDev = 0;

        List<double> approaches = new List<double>();



        public void addValues(double a)
        {
            approaches.Add(a);
        }
        public void getValues()
        {
            double totalApproach = 0;
            foreach (double a in approaches)
            {
                totalApproach += a;
            }
            if (approaches.Count != 0)
            {
                Mean = totalApproach / approaches.Count;
                int HalfIndex = approaches.Count() / 2;
                var sortedapproaches = approaches.OrderBy(i => i);

                Mode = approaches.GroupBy(i => i).
                    OrderByDescending(n => n.Count()).
                    Select(n => n.Key).FirstOrDefault();

                double RangeMin = sortedapproaches.Min();
                double RangeMax = sortedapproaches.Max();
                Range = Math.Abs(RangeMax - RangeMin);


                Variance = approaches.Select(i => (i - Mean) * (i - Mean)).Sum();
                StdDev = Math.Sqrt(Variance);

                if (approaches.Count % 2 == 0)
                {
                    Median = ((sortedapproaches.ElementAt(HalfIndex) + sortedapproaches.ElementAt(HalfIndex - 1))) / 2;

                }
                else
                {
                    Median = sortedapproaches.ElementAt(HalfIndex);
                }

            }

        }

    }
    class ApproachesInfo
    {
        public ApproachInfo TotalInfo = new ApproachInfo();
        public ApproachInfo PosInfo = new ApproachInfo();
        public ApproachInfo NegInfo = new ApproachInfo();


    }
    class Program
    {

        public void getMinApproach(String byTimeType, String displayTimeType, String path, String fileVersion, String szDay)
        {
            String filePath = path + szDay + fileVersion;



            Dictionary<String, Dictionary<String, ApproachesInfo>> approachMinute = new Dictionary<string, Dictionary<String, ApproachesInfo>>();


            using (StreamReader sr = new StreamReader("/Users/chitrabanarjee/Desktop/Messinger/approaches_tenthofasec/approach_trunk_2_20_2019_7_31_2019_1777209647.CSV"))
            //filePath))
            {
                sr.ReadLine();
                while ((!sr.EndOfStream))// && lineCount<10000)
                {
                    String commaLine = sr.ReadLine();
                    String[] columns = commaLine.Split(',');
                    if (columns.Length > 6)
                    {
                        String person1 = columns[0];
                        String person2 = columns[1];
                        DateTime stime = DateTime.Parse(columns[2]);
                        String szTime = "";
                        DateTime time = new DateTime(Convert.ToInt32(szDay.Split('_')[2]), Convert.ToInt32(szDay.Split('_')[0]), Convert.ToInt32(szDay.Split('_')[1]));
                        time = time.AddHours(stime.Hour);
                        time = time.AddMinutes(stime.Minute);
                        time = time.AddSeconds(stime.Second);
                        time = time.AddMilliseconds(stime.Millisecond);

                        if (byTimeType == "SEC")
                        {
                            szTime = time.ToShortDateString() + " " + time.ToShortTimeString();
                            szTime = szTime.Replace(" AM", ":" + time.Second + " AM").Replace(" PM", ":" + time.Second + " PM");
                        }
                        else if (byTimeType == "MIN")
                        {
                            szTime = time.ToShortDateString() + " " + time.ToShortTimeString();
                        }
                        else
                        {
                            szTime = time.ToShortDateString();
                        }
                        double approach = Convert.ToDouble(columns[6]);
                        String pair = person1 + "|" + person2;

                        if (!approachMinute.ContainsKey(pair))
                            approachMinute.Add(pair, new Dictionary<String, ApproachesInfo>());

                        ApproachInfo pairInfo = new ApproachInfo();

                        if (!approachMinute[pair].ContainsKey(szTime))
                        {
                            ApproachesInfo Totalinfo = new ApproachesInfo();
                            approachMinute[pair].Add(szTime, new ApproachesInfo());


                        }
                        approachMinute[pair][szTime].TotalInfo.addValues(approach);

                        if (approach > 0)
                            approachMinute[pair][szTime].PosInfo.addValues(approach);
                        else
                            approachMinute[pair][szTime].NegInfo.addValues(approach);



                    }
                }
            }

            String todayVersion = "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year + "_" + (new Random().Next());
            String outputFileName = "approachesDATA_" + szDay + todayVersion + "_TIME" + byTimeType + "_DISPLAY" + displayTimeType + ".CSV";
            TextWriter approachPairs = new StreamWriter(outputFileName, false, System.Text.Encoding.Default);// "approachesDATA_05_30_19perHOUR.csv");

            approachPairs.WriteLine("Pair, Approacher, Approachee, Time, " +
                "Mean, PosMean, NegMean, " +
                "Median, PosMedian, NegMedian, " +
                "Mode, PosMode, NegMode, " +
                "Range, PosRange, NegRange, " +
                "Variance, PosVariance, NegVariance, " +
                "StdDev, PosStdDev, NegStdDev ");

            int multForDisplay = displayTimeType == "SEC" ? 10 : (displayTimeType == "MIN" ? 600 : 36000);

            foreach (KeyValuePair<String, Dictionary<String, ApproachesInfo>> pairValue in approachMinute.OrderBy(i => i.Key))
            {
                String pair = pairValue.Key;
                foreach (String t in approachMinute[pair].Keys)
                {
                    approachMinute[pair][t].TotalInfo.getValues();
                    approachMinute[pair][t].PosInfo.getValues();
                    approachMinute[pair][t].NegInfo.getValues();

                    approachPairs.WriteLine(pair + "," + pair.Replace("|", ",") + "," + t + "," +
                        approachMinute[pair][t].TotalInfo.Mean * multForDisplay + "," +////////////////
                        approachMinute[pair][t].PosInfo.Mean + "," +
                        approachMinute[pair][t].NegInfo.Mean + "," +
                        approachMinute[pair][t].TotalInfo.Median + "," +
                        approachMinute[pair][t].PosInfo.Median + "," +
                        approachMinute[pair][t].NegInfo.Median + "," +
                        approachMinute[pair][t].TotalInfo.Mode + "," +
                        approachMinute[pair][t].PosInfo.Mode + "," +
                        approachMinute[pair][t].NegInfo.Mode + "," +
                        approachMinute[pair][t].TotalInfo.Range + "," +
                        approachMinute[pair][t].PosInfo.Range + "," +
                        approachMinute[pair][t].NegInfo.Range + "," +
                        approachMinute[pair][t].TotalInfo.Variance + "," +
                        approachMinute[pair][t].PosInfo.Variance + "," +
                        approachMinute[pair][t].NegInfo.Variance + "," +
                        approachMinute[pair][t].TotalInfo.StdDev + "," +
                        approachMinute[pair][t].PosInfo.StdDev + "," +
                        approachMinute[pair][t].NegInfo.StdDev);


                }

                //approachPairs.WriteLine(pair + "," + pairApproaches[pair].Item1 + "," + pairApproaches[pair].Item2 + "," + (pairApproaches[pair].Item2 / pairApproaches[pair].Item1));
            }
            approachPairs.Close();

        }
        //NOT RUN!!!!!
        public void getGeneralApproach()
        {
            String filePath = "/Volumes/My Passport/PRIDE_LEAP/PRIDE_LEAP_AM/SYNC/approach_trunk_1_23_2019_7_12_2019_1936293797.CSV";
            Dictionary<String, Dictionary<DateTime, double>> approaches = new Dictionary<string, Dictionary<DateTime, double>>();
            Dictionary<String, Tuple<double, double>> pairApproaches = new Dictionary<string, Tuple<double, double>>();

            using (StreamReader sr = new StreamReader(filePath))
            {
                sr.ReadLine();
                while ((!sr.EndOfStream))// && lineCount<10000)
                {
                    String commaLine = sr.ReadLine();
                    String[] columns = commaLine.Split(',');
                    if (columns.Length > 6)
                    {
                        String person1 = columns[0];
                        String person2 = columns[1];
                        DateTime time = DateTime.Parse(columns[2]);

                        int ms = Convert.ToInt16(columns[3]);
                        if (ms > 0)
                        {
                            bool stop = true;
                        }
                        time = time.AddMilliseconds(ms);
                        double approach = Convert.ToDouble(columns[6]);

                        String pair = person1 + "|" + person2;
                        if (!approaches.ContainsKey(pair))
                        {
                            approaches.Add(pair, new Dictionary<DateTime, double>());
                            pairApproaches.Add(pair, new Tuple<double, double>(0.1, approach));
                        }
                        else
                        {
                            Tuple<double, double> newvalues = new Tuple<double, double>(pairApproaches[pair].Item1 + 0.1, pairApproaches[pair].Item2 + approach);
                            pairApproaches[pair] = newvalues;
                            if (Double.IsNaN(newvalues.Item2))
                            {
                                bool stop = true;
                            }

                        }
                        if (!approaches[pair].ContainsKey(time))
                        {
                            approaches[pair].Add(time, approach);
                        }
                    }

                }
            }

            TextWriter approachPairs = new StreamWriter("approaches.csv");

            approachPairs.WriteLine("Pair, TotalTime, SumApproach, AvgApproach,");

            foreach (KeyValuePair<String, Tuple<double, double>> pairValue in pairApproaches.OrderBy(i => i.Key))
            {
                String pair = pairValue.Key;
                approachPairs.WriteLine(pair + "," + pairApproaches[pair].Item1 + "," + pairApproaches[pair].Item2 + "," + (pairApproaches[pair].Item2 / pairApproaches[pair].Item1));
            }
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
        static void MainStats()
        {
            // String filePath = "/Volumes/My Passport/APPROACH/PRIDE_LEAP/approach_trunk_5_30_2019_7_24_2019_2041261642.CSV";

            String fileVersion = "_7_31_2019_1777209647.CSV";
            String path = "approach_trunk_";
            //"/Users/chitrabanarjee/Desktop/Messinger/approach_trunk_";
            //5_30_2019_7_24_2019_2041261642.CSV";
            String szDays = "2_20_19";
            String[] days = szDays.Split(',');

            foreach (String szDay in days)
            {

                Program p = new Program();
                p.getMinApproach("DAY", "HOUR", path, fileVersion, szDay);

            }





            Console.WriteLine("Finished!");
            Console.ReadLine();
        }

        static void approachesSecond(String FileName, Boolean includeTeachers, Boolean includeLabs)
        {
            Dictionary<string, Dictionary<DateTime, PairInfo>> pairApproaches = new Dictionary<string, Dictionary<DateTime, PairInfo>>();

            TextWriter SecondFile = new StreamWriter(FileName.Replace(".", "_SECOND."), false);
            using (StreamReader readerFile = new StreamReader(FileName))
            {
                //"/Users/chitrabanarjee/Desktop/Messinger/approaches_tenthofasec/approach_trunk_2_20_2019_7_31_2019_1777209647.CSV"
                string line = readerFile.ReadLine();
                SecondFile.WriteLine(line + ",PairA,PairOrdered,Date,Time");
                while ((!readerFile.EndOfStream))
                {
                    line = readerFile.ReadLine();
                    string[] lineColumns = line.Split(',');
                    string person1 = lineColumns[0];
                    string person2 = lineColumns[1];
                    string szTime = lineColumns[2];
                    DateTime time = Convert.ToDateTime(szTime);
                    string szMillisec = lineColumns[3];

                    if ((includeLabs && includeTeachers) ||
                        ((!includeLabs) && (!(person1.Contains("Lab") || person2.Contains("Lab")))) ||
                        ((!includeTeachers) && (!(person1.Contains("T") || person2.Contains("T")))))
                    {

                        string pairA = person1 + "|" + person2;
                        int iperson1 = Convert.ToInt32(new string(person1.Where(c => char.IsDigit(c)).ToArray()));
                        int iperson2 = Convert.ToInt32(new string(person2.Where(c => char.IsDigit(c)).ToArray()));
                        string pairOrdered = iperson1 < iperson2 ? iperson1 + "|" + iperson2 : iperson2 + "|" + iperson1;

                        string szDay = FileName.Substring(FileName.LastIndexOf("approach_trunk_", StringComparison.CurrentCulture) + 15);
                        szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));
                        szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));
                        szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));
                        szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));

                        PairInfo pi = new PairInfo();
                        pi.PairA = pairA;
                        pi.PairOrdered = pairOrdered;


                        PairInfo pi0 = new PairInfo();
                        PairInfo pi1 = new PairInfo();
                        Position posP1T0 = new Position();
                        Position posP2T0 = new Position();
                        posP1T0.t = time;
                        posP2T0.t = time;
                        posP1T0.x = Convert.ToDouble(lineColumns[7]);
                        posP1T0.y = Convert.ToDouble(lineColumns[8]);
                        posP2T0.x = Convert.ToDouble(lineColumns[9]);
                        posP2T0.y = Convert.ToDouble(lineColumns[10]);


                        if (!pairApproaches.ContainsKey(pairOrdered))
                        {
                            pairApproaches.Add(pairOrdered, new Dictionary<DateTime, PairInfo>());
                        }
                        if (!pairApproaches[pairOrdered].ContainsKey(time))
                        {
                            pi0.PosPers1T0 = posP1T0;
                            pi0.PosPers2T0 = posP2T0;
                            pi1.PosPers1T1 = posP1T0;
                            pi1.PosPers2T1 = posP2T0;

                            //pairApproaches[pairOrdered].Add(time, PairInfo);

                        }

                        //SecondFile.WriteLine(line + "," + pairA + "," + pairOrdered + "," + szDay + "," + szTimeMS);
                    }

                }

            }


            SecondFile.Close();

        }
        static String getShortId(String szId)
        {
            String szRes = szId;
            //bool isLongId = szId.IndexOf("_") != szId.LastIndexOf("_");
            //if(isLongId)
            {
                szRes = szRes.Substring(szRes.LastIndexOf("_") + 1);

            }
            return szRes;


        }
        static List<String> MainConvertTenToOrdered(String FileName, Boolean includeTeachers, Boolean includeLabs, String version)
        {
            int lineCount = 1;
            int lineMax = 1000000;
            string line = "";
            /*using (StreamReader readerFile = new StreamReader(FileName))
            {
                //"/Users/chitrabanarjee/Desktop/Messinger/approaches_tenthofasec/approach_trunk_2_20_2019_7_31_2019_1777209647.CSV"
                line = readerFile.ReadLine();
                while ((!readerFile.EndOfStream))
                {
                    line = readerFile.ReadLine();
                    string[] lineColumns = line.Split(',');
                    if(lineColumns.Length!=25)
                    {
                        Console.WriteLine("lineLength " + lineColumns.Length + "lineCount " + lineCount);
                        Console.WriteLine(line);
                        /*swa.WriteLine("Person 1, 
                         * Person2, 
                         * Interaction Time, 
                         * Interaction Millisecond,
                         * d1,
                         * d2,
                         * approachMeters,
                         * x10,
                         * y10,
                         * x20,
                         * y20,
                         * x11,
                         * y11,x21,y21, WithinGR, WithinGRAnd45deg, Angle1, Angle2,Type1, Type2, Gender1, Gender2, Diagnosis1, Diagnosis2 ");
                         *//*
                    }
                    lineCount++;
                }
            }
            Console.WriteLine("lineCount " + lineCount);
            Console.WriteLine("lastLine " + line );
    */
            //Console.ReadLine();



                    List<String> orderedPairList = new List<string>();

            TextWriter OrderedFile = new StreamWriter(FileName.Replace(".", "_"+ version+"Ordered."), false);
            using (StreamReader readerFile = new StreamReader(FileName))
            {
                //"/Users/chitrabanarjee/Desktop/Messinger/approaches_tenthofasec/approach_trunk_2_20_2019_7_31_2019_1777209647.CSV"
                line = readerFile.ReadLine();
                OrderedFile.WriteLine(line + ",PairA,PairOrdered,Date,TimeMS");
                while ((!readerFile.EndOfStream))
                {
                    lineCount++;
                    //if (lineCount > lineMax)
                    //    break;

                    //DELETE


                    line = readerFile.ReadLine();
                    string[] lineColumns = line.Split(',');
                    string person1 = lineColumns[0];
                    string person2 = lineColumns[1];

                    bool isLongId = person1.IndexOf("_") != person1.LastIndexOf("_");
                    if (isLongId)
                    {
                        person1 = getShortId(person1);
                        person2 = getShortId(person2);
                    }

                    string szTime = lineColumns[2];
                    string szMillisec = lineColumns[3];
                    string szTimeMS = szTime.Replace(" AM", "." + szMillisec + " AM").Replace(" PM", "." + szMillisec + " PM");
                    Boolean isTeacher1 = person1.Contains("T");
                    Boolean isLab1 = (person1.Contains("L"));//: (person1.Contains("Lab") || person2.Contains("Lab"));
                    Boolean isTeacher2 = person2.Contains("T");
                    Boolean isLab2 = (person2.Contains("L"));//: (person1.Contains("Lab") || person2.Contains("Lab"));

                    if ((includeLabs && includeTeachers) ||
                        ((!includeLabs) && (!includeTeachers) && (!(isTeacher1 || isLab1 || isTeacher2 || isLab2))) ||
                        (((!includeLabs) && (!(isLab1 || isLab2))) &&
                        ((!includeTeachers) && (!(isTeacher1 || isTeacher2))))
                        //((!includeLabs) && (!(person1.Contains("Lab") || person2.Contains("Lab")))) &&
                        //((!includeTeachers) && (!(person1.Contains("T") || person2.Contains("T"))))
                        )
                    {
                        string pairA = person1 + "|" + person2;
                        int iperson1 = Convert.ToInt32(new string(person1.Where(c => char.IsDigit(c)).ToArray()));
                        int iperson2 = Convert.ToInt32(new string(person2.Where(c => char.IsDigit(c)).ToArray()));
                        string pairOrdered = iperson1 < iperson2 ? iperson1 + "|" + iperson2 : iperson2 + "|" + iperson1;

                        if (!orderedPairList.Contains(pairOrdered))
                        {
                            orderedPairList.Add(pairOrdered);
                        }

                        String szNewLine = line;//.IndexOf(",") != line.LastIndexOf(",") ? iperson1 + "," + iperson1 + "," + line.Substring(line.IndexOf(",") + 1).Substring(line.IndexOf(",") + 1) : line;
                        if (line.IndexOf(",") != line.LastIndexOf(","))
                        {
                            szNewLine = szNewLine.Substring(szNewLine.IndexOf(",") + 1);
                            szNewLine = szNewLine.Substring(szNewLine.IndexOf(",") + 1);
                            szNewLine = iperson1 + "," + iperson2 + "," + szNewLine;

                        }

                        string szDay = FileName.Substring(FileName.LastIndexOf("approach_trunk_", StringComparison.CurrentCulture) + 15);
                        szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));
                        szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));
                        szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));
                        szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));


                        OrderedFile.WriteLine(szNewLine + "," + pairA + "," + pairOrdered + "," + szDay + "," + szTimeMS);
                    }

                }

            }


            OrderedFile.Close();
            return orderedPairList;

        }

        static void HistFile(String FileName)
        {
            TextWriter OrderedFile = new StreamWriter(FileName.Replace(".", "_Hist."));
            Dictionary<String, Tuple<double, double>> pairMinMax = new Dictionary<string, Tuple<double, double>>();
            Dictionary<String, List<double>> pairApps = new Dictionary<string, List<double>>();
            Dictionary<String, histBin[]> pairBins = new Dictionary<string, histBin[]>();

            using (StreamReader readerFile = new StreamReader(FileName))
            {
                string line = readerFile.ReadLine();
                //OrderedFile.WriteLine(line + ",PairA,PairOrdered,Date,TimeMS");
                while ((!readerFile.EndOfStream))
                {
                    line = readerFile.ReadLine();
                    string[] lineColumns = line.Split(',');
                    string person1 = lineColumns[0];
                    string person2 = lineColumns[1];
                    double approach = Convert.ToDouble(lineColumns[6]);

                    string pairA = person1 + "|" + person2;
                    int iperson1 = Convert.ToInt32(new string(person1.Where(c => char.IsDigit(c)).ToArray()));
                    int iperson2 = Convert.ToInt32(new string(person2.Where(c => char.IsDigit(c)).ToArray()));
                    string pairOrdered = iperson1 < iperson2 ? iperson1 + "|" + iperson2 : iperson2 + "|" + iperson1;
                    if (!pairMinMax.ContainsKey(pairA))
                    {
                        pairMinMax.Add(pairA, new Tuple<double, double>(approach, approach));
                        pairApps.Add(pairA, new List<double>());
                        pairBins.Add(pairA, new histBin[100]);
                    }
                    else
                    {
                        double min = pairMinMax[pairA].Item1 > approach ? approach : pairMinMax[pairA].Item1;
                        double max = pairMinMax[pairA].Item2 < approach ? approach : pairMinMax[pairA].Item2;
                        pairMinMax[pairA] = new Tuple<double, double>(min, max);
                    }
                    pairApps[pairA].Add(approach);
                }





            }


            foreach (String pair in pairMinMax.Keys)
            {
                double pairMin = pairMinMax[pair].Item1;
                double pairMax = pairMinMax[pair].Item2;
                double binSize = (pairMax - pairMin) / 100;

                for (int listPos = 0; listPos < pairApps[pair].Count; listPos++)
                {
                    double thisApproach = pairApps[pair][listPos];

                    // -100
                    double cap = pairMax - binSize;
                    int binNum = 99;
                    for (; binNum > 0; binNum--)
                    {
                        if (thisApproach > cap)
                            break;
                        else
                            cap = cap - binSize;

                    }

                    if (pairBins[pair][binNum] == null)
                    {
                        pairBins[pair][binNum] = new histBin();
                    }
                    pairBins[pair][binNum].binFreq++;



                }




            }

            OrderedFile.WriteLine("Pair, Bin Number, Bin Start, Bin End, BinFreq");
            foreach (String pair in pairBins.Keys)
            {
                for (int binNum = 0; binNum < 100; binNum++)
                {
                    if (pairBins[pair][binNum] == null)
                    {
                        pairBins[pair][binNum] = new histBin();
                    }
                    double pairMin = pairMinMax[pair].Item1;
                    double pairMax = pairMinMax[pair].Item2;
                    double binSize = (pairMax - pairMin) / 100;
                    double binStart = pairMin + (binSize * binNum);
                    double binEnd = binStart + binSize;

                    OrderedFile.WriteLine(pair + "," + binNum + "," + binStart + "," + binEnd + ","
                        + pairBins[pair][binNum].binFreq);
                }


                //
            }



            OrderedFile.Close();
        }

        static string GetApproachSecondFile(String nameFile)
        {
            Dictionary<String, Tuple<Position, Position, Position>> approachSec = new Dictionary<string, Tuple<Position, Position, Position>>();

            String szNewFile = nameFile.Replace(".", "_bySec.");
            TextWriter OrderedFile = new StreamWriter(szNewFile, false);
            using (StreamReader readerFile = new StreamReader(nameFile))
            {
                //"/Users/chitrabanarjee/Desktop/Messinger/approaches_tenthofasec/approach_trunk_2_20_2019_7_31_2019_1777209647.CSV"
                string line = readerFile.ReadLine();
                OrderedFile.WriteLine("Person1, Person2, Interaction Time, Interaction Millisecond,d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21");
                //"Person 1, Person2, Interaction Time, Interaction Millisecond,d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21 "
                while ((!readerFile.EndOfStream))
                {
                    line = readerFile.ReadLine();
                    string[] lineColumns = line.Split(',');

                    if (lineColumns.Length > 12)
                    {


                        string person1 = lineColumns[0];
                        string person2 = lineColumns[1];
                        string szTime = lineColumns[2];
                        string szMillisec = lineColumns[3];
                        //string szTimeMS = szTime.Replace(" AM", "." + szMillisec + " AM").Replace(" PM", "." + szMillisec + " PM");
                        DateTime timeAt = Convert.ToDateTime(szTime);
                        timeAt = timeAt.AddMilliseconds(Convert.ToDouble(szMillisec));


                        string PairA = person1 + "|" + person2;

                        Position positiona0 = new Position();
                        positiona0.t = Convert.ToDateTime(timeAt);
                        positiona0.x = Convert.ToDouble(lineColumns[8]);
                        positiona0.y = Convert.ToDouble(lineColumns[9]);
                        Position positionb0 = new Position();
                        positionb0.t = Convert.ToDateTime(timeAt);
                        positionb0.x = Convert.ToDouble(lineColumns[10]);
                        positionb0.y = Convert.ToDouble(lineColumns[11]);
                        Position positiona1 = new Position();
                        positiona1.t = Convert.ToDateTime(timeAt);
                        positiona1.x = Convert.ToDouble(lineColumns[12]);
                        positiona1.y = Convert.ToDouble(lineColumns[13]);



                        if (!approachSec.ContainsKey(PairA))
                        {
                            Tuple<Position, Position, Position> pos = new Tuple<Position, Position, Position>(positiona0, positionb0, positiona1);
                            approachSec.Add(PairA, pos);




                        }
                        else
                        {
                            TimeSpan difference = positiona0.t.Subtract(approachSec[PairA].Item3.t);
                            if (difference.TotalSeconds < 1)
                            {
                                Tuple<Position, Position, Position> pos = new Tuple<Position, Position, Position>(approachSec[PairA].Item1, approachSec[PairA].Item2, positiona1);
                                approachSec[PairA] = pos;

                            }
                            else
                            {
                                Position x10p = approachSec[PairA].Item1;
                                Position x20p = approachSec[PairA].Item2;
                                Position x11p = approachSec[PairA].Item3;
                                double d0 = Math.Sqrt(Math.Pow((x10p.x - x20p.x), 2) + Math.Pow((x10p.y - x20p.y), 2));
                                double d1 = Math.Sqrt(Math.Pow((x11p.x - x20p.x), 2) + Math.Pow((x11p.y - x20p.y), 2));
                                double approachMeters = d0 - d1;
                                //OrderedFile.WriteLine()
                                ////"Person 1, Person2, Interaction Time, Interaction Millisecond,d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21 "
                                OrderedFile.WriteLine(lineColumns[0] + "," +
                                    lineColumns[1] + "," +
                                    lineColumns[2] + "," +
                                    lineColumns[3] + "," +
                                    d0 + "," +
                                    d1 + "," +
                                    approachMeters + "," +
                                    x10p.x + "," + x10p.y + "," +
                                    x20p.x + "," + x20p.y + "," +
                                    x11p.x + "," + x11p.y
                                    );

                                Tuple<Position, Position, Position> pos = new Tuple<Position, Position, Position>(positiona0, positionb0, positiona1);
                                approachSec[PairA] = pos;


                                //if (positiona0.t.Second=!)
                            }




                            if (szMillisec.Trim() == "0")
                            {
                                OrderedFile.WriteLine(line);
                            }
                            string szTimeMS = szTime.Replace(" AM", "." + szMillisec + " AM").Replace(" PM", "." + szMillisec + " PM");
                        }
                    }
                }

            }
            return szNewFile;
        }

        class smaClass
        {
            public double d0 = 0;
            public double app = 0;
            public string person1 = "";
            public string person2 = "";
            public string pairA = "";
            public string pairOrdered = "";
            public string date = "";
            public int index = 0;
            public DateTime timeMS = DateTime.Now;

        }


        static Dictionary<String, List<smaClass>> SMA(String fileAppTenthOrdered, int order, String szSMAfile, List<String> pairsToInclude, Boolean append)
        {
            // Dictionary<String, List<smaClass>> smas = new Dictionary<string, List<smaClass>>();
            Dictionary<String, List<smaClass>> smasByOrder = new Dictionary<string, List<smaClass>>();

            Dictionary<String, List<String>> szSmas = new Dictionary<string, List<String>>();



            TextWriter NewFile = new StreamWriter(szSMAfile, append);
            if (!append)
                NewFile.WriteLine("Person1,Person2,d0,approachMeters,approachWeighted,PairA,PairOrdered,Date,RealIndex,Index,Time");

            int lineCount = 0;
            using (StreamReader sr = new StreamReader(fileAppTenthOrdered))

            {
                sr.ReadLine();
                while ((!sr.EndOfStream))// && lineCount<1000000)// && lineCount<10000)
                {
                    lineCount++;

                    if (lineCount == 7201231)
                        lineCount = lineCount;
                    try
                    {
                        String commaLine = sr.ReadLine();
                        String[] columns = commaLine.Split(',');
                        if (columns.Length > 4)//16)
                        {
                            //  Person 1, Person2, Interaction Time, Interaction Millisecond,d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21, 
                            // WithinGR, WithinGRAnd45deg, Angle1, Angle2,Type1, Type2, Gender1, Gender2, Diagnosis1, Diagnosis2 ,
                            //PairA,PairOrdered,Date,TimeMS

                            String person1 = columns[0].Trim();
                            String person2 = columns[1].Trim();
                            double d0 = Convert.ToDouble(columns[4]);
                            double d1 = Convert.ToDouble(columns[5]);
                            double app = Convert.ToDouble(columns[6]);
                            String pairA = columns[25].Trim();//15
                            String pairOrdered = columns[26].Trim();//16
                            if (pairsToInclude.Contains(pairOrdered))
                            {
                                String date = columns[27].Trim();//17
                                String szTimeMs = columns[28].Trim();
                                // if (!smas.ContainsKey(pairA))
                                //     smas.Add(pairA, new List<smaClass>());
                                if (!szSmas.ContainsKey(pairA))
                                    szSmas.Add(pairA, new List<String>());
                                szSmas[pairA].Add(
                                    person1 + "," +
                                    person2 + "," +
                                    d0 + "," +
                                    app + "," +
                                    pairA + "," +
                                    pairOrdered + "," +
                                    date+","+ szTimeMs);

                                
                                /* smaClass sma = new smaClass();
                                 sma.d0 = d0;
                                 sma.app = app;
                                 sma.person1 = person1;
                                 sma.person2 = person2;
                                 sma.pairA = pairA;
                                 sma.pairOrdered = pairOrdered;
                                 sma.date = date;
                                 smas[pairA].Add(sma);*/
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR" + e.Message);

                    }
                }

                foreach (String pairA in szSmas.Keys)
                {

                    int pairIndex = 0;
                    int realPairIndex = 0;
                    DateTime lastTimeStamp = DateTime.Now;
                    Boolean notFirstStamp = false;
                    for (int orderPos = 0; orderPos + order <= szSmas[pairA].Count; orderPos++)
                    {

                        try
                        {
                            smaClass smaVals = new smaClass();
                            String[] lineColumns = szSmas[pairA][orderPos].Split(',');
                            double totalD0 = Convert.ToDouble(lineColumns[2]);
                            double totalApp = Convert.ToDouble(lineColumns[3]);
                            for (int smaPos = 1; smaPos < order; smaPos++)
                            {
                                String[] lineColumns2 = szSmas[pairA][orderPos + smaPos].Split(',');

                                totalApp += Convert.ToDouble(lineColumns2[3]);
                                totalD0 += Convert.ToDouble(lineColumns2[2]);
                            }
                            /*szSmas[pairA].Add( 
                                person1 + "," +
                                person2 + "," +
                                d0 + "," +
                                app + "," +
                                pairA + "," +
                                pairOrdered + ","+
                                date);*/
                            smaVals.app = totalApp != 0 ? totalApp / order : 0;
                            smaVals.d0 = totalD0 != 0 ? totalD0 / order : 0;
                            smaVals.person1 = lineColumns[0].Trim();
                            smaVals.person2 = lineColumns[1].Trim();
                            smaVals.pairA = lineColumns[4].Trim();
                            smaVals.pairOrdered = lineColumns[5].Trim();
                            smaVals.date = lineColumns[6].Trim();

                            smaVals.timeMS = Convert.ToDateTime(lineColumns[7].Trim());
                            if (!smasByOrder.ContainsKey(pairA))
                            {
                                smasByOrder.Add(pairA, new List<smaClass>());
                            }
                            //pairIndex
                            smaVals.index = pairIndex;


                            if (notFirstStamp)
                            {
                                
                                TimeSpan difference = smaVals.timeMS.Subtract(lastTimeStamp);
                                if (difference.TotalMilliseconds > 100)
                                {
                                    smaClass smaValsMissing = new smaClass();

                                    smaValsMissing.app =  0;
                                    smaValsMissing.d0 = 0;
                                    smaValsMissing.person1 = lineColumns[0].Trim();
                                    smaValsMissing.person2 = lineColumns[1].Trim();
                                    smaValsMissing.pairA = lineColumns[4].Trim();
                                    smaValsMissing.pairOrdered = lineColumns[5].Trim();
                                    smaValsMissing.date = lineColumns[6].Trim();
                                    smaVals.index = pairIndex;
                                     

                                    while(difference.TotalMilliseconds > 100)
                                    {
                                        smaValsMissing.timeMS = Convert.ToDateTime(lastTimeStamp).AddMilliseconds(100);
                                        
                                        if (!smasByOrder.ContainsKey(pairA))
                                        {
                                            smasByOrder.Add(pairA, new List<smaClass>());
                                        }
                                        smasByOrder[pairA].Add(smaValsMissing);
                                        
                                        NewFile.WriteLine(smaValsMissing.person1 + "," +
                                        smaValsMissing.person2 + ",,,," +
                                        smaValsMissing.pairA + "," + smaValsMissing.pairOrdered +
                                        "," + smaValsMissing.date + "," + realPairIndex + "," + pairIndex + "," + 
                                        smaValsMissing.timeMS.Hour+":"+ smaValsMissing.timeMS.Minute + ":" + smaValsMissing.timeMS.Second + "." + smaValsMissing.timeMS.Millisecond );
                                        realPairIndex++;
                                        lastTimeStamp = smaValsMissing.timeMS;
                                        difference = smaVals.timeMS.Subtract(lastTimeStamp);
                                    }



                                    //realPairIndex += (Convert.ToInt16(difference.TotalMilliseconds/10) - 100);
                                }
                            }
                            else
                            {
                                notFirstStamp = true;
                            }
                            lastTimeStamp = smaVals.timeMS;


                            smasByOrder[pairA].Add(smaVals);
                            double weightA = smaVals.app / smaVals.d0;
                            NewFile.WriteLine(smaVals.person1 + "," +
                            smaVals.person2    + "," + smaVals.d0 + "," + smaVals.app + "," +
                            weightA + "," + smaVals.pairA + "," + smaVals.pairOrdered +
                            "," + smaVals.date + "," + realPairIndex + "," + pairIndex + "," +
                             smaVals.timeMS.Hour + ":" + smaVals.timeMS.Minute + ":" + smaVals.timeMS.Second + "." + smaVals.timeMS.Millisecond);

                          /*  Console.WriteLine(smaVals.person1 + "," +
                            smaVals.person2  + "," + smaVals.d0 + "," + smaVals.app + "," +
                            weightA + "," + smaVals.pairA + "," + smaVals.pairOrdered +
                            "," + smaVals.date + "," + pairIndex);*/
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR2: " + e.Message);
                        }
                        pairIndex++;
                        realPairIndex++;
                    }


                }
                //Person1, Person2, Interaction Time, Interaction Millisecond,d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21,PairA,PairOrdered,Date

            }

            // 1 2 3 4
            /*  foreach (String pairA in smas.Keys)
              {
                  for (int orderPos = 0; orderPos + order <= smas[pairA].Count; orderPos++)
                  {

                      try
                      {
                          smaClass smaVals = new smaClass();
                          double totalD0 = smas[pairA][orderPos].d0;
                          double totalApp = smas[pairA][orderPos].app;
                          for (int smaPos = 1; smaPos < order; smaPos++)
                          {
                              totalApp += smas[pairA][orderPos].app;
                              totalD0 += smas[pairA][orderPos].d0;
                          }
                          smaVals.app = totalApp != 0 ? totalApp / order : 0;
                          smaVals.d0 = totalD0 != 0 ? totalD0 / order : 0;
                          smaVals.person1 = smas[pairA][orderPos].person1;
                          smaVals.person2 = smas[pairA][orderPos].person2;
                          smaVals.pairA = smas[pairA][orderPos].pairA;
                          smaVals.pairOrdered = smas[pairA][orderPos].pairOrdered;
                          smaVals.date = smas[pairA][orderPos].date;
                          if (!smasByOrder.ContainsKey(pairA))
                          {
                              smasByOrder.Add(pairA, new List<smaClass>());
                          }

                          smasByOrder[pairA].Add(smaVals);
                          double weightA = smaVals.app / smaVals.d0;
                          NewFile.WriteLine(smaVals.person1 + "," +
                          smaVals.person2 + "," + smaVals.d0 + "," + smaVals.app + "," +
                          weightA + "," + smaVals.pairA + "," + smaVals.pairOrdered +
                          "," + smaVals.date);

                          Console.WriteLine(smaVals.person1 + "," +
                          smaVals.person2 + "," + smaVals.d0 + "," + smaVals.app + "," +
                          weightA + "," + smaVals.pairA + "," + smaVals.pairOrdered +
                          "," + smaVals.date);
                      }
                      catch (Exception e)
                      {
                          Console.WriteLine("ERROR2: " + e.Message);
                      }
                  }


              }}*/
            //Person1, Person2, Interaction Time, Interaction Millisecond,d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21,PairA,PairOrdered,Date


            NewFile.Close();
            return smasByOrder;
        }
        //  Corr("test_data_SMA-10.csv",20, "test_data_SMA-10_CORRS.csv");
        static void OldCorr(String szFileName, int intervalSize, String szOutputFileName)
        {

            List<double> originalArray1 = new List<double>();// { 3, 2, 4, 5, 6 };
            List<double> originalArray2 = new List<double>();// { 9, 7, 12, 15, 17 };

            using (StreamReader sr = new StreamReader(szFileName))
            {
                sr.ReadLine();
                while ((!sr.EndOfStream))// && lineCount<10000)
                {
                    String commaLine = sr.ReadLine();
                    String[] columns = commaLine.Split(',');
                    if (columns.Length > 2)
                    {
                        double person1to2SMA = Convert.ToDouble(columns[1]);
                        double person2to1SMA = Convert.ToDouble(columns[2]);
                        originalArray1.Add(person1to2SMA);
                        originalArray2.Add(person2to1SMA);
                        int time = Convert.ToInt32(columns[0]) - 10;
                    }
                }
            }

            TextWriter correlations = new StreamWriter(szOutputFileName);// "approachesDATA_05_30_19perHOUR.csv");
            correlations.WriteLine("INDEX, FROM, TO, CORRELATION");

            int indexCol = 0;
            for (int s = 0; s + intervalSize < originalArray1.Count; s = s + intervalSize)
            {
                List<double> array1 = originalArray1.GetRange(s, intervalSize);
                List<double> array2 = originalArray2.GetRange(s, intervalSize);
                List<double> array_xy = new List<double>(array1.Count);
                List<double> array_xp2 = new List<double>(array1.Count);
                List<double> array_yp2 = new List<double>(array1.Count);
                for (int i = 0; i < array1.Count; i++)
                    array_xy.Add(array1[i] * array2[i]);
                for (int i = 0; i < array1.Count; i++)
                    array_xp2.Add(Math.Pow(array1[i], 2.0));
                for (int i = 0; i < array1.Count; i++)
                    array_yp2.Add(Math.Pow(array2[i], 2.0));
                double sum_x = 0;
                double sum_y = 0;
                foreach (double n in array1)
                    sum_x += n;
                foreach (double n in array2)
                    sum_y += n;
                double sum_xy = 0;
                foreach (double n in array_xy)
                    sum_xy += n;
                double sum_xpow2 = 0;
                foreach (double n in array_xp2)
                    sum_xpow2 += n;
                double sum_ypow2 = 0;
                foreach (double n in array_yp2)
                    sum_ypow2 += n;
                double Ex2 = Math.Pow(sum_x, 2.00);
                double Ey2 = Math.Pow(sum_y, 2.00);

                double Correl =
                (array1.Count * sum_xy - sum_x * sum_y) /
                Math.Sqrt((array1.Count * sum_xpow2 - Ex2) * (array1.Count * sum_ypow2 - Ey2));



               // Console.WriteLine("CORREL FOR INTERVAL " + s + "-" + (s + intervalSize - 1) + " : " + Correl);

                correlations.WriteLine(indexCol + "," + s + "," + (s + intervalSize - 1) + "," + Correl);
                indexCol++;
            }
            correlations.Close();
        }
        static void Corr(String szFileName, int intervalSize, String szOutputFileName)
        {//intervalSize -1 if we want to correlate all 

            Dictionary<String, List<double>> pairWeightedApps1 = new Dictionary<string, List<double>>();
            Dictionary<String, List<double>> pairWeightedApps2 = new Dictionary<string, List<double>>();
            Dictionary<String, List<double>> pairWeightedApps1s = new Dictionary<string, List<double>>();
            Dictionary<String, List<double>> pairWeightedApps2s = new Dictionary<string, List<double>>();

            Dictionary<String, List<String>> pairFroms = new Dictionary<string, List<String>>();
            //Dictionary<String, List<String>> pairTos = new Dictionary<string, List<String>>();




            int lineNumber = 0;
            String p1 = "";
            String p2 = "";
            using (StreamReader sr = new StreamReader(szFileName))
            {
                sr.ReadLine();
                while ((!sr.EndOfStream))// && lineCount<10000)
                {
                    lineNumber++;
                    if (lineNumber == 17612)
                    {
                        lineNumber = lineNumber;
                    }
                    String commaLine = sr.ReadLine();
                    String[] columns = commaLine.Split(',');
                    if (columns.Length > 7)
                    {
                        p1 = columns[0].Trim();
                        p2 = columns[1].Trim();
                        String pairOrdered = columns[6].Trim();
                        //2 13 
                        if (columns[2].Trim()!="" && columns[13].Trim() != "")
                        {
                            double pairSMA = Convert.ToDouble(columns[4]);
                            double pairSMAshuffle = Convert.ToDouble(columns[13]);
                            if (p1 == pairOrdered.Split('|')[0])
                            {
                                if (!pairWeightedApps1.ContainsKey(pairOrdered))
                                    pairWeightedApps1.Add(pairOrdered, new List<double>());
                                pairWeightedApps1[pairOrdered].Add(pairSMA);

                                if (!pairWeightedApps1s.ContainsKey(pairOrdered))
                                    pairWeightedApps1s.Add(pairOrdered, new List<double>());
                                pairWeightedApps1s[pairOrdered].Add(pairSMAshuffle);

                                if (!pairFroms.ContainsKey(pairOrdered))
                                    pairFroms.Add(pairOrdered, new List<String>());
                                pairFroms[pairOrdered].Add(columns[10].Trim());

                            }
                            else
                            {
                                if (!pairWeightedApps2.ContainsKey(pairOrdered))
                                    pairWeightedApps2.Add(pairOrdered, new List<double>());
                                pairWeightedApps2[pairOrdered].Add(pairSMA);

                                if (!pairWeightedApps2s.ContainsKey(pairOrdered))
                                    pairWeightedApps2s.Add(pairOrdered, new List<double>());
                                pairWeightedApps2s[pairOrdered].Add(pairSMAshuffle);
                            }
                        }
                        

                    }
                }
            }
            TextWriter correlations = new StreamWriter(szOutputFileName);// "approachesDATA_05_30_19perHOUR.csv");
            correlations.WriteLine("PAIR,REALTIMEINDEX, FROM, TO, CORRELATION,TIMEFROM, TIMETO,SHUFFLED_PAIR,SHUFFLED_REALTIMEINDEX, SHUFFLED_FROM, SHUFFLED_TO, SHUFFLED_CORRELATION,TIMEFROM, TIMETO");
            correlations.Close();
            foreach (String pair in pairWeightedApps1.Keys)
            {
                if (pairWeightedApps2.ContainsKey(pair))
                {
                    CorrFourArrays(pair, pairWeightedApps1[pair], pairWeightedApps2[pair], pairWeightedApps1s[pair], pairWeightedApps2s[pair], intervalSize<0? pairWeightedApps1s[pair].Count-1: intervalSize, szOutputFileName, pairFroms[pair]);

                    //Corr(pair, pairWeightedApps1[pair], pairWeightedApps2[pair], intervalSize, szOutputFileName);
                }
            }

        }
        static void getTail(String szFileName, int numberOfLines, String szOutputFileName)
        {
            int lastLine = 0;
            TextWriter tail = new StreamWriter(szOutputFileName);// "approachesDATA_05_30_19perHOUR.csv");

            using (StreamReader sr = new StreamReader(szFileName))
            {
                tail.WriteLine(sr.ReadLine());
                while ((!sr.EndOfStream))// && lineCount<10000)
                {
                    //if (lastLine <= intervalSize)
                    {
                      //  tail.WriteLine(sr.ReadLine());
                    }
                    //else
                     //   break;
                    lastLine++;

                }
            }
             int lineNumber = 0;
           
            using (StreamReader sr = new StreamReader(szFileName))
            {
                tail.WriteLine(sr.ReadLine());
                while ((!sr.EndOfStream))// && lineCount<10000)
                {
                    if(lineNumber>=lastLine- numberOfLines)
                    {
                        tail.WriteLine(sr.ReadLine());
                    }
                    lineNumber++;

                }
            } 
            tail.Close();



        }
        static void getHead(String szFileName, int numberOfLines, String szOutputFileName)
        {
            int lastLine = 0;
            TextWriter head = new StreamWriter(szOutputFileName);// "approachesDATA_05_30_19perHOUR.csv");

            using (StreamReader sr = new StreamReader(szFileName))
            {
                head.WriteLine(sr.ReadLine());
                while ((!sr.EndOfStream))// && lineCount<10000)
                {
                    if (numberOfLines == 0 || lastLine <= numberOfLines)
                    {
                        head.WriteLine(sr.ReadLine());
                    }
                    else
                        break;
                    lastLine++;

                }
            }

            head.Close();



        }
        static void getHead(String szFileName, String szOutputFileName)
        {
            getHead(szFileName, 0, szOutputFileName);



        }
        static List<String> CorrTwoArrays(String pair, List<double> originalArray1, List<double> originalArray2, int intervalSize, String szOutputFileName, List<String> szTimes)
        {

            List<String> fileLine = new List<string>();
            
            int indexCol = 0;
            for (int s = 0; s + intervalSize < originalArray1.Count; s = s + intervalSize)
            {
                List<String> szTimeFroms = szTimes.GetRange(s, intervalSize);

                int intervalSize1 = s+ intervalSize <= originalArray1.Count?  intervalSize: originalArray1.Count-s;
                int intervalSize2 = s + intervalSize <= originalArray2.Count ? intervalSize : originalArray2.Count - s;
                int theSize = intervalSize1 < intervalSize2 ? intervalSize1 : intervalSize2;

                List <double> array1 = originalArray1.GetRange(s, theSize);
                List<double> array2 = originalArray2.GetRange(s, theSize);
                List<double> array_xy = new List<double>(array1.Count);
                List<double> array_xp2 = new List<double>(array1.Count);
                List<double> array_yp2 = new List<double>(array1.Count);
                for (int i = 0; i < array1.Count; i++)
                    array_xy.Add(array1[i] * array2[i]);
                for (int i = 0; i < array1.Count; i++)
                    array_xp2.Add(Math.Pow(array1[i], 2.0));
                for (int i = 0; i < array1.Count; i++)
                    array_yp2.Add(Math.Pow(array2[i], 2.0));
                double sum_x = 0;
                double sum_y = 0;
                foreach (double n in array1)
                    sum_x += n;
                foreach (double n in array2)
                    sum_y += n;
                double sum_xy = 0;
                foreach (double n in array_xy)
                    sum_xy += n;
                double sum_xpow2 = 0;
                foreach (double n in array_xp2)
                    sum_xpow2 += n;
                double sum_ypow2 = 0;
                foreach (double n in array_yp2)
                    sum_ypow2 += n;
                double Ex2 = Math.Pow(sum_x, 2.00);
                double Ey2 = Math.Pow(sum_y, 2.00);

                double Correl =
                (array1.Count * sum_xy - sum_x * sum_y) /
                Math.Sqrt((array1.Count * sum_xpow2 - Ex2) * (array1.Count * sum_ypow2 - Ey2));



                //Console.WriteLine("CORREL FOR " + pair + " INTERVAL " + s + "-" + (s + intervalSize - 1) + " : " + Correl);

                fileLine.Add(pair + "," + indexCol + "," + s + "," + (s + intervalSize - 1) + "," + Correl+","+ szTimeFroms[0] + "," + szTimeFroms[szTimeFroms.Count-1]);
                indexCol++;
            }
            return fileLine;
            
        }
        static void CorrFourArrays(String pair, List<double> originalArray1, List<double> originalArray2, List<double> originalArray1s, List<double> originalArray2s, int intervalSize, String szOutputFileName, List<String> pairFroms)
        {


            TextWriter correlations = new StreamWriter(szOutputFileName, true);// "approachesDATA_05_30_19perHOUR.csv");
            /*
            int indexCol = 0;
            for (int s = 0; s + intervalSize < originalArray1.Count; s = s + intervalSize)
            {
                List<double> array1 = originalArray1.GetRange(s, intervalSize);
                List<double> array2 = originalArray2.GetRange(s, intervalSize);
                List<double> array_xy = new List<double>(array1.Count);
                List<double> array_xp2 = new List<double>(array1.Count);
                List<double> array_yp2 = new List<double>(array1.Count);
                for (int i = 0; i < array1.Count; i++)
                    array_xy.Add(array1[i] * array2[i]);
                for (int i = 0; i < array1.Count; i++)
                    array_xp2.Add(Math.Pow(array1[i], 2.0));
                for (int i = 0; i < array1.Count; i++)
                    array_yp2.Add(Math.Pow(array2[i], 2.0));
                double sum_x = 0;
                double sum_y = 0;
                foreach (double n in array1)
                    sum_x += n;
                foreach (double n in array2)
                    sum_y += n;
                double sum_xy = 0;
                foreach (double n in array_xy)
                    sum_xy += n;
                double sum_xpow2 = 0;
                foreach (double n in array_xp2)
                    sum_xpow2 += n;
                double sum_ypow2 = 0;
                foreach (double n in array_yp2)
                    sum_ypow2 += n;
                double Ex2 = Math.Pow(sum_x, 2.00);
                double Ey2 = Math.Pow(sum_y, 2.00);

                double Correl =
                (array1.Count * sum_xy - sum_x * sum_y) /
                Math.Sqrt((array1.Count * sum_xpow2 - Ex2) * (array1.Count * sum_ypow2 - Ey2));



                Console.WriteLine("CORREL FOR " + pair + " INTERVAL " + s + "-" + (s + intervalSize - 1) + " : " + Correl);

                correlations.WriteLine(pair + "," + indexCol + "," + s + "," + (s + intervalSize - 1) + "," + Correl);
                indexCol++;
            }*/

            List<String> corrs1 = CorrTwoArrays(pair, originalArray1, originalArray2, intervalSize, szOutputFileName, pairFroms);
            List<String> corrs2 = CorrTwoArrays(pair, originalArray1s, originalArray2s, intervalSize, szOutputFileName, pairFroms);


            for(int l=0;l<corrs1.Count;l++)
            {
                correlations.WriteLine(corrs1.ElementAt(l) + ","+corrs2.ElementAt(l));

            }

            correlations.Close();
        }
        static void ShuffleSMAFile(String szOldFile, String szNewFile)
        {//nonRandomShuffle true if shufflesize is going to be used
            
            TextWriter shuffledSmas = new StreamWriter(szNewFile, true);// "approachesDATA_05_30_19perHOUR.csv");
            Dictionary<String, List<String>> pairLines = new Dictionary<string, List<string>>();
            Dictionary<String, List<String>> pairShuffledLines = new Dictionary<string, List<string>>();
            List<String> currentPairLines = new List<string>();
            String currentPair = "";
            using (StreamReader sr = new StreamReader(szOldFile))
            {
                String commaLine = sr.ReadLine();
                shuffledSmas.WriteLine(commaLine+",Shuffled_"+ commaLine.Replace(",",",Shuffled_"));
                while ((!sr.EndOfStream))// && lineCount<10000)
                {
                   
                    commaLine = sr.ReadLine();
                    String[] columns = commaLine.Split(',');
                    String pairA = columns[5].Trim();
                     
                    if(currentPair!="" && currentPair!= pairA)
                    {
                        if (!pairLines.ContainsKey(pairA))
                        {
                            pairLines.Add(pairA, new List<string>());
                        }
                        pairLines[pairA] = currentPairLines;

                        currentPairLines = currentPairLines.OrderBy(a => Guid.NewGuid()).ToList();
                        if (!pairShuffledLines.ContainsKey(pairA))
                        {
                            pairShuffledLines.Add(pairA, new List<string>());
                        }
                        pairShuffledLines[pairA] = currentPairLines;
                        currentPairLines = new List<string>();
                    }
                    currentPairLines.Add(commaLine);
                    currentPair = pairA;
                    // shuffledSmas.WriteLine("");
                }
            }
          
            
            
          foreach(String key in pairShuffledLines.Keys)
            {

                for (int l = 0; l < pairShuffledLines[key].Count; l++)
                {
                    shuffledSmas.WriteLine(pairLines[key].ElementAt(l) + "," + pairShuffledLines[key].ElementAt(l));
                }
            }
            
        
                 
     
            shuffledSmas.Close();
        }
        static void ShuffleSMAFile(String szOldFile, String szNewFile, bool nonRandomShuffle, int shuffleSize)
        {//nonRandomShuffle true if shufflesize is going to be used

            TextWriter shuffledSmas = new StreamWriter(szNewFile, true);// "approachesDATA_05_30_19perHOUR.csv");

            Dictionary<String, List<String>> pairLines = new Dictionary<string, List<string>>();
            Dictionary<String, List<List<String>>> pairShuffledLineLists = new Dictionary<string, List<List<String>>>();

            List<String> currentPairLines = new List<string>();
            List<String> currentShuffledPairLines = new List<string>();
            String currentPair = "";
            int currentPos = 0;
            int linesRead = 0;
            bool shuffling = false;

            using (StreamReader sr = new StreamReader(szOldFile))
            {
                String commaLine = sr.ReadLine();
                shuffledSmas.WriteLine(commaLine + ",Shuffled_" + commaLine.Replace(",", ",Shuffled_"));
                 
                try
                {


                    while ((!sr.EndOfStream))// && lineCount<10000)
                    {
                        linesRead++;

                        commaLine = sr.ReadLine();
                        String[] columns = commaLine.Split(',');
                        String pairA = columns[5].Trim();

                        if (currentPair != "" && currentPair != pairA)
                        {
                            if (!pairLines.ContainsKey(currentPair))
                            {
                                pairLines.Add(currentPair, new List<string>());
                            }
                            pairLines[currentPair] = currentPairLines;

                            currentPairLines = new List<string>();



                            currentShuffledPairLines = currentShuffledPairLines.OrderBy(a => Guid.NewGuid()).ToList();
                            if (!pairShuffledLineLists.ContainsKey(currentPair))
                            {
                                pairShuffledLineLists.Add(currentPair, new List<List<string>>());
                            }
                            pairShuffledLineLists[currentPair].Add(currentShuffledPairLines);
                            currentShuffledPairLines = new List<string>();
                            currentPos = 0;

                        }
                        currentPairLines.Add(commaLine);
                        currentPair = pairA;

                        if (nonRandomShuffle)
                        {
                            if (currentPos >= shuffleSize)
                            {
                                currentShuffledPairLines = currentShuffledPairLines.OrderBy(a => Guid.NewGuid()).ToList();
                                if (!pairShuffledLineLists.ContainsKey(pairA))
                                {
                                    pairShuffledLineLists.Add(pairA, new List<List<string>>());
                                }
                                pairShuffledLineLists[pairA].Add(currentShuffledPairLines);
                                currentShuffledPairLines = new List<string>();
                                currentPos = 0;
                            }
                        }

                        currentPos++;

                        currentShuffledPairLines.Add(commaLine);


                        // shuffledSmas.WriteLine("");
                    }

                    if (currentPair != "")
                    {

                        if (!pairLines.ContainsKey(currentPair))
                        {
                            pairLines.Add(currentPair, new List<string>());
                        }
                        pairLines[currentPair] = currentPairLines;

                        currentPairLines = new List<string>();


                        shuffling = true;
                        currentShuffledPairLines = currentShuffledPairLines.OrderBy(a => Guid.NewGuid()).ToList();
                        shuffling = false;
                        if (!pairShuffledLineLists.ContainsKey(currentPair))
                        {
                            pairShuffledLineLists.Add(currentPair, new List<List<string>>());
                        }
                        pairShuffledLineLists[currentPair].Add(currentShuffledPairLines);

                    }
                }
                catch(Exception e)
                {

                    Console.WriteLine("linesRead " + linesRead + " shuffling"+ shuffling);
                }
            }


          
            foreach (String key in pairShuffledLineLists.Keys)
            {
                int l = 0;
                foreach(List<String> shuffledList in pairShuffledLineLists[key])
                {
                    foreach (String shuffeledLine in shuffledList)
                    {

                        shuffledSmas.WriteLine(pairLines[key].ElementAt(l) + "," + shuffeledLine);
                        l++;

                    }

                }
            }
            



            shuffledSmas.Close();
        }
        static void mergeFiles(String[] files, String newFileName)
        {
            TextWriter sw = new StreamWriter(newFileName);
            Boolean skipHeader = false;
            foreach (String file in files)
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    if (skipHeader)
                        sr.ReadLine();
                    skipHeader = true;
                    while ((!sr.EndOfStream))
                    {
                        sw.WriteLine(sr.ReadLine());
                    }
                }
            }


            sw.Close();
        }
        static void cleanFile(String szFileName)
        {
            TextWriter sw = new StreamWriter(szFileName.Replace(".CSV","_CLEANED.CSV"));
            using (StreamReader sr = new StreamReader(szFileName))
            {
                     
                while ((!sr.EndOfStream))
                {
                    String szLine = sr.ReadLine();
                    String[] lineCols = szLine.Split(',');
                    if (lineCols[2].Trim()!=""&& lineCols[13].Trim() != "")
                    sw.WriteLine(szLine);
                }
            }
            

            sw.Close();

        }
        static void Main(string[] args)
        {

            String dir = "E:\\PRIDE_LEAP\\PRIDE_LEAP_AM\\SYNC\\";
            dir = "C:\\LVL\\";


            //approach_trunk_4_16_2019_10_3_2019_1707233820_1280177929Ordered.CSV
            String version = "11_11_2019_939400824";// "10_3_2019_1707233820";// "9_12_2019_839583655";// "9_10_2019_93585323";//approach_trunk_4_16_2019_9_12_2019_839583655.CSV
            String szDates = "1_23_2019,4_16_2019";
            szDates = "4_16_2019,1_23_2019,2_20_2019,3_20_2019,5_30_2019";
            szDates = "4_16_2019";
            String[] dates = szDates.Split(',');
            Boolean processTeachers =  false;
            Boolean processLabs =  false; ;
            int processSize = 8;
            String szVersion = "";// new Random().Next().ToString();

            if (args.Length >= 3)
            {
                dir = args[0];
                version = args[1];
                szDates = args[2];
                dates = szDates.Split(',');
                processTeachers = args.Length >= 4 && args[3].Trim().ToLower() == "true" ? true : false;
                processLabs = args.Length >= 5 && args[4].Trim().ToLower() == "true" ? true : false; ;
            } 
              
                foreach (String szd in dates)
                {
                /********APPROACH HEADER*******/
                //  Person 1, Person2, Interaction Time, Interaction Millisecond,d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21, 
                // WithinGR, WithinGRAnd45deg, Angle1, Angle2,Type1, Type2, Gender1, Gender2, Diagnosis1, Diagnosis2 ,
                String approachFileName = dir + "approach_trunk_" + szd + "_" + version + ".CSV";
                //Add Ordered pair and diagnosis gender etc to file.
                String tailName = dir + "approach_trunk_" + szd + "_" + version + "tail.CSV";
                String headName = dir + "approach_trunk_" + szd + "_" + version + "head.CSV";
                //getHead(approachFileName, 1000000, headName); //500500, baseName);
                //getTail(dir + "approach_trunk_" + szd + "_" + version + ".CSV", 100000, tailName); //500500, baseName);

                //approachFileName = headName;
                //approach_trunk_4_16_2019_11_11_2019_939400824_trunk500500
                Console.WriteLine("PROCESSING orderedPairList " + szd + " (" + DateTime.Now.ToShortTimeString());
               // List<String> orderedPairList = MainConvertTenToOrdered(approachFileName, processTeachers, processLabs, szVersion);
                //new List<string>();//  MainConvertTenToOrdered(approachFileName, processTeachers, processLabs,szVersion);
                 //PairA,PairOrdered,Date,TimeMS
                //"E:\\PRIDE_LEAP\\SYNC\\approach_trunk_4_16_2019_9_10_2019_93585323.CSV", false, false);
                String szFNRaw = approachFileName.Replace(".CSV", "_Ordered.CSV");
                String szFNAll = dir + "approach_trunk_" + szd + "_" + version + "_" + szVersion + "weightedSMAapp.CSV";
                String szFN = dir + "approach_trunk_" + szd + "_" + version + "_" + szVersion + "weightedSMAapp.CSV";
                //"approach_trunk_4_16_2019_8_14_2019_626094195_weightedSMAapp" + new Random().Next() + ".CSV";
                int p = 0;
                String shuffleSmaFileName = szFN.Replace(".CSV", "_SHUFFFLED.CSV");
                String szFNChunk = szFN;
                String szSmaFileNames = "";
                String szShuffledFileNames = "";
                String szCorrFileNames = "";
                /*  for (; p + processSize < orderedPairList.Count; p = (p + processSize))
                  {
                      szFNChunk = szFN.Replace(".CSV", "_" + p + ".CSV");
                      Console.WriteLine(p + " TO " + (p + processSize - 1) + " [" + (p + 1) + " TO " + (p + processSize) + "]" + " (" + DateTime.Now.ToShortTimeString());
                      List<String> includedPairs = orderedPairList.GetRange(p, processSize);
                      SMA(szFNRaw, 10, szFNChunk, includedPairs, false);// p > 0);
                      Console.WriteLine("PROCESSING ShuffleSMAFile " + szd + " (" + DateTime.Now.ToShortTimeString());
                      shuffleSmaFileName = szFNChunk.Replace(".CSV", "_SHUFFFLEDALL.CSV");

                      ShuffleSMAFile(szFNChunk, shuffleSmaFileName, false, 10);

                      szShuffledFileNames += shuffleSmaFileName + ",";
                      szCorrFileNames += shuffleSmaFileName.Replace(".CSV", "_CORR600.CSV") + ",";

                      Corr(shuffleSmaFileName, 600, shuffleSmaFileName.Replace(".CSV", "_CORR600.CSV"));


                  }
                  if (p <= orderedPairList.Count)
                  {
                      szFNChunk = szFN.Replace(".CSV", "_" + p + ".CSV");
                      int lastIndex = p;
                      int countLeft = orderedPairList.Count - lastIndex;
                      Console.WriteLine(p + " TO " + (p + countLeft - 1) + " [" + (p + 1) + " TO " + (p + countLeft) + "]" + " (" + DateTime.Now.ToShortTimeString());
                      List<String> includedPairs = orderedPairList.GetRange(lastIndex, countLeft);
                      SMA(szFNRaw, 10, szFNChunk, includedPairs, false);// p > 0);
                      Console.WriteLine("PROCESSING ShuffleSMAFile " + szd + " (" + DateTime.Now.ToShortTimeString());
                      shuffleSmaFileName = szFNChunk.Replace(".CSV", "_SHUFFFLEDALL.CSV");
                      ShuffleSMAFile(szFNChunk, shuffleSmaFileName, false, 10);

                      szShuffledFileNames += shuffleSmaFileName + ",";
                      Corr(shuffleSmaFileName, 600, shuffleSmaFileName.Replace(".CSV", "_CORR600.CSV"));
                      szCorrFileNames += shuffleSmaFileName.Replace(".CSV", "_CORR600.CSV") + ",";

                  }
                  */
                szShuffledFileNames = dir + "approach_trunk_4_16_2019_11_11_2019_939400824_weightedSMAapp_0_SHUFFFLEDALL.CSV" + "," +
                                        dir + "approach_trunk_4_16_2019_11_11_2019_939400824_weightedSMAapp_8_SHUFFFLEDALL.CSV" + "," +
                                        dir + "approach_trunk_4_16_2019_11_11_2019_939400824_weightedSMAapp_16_SHUFFFLEDALL.CSV" + "," +
                                        dir + "approach_trunk_4_16_2019_11_11_2019_939400824_weightedSMAapp_24_SHUFFFLEDALL.CSV" + "," +
                                        dir + "approach_trunk_4_16_2019_11_11_2019_939400824_weightedSMAapp_32_SHUFFFLEDALL.CSV" + "," +
                                        dir + "approach_trunk_4_16_2019_11_11_2019_939400824_weightedSMAapp_40_SHUFFFLEDALL.CSV" + "," +
                                        dir + "approach_trunk_4_16_2019_11_11_2019_939400824_weightedSMAapp_48_SHUFFFLEDALL.CSV" + "," +
                                        dir + "approach_trunk_4_16_2019_11_11_2019_939400824_weightedSMAapp_56_SHUFFFLEDALL.CSV" + "," +
                                        dir + "approach_trunk_4_16_2019_11_11_2019_939400824_weightedSMAapp_64_SHUFFFLEDALL.CSV" + ",";


                String[] shuffled = szShuffledFileNames.Substring(0, szShuffledFileNames.Length - 1).Split(',');
                foreach (String szSFile in shuffled)
                {
                    String cleanFileName = szSFile.Replace(".CSV", "_CLEANED.CSV");
                    cleanFile(szSFile);
                    szCorrFileNames += cleanFileName.Replace(".CSV", "_CORR600.CSV") + ",";

                    Console.WriteLine("PROCESSING Corr for "+ cleanFileName + " (" + DateTime.Now.ToShortTimeString());
                    Corr(cleanFileName, 600, cleanFileName.Replace(".CSV", "_CORR600.CSV"));


                }
               // shuffleSmaFileName = szFN.Replace(".CSV", "_SHUFFFLEDALL.CSV");
                Console.WriteLine("PROCESSING mergeFiles " + " (" + DateTime.Now.ToShortTimeString());
                mergeFiles(szCorrFileNames.Substring(0, szCorrFileNames.Length-1).Split(','), szFN.Replace(".CSV", "_SHUFFFLEDALLCORR.CSV"));
                 
                //Corr(shuffleSmaFileName, 600, shuffleSmaFileName.Replace(".CSV", "_CORR600.CSV"));
                //Corr(shuffleSmaFileName, -1, shuffleSmaFileName.Replace(".CSV", "_CORR.CSV"));//-1 if we want to correlate all 
                // Corr(shuffleSmaFileName, 10, shuffleSmaFileName.Replace(".CSV", "_CORR.CSV"));
            }

        }
    }
}