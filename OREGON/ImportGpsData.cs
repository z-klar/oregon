using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

namespace cz.zk.OREGON
{
    class ImportGpsData
    {
        private ListBox _LbLog;

        private CultureInfo cult = new CultureInfo("en-US");

        public ImportGpsData(ListBox lbLog)
        {
            _LbLog = lbLog;
        }


        /**
         * Read the given txt file with GDB data. File containd several types of
         * rows. For ua the important starts with "Trackpoint" keyword and
         * contain at least 18 items. The line contains:
         * Trackpoint L L L A A A Date Time Alt m Len m Duration speed kph dir emp
         *
         * @param strFname Filename of the file tp be read
         */
        public TripRecord ReadTxtFile(String strFname) {
            String InputLine = "";
            int count, NoValidLines, BylZaznam;
            String strDummy, strDatum, strAlt, strLen, strDuration, strSpeed, Dat;
            String strDummy1, strDummy2, strDummy3, strDummy4, strDummy5, strDummy6, strDummy7;
            String LenUnits, sPom;
            int i, LastAltitude, Altitude, Duration;
            int SumUp, SumDown, SumDur;
            float MaxSpeed, Speed, Len, SumLen;
            TripRecord tr = new TripRecord("", 0, 0, 0, 0.0F, 0, 0);

            NoValidLines = 0;
            BylZaznam = LastAltitude = 0;
            SumUp = SumDown = SumDur =  0;
            MaxSpeed = SumLen = 0.0F;
            strDatum = "XX-ZZ"; Dat = "";


            StreamReader sw = null;
            FileStream fs = File.Open(strFname, FileMode.Open, FileAccess.Read);
            sw = new StreamReader(fs, System.Text.Encoding.ASCII);

            while((InputLine = sw.ReadLine()) != null) {
                if(InputLine.Length < 10) continue;

                String[] Phrases = InputLine.Split(new Char[] {' ','\t'}, StringSplitOptions.RemoveEmptyEntries);
                //String[] Phrases = InputLine.Split(null);
                count = Phrases.GetLength(0);
                if(count < 16) continue;

                strDummy = Phrases[0];   // [0]
                if(string.Compare(strDummy, "Trackpoint", true) != 0) continue;

                i = 1;
                strDummy1    = Phrases[i++];   // [1]  N49
                strDummy2    = Phrases[i++];   // [2]  40.480
                strDummy3    = Phrases[i++];   // [3]  E14
                strDummy4    = Phrases[i++];   // [4]  01.031
                strDatum     = Phrases[i++];   // [5]  24.4.2011
                strDummy5    = Phrases[i++];   // [6]  8:46:29
                strAlt       = Phrases[i++];   // [7]  527
                strDummy6    = Phrases[i++];   // [8]  m
                strLen       = Phrases[i++];   // [9]  0
                LenUnits     = Phrases[i++];   // [10] m
                strDuration  = Phrases[i++];   // [11] 0:00:01
                strSpeed     = Phrases[i++];   // [12] 0
                strDummy7    = Phrases[i++];   // [13] kph

                try {
                    Len = float.Parse(strLen);
                    if (Len > 0.0F)
                    {
                        if (BylZaznam == 0)
                        {   // 1st record
                            BylZaznam = 1;
                            LastAltitude = int.Parse(strAlt);
                        }
                        // length is by default in meters, can be in KM !!!!!
                        if (LenUnits.CompareTo("km") == 0) Len = Len * 1000.0F;
                        Altitude = int.Parse(strAlt);
                        Duration = ConvertHMSToSecs(strDuration);
                        Speed = float.Parse(strSpeed, cult);

                        if (Altitude > LastAltitude)
                        {
                            SumUp += (Altitude - LastAltitude);
                        }
                        else
                        {
                            SumDown += (LastAltitude - Altitude);
                        }
                        LastAltitude = Altitude;
                        SumLen += Len;
                        SumDur += Duration;
                        // eliminate rrors in speed (experienced 1448 kph)
                        if ((Speed > MaxSpeed) && (Speed < 100.0F))
                        {
                            MaxSpeed = Speed;
                            // whole record contains sometimes data from more days
                            // typically next day if the track was downloaded later
                            // we want to take date only for valid records
                            Dat = strDatum;
                        }

                        NoValidLines++;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error when parsing input file - see logger!");
                    _LbLog.Items.Add(ex.Message);
                    _LbLog.Items.Add(InputLine);
                    sPom = string.Format("Line: {0} - Params:", NoValidLines);
                    _LbLog.Items.Add(sPom);
                    sPom = string.Format("Dummy1=[{0}], Dummy1=[{1}], Dummy1=[{2}], Dummy1=[{3}] ", 
                                          strDummy1, strDummy2, strDummy3, strDummy4);
                    _LbLog.Items.Add(sPom);
                    sPom = string.Format("Datum=[{0}], Dummy5=[{1}], Alt=[{2}], Dummy6=[{3}] ",
                                          strDatum, strDummy5, strAlt, strDummy6);
                    _LbLog.Items.Add(sPom);
                    sPom = string.Format("Len=[{0}], Units=[{1}], Duration=[{2}], Speed=[{3}] ",
                                          strLen,   LenUnits,   strDuration,   strSpeed);
                    _LbLog.Items.Add(sPom);

                    return(tr);
                }

            }
            sw.Close();
            fs.Close();

            tr.Datum = Dat;
            tr.Duration = SumDur;
            tr.Length = (int)SumLen;
            tr.MaxSpeed = MaxSpeed;
            tr.NoRecs = NoValidLines;
            tr.SumDownhill = SumDown;
            tr.SumUphill = SumUp;

            return(tr);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inp"></param>
        /// <returns></returns>
        public int ConvertHMSToSecs(String inp)
        {
            int iPom = 0, count;
            String h = "";
            String m = "";
            String s = "";

            String[] Phrases = inp.Split(':');
            count =  Phrases.GetLength(0);
            if (count < 3) return (-1);
            h = Phrases[0];
            m = Phrases[1]; ;
            s = Phrases[2]; ;

            iPom = int.Parse(s) + 60 * int.Parse(m) + 3600 * int.Parse(h);
            return (iPom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sFileName"></param>
        /// <returns></returns>
        public TripRecord ReadGpxFile(string sFileName)
        {
            string sPom;
            String sName;
            double lat1, lon1, alt1, lat2, lon2, alt2;
            double lat, lon, alt;
            DateTime datum1, datum2, datum, StartDate, EndDate;
            int trSumUp, trDuration, trLen, trNoRecs, FirstRec, iRes;
            int Length;
            double Speed, trMaxSpeed;
            TripRecord tr = new TripRecord("", 0, 0, 0, 0.0F, 0, 0);
            TimeSpan tspan, ts2;

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(sFileName);
                foreach (XmlNode Node in xmlDoc)
                {
                    if ((Node.NodeType == XmlNodeType.Element)       // top node GPX
                        && (Node.Name.CompareTo("gpx") == 0))
                    {
                        foreach (XmlNode stock in Node.ChildNodes)     // search for TRK elements
                        {
                            if ((stock.NodeType == XmlNodeType.Element)
                                && (stock.Name.CompareTo("trk") == 0))
                            {
                                sName = string.Format("New track: {0}, ", GetChildName(stock, "name"));
                                _LbLog.Items.Add(sName);

                                lat1 = lon1 = alt1 = lat2 = lon2 = alt2 = 0;
                                trSumUp = trDuration = trLen = trNoRecs = 0;
                                FirstRec = 1;
                                datum1 = datum2 = StartDate = EndDate = DateTime.Now;
                                trMaxSpeed = 0;

                                foreach (XmlNode trkseg in stock.ChildNodes)     // search for TRKSEG elements
                                {
                                    if ((trkseg.NodeType == XmlNodeType.Element)
                                        && (trkseg.Name.CompareTo("trkseg") == 0))
                                    {
                                        sName = string.Format(" NoPoints={0}", trkseg.ChildNodes.Count);
                                        _LbLog.Items.Add(sName);
                                        if (trkseg.ChildNodes.Count > 20)
                                        {
                                            foreach (XmlNode par in trkseg.ChildNodes)
                                            {
                                                iRes = ProcessTrackPoint(par, out lat, out lon, out alt, out datum);
                                                if (iRes == 0)
                                                {
                                                    trNoRecs++;
                                                    if (FirstRec == 1)
                                                    {
                                                        FirstRec = 0;
                                                        lat1 = lat; lon1 = lon; alt1 = alt; datum1 = datum;
                                                        StartDate = datum;
                                                    }
                                                    else
                                                    {
                                                        Speed = 0;
                                                        lat2 = lat; lon2 = lon; alt2 = alt; datum2 = datum;
                                                        EndDate = datum;

                                                        Length = CalcDist(lat1, lon1, lat2, lon2);

                                                        ts2 = datum2 - datum1;
                                                        if (ts2.Seconds > 0.000001)
                                                        {
                                                            Speed = (double)Length / ts2.Seconds;
                                                        }
                                                        if (Speed > 0.2)
                                                        {

                                                            trLen += Length;
                                                            if (alt > alt1) trSumUp += (int)(Math.Round(alt - alt1));
                                                            if (Speed > trMaxSpeed) trMaxSpeed = Speed;
                                                            trDuration += ts2.Seconds;

                                                        }


                                                        lat1 = lat2; lon1 = lon2; alt1 = alt2; datum1 = datum2;


                                                        sPom = string.Format("LAT={0}     LON={1}     ALT={2:F1}      DATE={3}     DIST={4:F1}     SPEED={5:F1}",
                                                                              lat, lon, alt, datum, Length, Speed);
                                                        _LbLog.Items.Add(sPom);


                                                    }

                                                }
                                            }
                                            tspan = EndDate - StartDate;
                                            sPom = string.Format("  ----  SumLEN={0}  SumUp={1}  Duration={2} Min   MAX={3:F2}  DURATION={4} ",
                                                                               trLen, trSumUp, tspan.Minutes, 3.6 * trMaxSpeed, trDuration);
                                            _LbLog.Items.Add(sPom);

                                            tr.Datum = string.Format("{0:s}", StartDate);
                                            tr.Duration += trDuration;
                                            tr.Length += trLen;
                                            if((3.6F * (float)trMaxSpeed) > tr.MaxSpeed) tr.MaxSpeed = 3.6F * (float)trMaxSpeed;
                                            tr.NoRecs += trNoRecs;
                                            tr.SumUphill += trSumUp;

                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception: {0}", ex.Message));
                return (tr);
            }
            return (tr);
        }


        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Lat1"></param>
        /// <param name="Lon1"></param>
        /// <param name="Lat2"></param>
        /// <param name="Lon2"></param>
        /// <returns></returns>
        private int CalcDist(double Lat1, double Lon1, double Lat2, double Lon2) 
        {
            double AvgLatDeg, AvgLatRad, K1, K2, Result, DeltaLat, DeltaLon;

            DeltaLat = Lat2 - Lat1;
            DeltaLon = Lon2 - Lon1;
            AvgLatDeg = (Lat1 + Lat2) / 2;
            AvgLatRad = AvgLatDeg * Math.PI / 180;

            K1 = 111.13209 - 0.56605 * Math.Cos(2 * AvgLatRad) + 0.0012 * Math.Cos(4 * AvgLatRad);
            K2 = 111.41513 * Math.Cos(AvgLatRad) - 0.09455 * Math.Cos(3 * AvgLatRad) + 0.00012 * Math.Cos(5 * AvgLatRad);

            Result = Math.Sqrt(K1 * DeltaLat * K1 * DeltaLat + K2 * DeltaLon * K2 * DeltaLon);

            return((int) (Math.Round( Result * 1000)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="sName"></param>
        /// <returns></returns>
        private string GetChildName(XmlNode node, string sName)
        {
            string sPom = "";
            foreach (XmlNode child in node)
            {
                if (child.Name.CompareTo(sName) == 0) sPom = child.InnerText;
            }
            return (sPom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <param name="datum"></param>
        /// <returns></returns>
        private int ProcessTrackPoint(XmlNode point, out double lat, out double lon,
                                      out double alt, out DateTime datum)
        {
            lat = lon = alt = 0;
            datum =DateTime.Now;

            foreach (XmlAttribute par in point.Attributes)
            {
                if (par.Name.CompareTo("lat") == 0) lat = float.Parse(par.Value, cult);
                else if (par.Name.CompareTo("lon") == 0) lon = float.Parse(par.Value, cult);
            }


            foreach (XmlNode par in point.ChildNodes)
            {
                if (par.Name.CompareTo("ele") == 0) alt = float.Parse(par.InnerText, cult);
                else if (par.Name.CompareTo("time") == 0) datum = DateTime.Parse(par.InnerText);
            }

            if ((lat != 0) && (lon != 0) && (alt != 0)) return (0);
            else return (1);

        }
    }
}
