using System;
using OpenGLDotNet;
using System.Runtime.InteropServices; //for GCHandle
using RayTracer_App.World;
using RayTracer_App.Camera;
using RayTracer_App.Scene_Objects;
using System.Collections.Generic;
using RayTracer_App.aux_classes;
using RayTracer_App.Illumination_Models;
//DOUBLE -> FLOAT

//Booksmarks... Ctrl + K, Ctrl K to toogle...
// Ctrl + K + N for next..
// Ctrl + N + K for previous
//https://matrix.reshish.com/multiplication.php 
//https://antongerdelan.net/colour/
public class RayTracerMain
{
	static int imageWidth;
	static int imageHeight;
	static IntPtr colsPtr = default;
	//static GCHandle colsHandle;
	static bool[] valid = new bool[1];
	static float[] rat = new float[4];

////////////////////////////////////////////////////////// SET-UP SCENES ////////////////////////////////////////////////////////////////////

	//render the actual swimming pool compartment
	public static void drawPoolCompartment( World world, float cbXLim, float poolX, float cbYLim, float poolZ, float botPoolZ )
	{
		float botPoolY = cbYLim + 1.0f; // the pool's bottom
		float waterPoolY = cbYLim + .05f; // the pool's water

		float waterTrans = 1f;
		float waterRefl = 1f - waterTrans;
		float waterIOR = 1.33f;

		//draw the front wall
		Point frontPoolTL = new Point( -poolX, cbYLim, poolZ ); //front left of plane
		Point frontPoolTR = new Point( poolX, cbYLim, poolZ );
		Point frontPoolBL = new Point( -poolX, botPoolY, poolZ );
		Point frontPoolBR = new Point( poolX, botPoolY, poolZ );

		List<Point> frontPoolVerts1 = new List<Point> { frontPoolTL, frontPoolBL, frontPoolTR };
		List<Point> frontPoolVerts2 = new List<Point> { frontPoolBR.copy(), frontPoolTR.copy(), frontPoolBL.copy() };

		Polygon frontPoolTri1 = new Polygon( frontPoolVerts1, Color.cbChrome, Phong.cornellPhong );
		Polygon frontPoolTri2 = new Polygon( frontPoolVerts2, Color.cbChrome, Phong.cornellPhong );

		//draw the rear wall
		Point rearPoolTL = new Point( -poolX, cbYLim, botPoolZ ); //rear left of plane
		Point rearPoolTR = new Point( poolX, cbYLim, botPoolZ );
		Point rearPoolBL = new Point( -poolX, botPoolY, botPoolZ );
		Point rearPoolBR = new Point( poolX, botPoolY, botPoolZ );

		List<Point> rearPoolVerts1 = new List<Point> { rearPoolTR, rearPoolBL, rearPoolTL };
		List<Point> rearPoolVerts2 = new List<Point> { rearPoolBL.copy(), rearPoolTR.copy(), rearPoolBR.copy() };

		Polygon rearPoolTri1 = new Polygon( rearPoolVerts1, Color.cbChrome, Phong.cornellPhong );
		Polygon rearPoolTri2 = new Polygon( rearPoolVerts2, Color.cbChrome, Phong.cornellPhong );
		//draw the right wall
		Point rightPoolTL = new Point( poolX, cbYLim, poolZ ); //right left of plane
		Point rightPoolTR = new Point( poolX, cbYLim, botPoolZ );
		Point rightPoolBL = new Point( poolX, botPoolY, poolZ );
		Point rightPoolBR = new Point( poolX, botPoolY, botPoolZ );

		List<Point> rightPoolVerts1 = new List<Point> { rightPoolTL, rightPoolBL, rightPoolTR };
		List<Point> rightPoolVerts2 = new List<Point> { rightPoolBR.copy(), rightPoolTR.copy(), rightPoolBL.copy() };

		Polygon rightPoolTri1 = new Polygon( rightPoolVerts1, Color.cbChrome, Phong.cornellPhong );
		Polygon rightPoolTri2 = new Polygon( rightPoolVerts2, Color.cbChrome, Phong.cornellPhong );
		//draw the left wall
		Point leftPoolTL = new Point( -poolX, cbYLim, poolZ ); //left left of plane
		Point leftPoolTR = new Point( -poolX, cbYLim, botPoolZ );
		Point leftPoolBL = new Point( -poolX, botPoolY, poolZ );
		Point leftPoolBR = new Point( -poolX, botPoolY, botPoolZ );

		List<Point> leftPoolVerts1 = new List<Point> { leftPoolTR, leftPoolBL, leftPoolTL };
		List<Point> leftPoolVerts2 = new List<Point> { leftPoolBL.copy(), leftPoolTR.copy(), leftPoolBR.copy() };

		Polygon leftPoolTri1 = new Polygon( leftPoolVerts1, Color.cbChrome, Phong.cornellPhong );
		Polygon leftPoolTri2 = new Polygon( leftPoolVerts2, Color.cbChrome, Phong.cornellPhong );

		//draw the pool's bottom
		Point botPoolTL = new Point( -poolX, botPoolY, poolZ ); //bot bot of plane
		Point botPoolTR = new Point( poolX, botPoolY, poolZ );
		Point botPoolBL = new Point( -poolX, botPoolY, botPoolZ );
		Point botPoolBR = new Point( poolX, botPoolY, botPoolZ );

		List<Point> botPoolVerts1 = new List<Point> { botPoolTL, botPoolBL, botPoolTR };
		List<Point> botPoolVerts2 = new List<Point> { botPoolBR.copy(), botPoolTR.copy(), botPoolBL.copy() };

		Polygon botPoolTri1 = new Polygon( botPoolVerts1, Color.cbChrome, Phong.cornellPhong );
		Polygon botPoolTri2 = new Polygon( botPoolVerts2, Color.cbChrome, Phong.cornellPhong );

		//draw the pool's water.. slightly below the pool borders
		Point waterPoolTL = new Point( -poolX, waterPoolY, poolZ ); //water water of plane
		Point waterPoolTR = new Point( poolX, waterPoolY, poolZ );
		Point waterPoolBL = new Point( -poolX, waterPoolY, botPoolZ );
		Point waterPoolBR = new Point( poolX, waterPoolY, botPoolZ );

		List<Point> waterPoolVerts1 = new List<Point> { waterPoolTL, waterPoolBL, waterPoolTR };
		List<Point> waterPoolVerts2 = new List<Point> { waterPoolBR.copy(), waterPoolTR.copy(), waterPoolBL.copy() };

		Polygon waterPoolTri1 = new Polygon( waterPoolVerts1, Color.poolWater, Phong.cornellBallPhong, waterRefl, waterTrans, waterIOR );
		Polygon waterPoolTri2 = new Polygon( waterPoolVerts2, Color.poolWater, Phong.cornellBallPhong, waterRefl, waterTrans, waterIOR );

		world.addObject( frontPoolTri1 );
		world.addObject( frontPoolTri2 );
		world.addObject( rearPoolTri1 );
		world.addObject( rearPoolTri2 );
		world.addObject( rightPoolTri1 );
		world.addObject( rightPoolTri2 );
		world.addObject( leftPoolTri1 );
		world.addObject( leftPoolTri2 );
		world.addObject( botPoolTri1 );
		world.addObject( botPoolTri2 );
		world.addObject( waterPoolTri1 );
		world.addObject( waterPoolTri2 );
	}


	//helper to make the pool compartment. This consists of 18 triangles
	public static void drawPool( World world, float cbXLim, float cbYLim, float cbZLim )
	{
		////floor border param. 2 triangles.. draw along xz plane as in Whitted.. only difference between top and bottom is the y value
		float poolX = (3 * cbXLim) / 4f;
		float poolZ = cbZLim / 2f;
		float botPoolZ = -(5 * cbZLim / 6f);
		float poolY = cbYLim + .5f;

		// topBorder 
		Point topPoolTL = new Point( -cbXLim, cbYLim, cbZLim ); //top left of plane
		Point topPoolTR = new Point( cbXLim, cbYLim, cbZLim );
		Point topPoolBL = new Point( -cbXLim, cbYLim, poolZ );
		Point topPoolBR = new Point( cbXLim, cbYLim, poolZ );

		List<Point> topPoolVerts1 = new List<Point> { topPoolTL, topPoolBL, topPoolTR };
		List<Point> topPoolVerts2 = new List<Point> { topPoolBR.copy(), topPoolTR.copy(), topPoolBL.copy() };

		Polygon topPoolTri1 = new Polygon( topPoolVerts1, Color.poolBorders, Phong.cornellPhong );
		Polygon topPoolTri2 = new Polygon( topPoolVerts2, Color.poolBorders, Phong.cornellPhong );

		// botBorder 
		Point botPoolTL = new Point( -cbXLim, cbYLim, -cbZLim ); //bot left of plane
		Point botPoolTR = new Point( cbXLim, cbYLim, -cbZLim );
		Point botPoolBL = new Point( -cbXLim, cbYLim, botPoolZ );
		Point botPoolBR = new Point( cbXLim, cbYLim, botPoolZ );

		List<Point> botPoolVerts1 = new List<Point> { botPoolTR, botPoolBL, botPoolTL };
		List<Point> botPoolVerts2 = new List<Point> { botPoolBL.copy(), botPoolTR.copy(), botPoolBR.copy() };

		Polygon botPoolTri1 = new Polygon( botPoolVerts1, Color.poolBorders, Phong.cornellPhong );
		Polygon botPoolTri2 = new Polygon( botPoolVerts2, Color.poolBorders, Phong.cornellPhong );

		// leftBorder
		Point leftPoolTL = new Point( -cbXLim, cbYLim, poolZ ); //skinny left border
		Point leftPoolTR = new Point( -poolX, cbYLim, poolZ );
		Point leftPoolBL = new Point( -cbXLim, cbYLim, botPoolZ );
		Point leftPoolBR = new Point( -poolX, cbYLim, botPoolZ );

		List<Point> leftPoolVerts1 = new List<Point> { leftPoolTL, leftPoolBL, leftPoolTR };
		List<Point> leftPoolVerts2 = new List<Point> { leftPoolBR.copy(), leftPoolTR.copy(), leftPoolBL.copy() };

		Polygon leftPoolTri1 = new Polygon( leftPoolVerts1, Color.poolBorders, Phong.cornellPhong );
		Polygon leftPoolTri2 = new Polygon( leftPoolVerts2, Color.poolBorders, Phong.cornellPhong );

		// rightBorder
		Point rightPoolTL = new Point( cbXLim, cbYLim, poolZ ); //skinny right border
		Point rightPoolTR = new Point( poolX, cbYLim, poolZ );
		Point rightPoolBL = new Point( cbXLim, cbYLim, botPoolZ );
		Point rightPoolBR = new Point( poolX, cbYLim, botPoolZ );

		List<Point> rightPoolVerts1 = new List<Point> { rightPoolTR, rightPoolBL, rightPoolTL };
		List<Point> rightPoolVerts2 = new List<Point> { rightPoolBL.copy(), rightPoolTR.copy(), rightPoolBR.copy() };

		Polygon rightPoolTri1 = new Polygon( rightPoolVerts1, Color.poolBorders, Phong.cornellPhong );
		Polygon rightPoolTri2 = new Polygon( rightPoolVerts2, Color.poolBorders, Phong.cornellPhong );

		world.addObject( topPoolTri1 );
		world.addObject( topPoolTri2 );
		world.addObject( botPoolTri1 );
		world.addObject( botPoolTri2 );
		world.addObject( leftPoolTri1 );
		world.addObject( leftPoolTri2 );
		world.addObject( rightPoolTri1 );
		world.addObject( rightPoolTri2 );

		drawPoolCompartment( world, cbXLim, poolX, cbYLim, poolZ, botPoolZ );
	}


	//The project scene I plan on fleshing out post-class
	public static Camera setupFuturePool( World world, bool includeBunny = false )
	{
		// THESE WEREN'T BEING DRAWN PAST THE FILM PLANE
		float cbXLim = 1.5f; //3
		float cbYLim = 1.5f; //3
		float cbZLim = 1.5f; //3

		drawPool( world, cbXLim, cbYLim, cbZLim );

		//top border param. 2 triangles.. Color is pale grey. Shared w bot and rear
		Point topTL = new Point( -cbXLim, -cbYLim, cbZLim );
		Point topTR = new Point( cbXLim, -cbYLim, cbZLim );
		Point topBL = new Point( -cbXLim, -cbYLim, -cbZLim );
		Point topBR = new Point( cbXLim, -cbYLim, -cbZLim );

		//draw opposite of bottom to make normals face down!
		List<Point> topVerts1 = new List<Point> { topTR, topBL, topTL, };
		List<Point> topVerts2 = new List<Point> { topBL.copy(), topTR.copy(), topBR.copy() };

		Polygon topTri1 = new Polygon( topVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon topTri2 = new Polygon( topVerts2, Color.cbGrey, Phong.cornellPhong );

		//right border param. 2 triangles.. draw along the yz plane... only difference will be the x value. Color is red
		Point rightTL = new Point( cbXLim, -cbYLim, cbZLim );
		Point rightTR = new Point( cbXLim, cbYLim, cbZLim );
		Point rightBL = new Point( cbXLim, -cbYLim, -cbZLim );
		Point rightBR = new Point( cbXLim, cbYLim, -cbZLim );

		List<Point> rightVerts1 = new List<Point> { rightTR, rightBL, rightTL };
		List<Point> rightVerts2 = new List<Point> { rightBL.copy(), rightTR.copy(), rightBR.copy() };
		Polygon rightTri1 = new Polygon( rightVerts1, Color.cbBlue, Phong.cornellPhong );
		Polygon rightTri2 = new Polygon( rightVerts2, Color.cbBlue, Phong.cornellPhong );

		//left border param. 2 triangles... Color is blue
		Point leftTL = new Point( -cbXLim, -cbYLim, -cbZLim );
		Point leftTR = new Point( -cbXLim, -cbYLim, cbZLim );
		Point leftBL = new Point( -cbXLim, cbYLim, -cbZLim );
		Point leftBR = new Point( -cbXLim, cbYLim, cbZLim );

		List<Point> leftVerts1 = new List<Point> { leftTL, leftBL, leftTR };
		List<Point> leftVerts2 = new List<Point> { leftBR.copy(), leftTR.copy(), leftBL.copy() };
		Polygon leftTri1 = new Polygon( leftVerts1, Color.cbRed, Phong.cornellPhong );
		Polygon leftTri2 = new Polygon( leftVerts2, Color.cbRed, Phong.cornellPhong );

		//rear wall border param. 2 triangles... intersection of left&top, right&top, left&bot, right&bot at higher z
		//left border param. 2 triangles... Color is blue
		Point rearTL = new Point( -cbXLim, -cbYLim, cbZLim );
		Point rearTR = new Point( cbXLim, -cbYLim, cbZLim );
		Point rearBL = new Point( -cbXLim, cbYLim, cbZLim );
		Point rearBR = new Point( cbXLim, cbYLim, cbZLim );


		List<Point> rearVerts1 = new List<Point> { rearTL, rearBL, rearTR };
		List<Point> rearVerts2 = new List<Point> { rearBR.copy(), rearTR.copy(), rearBL.copy() };
		Polygon rearTri1 = new Polygon( rearVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon rearTri2 = new Polygon( rearVerts2, Color.cbGrey, Phong.cornellPhong );

		//front wall border param. 2 triangles... intersection of left&top, right&top, left&bot, right&bot at higher z
		//left border param. 2 triangles... Color is blue
		Point frontTL = new Point( -cbXLim, -cbYLim, -cbZLim );
		Point frontTR = new Point( cbXLim, -cbYLim, -cbZLim );
		Point frontBL = new Point( -cbXLim, cbYLim, -cbZLim );
		Point frontBR = new Point( cbXLim, cbYLim, -cbZLim );

		List<Point> frontVerts1 = new List<Point> { frontTR, frontBL, frontTL };
		List<Point> frontVerts2 = new List<Point> { frontBL.copy(), frontTR.copy(), frontBR.copy() };
		Polygon frontTri1 = new Polygon( frontVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon frontTri2 = new Polygon( frontVerts2, Color.cbGrey, Phong.cornellPhong );


		//place mainLight on top wall near its center
		Point ceilLightPos = new Point( 0f, -cbYLim + .25f, 0f ); ; // 0f, -cbYLim + .5f, 0f ... 0f, -2f, s1Depth - 1f... Top left -.5f, -cbXLim + .25f, s1Depth 
		Color ceilLightColor = Color.whiteSpecular;
		LightSource ceilLight = new LightSource( ceilLightPos, ceilLightColor );

		//add world stuff
		world.addLight( ceilLight );
		world.addObject( topTri1 );
		world.addObject( topTri2 );
		world.addObject( rightTri1 );
		world.addObject( rightTri2 );
		world.addObject( leftTri1 );
		world.addObject( leftTri2 );
		world.addObject( rearTri1 );
		world.addObject( rearTri2 );
		world.addObject( frontTri1 );
		world.addObject( frontTri2 );

		Vector up = new Vector( 0f, 1f, 0f );
		Point eyePos = new Point( 0f, .75f / 2f, -2.75f / 2f ); //0f, .5f, -2.75f... facing rear // facing frontBound 0f, .5f, -.75f
		Point lookAt = new Point( 0f, -1.5f / 2f, cbZLim ); //0f, -1.5f, cbZLim...-cbZlim for rear
		Camera cam = new Camera( up, eyePos, lookAt ); //-z = backing up...

		return cam;
	}
	
	//Cornell scene for Photon Mapping
	public static Camera setupCornell( World world, bool includeBunny = false )
	{
		// THESE WEREN'T BEING DRAWN PAST THE FILM PLANE
		float cbXLim = 1.5f; //3
		float cbYLim = 1.5f; //3
		float cbZLim = 1.5f; //3

		//floor border param. 2 triangles.. draw along xz plane as in Whitted.. only difference between top and bottom is the y value
		Point botTL = new Point( -cbXLim, cbYLim, cbZLim );
		Point botTR = new Point( cbXLim, cbYLim, cbZLim );
		Point botBL = new Point( -cbXLim, cbYLim, -cbZLim );
		Point botBR = new Point( cbXLim, cbYLim, -cbZLim );

		List<Point> botVerts1 = new List<Point> { botTL, botBL, botTR };
		List<Point> botVerts2 = new List<Point> { botBR.copy(), botTR.copy(), botBL.copy() };

		Polygon botTri1 = new Polygon( botVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon botTri2 = new Polygon( botVerts2, Color.cbGrey, Phong.cornellPhong );

		//top border param. 2 triangles.. Color is pale grey. Shared w bot and rear
		Point topTL = new Point( -cbXLim, -cbYLim, cbZLim );
		Point topTR = new Point( cbXLim, -cbYLim, cbZLim );
		Point topBL = new Point( -cbXLim, -cbYLim, -cbZLim );
		Point topBR = new Point( cbXLim, -cbYLim, -cbZLim );

		//draw opposite of bottom to make normals face down!
		List<Point> topVerts1 = new List<Point> { topTR, topBL, topTL, };
		List<Point> topVerts2 = new List<Point> { topBL.copy(), topTR.copy(), topBR.copy() };

		Polygon topTri1 = new Polygon( topVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon topTri2 = new Polygon( topVerts2, Color.cbGrey, Phong.cornellPhong );

		//right border param. 2 triangles.. draw along the yz plane... only difference will be the x value. Color is red
		Point rightTL = new Point( cbXLim, -cbYLim, cbZLim );
		Point rightTR = new Point( cbXLim, cbYLim, cbZLim );
		Point rightBL = new Point( cbXLim, -cbYLim, -cbZLim );
		Point rightBR = new Point( cbXLim, cbYLim, -cbZLim );

		List<Point> rightVerts1 = new List<Point> { rightTR, rightBL, rightTL };
		List<Point> rightVerts2 = new List<Point> { rightBL.copy(), rightTR.copy(), rightBR.copy() };
		Polygon rightTri1 = new Polygon( rightVerts1, Color.cbBlue, Phong.cornellPhong );
		Polygon rightTri2 = new Polygon( rightVerts2, Color.cbBlue, Phong.cornellPhong );

		//left border param. 2 triangles... Color is blue
		Point leftTL = new Point( -cbXLim, -cbYLim, -cbZLim );
		Point leftTR = new Point( -cbXLim, -cbYLim, cbZLim );
		Point leftBL = new Point( -cbXLim, cbYLim, -cbZLim );
		Point leftBR = new Point( -cbXLim, cbYLim, cbZLim );

		List<Point> leftVerts1 = new List<Point> { leftTL, leftBL, leftTR };
		List<Point> leftVerts2 = new List<Point> { leftBR.copy(), leftTR.copy(), leftBL.copy() };
		Polygon leftTri1 = new Polygon( leftVerts1, Color.cbRed, Phong.cornellPhong );
		Polygon leftTri2 = new Polygon( leftVerts2, Color.cbRed, Phong.cornellPhong );

		//rear wall border param. 2 triangles... intersection of left&top, right&top, left&bot, right&bot at higher z
		//left border param. 2 triangles... Color is blue
		Point rearTL = new Point( -cbXLim, -cbYLim, cbZLim );
		Point rearTR = new Point( cbXLim, -cbYLim, cbZLim );
		Point rearBL = new Point( -cbXLim, cbYLim, cbZLim );
		Point rearBR = new Point( cbXLim, cbYLim, cbZLim );


		List<Point> rearVerts1 = new List<Point> { rearTL, rearBL, rearTR };
		List<Point> rearVerts2 = new List<Point> { rearBR.copy(), rearTR.copy(), rearBL.copy() };
		Polygon rearTri1 = new Polygon( rearVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon rearTri2 = new Polygon( rearVerts2, Color.cbGrey, Phong.cornellPhong );

		//front wall border param. 2 triangles... intersection of left&top, right&top, left&bot, right&bot at higher z
		//left border param. 2 triangles... Color is blue
		Point frontTL = new Point( -cbXLim, -cbYLim, -cbZLim );
		Point frontTR = new Point( cbXLim, -cbYLim, -cbZLim );
		Point frontBL = new Point( -cbXLim, cbYLim, -cbZLim );
		Point frontBR = new Point( cbXLim, cbYLim, -cbZLim );

		List<Point> frontVerts1 = new List<Point> { frontTR, frontBL, frontTL };
		List<Point> frontVerts2 = new List<Point> { frontBL.copy(), frontTR.copy(), frontBR.copy() };
		Polygon frontTri1 = new Polygon( frontVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon frontTri2 = new Polygon( frontVerts2, Color.cbGrey, Phong.cornellPhong );
		// finally make spheres
		//left sphere params
		float sphereRad = .5f; //1f

		float s1X = cbXLim - (sphereRad * 1.25f);
		float s1Depth = 0f; //cbZLim - sphereRad; //+z into the scene... I am IN LHS
		float s1Height = cbYLim - sphereRad; //1.75f.. 45 is good for lots of sky
		float s1Trans = .35f;
		float s1Refl = 1 - s1Trans;
		float s1RefIdx = .995f; // ni > nt for TIRGives random direction

		//right sphere param
		float s2X = -cbXLim + (sphereRad * 1.65f);
		float s2Depth = s1Depth - .75f; //1.85.. like Whitted... 2.75 for far apart
		float s2Height = s1Height;
		float s2Refl = .65f;
		float s2Trans = 1f - s2Refl;
		float s2RefIdx = 1.33f; //.955f;

		Sphere sphere1 = new Sphere( new Point( s1X, s1Height, s1Depth ), sphereRad, Color.cbChrome, s1Refl, s1Trans, s1RefIdx, Phong.cornellBallPhong );
		Sphere sphere2 = new Sphere( new Point( s2X, s2Height, s2Depth ), sphereRad, Color.cbChrome, s2Refl, s2Trans, s2RefIdx, Phong.cornellBallPhong );

		if (includeBunny)
		{
			float bunnyDepth = 0f; //s1Depth - 4f;
			Point bunnyOrigin = new Point( -.5f, 0f, bunnyDepth );
			List<Polygon> bunnyTris = PlyParser.parseEdgePly( bunnyOrigin.toVec() );

			//bunny loop
			foreach (Polygon p in bunnyTris)
				world.addObject( p );
		}

		//place mainLight on top wall near its center
		Point ceilLightPos = new Point( 0f, 0f, s1Depth ); ; // 0f, -cbYLim + .5f, 0f ... 0f, -2f, s1Depth - 1f... Top left -.5f, -cbXLim + .25f, s1Depth 
		Color ceilLightColor = Color.whiteSpecular;
		LightSource ceilLight = new LightSource( ceilLightPos, ceilLightColor );

		//add world stuff
		world.addLight( ceilLight );
		world.addObject( botTri1 );
		world.addObject( botTri2 );
		world.addObject( topTri1 );
		world.addObject( topTri2 );
		world.addObject( rightTri1 );
		world.addObject( rightTri2 );
		world.addObject( leftTri1 );
		world.addObject( leftTri2 );
		world.addObject( rearTri1 );
		world.addObject( rearTri2 );
		world.addObject( frontTri1 );
		world.addObject( frontTri2 );
		world.addObject( sphere1 );
		world.addObject( sphere2 );

		Vector up = new Vector( 0f, 1f, 0f );
		Point eyePos = new Point( 0f, .75f / 2f, -2.75f / 2f ); //0f, .5f, -2.75f... facing rear // facing frontBound 0f, .5f, -.75f
		Point lookAt = new Point( 0f, -1.5f / 2f, cbZLim ); //0f, -1.5f, cbZLim...-cbZlim for rear
		Camera cam = new Camera( up, eyePos, lookAt ); //-z = backing up...

		return cam;
	}

	// the original raytraced scene by Turner Whitted
	public static Camera setupWhitted( World world, bool includeBunny = false )
	{
		//transparent middle sphere
		float s1Depth = 8.75f; //+z into the scene... I am IN LHS
		float s1Height = .75f; //1.75f.. 45 is good for lots of sky
		float s1Trans = .85f;
		float s1Refl = 0f;
		float s1RefIdx = .955f; // ni > nt for TIR

		//reflective right sphere
		float s2Depth = s1Depth + 1.75f; //1.85.. like Whitted... 2.75 for far apart
		float sphereRad = 1.5f;
		float s2Refl = 1f;
		float s2Trans = 1 - s2Refl;
		float s2RefIdx = SceneObject.AIR_REF_INDEX;

		float floorHeight = 5.5f;

		float bunnyDepth = s1Depth - 10f; //s1Depth - 4f;

		// THESE WEREN'T BEING DRAWN PAST THE FILM PLANE
		Point topLeft = new Point( -6f, floorHeight, 68.5f );
		Point topRight = new Point( 30f, floorHeight, 68.5f );
		Point topRight2 = new Point( 30f, floorHeight, 68.5f );
		Point bottomLeft = new Point( -6f, floorHeight, 2.0f );
		Point bottomLeft2 = new Point( -6f, floorHeight, 2.0f );
		Point bottomRight = new Point( 30f, floorHeight, 2.0f );

		topLeft.texCoord = new Point( 0, 0, 0 );
		topRight.texCoord = new Point( 1, 0, 0 );
		topRight2.texCoord = new Point( 1, 0, 0 );
		bottomLeft.texCoord = new Point( 0, 0, 1 );
		bottomLeft2.texCoord = new Point( 0, 0, 1 );
		bottomRight.texCoord = new Point( 1, 0, 1 );

		List<Point> triVerts1 = new List<Point> { topLeft, bottomLeft, topRight };
		List<Point> triVerts2 = new List<Point> { bottomRight, topRight2, bottomLeft2 };

		Polygon triangle1 = new Polygon( triVerts1 );
		Polygon triangle2 = new Polygon( triVerts2 );
		triangle1.translate( -5f, 0, 0 );
		triangle2.translate( -5f, 0, 0 );

		Sphere sphere1 = new Sphere( new Point( 0, s1Height, s1Depth ), sphereRad, s1Refl, s1Trans, s1RefIdx );
		Sphere sphere2 = new Sphere( new Point( 0, 0f, s2Depth ), sphereRad, s2Refl, s2Trans, s2RefIdx ); //setting the point elsewhere gives translating whole sphere
		sphere2.translate( 1.75f, s1Height + 1.4f, 0 ); //doing it here gives same results as after cam transform ... ( 1.75f, s1Height + 1.4f, 0 );
														//sphere1.translate( 1.75f, +2.0f, 0 ); //sphere 1 experiments

		//adv cp 1... parse Bunny
		if (includeBunny)
		{
			Point bunnyOrigin = new Point( -.5f, 0f, bunnyDepth );
			List<Polygon> bunnyTris = PlyParser.parseEdgePly( bunnyOrigin.toVec() );

			//bunny loop
			foreach (Polygon p in bunnyTris)
				world.addObject( p );
		}

		//cp3... place mainLight source above the spheres 	// 1.5f, -1f, -5.0f //.85f, -30.85f, s1Depth - 5.5f , in front and way high

		Point mainLightPos = new Point( .85f, -30.85f, s1Depth + .75f ); // .85f, -30.85f, s1Depth + .75f

		Color mainLightColor = Color.whiteSpecular;
		LightSource mainLight = new LightSource( mainLightPos, mainLightColor );

		//mandatory whitted box
		world.addLight( mainLight );
		world.addObject( triangle1 );
		world.addObject( triangle2 );
		world.addObject( sphere2 );
		world.addObject( sphere1 );

		Vector up = new Vector( 0f, 1f, 0f );
		Point eyePos = new Point( 0f, -.5f, 3f ); //0f, -1f, -5f
		Point lookAt = new Point( .5f, .5f, s1Depth + 1f );
		Camera cam = new Camera( up, eyePos, lookAt, Camera.TR_MODEL.LINEAR ); //-z = backing up...

		return cam;
	}


////////////////////////////////////////////////////////// RAYTRACING & DISPLAY METHODS ////////////////////////////////////////////////////////////////////

	//OPENGL DRAW CCW order matters. We are in LHS system. +y is down. +x is right. +z into screen. Row major. Postmultiply.
	//list triangles in CCW ORDER from the point containing the largest angle/ opposite of the hypotenuse!
	public static void doRayTracing()
	{
		/* PHOTON MAPPING TODO LIST (page 47 onwards in Jensen's 2008 notes) :
		* * figure out how to shoot photons ( the points where photons land will be sent into the kdTree as splitting criterion)  //confident this works
		* * figure out how to balance photons in kdTree as we go (implement Jensen's method)
		* * photon tracing
		* * * collect the k nearest photons and make calculation for global and caustic PMs
		* figure out caustics and indirect illumination
		*/

		/*
		* Current behavior: indirect and & caustics are visualized at the point of intersection in pass2. Bottleneck is caustics firing at spheres if on.
		 */
		float focalLen = 1.25f; //distance from camera to film plane center along N... //1.25 for pool and cornell -1.25 for whitted

		World world = new World();

		// initialize camera and render world
		imageWidth = 1000;
		imageHeight = imageWidth;

		Camera cam = setupWhitted( world, false );
		//Camera cam = setupCornell( world);
		//Camera cam = setupFuturePool( world );

		// ditto with floats from 0-1 and 0-255, uint, now try byte
		byte[] pixColors = cam.render( world, imageHeight, imageWidth, focalLen); //last 3 bools control... kdTree (buggy), globalPM, causticsPM

		unsafe //this is how to work with pointers in C#
		{
			fixed (byte* colArrPtr = pixColors) { colsPtr = new IntPtr( (void*)colArrPtr ); }
		}

		return;

	}


	//helper display method for this OpenGL C# Binding
	public static void display()
	{
		//put in display function
		GL.ClearColor( 0, 0, 0, 1 );
		GL.Clear( GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT );
		GL.RasterPos2i( -1, -1 );

		GL.PixelStorei( GL.GL_UNPACK_ALIGNMENT, 1);
		GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		GL.GetFloatv( GL.GL_CURRENT_RASTER_POSITION, rat );
		Console.WriteLine( $" {rat[0]} , {rat[1]}, {rat[2]} is valid: {valid[0]}" ); //print out default { 0,0,0}"
		GL.DrawPixels( imageWidth, imageHeight, GL.GL_RGB, GL.GL_UNSIGNED_BYTE, colsPtr );

		FG.SwapBuffers();
		return;
	}

////////////////////////////////////////////////////////// MAIN ////////////////////////////////////////////////////////////////////

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
		
		if (colsPtr == default)
		{
			Console.WriteLine( "Did not receive information to draw OpenGL pixels. Stopping now" );
			return -1;
		}

		//begin OpenGl
		int[] argc = new int[1]; argc[0] = 0; string[] argv = null;
		FG.Init( argc, argv );
		FG.InitDisplayMode( GLUT.GLUT_RGB | GLUT.GLUT_SINGLE | GLUT.GLUT_DEPTH );
		FG.InitWindowSize( imageWidth, imageHeight );
		FG.InitWindowPosition( 0, 0 );
		FG.CreateWindow( "RayTracing KdTree" );
		GL.Init( true );            //I forgot to call this...

		//fixed pixels being at a higher depth being in front of those with lower depth
		GL.Enable( GL.GL_DEPTH_TEST );
		GL.DepthFunc( GL.GL_LEQUAL );
		GL.FrontFace( GL.GL_CCW );
		GL.CullFace( GL.GL_BACK );

		GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		FG.DisplayFunc( display ); //white screen without this
		FG.MainLoop(); //end

		return 0;
	}
}

