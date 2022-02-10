/*
author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a RGB color as percents
*/


using System;
using OpenGLDotNet;

public class Color
{
    //TODO DEFINE STATIC CONSTANTS FOR SPHERE COLOR, FLOOR COLOR, AND BACKGROUND COLOR
    const int COLOR_MAX = 255;

    private double _r;
    private double _g;
    private double _b;

    public double r { get => this._r; set => this._r = value; }
    public double g { get => this._g; set => this._g = value; }
    public double b { get => this._b; set => this._b = value; }

    //default constructor.. THE BACKGROUND COLOR
    public Color() 
    {
        this._r = 0;
        this._g = 1;
        this._b = 0;
    }

    public Color( double r, double g, double b)
    {
        this._r = r;
        this._g = g;
        this._b = b;
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