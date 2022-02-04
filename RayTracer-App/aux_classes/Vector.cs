/*
 author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a 3d vector
*/

using System;

public class Vector
{


    // fields
    private double _v1;
    private double _v2;
    private double _v3;

    //properties
    public double v1 { get => this._v1; set => this._v1 = value; }
    public double v2 { get => this._v2; set => this._v2 = value; }
    public double v3 { get => this._v3; set => this._v3 = value; }

    // default constructor
    public Vector()
    {
        this.v1 = 1;
        this.v2 = 1;
        this.v3 = 1;
    }

    // constructor
    public Vector(double v1, double v2, double v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;

        this.normalize();
    }

    //operator overloads + and -
    public static Vector operator +(Vector vec1) => new Vector( (vec1.v1), (vec1.v2), (vec1.v3) );
    public static Vector operator -(Vector vec1) => new Vector(-(vec1.v1), -(vec1.v2), -(vec1.v3));

    public static Vector operator +(Vector vec1, Vector vec2) => new Vector(vec1.v1 + vec2.v1, vec1.v2 + vec2.v2, vec1.v3 + vec2.v3);
    public static Vector operator -(Vector vec1, Vector vec2) => new Vector(vec1.v1 - vec2.v1, vec1.v2 - vec2.v2, vec1.v3 - vec2.v3);


    // self-operations
    public Vector scale(double k)
    {
        return new Vector(k * this.v1, k * this.v2, k * this.v3);
    }

    public double getLen()
    {
        return Math.Sqrt(Math.Pow(this.v1, 2) + Math.Pow(this.v2, 2) + Math.Pow(this.v3, 2));
    }

    // normalize this vector to have 1 length
    public void normalize()
    {
        double magn = getLen();
        this.v1 /= magn;
        this.v2 /= magn;
        this.v3 /= magn;

        return;
    }

    //operators with other vecs

    /*
     * return dot product scalar value
     *  vec2:  the vector on the RHS of tthe dot product (commutative)
     */
    public double dotProduct( Vector vec2 )
    {

        return ((this.v1 * vec2.v1) + (this.v2 * vec2.v2) + (this.v3 * vec2.v3));
    }

    /* 
     * return the cosine between this vector and vec2 in degrees
     * vec2: the vector on the RHS of tthe operation
     */
    public double cosineBetween(Vector vec2)
    {
        return Math.Acos(this.dotProduct(vec2) /(this.getLen() * vec2.getLen()) * (180 / Math.PI));
    }


    /*
     * return the perpendicular vector betwn this vector and vec2
     * vec2: the vector on the RHS of tthe Cross product (order matters)
     */
    public Vector crossProduct(Vector vec2)
    {
        return new Vector((this.v2 * vec2.v3) - (this.v3 * vec2.v2),
                       (this.v1 * vec2.v3) - (this.v3 * vec2.v1),
                       (this.v1 * vec2.v2) - (this.v2 * vec2.v1)
                     );
    }

    /*
     * projection of v (vec2) onto  u (this vec)
     */
    public Vector projectOnto(Vector vec2)
    {
        return this.scale(dotProduct(vec2) / Math.Pow(this.getLen(), 2));
    }

    /*
     * vector comp of v perp. to u
     */
    public Vector perpTo(Vector vec2)
    {
        return vec2 - projectOnto(vec2);
    }

    //TOSTRING METHOD
    public override string ToString()
    {
        return $"Vector (u1, u2, u3) = ({this.v1}, {this.v2} , {this.v3})\n";
    }
}
