using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Approach
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
        static Double LawofCosines(String[] linecolumns)
        {
            double result = 0;



            double a2 = Math.Pow(Convert.ToDouble(linecolumns[11]) - Convert.ToDouble(linecolumns[9]), 2) + Math.Pow(Convert.ToDouble(linecolumns[12]) - Convert.ToDouble(linecolumns[10]), 2);
            double b2 = Math.Pow(Convert.ToDouble(linecolumns[7]) - Convert.ToDouble(linecolumns[9]), 2) + Math.Pow(Convert.ToDouble(linecolumns[8]) - Convert.ToDouble(linecolumns[10]), 2);
            double c2 = Math.Pow(Convert.ToDouble(linecolumns[11]) - Convert.ToDouble(linecolumns[7]), 2) + Math.Pow(Convert.ToDouble(linecolumns[12]) - Convert.ToDouble(linecolumns[8]), 2);
            //double neg2bc = -2*((Math.Sqrt(b2))*(Math.Sqrt(c2)));
            // THREE POINTS

            double p1t0x = (Convert.ToDouble(linecolumns[7])); //A = person1@time0
            double p1t0y = (Convert.ToDouble(linecolumns[8]));

            double p1t1x = (Convert.ToDouble(linecolumns[11]));  //B = person1@time1
            double p1t1y = (Convert.ToDouble(linecolumns[12]));

            double p2t0x = (Convert.ToDouble(linecolumns[9]));  //C = person2@t0
            double p2t0y = (Convert.ToDouble(linecolumns[10]));


            //making unit vector components
            double l1x = (p1t1x - p1t0x);
            double l1y = (p1t1y - p1t0y);
            double l1mag = Math.Sqrt((l1x * l1x) + (l1y * l1y));


            double l2x = (p2t0x - p1t0x);
            double l2y = (p2t0y - p1t0y);
            double l2mag = Math.Sqrt((l2x * l2x) + (l2y * l2y));
            //normalization of vectors
            double l1xp = l1x / l1mag;
            double l1yp = l1y / l1mag;
            double l2xp = l2x / l2mag;
            double l2yp = l2y / l2mag;

            double ax = p1t0x;
            double ay = p1t0y;

            double bx = ax + l1xp; // here, the addition was with the l1x not l1xp and the same with by,cx,cy. It was revised to utilize the normalized vector to create the normalized points.
            double by = ay + l1yp;

            double cx = ax + l2xp;
            double cy = ay + l2yp;


            /*BigFloat ax = BigFloat.Pow((Convert.ToDouble(linecolumns[11]) - Convert.ToDouble(linecolumns[9])), 2);
            BigFloat ay = BigFloat.Pow((Convert.ToDouble(linecolumns[12]) - Convert.ToDouble(linecolumns[10])), 2);

            BigFloat bx = BigFloat.Pow((Convert.ToDouble(linecolumns[7]) - Convert.ToDouble(linecolumns[9])), 2);
            BigFloat by = BigFloat.Pow((Convert.ToDouble(linecolumns[8]) - Convert.ToDouble(linecolumns[10])), 2);
           
            BigFloat cx = BigFloat.Pow((Convert.ToDouble(linecolumns[11]) - Convert.ToDouble(linecolumns[7])), 2);
            BigFloat cy = BigFloat.Pow((Convert.ToDouble(linecolumns[12]) - Convert.ToDouble(linecolumns[8])), 2);
            BigFloat eResult = new BigFloat(1234);*/

            double axy2 = Math.Pow(Convert.ToDouble((bx - cx)), 2) + Math.Pow(Convert.ToDouble((by - cy)), 2);
            double bxy2 = Math.Pow(Convert.ToDouble((ax - cx)), 2) + Math.Pow(Convert.ToDouble((ay - cy)), 2);
            double cxy2 = Math.Pow(Convert.ToDouble((bx - ax)), 2) + Math.Pow(Convert.ToDouble((by - ay)), 2); //WEIRD
            double neg2bc = (-2 * Math.Sqrt(bxy2) * Math.Sqrt(cxy2));
            if (neg2bc == 0 || (axy2 - bxy2 - cxy2) == 0)
            {
                result = 0;
            }
            else
            {
                result = ((axy2 - bxy2 - cxy2) / (neg2bc));
            }


            /*BigFloat axy2 = ax + ay;
            BigFloat bxy2 = bx + by;
            BigFloat cxy2 = cx + cy;
            BigFloat neg2bc = (-2 * BigFloat.Sqrt(bxy2) * BigFloat.Sqrt(cxy2));
           
           
           
            result = Convert.ToDouble(eResult.ToString()); */
            //expResult = Math.Exp(Math.Log(b) - Math.Log(2 * c)) + Math.Exp(Math.Log(c) - Math.Log(2 * b)) - Math.Exp(2 * Math.Log(a) - Math.Log(2 * c) - Math.Log(b));
            //result = (a2 - b2 - c2) / neg2bc;
            if (result > 1.000001 || result < -1.000001)
                throw new Exception("Squared cosine larger than 1");

            return result;
        }
        static List<String> MainConvertTenToOrdered(String FileName, Boolean includeTeachers, Boolean includeLabs, String version, Boolean weightYN)
        {
            List<String> orderedPairList = new List<string>();

            TextWriter OrderedFile = new StreamWriter(FileName.Replace(".", "_" + version + "Ordered."), false);
            using (StreamReader readerFile = new StreamReader(FileName))
            {
                //"/Users/chitrabanarjee/Desktop/Messinger/approaches_tenthofasec/approach_trunk_2_20_2019_7_31_2019_1777209647.CSV"
                string line = readerFile.ReadLine();
                //quick fix for extra columns/approach versions with long ID appended
                if (line.Split(',').Length > 25)
                {
                    String[] lineCols2 = new String[25];
                    String[] lineCols = line.Split(',');
                    Array.Copy(lineCols, 0, lineCols2, 0, 25);
                    line = String.Join(",", lineCols2);
                }
                OrderedFile.WriteLine(line + ",PairA,PairOrdered,Date,TimeMS,cosAngleM");
                while ((!readerFile.EndOfStream))
                {
                    line = readerFile.ReadLine();

                    string[] lineColumns = line.Split(',');
                    if (lineColumns.Length > 24)
                    {


                        string person1 = lineColumns[0];
                        string person2 = lineColumns[1];
                        if (person1.Trim() == "T1L" && person2.Trim() == "1L")
                        {
                            bool flag = true;
                        }

                        double angleA = Math.Pow(Math.Cos(Convert.ToDouble(lineColumns[17])), 2);
                        double lawCos = LawofCosines(lineColumns);
                        double approachMeters = Convert.ToDouble(lineColumns[6]);
                        double approachWeightedR = approachMeters * angleA * Math.Pow(lawCos, 2);// weighting by angle of movement and angle of orientation


                        //////
                        bool isLongId = person1.IndexOf("_") != person1.LastIndexOf("_");
                        if (isLongId)
                        {
                            person1 = getShortId(person1);
                            person2 = getShortId(person2);
                        }

                        string szTime = lineColumns[2];
                        string szMillisec = lineColumns[3];
                        string szTimeMS = szTime.Replace(" AM", "." + szMillisec + " AM").Replace(" PM", "." + szMillisec + " PM");
                        Boolean isTeacher1 = person1.IndexOf("T") == 0;
                        Boolean isLab1 = person1.IndexOf("L") == 0;//: (person1.Contains("Lab") || person2.Contains("Lab"));
                        Boolean isTeacher2 = person2.IndexOf("T") == 0;
                        Boolean isLab2 = person2.IndexOf("L") == 0;//: (person1.Contains("Lab") || person2.Contains("Lab"));

                        if ((includeLabs && includeTeachers) ||
                            ((!includeLabs) && (!includeTeachers) && (!(isTeacher1 || isLab1 || isTeacher2 || isLab2))) ||
                            (((!includeLabs) && (!(isLab1 || isLab2)))) ||
                            ((!includeTeachers) && (!(isTeacher1 || isTeacher2))))


                        {
                            string pairA = person1 + "|" + person2;
                            int iperson1 = Convert.ToInt32(new string(person1.Where(c => char.IsDigit(c)).ToArray()));
                            int iperson2 = Convert.ToInt32(new string(person2.Where(c => char.IsDigit(c)).ToArray()));
                            //quick fix for extra columns/approach versions with long ID appended
                            if (line.Split(',').Length > 25)
                            {
                                line = line.Substring(0, line.LastIndexOf(","));
                                line = line.Substring(0, line.LastIndexOf(","));
                                lineColumns = line.Split(',');
                            }

                            Console.WriteLine(line);


                            string pairP1 = "";// iperson1 < iperson2 ?(isTeacher1?person1: iperson1.ToString()) : (isTeacher2 ?person2: iperson2.ToString()) ;
                            string pairP2 = ""; // iperson1 > iperson2 ? (isTeacher1 ? person1 : iperson1.ToString()) : (isTeacher2 ? person2 : iperson2.ToString());
                            if (iperson1 <= iperson2)
                            {
                                if (isTeacher1 || isLab1)
                                {
                                    pairP1 = person1;

                                }
                                else
                                {
                                    pairP1 = iperson1.ToString();
                                }
                                if (isTeacher2 || isLab2)
                                {
                                    pairP2 = person2;
                                }
                                else
                                {
                                    pairP2 = iperson2.ToString();
                                }

                            }
                            else
                            {
                                if (isTeacher2 || isLab2)
                                {
                                    pairP1 = person2;
                                }
                                else
                                {
                                    pairP1 = iperson2.ToString();
                                }
                                if (isTeacher1 || isLab1)
                                {
                                    pairP2 = person1;
                                }
                                else
                                {
                                    pairP2 = iperson1.ToString();
                                }
                            }

                            string pairOrdered = pairP1 + "|" + pairP2; //iperson1 < iperson2 ? iperson1 + "|" + iperson2 : iperson2 + "|" + iperson1;
                            if (pairOrdered.Trim() == "1|1")
                            {
                                bool flag = true;
                            }

                            if (!orderedPairList.Contains(pairOrdered))
                            {
                                orderedPairList.Add(pairOrdered);
                            }

                            String[] part1 = new string[6];
                            String[] part2 = new string[lineColumns.Length - 7];
                            Array.Copy(lineColumns, 0, part1, 0, 6);
                            Array.Copy(lineColumns, 7, part2, 0, lineColumns.Length - 7);

                            String szNewLine = String.Join(",", part1) + "," + (weightYN ? approachWeightedR : approachMeters) + "," + String.Join(",", part2);

                            if (szNewLine.Length < 8)
                            {
                                bool flag = true;
                            }

                            if (szNewLine.IndexOf(",") != szNewLine.LastIndexOf(","))
                            {
                                szNewLine = szNewLine.Substring(szNewLine.IndexOf(",") + 1);
                                szNewLine = szNewLine.Substring(szNewLine.IndexOf(",") + 1);
                                szNewLine = pairA.Replace("|", ",") + "," + szNewLine;

                            }

                            string szDay = FileName.Substring(FileName.LastIndexOf("approach_trunk_", StringComparison.CurrentCulture) + 15);
                            szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));
                            szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));
                            szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));
                            szDay = szDay.Substring(0, szDay.LastIndexOf("_", StringComparison.CurrentCulture));


                            OrderedFile.WriteLine(szNewLine + "," + pairA + "," + pairOrdered + "," + szDay + "," + szTimeMS + "," + lawCos);
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR line length");
                    }
                }

            }


            OrderedFile.Close();
            return orderedPairList;

        }
        static List<String> GetPairsFromOrdered(String FileName)
        {
            List<String> orderedPairList = new List<string>();

            using (StreamReader readerFile = new StreamReader(FileName))
            {
                //"/Users/chitrabanarjee/Desktop/Messinger/approaches_tenthofasec/approach_trunk_2_20_2019_7_31_2019_1777209647.CSV"
                string line = readerFile.ReadLine();
                while ((!readerFile.EndOfStream))
                {
                    line = readerFile.ReadLine();

                    string[] lineColumns = line.Split(',');
                    if (lineColumns.Length > 25)
                    {
                        String pairOrdered = lineColumns[26].Trim();

                        if (!orderedPairList.Contains(pairOrdered))
                        {
                            orderedPairList.Add(pairOrdered);
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR line length");
                    }
                }





            }
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


        }


        static Dictionary<String, List<smaClass>> SMA(String fileAppTenthOrdered, int order, String szSMAfile, List<String> pairsToInclude, Boolean append)
        {
            //order is window size of smooth averaging
            // Dictionary<String, List<smaClass>> smas = new Dictionary<string, List<smaClass>>();
            Dictionary<String, List<smaClass>> smasByOrder = new Dictionary<string, List<smaClass>>();

            Dictionary<String, List<String>> szSmas = new Dictionary<string, List<String>>();

            TextWriter NewFile = new StreamWriter(szSMAfile, append);
            if (!append)
                NewFile.WriteLine("Person1,Person2,d0,approachMeters,approachWeighted,PairA,PairOrdered,Date");

            int lineCount = 0;
            using (StreamReader sr = new StreamReader(fileAppTenthOrdered))

            {
                sr.ReadLine();
                while ((!sr.EndOfStream))// && lineCount<1000000)// && lineCount<10000)
                {
                    lineCount++;
                    String commaLine = sr.ReadLine();
                    String[] columns = commaLine.Split(',');
                    try
                    {
                        //Person 1, Person2, Interaction Time, Interaction Millisecond,
                        //d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21, WithinGR,
                        //WithinGRAnd45deg, Angle1, Angle2,Type1, Type2, Gender1, Gender2,
                        //Diagnosis1, Diagnosis2 ,PairA,PairOrdered,Date,TimeMS
                        if (columns[6].Trim().ToUpper().IndexOf("NA") == 0)
                        {
                            bool stop = true;
                            columns[6] = "0";
                        }
                        if (columns.Length >= 16)//16)
                        {
                            String person1 = columns[0].Trim();
                            String person2 = columns[1].Trim();
                            double d0 = Convert.ToDouble(columns[4]);
                            double d1 = Convert.ToDouble(columns[5]);
                            double app = Convert.ToDouble(columns[6]);
                            String pairA = columns[25].Trim();
                            String pairOrdered = columns[26].Trim();
                            if (pairsToInclude.Contains(pairOrdered))
                            {
                                String date = columns[27].Trim();


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
                                    date + "," +
                                    columns[28]
                                    );



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
                    int IndexCol = 0;

                    //for (int orderPos = 0; orderPos + order <= szSmas[pairA].Count; orderPos++)
                    for (int orderPos = 0; orderPos + order <= szSmas[pairA].Count; orderPos++)
                    {
                        //op  0
                        try
                        {
                            smaClass smaVals = new smaClass();
                            String[] lineColumns = szSmas[pairA][orderPos].Split(',');
                            double totalD0 = 0;
                            double totalApp = 0;
                            int smaPos = 0;

                            DateTime priorTime = new DateTime(2000, 1, 1);

                            //op 0  smapos 0  100 200 300
                            //op 1  smapos 100 200 300
                            //op 2  smapos 200 300
                            //op 3  smapos 500 
                            //op 4  smapos 700 NA breaks and doesn not add
                            //op 5 smapos 900 10app
                            //op 6 smapos 1000 NAapp  NA breaks and doesn not add
                            //op 7 smapos 1100  20app 
                            for (; smaPos < order; smaPos++)
                            {
                                //05/8/2020 TODO  checck if t0-t1 is .1 sec if not break
                                String[] lineColumns2 = szSmas[pairA][orderPos + smaPos].Split(',');

                                String[] szThisDateArr = lineColumns2[6].Trim().Split('_');
                                DateTime thisTimeStamp = Convert.ToDateTime(lineColumns2[7]);//
                                DateTime thisTime = new DateTime(Convert.ToInt16(szThisDateArr[2]), Convert.ToInt16(szThisDateArr[0]), Convert.ToInt16(szThisDateArr[1]), thisTimeStamp.Hour, thisTimeStamp.Minute, thisTimeStamp.Second, thisTimeStamp.Millisecond);

                                // thisTime.AddMilliseconds(Convert.ToDouble(lineColumns2[7]));


                                //11:33:29.300 AM
                                if (lineColumns2[3].Trim().ToUpper() == "NA" || lineColumns2[3].Trim().ToUpper() == "NAN")
                                {
                                    bool stop = true;

                                }
                                if ((priorTime.Year < 2010 || ((thisTime - priorTime).Milliseconds <= 100)) && lineColumns2[3].Trim().ToUpper() != "NA")
                                {


                                    totalApp += Convert.ToDouble(lineColumns2[3]);
                                    totalD0 += Convert.ToDouble(lineColumns2[2]);


                                    priorTime = thisTime;


                                }
                                else
                                    break;


                            }


                            //05/8/2020 switched divisor from order to smaPos since there might be less than 10 items at th end using 
                            if (lineColumns[3].Trim().ToUpper() != "NA" || lineColumns[3].Trim().ToUpper() != "NAN")
                            {
                                smaVals.app = totalApp != 0 ? totalApp / smaPos : 0;
                                smaVals.d0 = totalD0 != 0 ? totalD0 / smaPos : 0;
                                smaVals.person1 = lineColumns[0].Trim();
                                smaVals.person2 = lineColumns[1].Trim();
                                smaVals.pairA = lineColumns[4].Trim();
                                smaVals.pairOrdered = lineColumns[5].Trim();
                                smaVals.date = lineColumns[6].Trim();
                                if (!smasByOrder.ContainsKey(pairA))
                                {
                                    smasByOrder.Add(pairA, new List<smaClass>());
                                }

                                smasByOrder[pairA].Add(smaVals);
                                double weightA = (smaVals.app / smaVals.d0); // weighting by (1/initial distance)
                                NewFile.WriteLine(smaVals.person1 + "," +
                                smaVals.person2 + "," + smaVals.d0 + "," + smaVals.app + "," +
                                weightA + "," + smaVals.pairA + "," + smaVals.pairOrdered +
                                "," + smaVals.date);

                                Console.WriteLine(IndexCol + "," + smaVals.person1 + "," +
                                smaVals.person2 + "," + smaVals.d0 + "," + smaVals.app + "," +
                                weightA + "," + smaVals.pairA + "," + smaVals.pairOrdered +
                                "," + smaVals.date);
                                IndexCol++;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR2: " + e.Message);
                        }
                    }


                }
                //Person1, Person2, Interaction Time, Interaction Millisecond,d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21,PairA,PairOrdered,Date

            }

            // 1 2 3 4

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



                Console.WriteLine("CORREL FOR INTERVAL " + s + "-" + (s + intervalSize - 1) + " : " + Correl);

                correlations.WriteLine(indexCol + "," + s + "," + (s + intervalSize - 1) + "," + Correl);
                indexCol++;
            }
            correlations.Close();
        }
        static void CorrFileName(String szFileName, int intervalSize, String szOutputFileName)
        {

            Dictionary<String, List<double>> pairWeightedApps1 = new Dictionary<string, List<double>>();
            Dictionary<String, List<double>> pairWeightedApps2 = new Dictionary<string, List<double>>();
            int lineNumber = 0;
            String p1 = "";
            String p2 = "";
            using (StreamReader sr = new StreamReader(szFileName))
            {
                sr.ReadLine();
                while ((!sr.EndOfStream))// && lineCount<10000)
                {
                    lineNumber++;
                    try
                    {
                        String commaLine = sr.ReadLine();
                        String[] columns = commaLine.Split(',');
                        if (p1 == "Lab1P" || p2 == "Lab1P")
                        {
                            bool stop = true;
                        }
                        if (columns.Length > 7)
                        {
                            p1 = columns[0].Trim();
                            p2 = columns[1].Trim();
                            String pairOrdered = columns[6].Trim();
                            String[] pairOrderedArray = pairOrdered.Split('|');
                            String p1A = pairOrderedArray[0];
                            String p2A = pairOrderedArray[1];
                            string p1num = string.Empty;
                            for (int i = 0; i < p1.Length; i++)
                            {
                                if (Char.IsDigit(p1[i]))
                                    p1num += p1[i];
                            }
                            string p2num = string.Empty;

                            for (int i = 0; i < p2.Length; i++)
                            {
                                if (Char.IsDigit(p2[i]))
                                    p2num += p2[i];
                            }
                            Boolean p1First = Convert.ToInt16(p1num) < Convert.ToInt16(p2num);

                            if (p1 == "3L" && p2 == "1L")
                            {
                                bool check = true;
                            }
                            if (!p1First)
                            {
                                bool check = true;
                            }
                            if (p1.IndexOf("T") >= 0 || p1.IndexOf("L") == 0 || p2.IndexOf("T") >= 0 || p2.IndexOf("L") == 0)
                            {
                                bool chekc = true;
                            }
                            if ((p1.ToUpper().IndexOf("T") == 0 || p1.ToUpper().IndexOf("L") == 0) &&
                                (p2.ToUpper().IndexOf("T") != 0 && p2.ToUpper().IndexOf("L") != 0))
                            {
                                p1First = false;
                            }
                            else if ((p2.ToUpper().IndexOf("T") == 0 || p2.ToUpper().IndexOf("L") == 0) &&
                                (p1.ToUpper().IndexOf("T") != 0 && p1.ToUpper().IndexOf("L") != 0))

                            {
                                p1First = true;
                            }
                            double pairSMA = Convert.ToDouble(columns[4]);



                            if (pairSMA.ToString().Trim().ToLower() == "nan")
                            {
                                bool check = true;
                            }
                            //if (p1 == pairOrdered.Split('|')[0])
                            if (p1First)//  pairOrdered.Split('|')[0].IndexOf(p1)==0)
                            {

                                if (!pairWeightedApps1.ContainsKey(pairOrdered))
                                    pairWeightedApps1.Add(pairOrdered, new List<double>());
                                pairWeightedApps1[pairOrdered].Add(pairSMA);
                            }
                            else
                            {
                                if (!pairWeightedApps2.ContainsKey(pairOrdered))
                                    pairWeightedApps2.Add(pairOrdered, new List<double>());
                                pairWeightedApps2[pairOrdered].Add(pairSMA);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        bool stop = true;
                    }
                }
            }
            TextWriter correlations = new StreamWriter(szOutputFileName);// "approachesDATA_05_30_19perHOUR.csv");
            correlations.WriteLine("PAIR,INDEX, FROM, TO, CORRELATION");
            correlations.Close();
            foreach (String pair in pairWeightedApps1.Keys)
            {
                if (pairWeightedApps2.ContainsKey(pair))
                {
                    CorrPair(pair, pairWeightedApps1[pair], pairWeightedApps2[pair], intervalSize, szOutputFileName);
                }
            }

        }

        static void CorrPair(String pair, List<double> originalArray1, List<double> originalArray2, int intervalSize, String szOutputFileName)
        {


            TextWriter correlations = new StreamWriter(szOutputFileName, true);// "approachesDATA_05_30_19perHOUR.csv");

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
                if (Correl.ToString().Trim() == "NaN")
                {
                    bool stop = true;
                }


                Console.WriteLine("CORREL FOR " + pair + " INTERVAL " + s + "-" + (s + intervalSize - 1) + " : " + Correl);

                correlations.WriteLine(pair + "," + indexCol + "," + s + "," + (s + intervalSize - 1) + "," + Correl);
                indexCol++;
            }
            correlations.Close();
        }
        static void Main(string[] args)
        {
            //PR_LEAP_PM_1819_chunk_5000_4_16_2020
            String dir = "C:\\LVL\\approach\\";

            String version = "5_7_2020_589199400";
            String szDates = "4_16_2020";// 1_23_2019";
            String[] dates = szDates.Split(',');
            Boolean processTeachers = false;
            Boolean processLabs = false;
            int processSize = 8;
            String szVersion = new Random().Next().ToString();
            String orderedFile = "";

            if (args.Length >= 6)
            {
                dir = "";
                version = args[1];
                szDates = args[2];
                dates = szDates.Split(',');
                processTeachers = args.Length >= 4 && args[3].Trim().ToLower() == "true" ? true : false;
                processLabs = args.Length >= 5 && args[4].Trim().ToLower() == "true" ? true : false; ;
                orderedFile = args[0].Trim();
            }
            if (args.Length >= 5)
            {
                dir = args[0];
                version = args[1];
                szDates = args[2];
                dates = szDates.Split(',');
                processTeachers = args.Length >= 4 && args[3].Trim().ToLower() == "true" ? true : false;
                processLabs = args.Length >= 5 && args[4].Trim().ToLower() == "true" ? true : false;
            }
            else if (args.Length >= 3)
            {
                dir = "";
                version = args[1];
                szDates = args[2];
                dates = szDates.Split(',');
                processTeachers = args.Length >= 4 && args[3].Trim().ToLower() == "true" ? true : false;
                processLabs = args.Length >= 5 && args[4].Trim().ToLower() == "true" ? true : false;
                orderedFile = args[0].Trim();
            }
            else if (args.Length >= 2)
            {
                dir = "";
                version = args[0];
                szDates = args[1];
                dates = szDates.Split(',');
                processTeachers = args.Length >= 3 && args[2].Trim().ToLower() == "true" ? true : false;
                processLabs = args.Length >= 4 && args[3].Trim().ToLower() == "true" ? true : false; ;
            }
            foreach (String szd in dates)
            {
                List<String> orderedPairList;
                orderedFile = "";// D:\\approachDATA\\LEAP_AM\\LEAP_AM_1920\\rawApproaches\\approach_trunk_11_1_2019_4_17_2020_1173190353_1877670914Ordered.CSV";  // debugging to delete later!!

                //ADD 5 COLS TO APP FILE WHOCH CONTAINS PAIR (PAIRA), PAIRORDERED (NUMERICAL ORDERED), DAY, MS, LAWCOS (MOVEMENT ANGLE)
                if (orderedFile == "")
                    orderedPairList = MainConvertTenToOrdered(dir + "approach_trunk_" + szd + "_" + version + ".CSV", processTeachers, processLabs, szVersion, true);
                else
                    orderedPairList = GetPairsFromOrdered(orderedFile);

                orderedPairList.Sort();

                String szFNRaw = orderedFile == "" ?
                    dir + "approach_trunk_" + szd + "_" + version + "_" + szVersion + "Ordered.CSV" : orderedFile;


                String szFN = dir + "approach_trunk_" + szd + "_" + version + "_" + szVersion + "weightedSMAapp.CSV";

                // debug to delete later!!! set szFNexisting = "";
                string szFNexisting = "";// "D:\\approachDATA\\LEAP_AM\\LEAP_AM_1819\\rawApproaches\\approach_trunk_4_16_2019_11_11_2019_939400824_2041033646weightedSMAapp.CSV";

                int p = 0;
                if (szFNexisting != "")
                {
                    szFN = szFNexisting;
                }
                else
                {
                    for (; p + processSize < orderedPairList.Count; p = (p + processSize))
                    {
                        Console.WriteLine(p + " TO " + (p + processSize - 1) + " [" + (p + 1) + " TO " + (p + processSize) + "]");
                        List<String> includedPairs = orderedPairList.GetRange(p, processSize);
                        //create Person1,Person2,d0,approachMeters,approachWeighted,PairA,PairOrdered,Date file
                        //note that there is no time in sma file but should be ordered??? 
                        SMA(szFNRaw, 10, szFN, includedPairs, p > 0);
                    }
                    if (p <= orderedPairList.Count)
                    {
                        int lastIndex = p;
                        int countLeft = orderedPairList.Count - lastIndex;
                        Console.WriteLine(p + " TO " + (p + countLeft - 1) + " [" + (p + 1) + " TO " + (p + countLeft) + "]");


                        List<String> includedPairs = orderedPairList.GetRange(lastIndex, countLeft);
                        SMA(szFNRaw, 10, szFN, includedPairs, p > 0);
                    }
                }
                CorrFileName(szFN, 600, szFN.Replace(".CSV", "_CORR.CSV"));


            }


        }
    }
}