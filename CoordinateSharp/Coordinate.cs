﻿/*
 (c) 2017, Justin Gielski
 CoordinateSharp is a .NET standard library that is intended to ease geographic coordinate 
 format conversions and location based celestial calculations.
 https://github.com/Tronald/CoordinateSharp

 Many celestial formulas in this library are based on Jean Meeus's 
 Astronomical Algorithms (2nd Edition). Comments that reference only a chapter
 are refering to this work.
*/

using System;
using System.ComponentModel;

namespace CoordinateSharp
{   
    /// <summary>
    /// Observable class for handling all location based information.
    /// This is the main class for CoordinateSharp.
    /// </summary>
    /// <remarks>
    /// All information should be pulled from this class to include celestial information
    /// </remarks>
    [Serializable]
    public class Coordinate : INotifyPropertyChanged
    {     
        /// <summary>
        /// Creates an empty Coordinate.
        /// </summary>
        /// <remarks>
        /// Values will need to be provided to latitude/longitude CoordinateParts manually
        /// </remarks>
        public Coordinate()
        {
            this.FormatOptions = new CoordinateFormatOptions();
            this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            latitude = new CoordinatePart(CoordinateType.Lat, this);
            longitude = new CoordinatePart(CoordinateType.Long, this);
            celestialInfo = new Celestial();
            utm = new UniversalTransverseMercator(latitude.ToDouble(), longitude.ToDouble(), this);
            mgrs = new MilitaryGridReferenceSystem(this.utm);
            cartesian = new Cartesian(this);
            EagerLoadSettings = new EagerLoad();
        }
        /// <summary>
        /// Creates an empty Coordinate with custom datum.
        /// </summary>
        /// <remarks>
        /// Values will need to be provided to latitude/longitude CoordinateParts manually
        /// </remarks>
        internal Coordinate(double equatorialRadius, double inverseFlattening, bool t)
        {
            this.FormatOptions = new CoordinateFormatOptions();
            this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            latitude = new CoordinatePart(CoordinateType.Lat, this);
            longitude = new CoordinatePart(CoordinateType.Long, this);
            celestialInfo = new Celestial();
            utm = new UniversalTransverseMercator(latitude.ToDouble(), longitude.ToDouble(), this, equatorialRadius,inverseFlattening);     
            mgrs = new MilitaryGridReferenceSystem(this.utm);
            cartesian = new Cartesian(this);
            //Set_Datum(equatorialRadius, inverseFlattening);
            EagerLoadSettings = new EagerLoad();
        }
        /// <summary>
        /// Creates a populated Coordinate based on decimal (signed degrees) formated latitude and longitude.
        /// </summary>
        /// <param name="lat">latitude</param>
        /// <param name="longi">longitude</param>
        /// <remarks>
        /// Geodate will default to 1/1/1900 GMT until provided
        /// </remarks>
        public Coordinate(double lat, double longi)
        {         
            this.FormatOptions = new CoordinateFormatOptions();
            this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            latitude = new CoordinatePart(lat, CoordinateType.Lat, this);
            longitude = new CoordinatePart(longi, CoordinateType.Long, this);
            celestialInfo = new Celestial(lat,longi,this.geoDate);
            utm = new UniversalTransverseMercator(lat, longi, this);
            mgrs = new MilitaryGridReferenceSystem(this.utm);
            cartesian = new Cartesian(this);
            EagerLoadSettings = new EagerLoad();
        }
        /// <summary>
        /// Creates a populated Coordinate object with an assigned GeoDate.
        /// </summary>
        /// <param name="lat">latitude</param>
        /// <param name="longi">longitude</param>
        /// <param name="date">DateTime (UTC)</param>
        public Coordinate(double lat, double longi, DateTime date)
        {
            this.FormatOptions = new CoordinateFormatOptions();
            latitude = new CoordinatePart(lat, CoordinateType.Lat, this);
            longitude = new CoordinatePart(longi, CoordinateType.Long, this);
            celestialInfo = new Celestial(lat, longi, date);            
            this.geoDate = date;
            utm = new UniversalTransverseMercator(lat, longi, this);
            mgrs = new MilitaryGridReferenceSystem(this.utm);
            cartesian = new Cartesian(this);
            EagerLoadSettings = new EagerLoad();
        }

        /// <summary>
        /// Creates an empty Coordinates object with specificied eager loading options.
        /// </summary>
        /// <remarks>
        /// Values will need to be provided to latitude/longitude manually
        /// </remarks>
        /// <param name="eagerLoad">Eager loading options</param>
        public Coordinate(EagerLoad eagerLoad)
        {
            this.FormatOptions = new CoordinateFormatOptions();
            this.geoDate = this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            latitude = new CoordinatePart(CoordinateType.Lat, this);
            longitude = new CoordinatePart(CoordinateType.Long, this);
            if (eagerLoad.Cartesian)
            {
                cartesian = new Cartesian(this);
            }
            if (eagerLoad.Celestial)
            {
                celestialInfo = new Celestial();
            }
            if (eagerLoad.UTM_MGRS)
            {
                utm = new UniversalTransverseMercator(latitude.ToDouble(), longitude.ToDouble(), this);
                mgrs = new MilitaryGridReferenceSystem(this.utm);
            }
           
            EagerLoadSettings = eagerLoad;
        }
        /// <summary>
        /// Creates a populated Coordinate object with specified eager loading options.
        /// </summary>
        /// <remarks>
        /// Geodate will default to 1/1/1900 GMT until provided
        /// </remarks>
        /// <param name="lat">latitude</param>
        /// <param name="longi">longitude</param>
        /// <param name="eagerLoad">Eager loading options</param>
        public Coordinate(double lat, double longi, EagerLoad eagerLoad)
        {
            this.FormatOptions = new CoordinateFormatOptions();
            this.geoDate = this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            latitude = new CoordinatePart(lat, CoordinateType.Lat, this);
            longitude = new CoordinatePart(longi, CoordinateType.Long, this);

            if (eagerLoad.Celestial)
            {
                celestialInfo = new Celestial(lat, longi, this.geoDate);
            }
            if (eagerLoad.UTM_MGRS)
            {
                utm = new UniversalTransverseMercator(lat, longi, this);
                mgrs = new MilitaryGridReferenceSystem(this.utm);
            }
            if (eagerLoad.Cartesian)
            {
                cartesian = new Cartesian(this);
            }

            EagerLoadSettings = eagerLoad;
        }
        /// <summary>
        /// Creates a populated Coordinate object with specified eager load options and an assigned GeoDate.
        /// </summary>
        /// <param name="lat">Decimal format latitude</param>
        /// <param name="longi">Decimal format longitude</param>
        /// <param name="date">DateTime you wish to use for celestial calculation</param>
        /// <param name="eagerLoad">Eager loading options</param>
        public Coordinate(double lat, double longi, DateTime date, EagerLoad eagerLoad)
        {
            this.FormatOptions = new CoordinateFormatOptions();
            latitude = new CoordinatePart(lat, CoordinateType.Lat, this);
            longitude = new CoordinatePart(longi, CoordinateType.Long, this);
            this.geoDate = date;

            if (eagerLoad.Celestial)
            {
                celestialInfo = new Celestial(lat, longi, date);
            }

            if (eagerLoad.UTM_MGRS)
            {
                utm = new UniversalTransverseMercator(lat, longi, this);
                mgrs = new MilitaryGridReferenceSystem(this.utm);
            }
            if (eagerLoad.Cartesian)
            {
                cartesian = new Cartesian(this);
            }

            EagerLoadSettings = eagerLoad;
        }
       
        private CoordinatePart latitude;
        private CoordinatePart longitude;
        private UniversalTransverseMercator utm;
        private MilitaryGridReferenceSystem mgrs;
        private Cartesian cartesian;
        private ECEF ecef;
        private DateTime geoDate;
        private Celestial celestialInfo;
        private double height;
       
        /// <summary>
        /// Latitudinal Coordinate Part
        /// </summary>
        public CoordinatePart Latitude
        {
            get { return this.latitude; }
            set
            {
                if (this.latitude != value)
                {
                    if (value.Position == CoordinatesPosition.E || value.Position == CoordinatesPosition.W)
                    { throw new ArgumentException("Invalid Position", "Latitudinal positions cannot be set to East or West."); }
                    this.latitude = value;
                    this.latitude.Parent = this;
                    if (EagerLoadSettings.Celestial)
                    {
                        celestialInfo.CalculateCelestialTime(this.Latitude.DecimalDegree, this.Longitude.DecimalDegree, this.geoDate);
                    }
                    if (longitude != null)
                    {

                        if (EagerLoadSettings.UTM_MGRS)
                        {
                            utm = new UniversalTransverseMercator(latitude.ToDouble(), longitude.ToDouble(), this, utm.equatorial_radius, utm.inverse_flattening);
                            mgrs = new MilitaryGridReferenceSystem(this.utm);
                        }
                        if (EagerLoadSettings.Cartesian)
                        {
                            cartesian = new Cartesian(this);
                        }
                    }
                    
                }
            }
        }
        /// <summary>
        /// Longitudinal Coordinate Part
        /// </summary>
        public CoordinatePart Longitude 
        {
            get { return this.longitude; }
            set
            {
                if (this.longitude != value)
                {
                    if (value.Position == CoordinatesPosition.N || value.Position == CoordinatesPosition.S)
                    { throw new ArgumentException("Invalid Position", "Longitudinal positions cannot be set to North or South."); }
                    this.longitude = value;
                    this.latitude.Parent = this;
                    if (EagerLoadSettings.Celestial)
                    {                      
                        celestialInfo.CalculateCelestialTime(this.Latitude.DecimalDegree, this.Longitude.DecimalDegree, this.geoDate);
                    }
                    if (latitude != null)
                    {
                        if (EagerLoadSettings.UTM_MGRS)
                        {
                            utm = new UniversalTransverseMercator(latitude.ToDouble(), longitude.ToDouble(), this, utm.equatorial_radius, utm.inverse_flattening);
                            mgrs = new MilitaryGridReferenceSystem(this.utm);
                        }
                        if (EagerLoadSettings.Cartesian)
                        {
                            cartesian = new Cartesian(this);
                        }
                    }
                }
            }
        }     
        /// <summary>
        /// Date used to calculate celestial information
        /// </summary>
        /// <remarks>
        /// Assumes all times are in UTC
        /// </remarks>
        public DateTime GeoDate
        {
            get { return this.geoDate; }
            set
            {
                if (this.geoDate != value)
                {
                    this.geoDate = value;
                    if (EagerLoadSettings.Celestial)
                    {
                        celestialInfo.CalculateCelestialTime(this.Latitude.DecimalDegree, this.Longitude.DecimalDegree, this.geoDate);
                        this.NotifyPropertyChanged("CelestialInfo");
                    }
                   
                    this.NotifyPropertyChanged("GeoDate");                                    
                }
            }
        }
        /// <summary>
        /// Universal Transverse Mercator Values
        /// </summary>
        public UniversalTransverseMercator UTM
        {
            get
            {
                return this.utm;
            }
            //set
            //{
            //    if (this.utm != value)
            //    {
            //        this.utm = value;
            //        this.NotifyPropertyChanged("UTM");
            //        celestialInfo.CalculateCelestialTime(this.Latitude.DecimalDegree, this.Longitude.DecimalDegree, this.geoDate);
            //        this.NotifyPropertyChanged("CelestialInfo");
            //    }
            //}
        }
        /// <summary>
        /// Military Grid Reference System (NATO UTM)
        /// </summary>
        public MilitaryGridReferenceSystem MGRS
        {
            get
            {
                return this.mgrs;
            }
            //set
            //{
            //    if (this.utm != value)
            //    {
            //        this.utm = value;
            //        this.NotifyPropertyChanged("UTM");
            //        celestialInfo.CalculateCelestialTime(this.Latitude.DecimalDegree, this.Longitude.DecimalDegree, this.geoDate);
            //        this.NotifyPropertyChanged("CelestialInfo");
            //    }
            //}
        }
        /// <summary>
        /// Cartesian (Based on Spherical Earth)
        /// </summary>
        public Cartesian Cartesian
        {
            get
            {
                return cartesian;
            }         
        }
        /// <summary>
        /// ECEF
        /// </summary>
        public ECEF ECEF
        {
            get
            {
                return ecef;
            }
        }
        /// <summary>
        /// Height (used during ECEF calculations only)
        /// </summary>
        public double Height
        {
            get { return this.height; }
            set
            {
                if (this.height != value)
                {
                    this.height = value;
                    this.NotifyPropertyChanged("Height");
                }
            }
        }

        /// <summary>
        /// Celestial information based on the objects location and geographic UTC date.
        /// </summary>
        public Celestial CelestialInfo
        {
            get { return this.celestialInfo; }          
        }

        /// <summary>
        /// Initialize celestial information (required if eager loading is turned off).
        /// </summary>
        public void LoadCelestialInfo()
        {
            this.celestialInfo = Celestial.LoadCelestial(this);
        }
        /// <summary>
        /// Initialize UTM and MGRS information (required if eager loading is turned off).
        /// </summary>
        public void LoadUTM_MGRS_Info()
        {
            utm = new UniversalTransverseMercator(latitude.ToDouble(), longitude.ToDouble(), this);
            mgrs = new MilitaryGridReferenceSystem(this.utm);
        }
        /// <summary>
        /// Initialize cartesian information (required if eager loading is turned off).
        /// </summary>
        public void LoadCartesianInfo()
        {
            cartesian = new Cartesian(this);
        }
        /// <summary>
        /// Initialize ECEF information (required if eager loading is turned off).
        /// </summary>
        public void LoadECEFInfo()
        {
            ecef = new ECEF(this);
        }

        /// <summary>
        /// Coordinate string formatting options.
        /// </summary>
        public CoordinateFormatOptions FormatOptions { get; set; }
        /// <summary>
        /// Eager loading settings.
        /// </summary>
        public EagerLoad EagerLoadSettings { get; set; }

        /// <summary>
        /// Bindable formatted coordinate string.
        /// </summary>
        /// <remarks>Bind to this property when MVVM patterns used</remarks>
        public string Display
        {
            get
            {
                return this.Latitude.Display + " " + this.Longitude.Display;
            }
        }
        /// <summary>
        /// Overridden Coordinate ToString() method.
        /// </summary>
        /// <returns>string (formatted).</returns>
        public override string ToString()
        {
            string latString = latitude.ToString();
            string longSting = longitude.ToString();
            return latString + " " + longSting;
        }     
        /// <summary>
        /// Overridden Coordinate ToString() method that accepts formatting. 
        /// Refer to documentation for coordinate format options.
        /// </summary>
        /// <param name="options">CoordinateFormatOptions</param>
        /// <returns>Custom formatted coordinate</returns>
        public string ToString(CoordinateFormatOptions options)
        {
            string latString = latitude.ToString(options);
            string longSting = longitude.ToString(options);
            return latString + " " + longSting;
        }

        /// <summary>
        /// Set a custom datum for coordinate conversions and distance calculation.
        /// </summary>
        /// <param name="radius">Equatorial Radius</param>
        /// <param name="flat">Inverse Flattening</param>
        public void Set_Datum(double radius, double flat)
        {
            //WGS84
            //RADIUS 6378137.0;
            //FLATTENING 298.257223563;
            if(utm == null)
            {
                throw new NullReferenceException("UTM and MGRS objects have not been loaded. If Eagerloading, ensure they are loaded prior to setting a datum.");
            }
            this.utm.equatorial_radius = radius;
            this.utm.inverse_flattening = flat;
            this.utm.ToUTM(this.Latitude.ToDouble(), this.Longitude.ToDouble(), this.utm);
            mgrs = new MilitaryGridReferenceSystem(this.utm);
            NotifyPropertyChanged("UTM");
            NotifyPropertyChanged("MGRS");
        }
        /// <summary>
        /// Returns a Distance object based on the current and specified coordinate (Haversine / Spherical Earth).
        /// </summary>
        /// <param name="c2">Coordinate</param>
        /// <returns>Distance</returns>
        public Distance Get_Distance_From_Coordinate(Coordinate c2)
        {
            return new Distance(this, c2);
        }
        /// <summary>
        /// Returns a Distance object based on the current and specified coordinate and specified earth shape.
        /// </summary>
        /// <param name="c2">Coordinate</param>
        /// <param name="shape">Earth shape</param>
        /// <returns>Distance</returns>
        public Distance Get_Distance_From_Coordinate(Coordinate c2, Shape shape)
        {
            return new Distance(this, c2, shape);
        }
        /// <summary>
        /// Move coordinate based on provided bearing and distance.
        /// </summary>
        /// <param name="distance">distance in meters</param>
        /// <param name="bearing">bearing</param>
        /// <param name="shape">shape of earth</param>
        public void Move(Double distance, double bearing, Shape shape)
        {
            //Convert to Radians for formula
            double lat1 = latitude.ToRadians();
            double lon1 = longitude.ToRadians();
            double crs12 = bearing * Math.PI / 180; //Convert bearing to radians

            double[] ellipse = new double[] { this.utm.Equatorial_Radius, this.UTM.Inverse_Flattening };

            if (shape == Shape.Sphere)
            {
                double[] cd = Distance_Assistant.Direct(lat1, lon1, crs12, distance);
                double lat2 = cd[0] * (180 / Math.PI);
                double lon2 = cd[1] * (180 / Math.PI);

                //ADJUST CORD
                Latitude.DecimalDegree = lat2;
                Longitude.DecimalDegree = lon2;
            }
            else
            {
                double[] cde = Distance_Assistant.Direct_Ell(lat1, -lon1, crs12, distance, ellipse);  // ellipse uses East negative
                //Convert back from radians 
                double lat2 = cde[0] * (180 / Math.PI);
                double lon2 = -cde[1] * (180 / Math.PI); // ellipse uses East negative             
                //ADJUST CORD
                Latitude.DecimalDegree = lat2;
                Longitude.DecimalDegree = lon2;
            }        
        }
        /// <summary>
        /// Move coordinate based on provided target coordinate and distance.
        /// </summary>
        /// <param name="c">Target coordinate</param>
        /// <param name="distance">Distance toward target</param>
        /// <param name="shape">Shape of earth</param>
        public void Move(Coordinate c, double distance, Shape shape)
        {
            Distance d = new Distance(this, c, shape);
            //Convert to Radians for formula
            double lat1 = latitude.ToRadians();
            double lon1 = longitude.ToRadians();
            double crs12 = d.Bearing * Math.PI / 180; //Convert bearing to radians

            double[] ellipse = new double[] { this.utm.Equatorial_Radius, this.UTM.Inverse_Flattening };

            if (shape == Shape.Sphere)
            {
                double[] cd = Distance_Assistant.Direct(lat1, lon1, crs12, distance);
                double lat2 = cd[0] * (180 / Math.PI);
                double lon2 = cd[1] * (180 / Math.PI);

                //ADJUST CORD
                Latitude.DecimalDegree = lat2;
                Longitude.DecimalDegree = lon2;
            }
            else
            {
                double[] cde = Distance_Assistant.Direct_Ell(lat1, -lon1, crs12, distance, ellipse);  // ellipse uses East negative
                //Convert back from radians 
                double lat2 = cde[0] * (180 / Math.PI);
                double lon2 = -cde[1] * (180 / Math.PI); // ellipse uses East negative             
                //ADJUST CORD
                Latitude.DecimalDegree = lat2;
                Longitude.DecimalDegree = lon2;
            }
        }

        /// <summary>
        /// Attempts to parse a string into a Coordinate.
        /// </summary>
        /// <param name="s">Coordinate string</param>
        /// <param name="c">Coordinate</param>
        /// <returns>boolean</returns>
        /// <example>
        /// <code>
        /// Coordinate c;
        /// if(Coordinate.TryParse("N 32.891º W 64.872º",out c))
        /// {
        ///     Console.WriteLine(c); //N 32º 53' 28.212" W 64º 52' 20.914"
        /// }
        /// </code>
        /// </example>
        public static bool TryParse(string s, out Coordinate c)
        {
            c = null;
            if (FormatFinder.TryParse(s, out c))
            {
                c = new Coordinate(c.Latitude.ToDouble(), c.Longitude.ToDouble()); //Reset with EagerLoad back on.
                return true;
            }
            return false;
        }
        /// <summary>
        /// Attempts to parse a string into a Coordinate with specified DateTime
        /// </summary>
        /// <param name="s">Coordinate string</param>
        /// <param name="geoDate">GeoDate</param>
        /// <param name="c">Coordinate</param>
        /// <returns>boolean</returns>
        /// <example>
        /// <code>
        /// Coordinate c;
        /// if(Coordinate.TryParse("N 32.891º W 64.872º", new DateTime(2018,7,7), out c))
        /// {
        ///     Console.WriteLine(c); //N 32º 53' 28.212" W 64º 52' 20.914"
        /// }
        /// </code>
        /// </example>
        public static bool TryParse(string s, DateTime geoDate, out Coordinate c)
        {
            c = null;
            if (FormatFinder.TryParse(s, out c))
            {
                c = new Coordinate(c.Latitude.ToDouble(), c.Longitude.ToDouble(), geoDate); //Reset with EagerLoad back on.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Notify property changed
        /// </summary>
        /// <param name="propName">Property name</param>
        public void NotifyPropertyChanged(string propName)
        {
            switch (propName)
            {
                case "CelestialInfo":
                    this.celestialInfo.CalculateCelestialTime(this.latitude.DecimalDegree, this.longitude.DecimalDegree, this.geoDate);
                    break;
                case "UTM":
                    this.utm.ToUTM(this.latitude.ToDouble(), this.longitude.ToDouble(), this.utm);
                    break;
                case "utm":
                    //Adjust case and notify of change. 
                    //Use to notify without calling ToUTM()
                    propName = "UTM";
                    break;
                case "MGRS":
                    this.MGRS.ToMGRS(this.utm);
                    break;
                case "Cartesian":
                    Cartesian.ToCartesian(this);
                    break;
                case "ECEF":
                    ECEF.ToECEF(this);
                    break;
                default:
                    break;
            }
            if (this.PropertyChanged != null)
            {                         
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    }
    /// <summary>
    /// Observable class for handling latitudinal and longitudinal coordinate parts.
    /// </summary>
    /// <remarks>
    /// Objects can be passed to Coordinate object Latitude and Longitude properties.
    /// </remarks>
    [Serializable]
    public class CoordinatePart : INotifyPropertyChanged
    {       
        //Defaults:
        //Format: Degrees Minutes Seconds
        //Rounding: Dependent upon selected format
        //Leading Zeros: False
        //Trailing Zeros: False
        //Display Symbols: True (All Symbols display)
        //Display Hyphens: False
        //Position Display: First                               

        private double decimalDegree;
        private double decimalMinute;
        private int degrees;
        private int minutes;
        private double seconds;
        private CoordinatesPosition position;
        private CoordinateType type;    

        /// <summary>
        /// Used to determine and notify the CoordinatePart parent Coordinate object.
        /// </summary>
        public Coordinate Parent { get; set; }

        /// <summary>
        /// Observable decimal format coordinate.
        /// </summary>
        public double DecimalDegree
        {
            get { return this.decimalDegree; }
            set
            {
                //If changing, notify the needed property changes
                if (this.decimalDegree != value)
                {
                    //Validate the value
                    if (type == CoordinateType.Lat)
                    {
                        if (value > 90)
                        {
                            throw new ArgumentOutOfRangeException("Degrees out of range", "Latitude degrees cannot be greater than 90");
                        }
                        if (value < -90)
                        {
                            throw new ArgumentOutOfRangeException("Degrees out of range", "Latitude degrees cannot be less than -90");
                        }

                    }
                    if (type == CoordinateType.Long)
                    {
                        if (value > 180)
                        {
                            throw new ArgumentOutOfRangeException("Degrees out of range", "Longitude degrees cannot be greater than 180");
                        }
                        if (value < -180)
                        {
                            throw new ArgumentOutOfRangeException("Degrees out of range", "Longitude degrees cannot be less than -180");
                        }

                    }
                    this.decimalDegree = value;
                   
                    //Update Position
                    if ((this.position == CoordinatesPosition.N || this.position == CoordinatesPosition.E) && this.decimalDegree < 0)
                    {
                        if (this.type == CoordinateType.Lat) { this.position = CoordinatesPosition.S; }
                        else { this.position = CoordinatesPosition.W; }
                       
                    }
                    if ((this.position == CoordinatesPosition.W || this.position == CoordinatesPosition.S) && this.decimalDegree >= 0)
                    {
                        if (this.type == CoordinateType.Lat) { this.position = CoordinatesPosition.N; }
                        else { this.position = CoordinatesPosition.E; }
                      
                    }
                    //Update the Degree & Decimal Minute
                    double degABS = Math.Abs(this.decimalDegree); //Make decimalDegree positive for calculations
                    double degFloor = Math.Truncate(degABS); //Truncate the number leftto extract the degree
                    decimal f = Convert.ToDecimal(degFloor); //Convert to degree to decimal to keep precision during calculations
                    decimal ddm = Convert.ToDecimal(degABS) - f; //Extract decimalMinute value from decimalDegree
                    ddm *= 60; //Multiply by 60 to get readable decimalMinute

                    double dm = Convert.ToDouble(ddm); //Convert decimalMinutes back to double for storage
                    int df = Convert.ToInt32(degFloor); //Convert degrees to int for storage

                    if (this.degrees != df)
                    {
                        this.degrees = df;
                      
                    }
                    if (this.decimalMinute != dm)
                    {
                        this.decimalMinute = dm;
                   
                    }
                    //Update Minutes Seconds              
                    double dmFloor = Math.Floor(dm); //Get number left of decimal to grab minute value
                    int mF = Convert.ToInt32(dmFloor); //Convert minute to int for storage
                    f = Convert.ToDecimal(dmFloor); //Create a second minute value and store as decimal for precise calculation

                    decimal s = ddm - f; //Get seconds from minutes
                    s *= 60; //Multiply by 60 to get readable seconds
                    double secs = Convert.ToDouble(s); //Convert back to double for storage

                    if (this.minutes != mF)
                    {
                        this.minutes = mF;
                      
                    }
                    if (this.seconds != secs)
                    {
                        this.seconds = secs;                    
                    }
                    NotifyProperties(PropertyTypes.DecimalDegree);
                }
            }
        }
        /// <summary>
        /// Observable decimal format minute.
        /// </summary>
        public double DecimalMinute
        {
            get { return this.decimalMinute; }
            set
            {
                if (this.decimalMinute != value)
                {
                    if (value < 0) { value *= -1; }//Adjust accidental negative input
                    //Validate values     
                   
                    decimal dm = Math.Abs(Convert.ToDecimal(value)) / 60;
                    double decMin = Convert.ToDouble(dm);
                    if (this.type == CoordinateType.Lat)
                    {

                        if (this.degrees + decMin > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal degrees cannot be greater than 90"); }
                    }
                    else
                    {
                        if (this.degrees + decMin > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal degrees cannot be greater than 180"); }
                    }
                    if (value >= 60) { throw new ArgumentOutOfRangeException("Minutes out of range", "Coordinate Minutes cannot be greater than or equal to 60"); }
                    if (value < 0) { throw new ArgumentOutOfRangeException("Minutes out of range", "Coordinate Minutes cannot be less than 0"); }


                    this.decimalMinute = value;
                   

                    decimal decValue = Convert.ToDecimal(value); //Convert value to decimal for precision during calculation
                    decimal dmFloor = Math.Floor(decValue); //Extract minutes
                    decimal secs = decValue - dmFloor; //Extract seconds
                    secs *= 60; //Convert seconds to human readable format

                    decimal newDM = decValue / 60; //divide decimalMinute by 60 to get storage value
                    decimal newDD = this.degrees + newDM;//Add new decimal value to the floor degree value to get new decimalDegree;
                    if (this.decimalDegree < 0) { newDD = newDD * -1; } //Restore negative if needed

                    this.decimalDegree = Convert.ToDouble(newDD);  //Convert back to double for storage                      
                   

                    this.minutes = Convert.ToInt32(dmFloor); //Convert minutes to int for storage
                   
                    this.seconds = Convert.ToDouble(secs); //Convert seconds to double for storage 
                    NotifyProperties(PropertyTypes.DecimalMinute);              
                }
            }

        }
        /// <summary>
        /// Observable coordinate degree.
        /// </summary>
        public int Degrees
        {
            get { return this.degrees; }
            set
            {              
                //Validate Value
                if (this.degrees != value)
                {
                   
                    if (value < 0) { value *= -1; }//Adjust accidental negative input
                    
                    if (type == CoordinateType.Lat)
                    {
                        if (value + this.decimalMinute /100.0 > 90)
                        {
                            throw new ArgumentOutOfRangeException("Degrees", "Latitude degrees cannot be greater than 90");
                        }
                    }
                    if (type == CoordinateType.Long)
                    {                    
                        if (value + this.decimalMinute /100.0 > 180)
                        {
                            throw new ArgumentOutOfRangeException("Degrees", "Longitude degrees cannot be greater than 180");
                        }

                    }

                    decimal f = Convert.ToDecimal(this.degrees);

                    this.degrees = value;

                    double degABS = Math.Abs(this.decimalDegree); //Make decimalDegree positive for calculations
                    decimal dDec = Convert.ToDecimal(degABS); //Convert to Decimal for precision during calculations              
                                                              //Convert degrees to decimal to keep precision        
                    decimal dm = dDec - f; //Extract minutes                                      
                    decimal newDD = this.degrees + dm; //Add minutes to new degree for decimalDegree
                 
                    if (this.decimalDegree < 0) { newDD *= -1; } //Set negative as required
                   
                    this.decimalDegree = Convert.ToDouble(newDD); // Convert decimalDegree to double for storage
                    NotifyProperties(PropertyTypes.Degree);
                }
            }
        }
        /// <summary>
        /// Observable coordinate minute.
        /// </summary>
        public int Minutes
        {
            get { return this.minutes; }
            set
            {
                if (this.minutes != value)
                {
                    if (value < 0) { value *= -1; }//Adjust accidental negative input
                    //Validate the minutes
                    decimal vMin = Convert.ToDecimal(value);
                    if (type == CoordinateType.Lat)
                    {
                        if (this.degrees + (vMin / 60) > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal degrees cannot be greater than 90"); }
                    }
                    else
                    {
                        if (this.degrees + (vMin / 60) > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal degrees cannot be greater than 180"); }
                    }
                    if (value >= 60)
                    {
                        throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be greater than or equal to 60");
                    }
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be less than 0");
                    }
                    decimal minFloor = Convert.ToDecimal(this.minutes);//Convert decimal to minutes for calculation
                    decimal f = Convert.ToDecimal(this.degrees); //Convert to degree to keep precision during calculation 

                    this.minutes = value;
                   

                    double degABS = Math.Abs(this.decimalDegree); //Make decimalDegree positive
                    decimal dDec = Convert.ToDecimal(degABS); //Convert to decimalDegree for precision during calucation                        

                    decimal dm = dDec - f; //Extract minutes
                    dm *= 60; //Make minutes human readable

                    decimal secs = dm - minFloor;//Extract Seconds

                    decimal newDM = this.minutes + secs;//Add seconds to minutes for decimalMinute
                    double decMin = Convert.ToDouble(newDM); //Convert decimalMinute to double for storage
                    this.decimalMinute = decMin; //Round to correct precision
                   

                    newDM /= 60; //Convert decimalMinute to storage format
                    decimal newDeg = f + newDM; //Add value to degree for decimalDegree
                    if (this.decimalDegree < 0) { newDeg *= -1; }// Set to negative as required.
                    this.decimalDegree = Convert.ToDouble(newDeg);//Convert to double and roun to correct precision for storage
                    NotifyProperties(PropertyTypes.Minute);
                }
            }
        }
        /// <summary>
        /// Observable coordinate second.
        /// </summary>
        public double Seconds
        {
            get { return this.seconds; }
            set
            {
                if (value < 0) { value *= -1; }//Adjust accidental negative input
                if (this.seconds != value)
                {
                    //Validate Seconds
                    decimal vSec = Convert.ToDecimal(value);
                    vSec /= 60;

                    decimal vMin = Convert.ToDecimal(this.minutes);
                    vMin += vSec;
                    vMin /= 60;

                    if (type == CoordinateType.Lat)
                    {
                        if (this.degrees + vMin > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal degrees cannot be greater than 90"); }
                    }
                    else
                    {
                        if (this.degrees + vMin > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal degrees cannot be greater than 180"); }
                    }
                    if (value >= 60)
                    {
                        throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be greater than or equal to 60");
                    }
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be less than 0");
                    }
                    this.seconds = value;
                 

                    double degABS = Math.Abs(this.decimalDegree); //Make decimalDegree positive
                    double degFloor = Math.Truncate(degABS); //Truncate the number left of the decimal
                    decimal f = Convert.ToDecimal(degFloor); //Convert to decimal to keep precision

                    decimal secs = Convert.ToDecimal(this.seconds); //Convert seconds to decimal for calculations
                    secs /= 60; //Convert to storage format
                    decimal dm = this.minutes + secs;//Add seconds to minutes for decimalMinute
                    double minFD = Convert.ToDouble(dm); //Convert decimalMinute for storage
                    this.decimalMinute = minFD;//Round to proper precision
                  
                    decimal nm = Convert.ToDecimal(this.decimalMinute) / 60;//Convert decimalMinute to decimal and divide by 60 to get storage format decimalMinute
                    double newDeg = this.degrees + Convert.ToDouble(nm);//Convert to double and add to degree for storage decimalDegree
                    if (this.decimalDegree < 0) { newDeg *= -1; }//Make negative as needed
                    this.decimalDegree = newDeg;//Update decimalDegree and round to proper precision    
                    NotifyProperties(PropertyTypes.Second);
                }
            }
        }       
        /// <summary>
        /// Formate coordinate part string.
        /// </summary>
        public string Display
        {
            get 
            {
                if (this.Parent != null)
                {
                    return ToString(Parent.FormatOptions);
                }
                else
                {
                    return ToString();
                }
            }
        }
        /// <summary>
        /// Observable coordinate position.
        /// </summary>
        public CoordinatesPosition Position
        {
            get { return this.position; }
            set
            {
                if (this.position != value)
                {
                    if (type == CoordinateType.Long && (value == CoordinatesPosition.N || value == CoordinatesPosition.S))
                    {
                        throw new InvalidOperationException("You cannot change a Longitudinal type coordinate into a Latitudinal");
                    }
                    if (type == CoordinateType.Lat && (value == CoordinatesPosition.E || value == CoordinatesPosition.W))
                    {
                        throw new InvalidOperationException("You cannot change a Latitudinal type coordinate into a Longitudinal");
                    }
                    this.decimalDegree *= -1; // Change the position
                    this.position = value;
                    NotifyProperties(PropertyTypes.Position);
                }
            }
        }

        /// <summary>
        /// Creates an empty CoordinatePart.
        /// </summary>
        /// <param name="t">CoordinateType</param>
        /// <param name="c">Parent Coordinate object</param>
        public CoordinatePart(CoordinateType t, Coordinate c)
        {     
            this.Parent = c;
            this.type = t;
            this.decimalDegree = 0;
            this.degrees = 0;
            this.minutes = 0;
            this.seconds = 0;
            if (this.type == CoordinateType.Lat) { this.position = CoordinatesPosition.N; }
            else { this.position = CoordinatesPosition.E; }
        }
        /// <summary>
        /// Creates a populated CoordinatePart from a decimal format part.
        /// </summary>
        /// <param name="value">Coordinate decimal value</param>
        /// <param name="t">Coordinate type</param>
        /// <param name="c">Parent Coordinate object</param>
        public CoordinatePart(double value, CoordinateType t, Coordinate c)
        {
            this.Parent = c;
            this.type = t;

            if (type == CoordinateType.Long)
            {
                if (value > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal coordinate decimal cannot be greater than 180."); }
                if (value < -180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal coordinate decimal cannot be less than 180."); }
                if (value < 0) { this.position = CoordinatesPosition.W; }
                else { this.position = CoordinatesPosition.E; }
            }
            else
            {
                if (value > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal coordinate decimal cannot be greater than 90."); }
                if (value < -90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal coordinate decimal cannot be less than 90."); }
                if (value < 0) { this.position = CoordinatesPosition.S; }
                else { this.position = CoordinatesPosition.N; }
            }
            decimal dd = Convert.ToDecimal(value);
            dd = Math.Abs(dd);
            decimal ddFloor = Math.Floor(dd);//DEGREE
            decimal dm = dd - ddFloor;
            dm *= 60; //DECIMAL MINUTE
            decimal dmFloor = Math.Floor(dm); //MINUTES
            decimal sec = dm - dmFloor;
            sec *= 60;//SECONDS


            this.decimalDegree = value;
            this.degrees = Convert.ToInt32(ddFloor);
            this.minutes = Convert.ToInt32(dmFloor);
            this.decimalMinute = Convert.ToDouble(dm);
            this.seconds = Convert.ToDouble(sec);
        }
        /// <summary>
        /// Creates a populated CoordinatePart object from a Degrees Minutes Seconds part.
        /// </summary>
        /// <param name="deg">Degrees</param>
        /// <param name="min">Minutes</param>
        /// <param name="sec">Seconds</param>
        /// <param name="pos">Coordinate Part Position</param>
        /// <param name="c">Parent Coordinate</param>
        public CoordinatePart(int deg, int min, double sec, CoordinatesPosition pos, Coordinate c)
        {
            this.Parent = c;
            if (pos == CoordinatesPosition.N || pos == CoordinatesPosition.S) { this.type = CoordinateType.Lat; }
            else { this.type = CoordinateType.Long; }

            if (deg < 0) { throw new ArgumentOutOfRangeException("Degrees out of range", "Degrees cannot be less than 0."); }
            if (min < 0) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be less than 0."); }
            if (sec < 0) { throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be less than 0."); }
            if (min >= 60) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be greater than or equal to 60."); }
            if (sec >= 60) { throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be greater than or equal to 60."); }
            this.degrees = deg;
            this.minutes = min;
            this.seconds = sec;
            this.position = pos;

            decimal secD = Convert.ToDecimal(sec);
            secD /= 60; //Decimal Seconds
            decimal minD = Convert.ToDecimal(min);
            minD += secD; //Decimal Minutes

            if (type == CoordinateType.Long)
            {           
                if (deg + (minD / 60) > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal Degrees cannot be greater than 180."); }
            }
            else
            {
                if (deg + (minD / 60) > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal Degrees cannot be greater than 90."); }
            }
            this.decimalMinute = Convert.ToDouble(minD);
            decimal dd = Convert.ToDecimal(deg) + (minD / 60);


            if (pos == CoordinatesPosition.S || pos == CoordinatesPosition.W)
            {
                dd *= -1;
            }
            this.decimalDegree = Convert.ToDouble(dd);
        }
        /// <summary>
        /// Creates a populated CoordinatePart from a Degrees Minutes Seconds part.
        /// </summary>
        /// <param name="deg">Degrees</param>
        /// <param name="minSec">Decimal Minutes</param> 
        /// <param name="pos">Coordinate Part Position</param>
        /// <param name="c">Parent Coordinate object</param>
        public CoordinatePart(int deg, double minSec, CoordinatesPosition pos, Coordinate c)
        {
            this.Parent = c;
         
            if (pos == CoordinatesPosition.N || pos == CoordinatesPosition.S) { this.type = CoordinateType.Lat; }
            else { this.type = CoordinateType.Long; }

            if (deg < 0) { throw new ArgumentOutOfRangeException("Degree out of range", "Degree cannot be less than 0."); }
            if (minSec < 0) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be less than 0."); }

            if (minSec >= 60) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be greater than or equal to 60."); }

            if (this.type == CoordinateType.Lat)
            {
                if (deg + (minSec / 60) > 90) { throw new ArgumentOutOfRangeException("Degree out of range", "Latitudinal degrees cannot be greater than 90."); }
            }
            else
            {
                if (deg + (minSec / 60) > 180) { throw new ArgumentOutOfRangeException("Degree out of range", "Longitudinal degrees cannot be greater than 180."); }
            }
            this.degrees = deg;
            this.decimalMinute = minSec;
            this.position = pos;

            decimal minD = Convert.ToDecimal(minSec);
            decimal minFloor = Math.Floor(minD);
            this.minutes = Convert.ToInt32(minFloor);
            decimal sec = minD - minFloor;
            sec *= 60;
            decimal secD = Convert.ToDecimal(sec);
            this.seconds = Convert.ToDouble(secD);
            decimal dd = deg + (minD / 60);

            if (pos == CoordinatesPosition.S || pos == CoordinatesPosition.W)
            {
                dd *= -1;
            }
            this.decimalDegree = Convert.ToDouble(dd);
        }

        /// <summary>
        /// Signed degrees (decimal) format coordinate.
        /// </summary>
        /// <returns>double</returns>
        public double ToDouble()
        {
            return this.decimalDegree;
        }

        /// <summary>
        /// Overridden Coordinate ToString() method
        /// </summary>
        /// <returns>Dstring</returns>
        public override string ToString()
        {
            return FormatString(this.Parent.FormatOptions);
        }
       
        /// <summary>
        /// Formatted CoordinatePart string.
        /// </summary>
        /// <param name="options">CoordinateFormatOptions</param>
        /// <returns>string (formatted)</returns>
        public string ToString(CoordinateFormatOptions options)
        {
            return FormatString(options);
        }
       /// <summary>
        /// String formatting logic
        /// </summary>
        /// <param name="options">CoordinateFormatOptions</param>
        /// <returns>Formatted coordinate part string</returns>
        private string FormatString(CoordinateFormatOptions options)
        {
            ToStringType type = ToStringType.Degree_Minute_Second;
            int? rounding = null;
            bool lead = false;
            bool trail = false;
            bool hyphen = false;
            bool symbols = true;
            bool degreeSymbol = true;
            bool minuteSymbol = true;
            bool secondsSymbol = true;
            bool positionFirst = true;           

            #region Assign Formatting Rules
            switch (options.Format)
            {
                case CoordinateFormatType.Degree_Minutes_Seconds:
                    type = ToStringType.Degree_Minute_Second;
                    break;
                case CoordinateFormatType.Degree_Decimal_Minutes:
                    type = ToStringType.Degree_Decimal_Minute;
                    break;
                case CoordinateFormatType.Decimal_Degree:
                    type = ToStringType.Decimal_Degree;
                    break;
                case CoordinateFormatType.Decimal:
                    type = ToStringType.Decimal;
                    break;
                default:
                    type = ToStringType.Degree_Minute_Second;
                    break;
            }
            rounding = options.Round;
            lead = options.Display_Leading_Zeros;
            trail = options.Display_Trailing_Zeros;
            symbols = options.Display_Symbols;
            degreeSymbol = options.Display_Degree_Symbol;
            minuteSymbol = options.Display_Minute_Symbol;
            secondsSymbol = options.Display_Seconds_Symbol;
            hyphen = options.Display_Hyphens;
            positionFirst = options.Position_First;                     
            #endregion

            switch (type)
            {
                case ToStringType.Decimal_Degree:
                    if (rounding == null) { rounding = 6; }
                    return ToDecimalDegreeString(rounding.Value, lead, trail, symbols, degreeSymbol, positionFirst, hyphen);
                case ToStringType.Degree_Decimal_Minute:
                    if (rounding == null) { rounding = 3; }
                    return ToDegreeDecimalMinuteString(rounding.Value, lead, trail, symbols, degreeSymbol, minuteSymbol, hyphen, positionFirst);
                case ToStringType.Degree_Minute_Second:
                    if (rounding == null) { rounding = 3; }
                    return ToDegreeMinuteSecondString(rounding.Value, lead, trail, symbols, degreeSymbol, minuteSymbol, secondsSymbol, hyphen, positionFirst);
                case ToStringType.Decimal:
                    if (rounding == null) { rounding = 9; }
                    double dub = this.ToDouble();
                    dub = Math.Round(dub, rounding.Value);
                    string lt = Leading_Trailing_Format(lead, trail, rounding.Value, Position);
                    return string.Format(lt, dub);
            }

            return string.Empty;
        }
        //DMS Coordinate Format
        private string ToDegreeMinuteSecondString(int rounding, bool lead, bool trail, bool symbols, bool degreeSymbol, bool minuteSymbol, bool secondSymbol, bool hyphen, bool positionFirst)
        {

            string leadString = Leading_Trailing_Format(lead, false, rounding, Position);
            string d = string.Format(leadString, Degrees); // Degree String
            string minute;
            if (lead) { minute = string.Format("{0:00}", Minutes); }
            else { minute = Minutes.ToString(); }
            string leadTrail = Leading_Trailing_Format(lead, trail, rounding);

            double sc = Math.Round(Seconds, rounding);
            string second = string.Format(leadTrail, sc);
            string hs = " ";
            string ds = "";
            string ms = "";
            string ss = "";
            if (symbols)
            {
                if (degreeSymbol) { ds = "º"; }
                if (minuteSymbol) { ms = "'"; }
                if (secondSymbol) { ss = "\""; }
            }
            if (hyphen) { hs = "-"; }

            if (positionFirst) { return Position.ToString() + hs + d + ds + hs + minute + ms + hs + second + ss; }
            else { return d + ds + hs + minute + ms + hs + second + ss + hs + Position.ToString(); }
        }
        //DDM Coordinate Format
        private string ToDegreeDecimalMinuteString(int rounding, bool lead, bool trail, bool symbols, bool degreeSymbol, bool minuteSymbol, bool hyphen, bool positionFirst)
        {
            string leadString = "{0:0";
            if (lead)
            {
                if (Position == CoordinatesPosition.E || Position == CoordinatesPosition.W)
                {
                    leadString += "00";
                }
                else
                {
                    leadString += "0";
                }
            }
            leadString += "}";
            string d = string.Format(leadString, Degrees); // Degree String

            string leadTrail = "{0:0";
            if (lead)
            {
                leadTrail += "0";
            }
            leadTrail += ".";
            if (trail)
            {
                for (int i = 0; i < rounding; i++)
                {
                    leadTrail += "0";
                }
            }
            else
            {
                leadTrail += "#########";
            }
            leadTrail += "}";

            double ns = Seconds / 60;
            double c = Math.Round(Minutes + ns, rounding);
            if(c == 60 && Degrees+1 <91) { c = 0;d = string.Format(leadString, Degrees + 1); }//Adjust for rounded maxed out Seconds. will Convert 42 60.0 to 43
            string ms = string.Format(leadTrail, c);
            string hs = " ";
            string ds = "";
            string ss = "";
            if (symbols)
            {
                if (degreeSymbol) { ds = "º"; }
                if (minuteSymbol) { ss = "'"; }
            }
            if (hyphen) { hs = "-"; }

            if (positionFirst) { return Position.ToString() + hs + d + ds + hs + ms + ss; }
            else { return d + ds + hs + ms + ss + hs + Position.ToString(); }

        }
        ////DD Coordinate Format
        private string ToDecimalDegreeString(int rounding, bool lead, bool trail, bool symbols, bool degreeSymbol, bool positionFirst, bool hyphen)
        {
            string degreeS = "";
            string hyph = " ";
            if (degreeSymbol) { degreeS = "º"; }
            if (!symbols) { degreeS = ""; }
            if (hyphen) { hyph = "-"; }

            string leadTrail = "{0:0";
            if (lead)
            {
                if (Position == CoordinatesPosition.E || Position == CoordinatesPosition.W)
                {
                    leadTrail += "00";
                }
                else
                {
                    leadTrail += "0";
                }
            }
            leadTrail += ".";
            if (trail)
            {
                for (int i = 0; i < rounding; i++)
                {
                    leadTrail += "0";
                }
            }
            else
            {
                leadTrail += "#########";
            }
            leadTrail += "}";

            double result = (Degrees) + (Convert.ToDouble(Minutes)) / 60 + (Convert.ToDouble(Seconds)) / 3600;
            result = Math.Round(result, rounding);
            string d = string.Format(leadTrail, Math.Abs(result));
            if (positionFirst) { return Position.ToString() + hyph + d + degreeS; }
            else { return d + degreeS + hyph + Position.ToString(); }

        }

        private string Leading_Trailing_Format(bool isLead, bool isTrail, int rounding, CoordinatesPosition? p = null)
        {
            string leadString = "{0:0";
            if (isLead)
            {
                if (p != null)
                {
                    if (p.Value == CoordinatesPosition.W || p.Value == CoordinatesPosition.E)
                    {
                        leadString += "00";
                    }
                }
                else
                {
                    leadString += "0";
                }
            }

            leadString += ".";
            if (isTrail)
            {
                for (int i = 0; i < rounding; i++)
                {
                    leadString += "0";
                }
            }
            else
            {
                leadString += "#########";
            }

            leadString += "}";
            return leadString;

        }

        private string FormatError(string argument, string rule)
        {
            return "'" + argument + "' is not a valid argument for string format rule: " + rule + ".";
        }

        private enum ToStringType
        {
            Decimal_Degree, Degree_Decimal_Minute, Degree_Minute_Second, Decimal
        }
        /// <summary>
        /// Notify the correct properties and parent properties.
        /// </summary>
        /// <param name="p">Property Type</param>
        private void NotifyProperties(PropertyTypes p)
        {
            switch (p)
            {
                case PropertyTypes.DecimalDegree:
                    this.NotifyPropertyChanged("DecimalDegree");
                    this.NotifyPropertyChanged("DecimalMinute");
                    this.NotifyPropertyChanged("Degrees");
                    this.NotifyPropertyChanged("Minutes");
                    this.NotifyPropertyChanged("Seconds");
                    this.NotifyPropertyChanged("Position");
                    break;
                case PropertyTypes.DecimalMinute:
                    this.NotifyPropertyChanged("DecimalDegree");
                    this.NotifyPropertyChanged("DecimalMinute");
                    this.NotifyPropertyChanged("Minutes");
                    this.NotifyPropertyChanged("Seconds");
                    break;
                case PropertyTypes.Degree:
                    this.NotifyPropertyChanged("DecimalDegree");
                    this.NotifyPropertyChanged("Degree");
                    break;
                case PropertyTypes.Minute:
                    this.NotifyPropertyChanged("DecimalDegree");
                    this.NotifyPropertyChanged("DecimalMinute");
                    this.NotifyPropertyChanged("Minutes");
                    break;
                case PropertyTypes.Position:
                    this.NotifyPropertyChanged("DecimalDegree");
                    this.NotifyPropertyChanged("Position");
                    break;
                case PropertyTypes.Second:
                    this.NotifyPropertyChanged("DecimalDegree");
                    this.NotifyPropertyChanged("DecimalMinute");
                    this.NotifyPropertyChanged("Seconds");
                    break;
                default:
                    this.NotifyPropertyChanged("DecimalDegree");
                    this.NotifyPropertyChanged("DecimalMinute");
                    this.NotifyPropertyChanged("Degrees");
                    this.NotifyPropertyChanged("Minutes");
                    this.NotifyPropertyChanged("Seconds");
                    this.NotifyPropertyChanged("Position");
                    break;
            }
            this.NotifyPropertyChanged("Display");
            this.Parent.NotifyPropertyChanged("Display");
            this.Parent.NotifyPropertyChanged("CelestialInfo");
            this.Parent.NotifyPropertyChanged("UTM");
            this.Parent.NotifyPropertyChanged("MGRS");
            this.Parent.NotifyPropertyChanged("Cartesian");
            this.Parent.NotifyPropertyChanged("ECEF");

        }

        /// <summary>
        /// Property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Notify property changed
        /// </summary>
        /// <param name="propName">Property name</param>
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        /// <summary>
        /// Used for notifying the correct properties.
        /// </summary>
        private enum PropertyTypes
        {
            DecimalDegree, DecimalMinute, Position, Degree, Minute, Second, FormatChange
        }

        /// <summary>
        /// Returns CoordinatePart in radians
        /// </summary>
        /// <returns></returns>
        public double ToRadians()
        {
            return decimalDegree * Math.PI / 180;
        }
        /// <summary>
        /// Attempts to parse a string into a CoordinatePart.
        /// </summary>
        /// <param name="s">CoordinatePart string</param>
        /// <param name="cp">CoordinatePart</param>
        /// <returns>boolean</returns>
        /// <example>
        /// <code>
        /// CoordinatePart cp;
        /// if(CoordinatePart.TryParse("N 32.891º", out cp))
        /// {
        ///     Console.WriteLine(cp); //N 32º 53' 28.212"
        /// }
        /// </code>
        /// </example>
        public static bool TryParse(string s, out CoordinatePart cp)
        {
            cp = null;
            
            if (FormatFinder_CoordPart.TryParse(s, out cp))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Attempts to parse a string into a CoordinatePart. 
        /// </summary>
        /// <param name="s">CoordinatePart string</param>
        /// <param name="t">CoordinateType</param>
        /// <param name="cp">CoordinatePart</param>
        /// <returns>boolean</returns>
        /// <example>
        /// <code>
        /// CoordinatePart cp;
        /// if(CoordinatePart.TryParse("-32.891º", CoordinateType.Long, out cp))
        /// {
        ///     Console.WriteLine(cp); //W 32º 53' 27.6"
        /// }
        /// </code>
        /// </example>
        public static bool TryParse(string s, CoordinateType t, out CoordinatePart cp)
        {
            cp = null;
            //Comma at beginning parses to long
            //Asterik forces lat
            if(t== CoordinateType.Long) { s = "," + s; }
            else { s = "*" + s; }
            if (FormatFinder_CoordPart.TryParse(s, out cp))
            {
                return true;
            }
            return false;
        }

    }
}
