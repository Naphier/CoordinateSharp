using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateSharp;
using System.Diagnostics;

namespace CoordinateSharp_TestProj
{
    public class DebugNullRefIssue
    {
        Stopwatch stopwatch;
        public DebugNullRefIssue(Action<string> outputAction, bool outputEveryCalc)
        {
            stopwatch = Stopwatch.StartNew();
            long counter = 0;
            for (float i = -90; i <= 90; i+= 1f)
            {
                for (float j = -180; j <= 180; j += 1f)
                {
                    counter++;
                    string mgrs = GetMgrsFromLatLong(i, j).ToString();
                    string output = string.Format("Coords: ({0}, {1})    MGRS: {2}", i, j, mgrs);
                    if (outputEveryCalc && outputAction != null)
                    {
                        try
                        {
                            outputAction.Invoke(output);
                        }
                        catch { }
                    }
                }
            }

            stopwatch.Stop();
            if (outputAction != null)
            {
                try
                {
                    double avg = (double)stopwatch.ElapsedMilliseconds / (double)counter;
                    outputAction.Invoke(string.Format(
                        "Iterations: {0} Total time: {1}ms  Average Calc: {2:G5}ms", 
                        counter, stopwatch.ElapsedMilliseconds, avg));
                }
                catch { }
            }

        }

        static Coordinate coordinate;
        public static MilitaryGridReferenceSystem GetMgrsFromLatLong(double lat, double lng)
        {
            if (coordinate == null)
            {
                var eagerLoad = new EagerLoad();
                eagerLoad.Cartesian = false;
                eagerLoad.Celestial = false;
                eagerLoad.UTM_MGRS = false;
                //eagerLoad.ECEF = false;
                coordinate = new Coordinate(lat, lng, eagerLoad);

            }
            else
            {
                coordinate.Latitude.DecimalDegree = lat;
                coordinate.Longitude.DecimalDegree = lng;
            }

            coordinate.LoadUTM_MGRS_Info();
            return coordinate.MGRS;
        }
    }
}
