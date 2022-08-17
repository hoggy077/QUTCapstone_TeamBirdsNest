using System;
using UnityEngine;

public class BowlPhysics{
     // returns the time step when the velocity of the bowl will be zero
    public static float DeliveryEndTime(float initVel, float angle, float MuScale){
        // velocity = 0 when t = initVecl/(MuScale * Gravity)
        return initVel/((0.025f + (MuScale * 0.003f)) * 9.8f);
    }

    
    public static Conditions GetBowlConditions(float initVel, float angle, float MU_scale, float t){
        angle = angle * (Mathf.PI / 180);

        var MU = 0.025f + (MU_scale * 0.003f);
        var G = 9.8f; //(m/s^2) velocity due to gravity
        var p = 3.8f; //(2.8*MU*R)/d 

        var r0 = (p * (initVel*initVel)) / (2*MU*G); // initial radius of curvature of the path of the bowl
    
        float v = initVel - MU*G*t; // velocity at particular time step
        float phi = (2/p)*Mathf.Log(initVel/v); // angle between the tangent of the bowls path with the x-axis

        return new Conditions(phi / (Mathf.PI/180), v);
    }

    public static Vector3 DeliveryPath(float initVel, float angle, float MU_scale, float t){

        angle = angle *(Mathf.PI / 180);

        var MU = 0.025f + (MU_scale * 0.003f);
        var G = 9.8f; //(m/s^2) velocity due to gravity
        //var R = 6f; // (cm) radius of the bowl
        //var Mass = 1.5f; // (kg)
        //var d = 0.7f; // (mm) distance of the center of gravity to the geometric center of the bowl
        var p = 3.8f; //(2.8*MU*R)/d 

        var r0 = (p * (initVel*initVel)) / (2*MU*G); // initial radius of curvature of the path of the bowl
    
        var v = initVel - MU*G*t; // velocity at particular time step
        float phi = (2/p)*Mathf.Log(initVel/v); // angle between the tangent of the bowls path with the x-axis
        var lamba = Mathf.Exp(-p*phi); // 

        float x;
        float y;

        // end points
        float Y_e = r0/(1+(p*p));
        float X_e = p*Y_e;

        // bias is on the left
        if(angle < 0){
            x = (r0/(1+p*p))*(p - p*lamba* Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
            y = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
        }
        // bias is on the right
        else{
            x = -(r0/(1+p*p))*(p - p*lamba*Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
            y = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
            angle += Mathf.PI;
        }

        float rotatedX = x*Mathf.Cos(angle) - y*Mathf.Sin(angle);
        float rotatedY = x*Mathf.Sin(angle) + y*Mathf.Cos(angle);

        return new Vector3(rotatedY, 0.01f, rotatedX-9);
    }

    public static float angle_between_points(Vector2 P1, Vector2 P2){
        // if points are 180 degrees from each other then there will be no
        // triangle to construct, we can make sure this isn't the case by 
        // checking if P2 is some multiple of P1
        if(P2.y - P1.y*(P2.x/P1.x) == 0){
            return Mathf.PI;
        }

        Vector2 P1ToP2 = P1 - P2;

        float angle = Mathf.Acos(((-Mathf.Pow(P1ToP2.magnitude, 2) + Mathf.Pow(P1.magnitude, 2) + Mathf.Pow(P2.magnitude, 2))/ (2*P1.magnitude*P2.magnitude)));
        Debug.Log(angle);
        float rot_x = P1.x * Mathf.Cos(angle) - P1.y * Mathf.Sin(angle);
        float rot_y = P1.x * Mathf.Sin(angle) + P1.y * Mathf.Cos(angle);

        if(rot_x != P2.x && rot_y != P2.y){
            angle = -angle;
        }

        return angle;
    }
}

public struct Conditions{

    public Conditions(float bowlAngle, float currentSpeed){
        BowlAngle = bowlAngle;
        CurrentSpeed = currentSpeed;
    }

    public float BowlAngle {get;}
    public float CurrentSpeed {get;}
}