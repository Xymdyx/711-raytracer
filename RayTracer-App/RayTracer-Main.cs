using System;
using OpenGLDotNet;
using OpenGLDotNet; // for draw pixels
using System.Runtime.InteropServices; //for GCHandle
using RayTracer_App.World;
using RayTracer_App.Camera;
using RayTracer_App.Scene_Objects;
using System.Collections.Generic;
using System.Numerics;

//DOUBLE -> FLOAT

//https://matrix.reshish.com/multiplication.php 
public class RayTracerMain
{
	static int imageWidth;
	static int imageHeight;
	static IntPtr colsPtr;
	//static GCHandle colsHandle;
	static bool[] valid = new bool[1];
	static float[] rat = new float[4];

	//OPENGL DRAW CCW order matters
	public static void doRayTracing() 
	{
		// drawPixels in the RGB array with glDrawPixels();... put this in main
		imageWidth = 1920;
		imageHeight = 1080;
		float focalLen = .5f; //distance from camera to film plane center along N... the higher, the further away it is


		//initialize objects
		float s1Depth = 5.0f;
		float s2Depth = 6.5f;
		float floorDept = 8.5f;

		//list triangles in CCW ORDER!
		List<Point> triVerts1 = new List<Point> { new Point( 0, 0, floorDept ), new Point( 0, 1, floorDept ), new Point( -1, 0, floorDept ) }; //ccw from point that forms the right angle
		List<Point> triVerts2 = new List<Point> {  new Point( -1, -1, floorDept ), new Point( -1, -2, floorDept ), new Point( 0, -1, floorDept )  }; //ccw manner.... positive is up, down is negative

		Polygon triangle1 = new Polygon( triVerts1 );
		Polygon triangle2 = new Polygon( triVerts2 );

		Sphere sphere1 = new Sphere( new Point( 0, 1.5f, s1Depth) ,2.5f );
		Sphere sphere2 = new Sphere( new Point( 3.5f, 1.75f, s2Depth ), 2.0f );

		World world = new World();
		world.add( triangle1 );
		world.add( triangle2 );
		world.add( sphere1 );
		world.add( sphere2 );

		// initialize camera and render world
		Camera cam = new Camera( new Vector( 0f, 1f, 0f ), new Point( 4f, 1f, 5f ), new Point( 0f, 2.0f, 5.0f ) );

		// ditto with floats from 0-1 and 0-255, uint, now try byte
		byte[] pixColors = cam.render( world, imageHeight, imageWidth, focalLen );

		unsafe
		{
			fixed (byte* colArrPtr = pixColors) { colsPtr = new IntPtr( (void*)colArrPtr ); }
		}

		return;

	}
	public static void display()
	{
		//put in display function
		GL.ClearColor( 0, 0, 0, 1 );
		GL.Clear( GL.GL_COLOR_BUFFER_BIT );

		GL.PixelStorei( GL.GL_UNPACK_ALIGNMENT, 1);
		GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		GL.GetFloatv( GL.GL_CURRENT_RASTER_POSITION, rat );
		Console.WriteLine( $" {rat[0]} , {rat[1]}, {rat[2]} is valid: {valid[0]}" ); //print out default { 0,0,0}"
		GL.DrawPixels( imageWidth, imageHeight, GL.GL_RGB, GL.GL_UNSIGNED_BYTE, colsPtr );

		FG.SwapBuffers();
		return;
	}

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
		doRayTracing();

		//begin OpenGl
		int[] argc = new int[1]; argc[0] = 0; string[] argv = null;
		FG.Init( argc, argv );
		FG.InitDisplayMode( GLUT.GLUT_RGB | GLUT.GLUT_SINGLE | GLUT.GLUT_DEPTH );
		FG.InitWindowSize( 1920, 1080 );
		FG.InitWindowPosition( 0, 0 );
		FG.CreateWindow( "RayTracing CheckPoint 2" );
		GL.Init( true );            //I forgot to call this...
		GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		FG.DisplayFunc( display ); //white screen without this
		FG.MainLoop(); //end
		/*		Matrix4x4 mat1= new Matrix4x4(
					1, 2, 3, 4,
					5, 6, 7, 8,
					9, 10, 11, 12
					, 13, 14, 15, 16 );
				Matrix4x4 mat2 = new Matrix4x4(
					17, 18, 19, 20,
					21, 22, 23, 24,
					25, 26, 27, 28,
					29, 30, 31, 32 );
				Console.WriteLine( $"Test of {mat1} and {mat2} addition:\n {mat1 + mat2}\n " );
				Console.WriteLine( $"Test of {mat1} and {mat2} subtraction:\n {mat1 - mat2} \n" );
				Console.WriteLine( $"Test of {mat1} and {mat2} multiplication:\n {mat1 * mat2} \n" );*/

		return 0;
	}
}