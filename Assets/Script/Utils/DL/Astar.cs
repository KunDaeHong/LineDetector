using System;
using System.Collections.Generic;

using UnityEngine;

//https://sam0308.tistory.com/77
public class Node<T>
{
    public int idx;
    public Vector2 pos;
    public T data;
    public List<float> priority;
    public List<int> connectNodes;

    public Node(int idx, Vector2 pos, T data, List<float> priority, List<int> connectNodes)
    {
        this.idx = idx;
        this.pos = pos;
        this.data = data;
        this.priority = priority;
        this.connectNodes = connectNodes;
    }
}

public class NodeData
{
    public float f;
    public float h;
    public float g;

    public NodeData(float f, float h, float g)
    {
        this.f = f;
        this.h = h;
        this.g = g;
    }
}

public class Astar
{
    //TEST
    static Vector2[] points =
    {
        new Vector2() { x = 0, y = 0 },
        new Vector2() { x = 2, y = 0 },
        new Vector2() { x = 2, y = 2 },
        new Vector2() { x = 1, y = 3 },
        new Vector2() { x = 3, y = 4 },
        new Vector2() { x = 3, y = 1 },
        new Vector2() { x = 4, y = 4 }
    };
    public static List<Node<NodeData>> aStarArr = new List<Node<NodeData>>()
    {
        new Node<NodeData>(0, points[0], new NodeData(0,0,0), new List<float>{2, 2.83f, 3.16f}, new List<int>{1, 2, 3}),
        new Node<NodeData>(1, points[1], new NodeData(0,0,0), new List<float>{2, 2, 1.41f}, new List<int>{0, 2, 5}),
        new Node<NodeData>(2, points[2], new NodeData(0,0,0), new List<float>{1.41f, 2.83f, 2, 1.41f, 2.24f}, new List<int>{3, 0, 1, 5, 4}),
        new Node<NodeData>(3, points[3], new NodeData(0,0,0), new List<float>{3.16f, 1.41f, 2.24f}, new List<int>{0, 2, 4}),
        new Node<NodeData>(4, points[4], new NodeData(0,0,0), new List<float>{2.24f, 2.24f, 1}, new List<int>{3, 2, 6}),
        new Node<NodeData>(5, points[5], new NodeData(0,0,0), new List<float>{1.41f, 1.41f, 3.16f}, new List<int>{1, 2, 6}),
        new Node<NodeData>(6, points[6], new NodeData(0,0,0), new List<float>{1, 3.16f}, new List<int>{4, 5}),
    };

    // float[,] graph =
    // {
    //     { -1f, 2.0f, 2.83f, 3.16f, -1f, -1f, -1f },
    //     { 2.0f, -1f, 2.0f, -1f, -1f, 1.41f, -1f },
    //     { 2.83f, 2.0f, -1f, 1.41f, 2.24f, 1.41f, -1f },
    //     { 3.16f, -1f, 1.41f, -1f, 2.24f, -1f, -1f },
    //     { -1f, -1f, 2.24f, 2.24f, -1f, -1f, 1.0f },
    //     { -1f, 1.41f, 1.41f, -1f, -1f, -1f, 3.16f },
    //     { -1f, -1f, -1f, -1f, 1.0f, 3.16f, -1f }
    // };

    public static float[] openList = new float[aStarArr.Count]; //방문될 예정 node 리스트
    public static bool[] closedList = new bool[aStarArr.Count]; //방문한 node 리스트

    //astar
    public static void fit(ref List<Node<NodeData>> nodesArr, int startNode, int finishNode)
    {
        Node<NodeData> sNode = nodesArr[startNode];
        Node<NodeData> fNode = nodesArr[finishNode];
        PriorityQueue<Node<NodeData>> priorityQueue = new PriorityQueue<Node<NodeData>>();
        priorityQueue.Enqueue(sNode, (int)Math.Ceiling(heuristic(sNode.pos, fNode.pos)));

        while (priorityQueue.count > 0)
        {
            Node<NodeData> curNode = priorityQueue.Dequeue();
            if (closedList[curNode.idx]) continue;

            closedList[curNode.idx] = true; //처음 시작점은 방문된 리스트로 저장
            Console.WriteLine($"{curNode.idx}번 정점 방문");

            if (curNode.idx == finishNode) break;

            foreach (int connectNodes in curNode.connectNodes)
            {
                if (closedList[connectNodes]) continue; //방문된 경우 스킵

                Node<NodeData> connectedNode = nodesArr[connectNodes]; //연결된 노드

                connectedNode.data.g = euclidean(connectedNode.pos, fNode.pos);
                connectedNode.data.h = manhattan(connectedNode.pos, fNode.pos);
                connectedNode.data.f = heuristic(connectedNode.pos, fNode.pos);

                if (openList[connectNodes] < connectedNode.data.f) continue; //현재 예상 비용이 기록된 값보다 크다면 스킵

                openList[connectNodes] = connectedNode.data.f;
                priorityQueue.Enqueue(connectedNode, (int)Math.Ceiling(connectedNode.data.f));
            }
        }

        Console.WriteLine($"{startNode} 부터 {finishNode}까지 최단거리 {Math.Round(openList[finishNode], 2)}");
    }

    // heuristic f
    public static float heuristic(Vector2 a, Vector2 b)
    {
        return manhattan(a, b) + euclidean(a, b);
    }


    //멘하탄 거리로 목표지점까지 최소거리 h
    public static float manhattan(Vector2 a, Vector2 b)
    {
        int aX = (int)a.x;
        int aY = (int)a.y;
        int bX = (int)b.x;
        int bY = (int)b.y;

        return Math.Abs(aX - bX) + Math.Abs(aY - bY);
    }

    //유클리디안거리로 목표지점까지 최소거리 g
    public static float euclidean(Vector2 a, Vector2 b)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
}