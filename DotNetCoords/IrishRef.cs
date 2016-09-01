using System;
using DotNetCoords.Datum;

namespace DotNetCoords
{
  /// <summary>
  /// Class to represent an Irish National Grid reference.
  /// <p>
  /// 	<b>Irish National Grid</b><br/>
  /// 	<ul>
  /// 		<li>Projection: Transverse Mercator</li>
  /// 		<li>Reference ellipsoid: Modified Airy</li>
  /// 		<li>Units: metres</li>
  /// 		<li>Origin: 53°30'N, 8°W</li>
  /// 		<li>False co-ordinates of origin: 200000m east, 250000m north</li>
  /// 	</ul>
  /// </p>
  /// </summary>
  public class IrishRef : CoordinateSystem 
  {
    /**
     * The easting in metres relative to the origin of the British National Grid.
     */
    private double _easting;

    /**
     * The northing in metres relative to the origin of the British National Grid.
     */
    private double _northing;

    private const double ScaleFactor = 1.000035;

    private const double FalseOriginLatitude = 53.5;

    private const double FalseOriginLongitude = -8.0;

    private const double FalseOriginEasting = 200000.0;

    private const double FalseOriginNorthing = 250000.0;

    /// <summary>
    /// Create a new Ordnance Survey grid reference using a given easting and
    /// northing. The easting and northing must be in metres and must be relative
    /// to the origin of the British National Grid.
    /// </summary>
    /// <param name="easting">the easting in metres. Must be greater than or equal to 0.0 and
    /// less than 800000.0.</param>
    /// <param name="northing">the northing in metres. Must be greater than or equal to 0.0 and
    /// less than 1400000.0.</param>
    public IrishRef(double easting, double northing) : base (Ireland1965Datum.Instance)
    {
      Easting = easting;
      Northing = northing;
    }

    /// <summary>
    /// Take a string formatted as a six-figure OS grid reference (e.g. "N514131")
    /// and create a new IrishRef object that represents that grid reference. 
    /// </summary>
    /// <param name="gridRef">A string representing a six-figure Irish grid reference
    /// in the form XY123456</param>
    public IrishRef(string gridRef) : base (Ireland1965Datum.Instance)
    {
      // if (ref.matches(""))
      // TODO 2006-02-05 : check format
      char ch = gridRef[0];
      // Thanks to Nick Holloway for pointing out the radix bug here
      int east = int.Parse(gridRef.Substring(1, 3)) * 100;
      int north = int.Parse(gridRef.Substring(4, 3)) * 100;
      if (ch > 73)
        ch--; // Adjust for no I
      double nx = ((ch - 65) % 5) * 100000;
      double ny = (4 - Math.Floor((double)(ch - 65) / 5)) * 100000;

      Easting = east + nx;
      Northing = north + ny;
    }

    /// <summary>
    /// Create an IrishRef object from the given latitude and longitude.
    /// </summary>
    /// <param name="ll">The latitude and longitude.</param>
    public IrishRef(LatLng ll) : base(Ireland1965Datum.Instance)
    {
      ll.ToDatum(Ireland1965Datum.Instance);

      Ellipsoid.Ellipsoid ellipsoid = Datum.ReferenceEllipsoid;
      const double n0 = FalseOriginNorthing;
      const double e0 = FalseOriginEasting;
      double phi0 = Util.ToRadians(FalseOriginLatitude);
      double lambda0 = Util.ToRadians(FalseOriginLongitude);
      double a = ellipsoid.SemiMajorAxis * ScaleFactor;
      double b = ellipsoid.SemiMinorAxis * ScaleFactor;
      double eSquared = ellipsoid.EccentricitySquared;
      double phi = Util.ToRadians(ll.Latitude);
      double lambda = Util.ToRadians(ll.Longitude);
      double n = (a - b) / (a + b);
      double v = a
          * Math.Pow(1.0 - eSquared * Util.SinSquared(phi), -0.5);
      double rho = a * (1.0 - eSquared)
          * Math.Pow(1.0 - eSquared * Util.SinSquared(phi), -1.5);
      double etaSquared = (v / rho) - 1.0;
      double m = b
          * (((1 + n + ((5.0 / 4.0) * n * n) + ((5.0 / 4.0) * n * n * n)) * (phi - phi0))
              - (((3 * n) + (3 * n * n) + ((21.0 / 8.0) * n * n * n))
                  * Math.Sin(phi - phi0) * Math.Cos(phi + phi0))
              + ((((15.0 / 8.0) * n * n) + ((15.0 / 8.0) * n * n * n))
                  * Math.Sin(2.0 * (phi - phi0)) * Math.Cos(2.0 * (phi + phi0))) - (((35.0 / 24.0)
              * n * n * n)
              * Math.Sin(3.0 * (phi - phi0)) * Math.Cos(3.0 * (phi + phi0))));
      double I = m + n0;
      double ii = (v / 2.0) * Math.Sin(phi) * Math.Cos(phi);
      double iii = (v / 24.0) * Math.Sin(phi) * Math.Pow(Math.Cos(phi), 3.0)
          * (5.0 - Util.TanSquared(phi) + (9.0 * etaSquared));
      double iiia = (v / 720.0) * Math.Sin(phi) * Math.Pow(Math.Cos(phi), 5.0)
          * (61.0 - (58.0 * Util.TanSquared(phi)) + Math.Pow(Math.Tan(phi), 4.0));
      double iv = v * Math.Cos(phi);
      double V = (v / 6.0) * Math.Pow(Math.Cos(phi), 3.0)
          * ((v / rho) - Util.TanSquared(phi));
      double vi = (v / 120.0)
          * Math.Pow(Math.Cos(phi), 5.0)
          * (5.0 - (18.0 * Util.TanSquared(phi)) + (Math.Pow(Math.Tan(phi), 4.0))
              + (14 * etaSquared) - (58 * Util.TanSquared(phi) * etaSquared));

      double N = I + (ii * Math.Pow(lambda - lambda0, 2.0))
                 + (iii * Math.Pow(lambda - lambda0, 4.0))
                 + (iiia * Math.Pow(lambda - lambda0, 6.0));
      double e = e0 + (iv * (lambda - lambda0)) + (V * Math.Pow(lambda - lambda0, 3.0))
                 + (vi * Math.Pow(lambda - lambda0, 5.0));

      Easting = e;
      Northing = N;
    }

    /// <summary>
    /// Return a String representation of this Irish grid reference showing the
    /// easting and northing in metres.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current Irish grid reference.
    /// </returns>
    public override string ToString() 
    {
      return "(" + _easting + ", " + _northing + ")";
    }

    /// <summary>
    /// Return a String representation of this Irish grid reference using the
    /// six-figure notation in the form X123456
    /// </summary>
    /// <returns>A string representing this Irish grid reference in six-figure
    /// notation</returns>
    public string ToSixFigureString() 
    {
      int hundredkmE = (int) Math.Floor(_easting / 100000);
      int hundredkmN = (int) Math.Floor(_northing / 100000);
      
      int charOffset = 4 - hundredkmN;
      int index = 65 + (5 * charOffset) + hundredkmE;
      if (index >= 73)
        index++;
      String letter = ((char)index).ToString();

      int e = (int) Math.Floor((_easting - (100000 * hundredkmE)) / 100);
      int n = (int) Math.Floor((_northing - (100000 * hundredkmN)) / 100);
      String es = "" + e;
      if (e < 100)
        es = "0" + es;
      if (e < 10)
        es = "0" + es;
      String ns = "" + n;
      if (n < 100)
        ns = "0" + ns;
      if (n < 10)
        ns = "0" + ns;

      return letter + es + ns;
    }

    /// <summary>
    /// Convert this Irish grid reference to a latitude/longitude pair using the
    /// Ireland 1965 datum. Note that, the LatLng object may need to be converted to the
    /// WGS84 datum depending on the application.
    /// </summary>
    /// <returns>
    /// A LatLng object representing this Irish grid reference using the
    /// Ireland 1965 datum
    /// </returns>
    public override LatLng ToLatLng() 
    {
      const double n0 = FalseOriginNorthing;
      const double e0 = FalseOriginEasting;
      double phi0 = Util.ToRadians(FalseOriginLatitude);
      double lambda0 = Util.ToRadians(FalseOriginLongitude);
      double a = Datum.ReferenceEllipsoid.SemiMajorAxis;
      double b = Datum.ReferenceEllipsoid.SemiMinorAxis;
      double eSquared = Datum.ReferenceEllipsoid.EccentricitySquared;
      double e = _easting;
      double N = _northing;
      double n = (a - b) / (a + b);
      double m;
      double phiPrime = ((N - n0) / (a * ScaleFactor)) + phi0;
      do {
        m = (b * ScaleFactor)
            * (((1 + n + ((5.0 / 4.0) * n * n) + ((5.0 / 4.0) * n * n * n)) * (phiPrime - phi0))
                - (((3 * n) + (3 * n * n) + ((21.0 / 8.0) * n * n * n))
                    * Math.Sin(phiPrime - phi0) * Math.Cos(phiPrime + phi0))
                + ((((15.0 / 8.0) * n * n) + ((15.0 / 8.0) * n * n * n))
                    * Math.Sin(2.0 * (phiPrime - phi0)) * Math
                    .Cos(2.0 * (phiPrime + phi0))) - (((35.0 / 24.0) * n * n * n)
                * Math.Sin(3.0 * (phiPrime - phi0)) * Math
                .Cos(3.0 * (phiPrime + phi0))));
        phiPrime += (N - n0 - m) / (a * ScaleFactor);
      } while ((N - n0 - m) >= 0.001);
      double v = a * ScaleFactor
          * Math.Pow(1.0 - eSquared * Util.SinSquared(phiPrime), -0.5);
      double rho = a * ScaleFactor * (1.0 - eSquared)
          * Math.Pow(1.0 - eSquared * Util.SinSquared(phiPrime), -1.5);
      double etaSquared = (v / rho) - 1.0;
      double vii = Math.Tan(phiPrime) / (2 * rho * v);
      double viii = (Math.Tan(phiPrime) / (24.0 * rho * Math.Pow(v, 3.0)))
          * (5.0 + (3.0 * Util.TanSquared(phiPrime)) + etaSquared - (9.0 * Util
              .TanSquared(phiPrime) * etaSquared));
      double ix = (Math.Tan(phiPrime) / (720.0 * rho * Math.Pow(v, 5.0)))
          * (61.0 + (90.0 * Util.TanSquared(phiPrime)) + (45.0 * Util
              .TanSquared(phiPrime) * Util.TanSquared(phiPrime)));
      double x = Util.Sec(phiPrime) / v;
      double xi = (Util.Sec(phiPrime) / (6.0 * v * v * v))
          * ((v / rho) + (2 * Util.TanSquared(phiPrime)));
      double xii = (Util.Sec(phiPrime) / (120.0 * Math.Pow(v, 5.0)))
          * (5.0 + (28.0 * Util.TanSquared(phiPrime)) + (24.0 * Util
              .TanSquared(phiPrime) * Util.TanSquared(phiPrime)));
      double xiia = (Util.Sec(phiPrime) / (5040.0 * Math.Pow(v, 7.0)))
          * (61.0 + (662.0 * Util.TanSquared(phiPrime))
              + (1320.0 * Util.TanSquared(phiPrime) * Util.TanSquared(phiPrime)) + (720.0
              * Util.TanSquared(phiPrime) * Util.TanSquared(phiPrime) * Util
              .TanSquared(phiPrime)));
      double phi = phiPrime - (vii * Math.Pow(e - e0, 2.0))
                   + (viii * Math.Pow(e - e0, 4.0)) - (ix * Math.Pow(e - e0, 6.0));
      double lambda = lambda0 + (x * (e - e0)) - (xi * Math.Pow(e - e0, 3.0))
                      + (xii * Math.Pow(e - e0, 5.0)) - (xiia * Math.Pow(e - e0, 7.0));

      return new LatLng(Util.ToDegrees(phi), Util.ToDegrees(lambda), 0, Datum);
    }


    /// <summary>
    /// Gets or sets the easting in metres relative to the origin of the Irish Grid.
    /// </summary>
    /// <value>The easting.</value>
    public double Easting 
    {
      get
      {
        return _easting;
      }
      private set
      {
        if (value < 0.0 || value >= 400000.0)
        {
          throw new ArgumentException("Easting (" + value
              + ") is invalid. Must be greather than or equal to 0.0 and "
              + "less than 400000.0.");
        }

        _easting = value;
      }
    }

    /// <summary>
    /// Gets or sets the northing in metres relative to the origin of the Irish
    /// Grid.
    /// </summary>
    /// <value>The northing.</value>
    public double Northing 
    {
      get
      {
        return _northing;
      }
      private set
      {
        if (value < 0.0 || value > 500000.0)
        {
          throw new ArgumentException("Northing (" + value
              + ") is invalid. Must be greather than or equal to 0.0 and less "
              + "than or equal to 500000.0.");
        }

        _northing = value;
      }
    }
  }
}
