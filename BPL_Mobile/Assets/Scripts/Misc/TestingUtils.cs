using System;
using System.Collections;
using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;

public class TestingUtils{

    // used for testing
    public static Vector3[] shiftPoints(Vector3[] points, Vector3 shiftTo){
        shiftTo = BowlPhysics.GameToUnityCoords(shiftTo);
        //Vector3 firstPoint = points[0];
        float x_shift = shiftTo.x - points[0].x;
        float z_shift = shiftTo.z - points[0].z;

        for(int i = 0; i < points.Length; i++){
            points[i].x += x_shift;
            points[i].z += z_shift;
        }
        
        return points;
    }

    public static Vector3[] normalizeAngleForPoints(Vector3[] points, Vector3 direction){
        Vector3 tmp = points[1] - points[0];
        float angle = Vector3.Angle(direction, tmp) * (MathF.PI/180);

        for(int i = 0; i < points.Length; i++){
            float x = points[i].x;
            float z = points[i].z;

            points[i].x = z * MathF.Sin(angle) + x * MathF.Cos(angle);
            points[i].z = z * MathF.Cos(angle) - x * MathF.Sin(angle);
        }

        return points;
    }

    public static void drawPolygon(List<Vector2> points, LineRenderer lr){
        lr.enabled = true;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.positionCount = points.Count + 1;

        for(int i = 0; i < points.Count; i++){
            lr.SetPosition(i, BowlPhysics.GameToUnityCoords(points[i]));
        }

        lr.SetPosition(points.Count, BowlPhysics.GameToUnityCoords(points[0]));
    }

    public static void DrawBowlTrajectory(InitialConditions ics, LineRenderer lineRenderer){
        float PredictorTimeStep = 0.5f;
        float PredictorEndTime = BowlPhysics.DeliveryEndTime(ics.InitVel, ics.Angle, 0);
        int steps = (int)Math.Ceiling(PredictorEndTime/PredictorTimeStep);
        Vector3[] points = new Vector3[steps];
        
        for(int step = 0; step < steps; step++){
            points[step] = BowlPhysics.GameToUnityCoords(BowlPhysics.DeliveryPath(ics.InitVel, ics.Angle, ics.Bias, 0, PredictorTimeStep * step));
        }

        lineRenderer.positionCount = steps;
        lineRenderer.SetPositions(points);
        lineRenderer.enabled = true;
    }
}