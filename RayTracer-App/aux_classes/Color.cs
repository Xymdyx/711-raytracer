/*
author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a RGB color as percents
*/


using System;
using OpenGLDotNet;
//CONVERTED DOUBLE -> FLOAT
public class Color
{
    //TODO DEFINE STATIC CONSTANTS FOR SPHERE COLOR, FLOOR COLOR, AND BACKGROUND COLOR
    //https://antongerdelan.net/colour/
    const int COLOR_MAX = 255;

    //whitted colors
    public static Color whiteSpecular = new Color( 1f, 1f , 1f);
    public static Color defaultBlack = new Color( 0f, 0f, 0f );
    public static Color bgColor = new Color( 0.148f, 0.661f, 0.740f ); //0, 1f, 0
    public static Color sphereColor = new Color( 0.0f, 0.0f, 1.0f );
    public static Color floorColor = new Color( 1.0f, 0.0f, 0.0f );
    public static Color bunnyColor = new Color( 0.770f, 0.590f, 0.354f );

    //cornell box
    public static Color cbBlue = new Color( 0.237f, 0.486f, 0.790f );
    public static Color cbRed = new Color( 0.760f, 0.0988f, 0.187f );
    public static Color cbGrey = new Color( 0.810f, 0.802f, 0.803f ); //0.810f, 0.802f, 0.803f
    public static Color cbGreen = new Color( 0.362f, 0.580f, 0.342f );
    public static Color cbChrome = new Color( 0.690f, 0.669f, 0.669f ); //chrome

    //swimming pool
    public static Color poolWater = new Color( 0.112f, 0.930f, 0.916f );
    public static Color poolBorders = new Color( 0.540f, 0.270f, 0.270f );

    // 0.362, 0.580, 0.342
    // photon colors
    public static Color causticColor = new Color( 0.890f, 0.877f, 0.080f ); //yellow
    public static Color photonColor = new Color( 0.254f, 0.940f, 0.437f ); //green
    public static Color volumetricColor = new Color( 0.871f, 0.254f, 0.940f ); //purple

    private float _r;
    private float _g;
    private float _b;

    public float r { get => this._r; set => this._r = value; }
    public float g { get => this._g; set => this._g = value; }
    public float b { get => this._b; set => this._b = value; }


    //default constructor.. THE BACKGROUND COLOR
    public Color() 
    {
        this._r = Color.bgColor.r;
        this._g = Color.bgColor.g;
        this._b = Color.bgColor.b;
    }

    public Color( float r, float g, float b)
    {
        this._r = r;
        this._g = g;
        this._b = b;
    }

    public static Color operator +( Color c1, Color c2 ) => new Color( c1.r + c2.r, c1.g + c2.g, c1.b + c2.b );
    public static Color operator -( Color c1, Color c2 ) => new Color( c1.r - c2.r, c1.g - c2.g, c1.b - c2.b );

    public static Color operator *(  Color c1, Color c2 ) => new Color( c1.r * c2.r, c1.g * c2.g, c1.b * c2.b );


    //operators
    //scale a Color by k....which ranges from 0-1
    public Color scale( float k ) 
    {
        return new Color( this.r * k, this.g * k, this.b * k );
    }

    public int[] asIntArr()
    {
        return  new int[] { (int) (_r * COLOR_MAX), (int)(_g * COLOR_MAX), (int)(_b * COLOR_MAX) } ;
    }

    public float[] asFloat255Arr()
    {
        return new float[] { (float)(_r * COLOR_MAX), (float)(_g * COLOR_MAX), (float)(_b * COLOR_MAX) };
    }

    public float[] asFloat1Arr()
    {
        return new float[] { (float)(_r ), (float)(_g ), (float)(_b ) };
    }

    public uint[] asUintArr() 
    {
        return new uint[] { (uint)(_r * COLOR_MAX), (uint)(_g * COLOR_MAX), (uint)(_b * COLOR_MAX) };
    }

    public byte[] asByteArr()
    {
        return new byte[] { (byte)(_r * COLOR_MAX), (byte)(_g * COLOR_MAX), (byte)(_b * COLOR_MAX) };
    }

    public override bool Equals( object obj )
	{
		if( obj == null || obj.GetType() != this.GetType()) return false;

        Color c = (Color) obj;
        return (c.r == this.r && c.g == this.g && c.b == this.b);
	}

//debug methods for photon mapping

    public bool whiteOrHigher( )
    {

        return (this.r >= 1 && this.g >= 1 && this.b >= 1);
    }

    public bool isShadowed()
    {

        return (this.r <= 0 && this.g <= 0 && this.b <= 0);
    }

//cp7 tr methods
    public float colVal()
	{
        return this.r + this.g + this.b;
	}

    public override string ToString()
	{
		return $"Color({r}, {g}, {b})";
	}
}