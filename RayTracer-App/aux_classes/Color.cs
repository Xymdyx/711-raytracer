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
    const int COLOR_MAX = 255;

    public static Color whiteSpecular = new Color( 1f, 1f , 1f);
    public static Color defaultBlack = new Color( 0f, 0f, 0f );
    public static Color bgColor = new Color( 0.148f, 0.661f, 0.740f ); //0, 1f, 0
    public static Color sphereColor = new Color( 0.0f, 0.0f, 1.0f );
    public static Color floorColor = new Color( 1.0f, 0.0f, 0.0f );
    public static Color bunnyColor = new Color( 0.770f, 0.590f, 0.354f );


    private float _r;
    private float _g;
    private float _b;

    public float r { get => this._r; set => this._r = value; }
    public float g { get => this._g; set => this._g = value; }
    public float b { get => this._b; set => this._b = value; }


    //default constructor.. THE BACKGROUND COLOR
    public Color() 
    {
        this._r = 0;
        this._g = 1;
        this._b = 0;
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
}