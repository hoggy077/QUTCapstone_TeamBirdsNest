using System;
using UnityEngine;

public class BowlPhysics{
     

    public static Vector3 UnityToGameCoords(Vector3 v3){
        return new Vector3(v3.x, v3.y, v3.z + 9);
    }

    public static Vector3 GameToUnityCoords(Vector3 v3){
        return new Vector3(v3.x, v3.y, v3.z - 9);
    }      

    // get the bounding box of the bowl at a given time step
    // public static Vector2[] GetMovingBowlBounds(float initVel, float angle, float MuScale, float t){
    //     Vector3 position = DeliveryPath(initVel, angle, MuScale, t);
    // }

    // get the bounding box of a bowl that is at rest
    // public static Vector2[] GetBoundingCircle(Vector3 BowlPos){
    // }

    // returns the length of time it takes for the bowl to come to a rest
    //
    //  Arguments:
    //  init_vel   - initial velocity, specified in m/s^2
    //  angle     - the angle of the delivery from the z axis
    //  mu_scale  - value from 0 to 1 which determins the "speed of the green"
    public static float DeliveryEndTime(float initVel, float angle, float mu_scale){
        // velocity = 0 when t = init_vel/(mu_scale * Gravity)
        return initVel/((0.025f + (mu_scale * 0.003f)) * 9.8f);
    }

    // returns the required initial velocity and angle of delivery for the resting point of the bowl to be 
    // at the given end point with the given bias of the bowl
    // 
    //  Arguments:
    //  endPoint - The final resting point of the bowls trajectory
    //  bias     - The bias of the bowl
    //  mu_scale - value from 0 to 1 which determins the "speed of the green"
    public static InitialConditions GetInitialConditions(Vector2 endPoint, Bias bias, float mu_scale){
        float mu = 0.025f + (mu_scale * 0.003f);
        float g = 9.8f;
        float p = 3.8f;           
        float init_vel = MathF.Sqrt((endPoint.magnitude * 2 * mu * g * MathF.Sqrt(1+p*p))/p);
        float r0 = (p * (init_vel*init_vel)) / (2*mu*g); //initial radius of curvature of the path of the bowl
        float X_end = r0/(1+(p*p));
        float Z_end = p*X_end;
        float angle;

        angle = AngleBetweenPoints(new Vector2(Z_end, X_end), endPoint);
        
        if(bias == Bias.Right){
            Vector2 mirror_point = new Vector2(Z_end, X_end);
            Vector2 actual_point = new Vector2(-Z_end, X_end);

            float res_angle = AngleBetweenPoints(actual_point, mirror_point);
            angle += res_angle + MathF.PI;
            Debug.Log(angle / (MathF.PI/180f));
        }

        return new InitialConditions(angle / (MathF.PI/180f), init_vel);
    }

    // 
    //
    //  Arguments:
    //  init_vel   - initial velocity, specified in m/s^2
    //  angle     - the angle of the delivery from the z axis in degrees
    //  mu_scale  - value from 0 to 1 which determins the "speed of the green"
    //  t - time to find the position of the bowl for relative to the start of the delivery
    public static Vector3 GetCurrentDirection(float init_vel, float angle, float mu_scale, float t){
        float end_time = DeliveryEndTime(init_vel, angle, mu_scale);

        // make sure t is a valid value
        if(t >= end_time){
            t = end_time - 0.5f;
        }
        
        float next_time = t + 0.5f;

        if(next_time > end_time){
            next_time = end_time;
        }

        Vector3 first_point = DeliveryPath(init_vel, angle, mu_scale, t);
        Vector3 second_point = DeliveryPath(init_vel, angle, mu_scale, next_time);

        Vector3 direction = second_point - first_point;

        return direction;
    }

    // 
    //
    //  Arguments:
    //  init_vel   - initial velocity, specified in m/s^2
    //  angle     - the angle of the delivery from the z axis in degrees
    //  mu_scale  - value from 0 to 1 which determins the "speed of the green"
    //  t - time to find the position of the bowl for relative to the start of the delivery
    public static float GetCurrentVelocity(float init_vel, float angle, float mu_scale, float t){
        float mu = 0.025f + (mu_scale * 0.003f);
        float g = 9.8f; //(m/s^2) velocity due to gravity
        float p = 3.8f; //(2.8*MU*R)/d 
        float r0 = (p * (init_vel*init_vel)) / (2*mu*g); // initial radius of curvature of the path of the bowl
        float v = init_vel - mu*g*t; // velocity at particular time step

        return v;
    }
    
    // returns an array of all of the bowls positions along the bowl trajectory with initial conditions (initial velocity and angle)
    // time_step specifies the time inbetween each point returned in the array and therefore setting a smaller time step will increase the
    // number of points returned
    //
    //  Arguments:
    //  init_vel   - initial velocity, specified in m/s^2
    //  angle     - the angle of the delivery from the z axis in degrees
    //  MU_scale  - value from 0 to 1 which determins the "speed of the green"
    //  time_step - time between each point in the trajectory 
    public static Vector3[] GetBowlTrajectory(float init_vel, float angle, float mu_scale, float time_step = 0.5f){
        // convert angle to radians
        angle = angle *(Mathf.PI / 180);

        float mu = 0.025f + (mu_scale * 0.003f);
        float g = 9.8f; //(m/s^2) velocity due to gravity
        float p = 3.8f; //(2.8*MU*R)/d 
        float r0 = (p * (init_vel*init_vel)) / (2*mu*g); // initial radius of curvature of the path of the bowl
        float delivery_end_time = BowlPhysics.DeliveryEndTime(init_vel, angle, mu_scale);
        int steps = (int)MathF.Floor(delivery_end_time/time_step);
        Vector3[] BowlTrajectory = new Vector3[steps];
    
        float time = 0;
        for(int i = 0; i < steps; i++){

            float v = init_vel - mu*g*time; // velocity at particular time step
            float phi = (2/p)*Mathf.Log(init_vel/v); // angle between the tangent of the bowls path with the x-axis
            float lamba = Mathf.Exp(-p*phi); // 
            float z;
            float x;
            // bias is on the left
            if(angle < 0){
                z = (r0/(1+p*p))*(p - p*lamba* Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
                x = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
            }
            // bias is on the right
            else{
                z = -(r0/(1+p*p))*(p - p*lamba*Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
                x = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
                angle += Mathf.PI;
            }

            BowlTrajectory[i] = new Vector3(z*Mathf.Sin(angle) + x*Mathf.Cos(angle), 0.01f, z*Mathf.Cos(angle) - x*Mathf.Sin(angle));
            time += time_step;
        }

        return BowlTrajectory;
    }

    // returns the point of the bowl at time t along the bowls trajectory with the initial conditions initial velocity and angle of delivery.
    //
    //  Arguments:
    //  init_vel   - initial velocity, specified in m/s^2
    //  angle     - the angle of the delivery from the z axis in degrees
    //  mu_scale  - value from 0 to 1 which determins the "speed of the green"
    public static Vector3 DeliveryPath(float init_vel, float angle, float mu_scale, float t){
        // convert angle to radians
        angle = angle *(Mathf.PI / 180);

        float mu = 0.025f + (mu_scale * 0.003f);
        float g = 9.8f; //(m/s^2) velocity due to gravity
        float p = 3.8f; //(2.8*MU*R)/d 
        float r0 = (p * (init_vel*init_vel)) / (2*mu*g); // initial radius of curvature of the path of the bowl
        float v = init_vel - mu*g*t; // velocity at particular time step
        float phi = (2/p)*Mathf.Log(init_vel/v); // angle between the tangent of the bowls path with the x-axis
        float lamba = Mathf.Exp(-p*phi);
        
        float z;
        float x;
        // bias is on the left
        if(angle < 0){
            z = (r0/(1+p*p))*(p - p*lamba* Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
            x = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
        }
        // bias is on the right
        else{
            z = -(r0/(1+p*p))*(p - p*lamba*Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
            x = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
            angle += Mathf.PI;
        }

        return new Vector3(z*Mathf.Sin(angle) + x*Mathf.Cos(angle), 0.01f, z*Mathf.Cos(angle) - x*Mathf.Sin(angle));
    }

    // TODO: check why I can't just get the angle from the dot product of the two vectors?
    // returns the angle between the two vectors P1 and P2 in radians
    //
    // arguments:
    // P1 - the first point
    // P2 - the second point
    public static float AngleBetweenPoints(Vector2 P1, Vector2 P2){
        // if points are 180 degrees from each other then there will be no
        // triangle to construct, we can make sure this isn't the case by 
        // checking if P2 is some multiple of P1
        if(P2.y - P1.y*(P2.x/P1.x) == 0){
            return Mathf.PI;
        }

        Vector2 P1ToP2 = P1 - P2;

        float angle = Mathf.Acos(((-Mathf.Pow(P1ToP2.magnitude, 2) + Mathf.Pow(P1.magnitude, 2) + Mathf.Pow(P2.magnitude, 2))/ (2*P1.magnitude*P2.magnitude)));
        float rot_x = P1.x * Mathf.Cos(angle) - P1.y * Mathf.Sin(angle);
        float rot_y = P1.x * Mathf.Sin(angle) + P1.y * Mathf.Cos(angle);

        if(rot_x != P2.x && rot_y != P2.y){
            angle = -angle;
        }

        return angle;
    }
}

public struct InitialConditions{
    public InitialConditions(float angle, float init_vel){
        Angle = angle;
        InitVel = init_vel;
    }

    public float Angle {get; set;}
    public float InitVel {get; set;}
}

public struct Conditions{
    public Conditions(float bowlAngle, float currentSpeed){
        BowlAngle = bowlAngle;
        CurrentSpeed = currentSpeed;
    }

    public float BowlAngle {get; set;}
    public float CurrentSpeed {get; set;}
}

public enum Bias{
    Left,
    Right
}