using System;
using System.Collections;
using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;
using Haze;
// polygon 
//     get collision polygon path for bowl and jack
//     get polygon path for circle centered at a point with a given radius
//     get polygon path for an arc of a certain radius

class Polygon{
    
    public static List<List<PointD>> GetPolygonPaths(List<BowlPosition> bowls1, List<BowlPosition> bowls2, Bias bias){
        List<List<PointD>> paths = new List<List<PointD>>();

        foreach(BowlPosition bowl in bowls1){
            paths.Add(GetPolygonPath(bowl.BowlPos, bias));
        }
        foreach(BowlPosition bowl in bowls2){
            paths.Add(GetPolygonPath(bowl.BowlPos, bias));
        }

        return Clipper.Union(paths, new List<List<PointD>>(), FillRule.NonZero);
    }

    public static List<List<PointD>> GetPolygonPaths(List<BowlPosition> bowls, Bias bias){
        List<List<PointD>> paths = new List<List<PointD>>();

        foreach(BowlPosition bowl in bowls){
            paths.Add(GetPolygonPath(bowl.BowlPos, bias));
        }
       
        return Clipper.Union(paths, new List<List<PointD>>(), FillRule.NonZero);
    }
    
    // TODO: generalise this so it can be used for the jack and bowls
    public static List<PointD> GetPolygonPath(Vector3 startPoint, Bias bias){
        Vector3 right_offset = (new Vector3(1, 0, 0)) * 0.14f;
        Vector3 left_offset = (new Vector3(-1f, 0, 0)) * 0.2f;
        Vector3 left_point = startPoint + left_offset;
        Vector3 right_point = startPoint + right_offset;

        Vector3[] leftPoints = BowlPhysics.getBoundaryPoints(left_point, bias);
        Vector3[] rightPoints = BowlPhysics.getBoundaryPoints(right_point, bias);

        double[] points = new double[2*leftPoints.Length+ 2*rightPoints.Length];
        int point_i = 0;
        for(int i = 0; i < leftPoints.Length; i++){
            points[point_i++] = leftPoints[i].x * 100;
            points[point_i++] = leftPoints[i].z * 100;
        }
        for(int i = rightPoints.Length-1; i >= 0; i--){
             points[point_i++] = rightPoints[i].x * 100;
             points[point_i++] = rightPoints[i].z * 100;
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

    public static Vector2 RndPointInsideTriangle(Triangulator.Triangle triangle){
        float randomVal = UnityEngine.Random.value;

        Vector2 start;
        Vector2 end;

        if(randomVal <= 1/3){
            Vector2 side1 = triangle.a - triangle.b;
            Vector2 side2 = triangle.a - triangle.c;
            start = triangle.b + UnityEngine.Random.value * side1;
            end = triangle.c + UnityEngine.Random.value * side2;
        }else if(randomVal > 1/3 && randomVal <= 2/3){
            Vector2 side1 = triangle.a - triangle.b;
            Vector2 side2 = triangle.b - triangle.c;
            start = triangle.b + UnityEngine.Random.value * side1;
            end = triangle.c + UnityEngine.Random.value * side2;
        }else{
            Vector2 side1 = triangle.a - triangle.c;
            Vector2 side2 = triangle.b - triangle.c;
            start = triangle.c + UnityEngine.Random.value * side1;
            end = triangle.c + UnityEngine.Random.value * side2;
        }

        Vector2 between = end - start;

        return start + UnityEngine.Random.value * between;
    }

    public static List<Triangulator.Triangle> TriangulatePolygon(List<PointD> path){
        List<Vector2> points = PathToVec2(path);

        return Triangulator.Triangulate(points);
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
        for(int i = 1; i <= totalPoints; i++){
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