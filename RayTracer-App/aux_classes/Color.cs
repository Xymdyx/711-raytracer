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