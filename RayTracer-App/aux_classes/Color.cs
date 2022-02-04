/*
author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a RGB color as percents
*/


using System;


public class Color
{
    const int COLOR_MAX = 255;

    private float r;
    private float g;
    private float b;

    public int[] asIntArray()
    {
        return  new int[] { (int) (r * COLOR_MAX), (int)(g * COLOR_MAX), (int)(b * COLOR_MAX) } ;
    }
}