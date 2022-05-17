using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cz.zk.OREGON
{
    class TripRecord
    {
        private String _Datum;
        private int _SumUphill;
        private int _SumDownhill;
        private int _Duration;
        private float _MaxSpeed;
        private int _Length;
        private int _NoRecs;

        public TripRecord(String iDatum, int iSumUp, int iSumDown,
                          int iDuration, float fMaxSpeed, int iLength,
                          int iNoRecs)
        {

            _Datum = iDatum;
            _SumUphill = iSumUp;
            _SumDownhill = iSumDown;
            _Duration = iDuration;
            _MaxSpeed = fMaxSpeed;
            _Length = iLength;
            _NoRecs = iNoRecs;
        }

        public String Datum
        {
            get { return _Datum; }
            set { _Datum = value; }
        }

        public int SumUphill
        {
            get { return _SumUphill; }
            set { _SumUphill = value; }
        }

        public int SumDownhill
        {
            get { return _SumDownhill; }
            set { _SumDownhill = value; }
        }

        public int Duration
        {
            get { return _Duration; }
            set { _Duration = value; }
        }

        public float MaxSpeed
        {
            get { return _MaxSpeed; }
            set { _MaxSpeed = value; }
        }

        public int Length
        {
            get { return _Length; }
            set { _Length = value; }
        }

        public int NoRecs
        {
            get { return _NoRecs; }
            set { _NoRecs = value; }
        }

    }
}
