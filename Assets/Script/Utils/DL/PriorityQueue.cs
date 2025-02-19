using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private class Node
    {
        public T data { get; private set; }
        public int priority { get; set; } = 0; // 기본 우선순위는 0

        public Node(T data, int priority)
        {
            this.data = data;
            this.priority = priority;
        }
    }

    private List<Node> nodes = new List<Node>();

    public int count => nodes.Count; //It's not pointer. It's Ramda exp.

    public void Enqueue(T data, int priority)
    {
        Node node = new Node(data, priority);

        if (count == 0)
        {
            nodes.Add(node);
        }
        else
        {
            int end = nodes.Count - 1;
            int start = 0;

            int half = 0;

            while (start != end)
            {
                if (end - start == 1) //근처일 때
                {
                    if (nodes[start].priority < priority) half = end;
                    if (nodes[start].priority > priority) half = start;

                    break;
                }
                else
                {
                    half = start + ((end - start) / 2); //처음과 중간 사이

                    if (nodes[half].priority > priority) end = half; //Down
                    if (nodes[half].priority < priority) start = half; //Up
                }
            }

            if (nodes[half].priority > priority) nodes.Insert(half, node);
            if (nodes[half].priority < priority) nodes.Insert(half + 1, node);
        }
    }

    public T Dequeue()
    {
        Node tail = null;

        if (count > 0)
        {
            tail = nodes[nodes.Count - 1];
            nodes.RemoveAt(nodes.Count - 1);
        }

        if (tail != null)
        {
            return tail.data;
        }

        return default(T);
    }

}