using System;
using System.Collections;
using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;
using EarcutNet;
// polygon 
//     get collision polygon path for bowl and jack
//     get polygon path for circle centered at a point with a given radius
//     get polygon path for an arc of a certain radius

class Polygon{
    public static List<List<PointD>> GetPolygonPaths(List<BowlPosition> bowls1, List<BowlPosition> bowls2, Bias bias, float biasStrength){
        List<List<PointD>> paths = new List<List<PointD>>();
        List<List<PointD>> circles = new List<List<PointD>>();
        float circleRadius = 0.17f;
        foreach(BowlPosition bowl in bowls1){
            paths.Add(GetPolygonPath(bowl.BowlPos, bias, biasStrength));
            circles.Add(GetCirclePolygon(bowl.BowlPos, circleRadius, 15)[0]);
        }
        foreach(BowlPosition bowl in bowls2){
            paths.Add(GetPolygonPath(bowl.BowlPos, bias, biasStrength));
            circles.Add(GetCirclePolygon(bowl.BowlPos, circleRadius, 15)[0]);
        }

        return Clipper.Union(paths, circles, FillRule.NonZero);
    }

    public static List<List<PointD>> GetPolygonPaths(List<BowlPosition> bowls, Bias bias, float biasStrength){
        List<List<PointD>> paths = new List<List<PointD>>();

        foreach(BowlPosition bowl in bowls){
            paths.Add(GetPolygonPath(bowl.BowlPos, bias, biasStrength));
        }
       
        return Clipper.Union(paths, new List<List<PointD>>(), FillRule.NonZero);
    }

    public static List<List<PointD>> GetBiasRinkBoundary(Bias bias, float biasStrength){
        float halfRinkW = 5/2;
        float offset = 0.17f;
        Vector2 point = new Vector2(0, 0);
        if(bias == Bias.Right){
            point = new Vector2(-halfRinkW + 0.17f, 0);
        }else{
            point = new Vector2(halfRinkW - 0.17f, 0);
        }

        Vector2[] boundaryPoints = BowlPhysics.getBoundaryPoints(point, bias, biasStrength);

        double[] points = new double[(boundaryPoints.Length + 2) * 2];
        if(bias == Bias.Right){
            points[0] = -5*100;
            points[1] = 0*100;
            points[2] = -5*100;
            points[3] = 30*100;
            
            int point_i = 4;
            for(int i = boundaryPoints.Length-1; i >= 0; i--){
                points[point_i++] = boundaryPoints[i].x * 100;
                points[point_i++] = boundaryPoints[i].y * 100;
            }
        }else{
            int point_i = 0;
            for(int i = 0; i < boundaryPoints.Length; i++){
                points[point_i++] = boundaryPoints[i].x * 100;
                points[point_i++] = boundaryPoints[i].y * 100;
            }

            points[point_i++] = 5*100;
            points[point_i++] = 30*100;
            points[point_i++] = 5*100;
            points[point_i++] = 0*100;
        }

        List<List<PointD>> paths = new List<List<PointD>>();
        paths.Add(Clipper.MakePath(points));

        List<List<PointD>> boundary = GetInternalRinkBoundary();

        return Clipper.Difference(boundary, paths, FillRule.NonZero);
    }

    public static List<List<PointD>> GetInternalRinkBoundary(){
        float halfRinkW = 5/2;
        float offset = 0.17f;

        List<List<PointD>> paths = new List<List<PointD>>();

        double[] points = new double[] {-halfRinkW + offset, 19 - offset, halfRinkW - offset, 19 - offset, halfRinkW - offset, 12 + offset, - halfRinkW + offset, 12 + offset};
        for(int i = 0; i<points.Length; i++){
            points[i] = points[i] * 100;
        }
        paths.Add(Clipper.MakePath(points));

        return paths;
    }
    
    // TODO: generalise this so it can be used for the jack and bowls
    public static List<PointD> GetPolygonPath(Vector3 startPoint, Bias bias, float biasStrength){
        Vector3 right_offset;
        Vector3 left_offset;
        float circleRadius = 0.17f;

        if(bias == Bias.Left){
            left_offset = new Vector2(MathF.Cos(MathF.PI * 1/4), MathF.Sin(MathF.PI*1/4)) * circleRadius;
            right_offset = new Vector2(MathF.Cos(MathF.PI * 3f/4), MathF.Sin(-MathF.PI*3/4)) * circleRadius;
        }
        else{
            left_offset = new Vector2(MathF.Cos(MathF.PI * 3/4), MathF.Sin(MathF.PI*3/4)) * circleRadius;
            right_offset = new Vector2(MathF.Cos(MathF.PI * 1/4), MathF.Sin(-MathF.PI*1/4)) * circleRadius;
        }
        
        Vector3 left_point = startPoint + left_offset;
        Vector3 right_point = startPoint + right_offset;

        Vector2[] leftPoints = BowlPhysics.getBoundaryPoints(left_point, bias, biasStrength);
        Vector2[] rightPoints = BowlPhysics.getBoundaryPoints(right_point, bias, biasStrength);

        double[] points = new double[2*leftPoints.Length + 2*rightPoints.Length];
        int point_i = 0;
        for(int i = 0; i < leftPoints.Length; i++){
            points[point_i++] = leftPoints[i].x * 100;
            points[point_i++] = leftPoints[i].y * 100;
        }
        for(int i = rightPoints.Length-1; i >= 0; i--){
             points[point_i++] = rightPoints[i].x * 100;
             points[point_i++] = rightPoints[i].y * 100;
        }

        List<PointD> path = Clipper.MakePath(points);
        
        //path = Clipper.RamerDouglasPeucker(path, 0.25);

        return path;
    }

    public static Vector3[] PathToVec(List<PointD> path){
        Vector3[] returnPoints = new Vector3[path.Count];
        for(int i = 0; i < path.Count; i++){
            
            returnPoints[i].x = (float)path[i].x/100;
            returnPoints[i].z = (float)path[i].y/100;
        }
        
        return returnPoints;
    }

    public static List<Vector2> PathToVec2(List<PointD> path){
        List<Vector2> returnPoints = new List<Vector2>();
        foreach(PointD point in path){
            returnPoints.Add(new Vector2((float)point.x/100, (float)point.y/100));
        }

        return returnPoints;
    }

    

    public static List<Triangle> TriangulatePolygon(List<PointD> path){
        List<double> points = new List<double>();

        foreach(PointD p in path){
            points.Add(p.x/100);
            points.Add(p.y/100);
        }

        List<int> ind = Earcut.Tessellate(points, new List<int>());
        List<Triangle> tri = new List<Triangle>();
        
        for(int i = 0; i < ind.Count; i+=3){
            Vector2 p1 = new Vector2((float)points[ind[i]*2], (float)points[ind[i]*2+1]);
            Vector2 p2 = new Vector2((float)points[ind[i+1]*2], (float)points[ind[i+1]*2+1]);
            Vector2 p3 = new Vector2((float)points[ind[i+2]*2], (float)points[ind[i+2]*2+1]);

            tri.Add(new Triangle(p1,p2,p3));
        }

        return tri;
    }

    public static List<List<PointD>> GetArcPolygon(Vector2 center, float radius, float arcWidth, float angleStart, float angleEnd, int totalPoints){
        // angle between points
        float angleDiff = (angleEnd - angleStart)/totalPoints;
        float angle = angleStart;

        List<PointD> path = new List<PointD>();
        for(int i = 1; i <= totalPoints; i++){

            path.Add(new PointD( (center.x + MathF.Cos(angle) * radius) * 100, (center.y + MathF.Sin(angle)*radius) * 100));
            angle += angleDiff;
        }

        radius -= arcWidth;

        for(int i = 1; i < totalPoints; i++){

            path.Add(new PointD( (center.x + MathF.Cos(angle) * radius) * 100, (center.y + MathF.Sin(angle)*radius) * 100));
            angle -= angleDiff;
        }

        List<List<PointD>> paths = new List<List<PointD>>();
        paths.Add(Clipper.RamerDouglasPeucker(path, 0.01));
        return paths;
    }

    // creates an approximation to a circle
    public static List<List<PointD>> GetCirclePolygon(Vector2 center, float radius, int totalPoints){
        
        // angle between points
        float angle = (2*MathF.PI)/totalPoints;

        List<PointD> path = new List<PointD>();
        for(int i = totalPoints; i != 0; i--){
            path.Add(new PointD( (center.x + MathF.Cos(angle*i) * radius) * 100, (center.y + MathF.Sin(angle*i)*radius) * 100));
        }
        List<List<PointD>> paths = new List<List<PointD>>();
        paths.Add(Clipper.RamerDouglasPeucker(path, 0.01));
        return paths;
    }

    public static List<List<PointD>> GetGreenPolygonPaths(){
        List<List<PointD>> paths = new List<List<PointD>>();

        double[] points = new double[] {-2.5, 0, -2.5, 10, 2.5, 10, 2.5, 0};
        for(int i = 0; i<points.Length; i++){
            points[i] = points[i] * 100;
        }
        paths.Add(Clipper.MakePath(points));

        return paths;
    }

}

public class Triangle : IComparable<Triangle>{
    public Vector2 a;
    public Vector2 b;
    public Vector2 c;
    public float area;

    public Triangle(Vector2 a, Vector2 b, Vector2 c){
        this.a = a;
        this.b = b;
        this.c = c;

        this.area = ((a-b).magnitude * (c-b).magnitude) / 2;
    }

    public int CompareTo(Triangle t){
        if(area < t.area){
            return 1;
        }
        else if(area > t.area){
            return -1;
        }
        else{
            return 0;
        }
    }

    public Vector2 RndPointInsideTriangle(){
        float randomVal = UnityEngine.Random.value;

        Vector2 start;
        Vector2 end;

        if(randomVal <= 1/3){
            Vector2 side1 = a - b;
            Vector2 side2 = a - c;
            start = b + UnityEngine.Random.value * side1;
            end = c + UnityEngine.Random.value * side2;
        }else if(randomVal > 1/3 && randomVal <= 2/3){
            Vector2 side1 = a - b;
            Vector2 side2 = b - c;
            start = b + UnityEngine.Random.value * side1;
            end = c + UnityEngine.Random.value * side2;
        }else{
            Vector2 side1 = a - c;
            Vector2 side2 = b - c;
            start = c + UnityEngine.Random.value * side1;
            end = c + UnityEngine.Random.value * side2;
        }

        Vector2 between = end - start;
        return start + UnityEngine.Random.value * between;
    }
}