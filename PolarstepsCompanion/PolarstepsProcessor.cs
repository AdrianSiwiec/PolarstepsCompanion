using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PolarstepsCompanion
{
    class PolarstepsProcessor
    {
        static private readonly string TRIP_DIR = "trip";
        static private readonly string USER_DIR = "trip";

        public bool isValidDirectory
        {
            get;
            private set;
        }
        public List<string> tripNames
        {
            get;
            private set;
        }

        public PolarstepsProcessor(string fileName)
        {
            List<string> dirs = new List<string>(Directory.EnumerateDirectories(fileName));

            bool containsTripDir = false;
            bool containsUserDir = false;

            foreach (string dir in dirs)
            {
                if (dir.EndsWith(TRIP_DIR))
                    containsTripDir = true;

                if (dir.EndsWith(USER_DIR))
                    containsUserDir = true;
            }

            if (containsUserDir && containsTripDir)
                isValidDirectory = true;
            else
            {
                isValidDirectory = false;
                return;
            }

            tripNames = new List<string>();
            foreach(string trip in Directory.EnumerateDirectories(Path.Combine(fileName, TRIP_DIR)))
            {
                tripNames.Add(trip.Substring(fileName.Length + 1 + TRIP_DIR.Length + 1).Split("_")?[0]);
            }
        }
    }
}
