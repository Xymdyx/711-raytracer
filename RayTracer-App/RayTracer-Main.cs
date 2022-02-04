using System;


public class RayTracerMain
{
	static int Main( string [] args )
	{
		//	//camera lookat(position, center, up): [ 0.0, 2.0, -7.2], [0, -1.5, 0], [0, 1, 0]
		//	//camera perspective projection(vertical fov, aspect ratio, near, far)): [radians(90.0), 1.0, 1.0, 300.0]

		//	//foreground - sphere size:[5,5,5]
		//	//foreground - sphere location:[1,1.0, -1.0]

		//	//background - sphere size:[5,5,5]
		//	//background - sphere location:[-2.5, -1.75 , 2.0]

		//	//floor - cube size:[25, 2.5, 60]
		//	//floor - cube location:[ -2.0, -6.5, -0.5]

		Console.WriteLine( "Creating vector" );

		Vector tVec = new Vector( 5.0, 6.0, 7.0 );
		Vector fVec = new Vector( 5.9, 6.8, 2.1 );
		Vector cpVec = tVec.crossProduct( fVec );
		double dpVal = tVec.dotProduct( fVec );

		Console.WriteLine( tVec );
		Console.WriteLine( fVec );
		Console.WriteLine( $" The dot product of {tVec} and {fVec} gives dot product {dpVal}" );
		Console.WriteLine( $" The cross product of {tVec} x {fVec} gives dot product {cpVec}" );

		Console.WriteLine( $"Adding both vecs: {tVec + fVec}" );
		Console.WriteLine( $"Subtracting fVec from tVec: {tVec - fVec}" );
		Console.WriteLine( $"Subtracting tVec from fVec: {fVec - tVec}" );

		return 0;
	}
}