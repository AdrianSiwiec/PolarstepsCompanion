using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PolarstepsCompanion
{
    // A dummy class to use with Json deserializer
    public class LocationsClass
    {
        public List<DataPoint> Locations { get; set; }

        public LocationsClass(List<DataPoint> locations)
        {
            this.Locations = locations;
        }
    }

    public class DataPoint : IComparable<DataPoint>
    {
        private int time;
        private double lat;
        private double lon;

        public int Time { get => time; set => time = value; }
        public double Lat { get => lat; set => lat = value; }
        public double Lon { get => lon; set => lon = value; }

        public DataPoint(decimal time, double lat, double lon)
        {
            this.time = Convert.ToInt32(time);
            this.lat = lat;
            this.lon = lon;
        }

        public int CompareTo([AllowNull] DataPoint other)
        {
            return time.CompareTo(other?.time);
        }
    }

    class PolarstepsTrip
    {
        private string filename;
        public string Filename { get => filename; private set => filename = value; }

        private string displayName;
        public string DisplayName { get => displayName; set => displayName = value; }


        public PolarstepsTrip(string filename)
        {
            this.filename = filename;
            this.displayName = filename.Split("_")?[0];
        }

        public override string ToString()
        {
            return displayName;
        }
    }

    class LocationsProcessor
    {
        private List<DataPoint> sortedDataPoints;
        public LocationsProcessor(List<DataPoint> dataPoints)
        {
            {
                sortedDataPoints = dataPoints;
                sortedDataPoints.Sort();
            }
        }

        //Not terribly accurate - should be average on a globe, not just average of coordinates. Should do for now.
        public DataPoint? GetLocation(int time)
        {
            //Outside of trip range by more than hour - do not provice location.
            if (time + TimeSpan.FromMinutes(60).TotalSeconds < sortedDataPoints[0].Time ||
               time - TimeSpan.FromMinutes(60).TotalSeconds > sortedDataPoints[sortedDataPoints.Count - 1].Time)
            {
                return null;
            }

            int index = sortedDataPoints.BinarySearch(new DataPoint(time, 0, 0));

            if (index >= 0)
            {
                return sortedDataPoints[index];
            }
            else
            {
                index = ~index;

                if (index == 0) return sortedDataPoints[0];
                if (index >= sortedDataPoints.Count) return sortedDataPoints[sortedDataPoints.Count - 1];

                return GetAverageDataPoint(time, sortedDataPoints[index - 1], sortedDataPoints[index]);
            }
        }

        static private DataPoint GetAverageDataPoint(int time, DataPoint d1, DataPoint d2)
        {
            if (d2.Time < d1.Time || time < d1.Time || time > d2.Time)
                throw new ArgumentException(string.Format("Unexpected argument to Average Data Point: time={0}, d1.time={1}, d2.time={2}", time, d1.Time, d2.Time));

            double proportion = (double)(time - d1.Time) / (d2.Time - d1.Time);

            double lat = d2.Lat * (1 - proportion) + d2.Lat * proportion;
            double lon = d1.Lon * (1 - proportion) + d2.Lon * proportion;

            //Trace.WriteLine("Proportion: " + proportion);
            //Trace.WriteLine("lon1: " + d1.Lon + ", lon2: " + d2.Lon + ", resLon: " + lon);
            //Trace.WriteLine("lon1: " + d1.Lat + ", lon2: " + d2.Lat + ", resLon: " + lat);

            return new DataPoint(time, lat, lon);
        }
    }

    class PolarstepsProcessor
    {
        static private readonly string TRIP_DIR = "trip";
        static private readonly string USER_DIR = "user";
        static private readonly string LOCATIONS_FILENAME = "locations.json";

        private string polarstepsMainDir;

        internal PolarstepsTrip SelectedTrip { get; private set; }
        private bool isTripProcessed = false;
        internal bool IsTripProcessed { get => isTripProcessed; set => isTripProcessed = value; }
        private LocationsProcessor locationsProcessor;

        public bool IsValidDirectory
        {
            get;
            private set;
        }
        public List<string> TripNames
        {
            get;
            private set;
        }

        public PolarstepsProcessor(string polarstepsMainDir)
        {
            this.polarstepsMainDir = polarstepsMainDir;

            List<string> dirs = new List<string>(Directory.EnumerateDirectories(polarstepsMainDir));

            bool containsTripDir = false;
            bool containsUserDir = false;

            foreach (string dir in dirs)
            {
                if (dir.EndsWith(TRIP_DIR, System.StringComparison.InvariantCultureIgnoreCase))
                    containsTripDir = true;

                if (dir.EndsWith(USER_DIR, System.StringComparison.InvariantCultureIgnoreCase))
                    containsUserDir = true;
            }

            if (containsUserDir && containsTripDir)
                IsValidDirectory = true;
            else
            {
                IsValidDirectory = false;
                return;
            }

            TripNames = new List<string>();
            foreach (string trip in Directory.EnumerateDirectories(Path.Combine(polarstepsMainDir, TRIP_DIR)))
            {
                TripNames.Add(trip.Substring(polarstepsMainDir.Length + 1 + TRIP_DIR.Length + 1));
            }
        }

        internal void TripSelected(object tripObject)
        {
            ComboBoxItem cbItem = tripObject as ComboBoxItem;
            SelectedTrip = cbItem?.Content as PolarstepsTrip;
        }

        private string GetLocationsFilePath()
        {
            return Path.Combine(polarstepsMainDir, TRIP_DIR, SelectedTrip.Filename, LOCATIONS_FILENAME);
        }

        internal void Process()
        {
            if (IsTripProcessed) return;

            string locationsJson = File.ReadAllText(GetLocationsFilePath());

            LocationsClass locationsClass = new LocationsClass(null);
            locationsClass = JsonConvert.DeserializeObject<LocationsClass>(locationsJson);

            locationsProcessor = new LocationsProcessor(locationsClass.Locations);
            IsTripProcessed = locationsProcessor != null;
        }

        public DataPoint? GetLocation(int time)
        {
            return locationsProcessor?.GetLocation(time);
        }
    }
}
