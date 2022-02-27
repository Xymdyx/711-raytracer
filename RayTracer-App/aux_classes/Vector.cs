/*
 author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a 3d vector
*/

using System;
//CONVERTED DOUBLE -> FLOAT!
public class Vector
{
    private static Vector ZERO_VEC = new Vector( 0, 0, 0 );

    // fields
    private float _v1;
    private float _v2;
    private float _v3;

    //properties
    public float v1 { get => this._v1; set => this._v1 = value; }
    public float v2 { get => this._v2; set => this._v2 = value; }
    public float v3 { get => this._v3; set => this._v3 = value; }

// default constructor
    public Vector()
    {
        this.v1 = 1;
        this.v2 = 1;
        this.v3 = 1;
        this.normalize();
    }

// constructor
    public Vector(float v1, float v2, float v3, bool normalize = true)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        if( normalize ) this.normalize();
    }

//operator overloads + and -
    public static Vector operator +(Vector vec1) => new Vector( (vec1.v1), (vec1.v2), (vec1.v3) );
    public static Vector operator -(Vector vec1) => new Vector(-(vec1.v1), -(vec1.v2), -(vec1.v3));

    public static Vector operator +(Vector vec1, Vector vec2) => new Vector(vec1.v1 + vec2.v1, vec1.v2 + vec2.v2, vec1.v3 + vec2.v3);
    public static Vector operator -(Vector vec1, Vector vec2) => vec1 + -vec2;

    public static bool operator ==( Vector lhs, Vector rhs ) 
    {
        if (lhs is null)
        {
            if (rhs is null)
            {
                // null == null = true.
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles the case of null on right side.
        return lhs.Equals( rhs );
    }

    public static bool operator !=( Vector lhs, Vector rhs ) => !(lhs == rhs);


    // self-operations
    public Vector scale(float k)
    {
        return new Vector(k * this.v1, k * this.v2, k * this.v3, false);
    }

    public float getLen()
    {
        return (float) Math.Sqrt( (this.v1 * this.v1 ) + (this.v2 * this.v2) + (this.v3 * this.v3) );
    }

    // normalize this vector to have 1 length
    public void normalize()
    {
        float magn = getLen();
        if (magn != 0)
        { 
            this.v1 /= magn;
            this.v2 /= magn;
            this.v3 /= magn;
        }
        return;
    }

    //operators with points

    //operators with other vecs

    /*
     * return dot product scalar value
     *  vec2:  the vector on the RHS of tthe dot product (commutative)
     */
    public float dotProduct( Vector vec2 )
    {

        return ((this.v1 * vec2.v1) + (this.v2 * vec2.v2) + (this.v3 * vec2.v3));
    }


    /* 
     * return the cosine between this vector and vec2 in degrees
     * vec2: the vector on the RHS of tthe operation
     */
    public float cosineBetween(Vector vec2)
    {
        return (float) Math.Acos(this.dotProduct(vec2) /(this.getLen() * vec2.getLen()) * (180 / Math.PI));
    }


    /*
     * return the perpendicular vector betwn this vector and vec2
     * vec2: the vector on the RHS of tthe Cross product (order matters)
     */
    public Vector crossProduct(Vector vec2, bool normalize = true) //fixed issue w second term...
    {
        return new Vector((this.v2 * vec2.v3) - (this.v3 * vec2.v2),
                       (this.v3 * vec2.v1) - ( this.v1 * vec2.v3) ,
                       (this.v1 * vec2.v2) - (this.v2 * vec2.v1) , normalize);
    }

    /*
     * projection of v (vec2) onto  u (this vec)
     */
    public Vector projectOnto(Vector vec2)
    {
        float magn = this.getLen();
        return this.scale( (float) (dotProduct(vec2) / (magn * magn) ));
    }

    /*
     * vector comp of v perp. to u
     */
    public Vector perpTo(Vector vec2)
    {
        return vec2 - projectOnto(vec2);
    }

    //used to determine if two vectors are equal
    public override bool Equals( Object obj )
	{
        if ( (obj == null) || !(this.GetType().Equals( obj.GetType())) )
            return false;

        Vector v = (Vector) obj;
        return ( (v.v1 == this.v1) && (v.v2 == this.v2) && (v.v3 == this.v3) );
	}

    // vec ops without normalizing
    public Vector addVec( Vector vec2 )
    {
        return new Vector( this.v1 + vec2.v1, this.v2 + vec2.v2, this.v3 + vec2.v3, false );
    }

    // vec ops without normalizing
    public Vector subVec( Vector vec2 )
    {
        return new Vector( this.v1 - vec2.v1, this.v2 - vec2.v2, this.v3 - vec2.v3, false );
    }

    //TODO... may cause problems
    public override int GetHashCode()
    {
        int hash = (int) v1 >> 2 + (int) v2 >> 2 + (int) v3 >> 2;
        return hash;
    }

    public bool isZeroVector()
	{
        return ((0 == this.v1) && (0 == this.v2) && (0 == this.v3));
    }

    //TOSTRING METHOD
    public override string ToString()
    {
        return $"Vector (u1, u2, u3) = ({this.v1}, {this.v2} , {this.v3})\n";
    }

    //REFLECT METHOD
    // reflect = Incoming - 2( (Incoming.dot(normal) * normal) / (normalLength^2) )
    public static Vector reflect( Vector incoming, Vector normal )
	{
        
        Vector rightTerm =  normal.scale( incoming.dotProduct( normal ) ); //does not normalize here
        float len = rightTerm.getLen();
        rightTerm = rightTerm.scale( (1 / (len * len)) ); //fixed
        rightTerm = rightTerm.scale( 2f );

        return incoming - rightTerm;
	}

    /*Console.WriteLine( "Creating vector" );

    Vector tVec = new Vector( 5.0, 6.0, 7.0 );
    Vector fVec = new Vector( 5.9, 6.8, 2.1 );
    Vector cpVec = tVec.crossProduct( fVec );
    float dpVal = tVec.dotProduct( fVec );

    Console.WriteLine(tVec );
    Console.WriteLine(fVec );
    Console.WriteLine( $" The dot product of {tVec} and {fVec} gives dot product {dpVal}" );
    Console.WriteLine( $" The cross product of {tVec} x {fVec} gives dot product {cpVec}" );

    Console.WriteLine( $"Adding both vecs: {tVec + fVec}" );
    Console.WriteLine( $"Subtracting fVec from tVec: {tVec - fVec}" );
    Console.WriteLine( $"Subtracting tVec from fVec: {fVec - tVec}" );*/

/*
Console.WriteLine( "Creating non-normalized vector" );

Vector tVec = new Vector( 5.0f, 6.0f, 7.0f, false ); //normalize works
Vector fVec = new Vector( 5.9f, 6.8f, 2.1f, false );
Vector cpVec = tVec.crossProduct( fVec, false );
Vector ftVec = fVec.crossProduct( tVec, false );

float dpVal = tVec.dotProduct( fVec );

Console.WriteLine(tVec );
Console.WriteLine(fVec );
Console.WriteLine( $" The dot product of {tVec} and {fVec} gives dot product {dpVal}" );
Console.WriteLine( $" The cross product of {tVec} x {fVec} gives dot product {cpVec}" );
Console.WriteLine( $" The cross product of {fVec} x {tVec} gives dot product {ftVec}" );

Console.WriteLine( $"Adding both vecs: {tVec.addVec(fVec) }" );
Console.WriteLine( $"Subtracting fVec from tVec: { tVec.subVec(fVec)}" );
Console.WriteLine( $"Subtracting tVec from fVec: { fVec.subVec(tVec) }" );*/
}
