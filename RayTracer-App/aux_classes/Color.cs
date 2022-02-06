/*
author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a RGB color as percents
*/


using System;

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
        this._r = 0.0198;
        this._g = 0.0465;
        this._b = 0.220;
    }

    public Color( double r, double g, double b)
    {
        this._r = r;
        this._g = g;
        this._b = b;
    }

    public int[] asIntArray()
    {
        return  new int[] { (int) (_r * COLOR_MAX), (int)(_g * COLOR_MAX), (int)(_b * COLOR_MAX) } ;
    }
}