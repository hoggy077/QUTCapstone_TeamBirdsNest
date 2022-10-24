using System;
using UnityEngine;

public class BowlPhysics{
    public static Vector2 UnityToGameCoords(Vector3 v){
        return new Vector2(v.x, v.z + 17);
    }
    public static Vector3 GameToUnityCoords(Vector2 v){
        return new Vector3(v.x, 0.01f, v.y - 17);
    }

    public static float GetBowlAngle(Vector2 direction){
        float angle = Vector2.Angle(Vector2.up, direction);
           
        if(direction.x > 0){
            return angle;
        }
        else{
            return -angle;
        }
    }

    public static Vector2[] getBoundaryPoints(Vector2 right_point, Bias bias, float biasStrength, int num_trials = 14, float mag_diff = 2){
        
        Vector2[] points = new Vector2[num_trials+1];
        points[0] = right_point;

        // get the first 4 points using 0.25 mag_diff
        for(int i = 1; i <=4 ; i++){
            var (rightPTwo, rightIV, right_end_time) = GetPointWithRadius(right_point.magnitude, right_point.magnitude+(0.25f * i), bias, 0);
           
            // find the angle between right point and right point two
            float rightAngle = Vector2.Angle(rightPTwo, right_point);
            
            if(rightPTwo.x > right_point.x){
                rightAngle = -rightAngle;
            }

            // get the last point
            Vector2 rlp = DeliveryPath(rightIV, rightAngle, bias, 0, right_end_time-0.1f, biasStrength);
            points[i] = rlp;
        }

        // get the rest of the points using the mag_diff provided
        for(int i = 5; i <= num_trials; i++){
            var (rightPTwo, rightIV, right_end_time) = GetPointWithRadius(right_point.magnitude, right_point.magnitude+(0.25f*4)+(mag_diff * (i-4)), bias, 0);
           
            // find the angle between right point and right point two
            float rightAngle = Vector2.Angle(rightPTwo, right_point);
            
            if(rightPTwo.x > right_point.x){
                rightAngle = -rightAngle;
            }

            // get the last point
            Vector2 rlp = DeliveryPath(rightIV, rightAngle, bias, 0, right_end_time-0.1f, biasStrength);
            points[i] = rlp;
        }

        return points;
    }

    // get 
    public static (Vector2 point, float init_vel, float end_time) GetPointWithRadius(float WantedPointLength, float EndPointLength, Bias bias, float mu_scale){
        float mu = 0.025f + (mu_scale * 0.003f);
        float g = 9.8f; //(m/s^2) velocity due to gravity
        float p = 3.8f; //(2.8*MU*R)/d
        float init_vel = MathF.Sqrt((EndPointLength * 2 * mu * g * MathF.Sqrt(1+p*p))/p);
        float r0 = (p * (init_vel*init_vel)) / (2*mu*g); // initial radius of curvature of the path of the bowl
        
        float tolerance = 0.00001f;
        float end_time = DeliveryEndTime(init_vel, 0, mu_scale);
        float z;
        float x;
        
        float lower = 0;
        float upper = end_time;

        float t = lower + ((upper - lower) / 2);

        float count = 0;
        while(true){
            count++;
            float v = init_vel - mu*g*t; // velocity at particular time step
            float phi = (2/p)*Mathf.Log(init_vel/v); // angle between the tangent of the bowls path with the x-axis
            float lamba = Mathf.Exp(-p*phi);

            // bias is on the left
            if(bias == Bias.Right){
                z = (r0/(1+p*p))*(p - p*lamba* Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
                x = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
            }
            // bias is on the right
            else{
                float c = Mathf.Cos(MathF.PI);
                float d = Mathf.Sin(MathF.PI);
                z = -(r0/(1+p*p))*(p - p*lamba*Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
                x = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
                new Vector2(z*d + x*c, z*c - x*d);
                z = z*c - x*d;
                x = z*d + x*c;
            }

            float point_mag = MathF.Sqrt(z*z + x*x);
            
            if(MathF.Abs(WantedPointLength-point_mag) < tolerance){
                break;
            }

            if(point_mag > WantedPointLength){
                upper = t;
            }else if(point_mag < WantedPointLength){
                lower = t;
            }

            t = lower + ((upper - lower) / 2);
        }

        return (new Vector2(x, z), init_vel, end_time);
    }

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

        angle = AngleBetweenPoints(new Vector2(X_end, Z_end), endPoint);
        
        if(bias == Bias.Left){
            Vector2 mirror_point = new Vector2(X_end, Z_end);
            Vector2 actual_point = new Vector2(X_end, -Z_end);

            float res_angle = AngleBetweenPoints(actual_point, mirror_point);
            angle += res_angle + MathF.PI;
        }

        return new InitialConditions(angle / (MathF.PI/180f), init_vel, bias);
    }

    // 
    //
    //  Arguments:
    //  init_vel   - initial velocity, specified in m/s^2
    //  angle     - the angle of the delivery from the z axis in degrees
    //  mu_scale  - value from 0 to 1 which determins the "speed of the green"
    //  t - time to find the position of the bowl for relative to the start of the delivery
    public static Vector2 GetCurrentDirection(float init_vel, float angle, Bias bias, float mu_scale, float t, float biasStrength){
        float end_time = DeliveryEndTime(init_vel, angle, mu_scale);

        // make sure t is a valid value
        if(t >= end_time){
            t = end_time - 0.5f;
        }
        
        float next_time = t + 0.5f;

        if(next_time > end_time){
            next_time = end_time-0.01f;
        }

        Vector2 first_point = DeliveryPath(init_vel, angle, bias, mu_scale, t, biasStrength);
        Vector2 second_point = DeliveryPath(init_vel, angle, bias, mu_scale, next_time, biasStrength);

        Vector2 direction = second_point - first_point;

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
    public static Vector2[] GetBowlTrajectory(float init_vel, float angle, Bias bias, float mu_scale, float time_step = 0.5f){
        // convert angle to radians
        angle = angle *(Mathf.PI / 180);

        float mu = 0.025f + (mu_scale * 0.003f);
        float g = 9.8f; //(m/s^2) velocity due to gravity
        float p = 3.8f; //(2.8*MU*R)/d 
        float r0 = (p * (init_vel*init_vel)) / (2*mu*g); // initial radius of curvature of the path of the bowl
        float delivery_end_time = BowlPhysics.DeliveryEndTime(init_vel, angle, mu_scale);
        int steps = (int)MathF.Floor(delivery_end_time/time_step);
        Vector2[] BowlTrajectory = new Vector2[steps];
    
        float time = 0;
        for(int i = 0; i < steps; i++){
            float v = init_vel - mu*g*time; // velocity at particular time step
            float phi = (2/p)*Mathf.Log(init_vel/v); // angle between the tangent of the bowls path with the x-axis
            float lamba = Mathf.Exp(-p*phi); // 
            float z;
            float x;
            // bias is on the left
            if(bias == Bias.Right){
                float a = lamba*Mathf.Cos(phi);
                float b = lamba*Mathf.Sin(phi);
                float c = Mathf.Cos(angle);
                float d = Mathf.Sin(angle);
                z = (r0/(1+p*p))*(p - p*a+b);
                x = (r0/(1+p*p))*(1 - a - p*b);
                BowlTrajectory[i] = new Vector2(z*d + x*c, z*c - x*d);
            }
            else{
                float a = lamba*Mathf.Cos(phi);
                float b = lamba*Mathf.Sin(phi);
                float c = Mathf.Cos(angle + MathF.PI);
                float d = Mathf.Sin(angle + MathF.PI);
                z = -(r0/(1+p*p))*(p - p*a+b);
                x = (r0/(1+p*p))*(1 - a - p*b);
                BowlTrajectory[i] = new Vector2(z*d + x*c, z*c - x*d);
            }

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
    public static Vector2 DeliveryPath(float init_vel, float angle, Bias bias, float mu_scale, float t, float biasStrength){
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
        
        if(bias == Bias.Right){
            z = biasStrength * (r0/(1+p*p))*(p - p*lamba* Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
            x = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
        }
        else{
            z = -biasStrength  * (r0/(1+p*p))*(p - p*lamba*Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
            x = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
            angle += Mathf.PI;
        }

        return new Vector2(z*Mathf.Sin(angle) + x*Mathf.Cos(angle), z*Mathf.Cos(angle) - x*Mathf.Sin(angle));
    }

    // returns the angle between the two vectors P1 and P2 in radians
    //
    // arguments:
    // P1 - the first point
    // P2 - the second point
    public static float AngleBetweenPoints(Vector2 p1, Vector2 p2){
        float angle = Vector2.Angle(p1, p2);

        // make sure that if p1 is rotated by the angle it will lie on
        // p2
        float rot_x = p1.x * Mathf.Cos(angle) - p1.y * Mathf.Sin(angle);
        float rot_y = p1.x * Mathf.Sin(angle) + p1.y * Mathf.Cos(angle);
        if(rot_x != p2.x && rot_y != p2.y){
            angle = -angle;
        }

        return angle * (MathF.PI/180);
    }
}

public struct InitialConditions{
    public InitialConditions(float angle, float init_vel, Bias bias){
        Angle = angle;
        InitVel = init_vel;
        Bias = bias;
    }

    public float Angle {get; set;}
    public float InitVel {get; set;}
    public Bias Bias {get; set;}
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