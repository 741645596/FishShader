using System;
using UnityEngine;

public class QueueBuffer
{
    private byte[] array;
    private int front;
    private int rear;
    private int capacity;
    private int length;
    public int Length { get => length; }

    public QueueBuffer(int capacity)
    {
        this.array = new byte[capacity];
        this.capacity = capacity;
        this.front = 0;
        this.rear = 0;
    }

    public void Enqueue(byte[] buffer, int size)
    {
        if(buffer == null || buffer.Length < size)
        {
            throw new Exception("CircularQueue.Enqueue buffer size error.");
        }
        if (size + length > capacity)
        {
            throw new Exception("CircularQueue.Enqueue queue is full.");
        }
        if (rear + size <= capacity)
        {
            Buffer.BlockCopy(buffer, 0, array, rear, size);
        }
        else
        {
            var count1 = capacity - rear;
            Buffer.BlockCopy(buffer, 0, array, rear, count1);
            var count2 = size - count1;
            Buffer.BlockCopy(buffer, count1, array, 0, count2);
        }
        rear = (rear + size) % capacity;
        length += size;
    }

    public void Dequeue(byte[] buffer, int size)
    {
        if (buffer == null || buffer.Length < size || length < size)
        {
            throw new Exception("CircularQueue.Dequeue buffer size error.");
        }

        if (front + size <= capacity)
        {
            Buffer.BlockCopy(array, front, buffer, 0, size);
        }
        else
        {
            var count1 = capacity - front;
            Buffer.BlockCopy(array, front, buffer, 0, count1);
            var count2 = size - count1;
            Buffer.BlockCopy(array, 0, buffer, count1, count2);
        }
        front = (front + size) % capacity;
        length -= size;
    }

    public void Copy(byte[] buffer, int offset, int count)
    {
        if(count > length)
        {
            throw new Exception("CircularQueue.Copy buffer size error.");
        }

        var startIndex = front + offset;
        if(startIndex + count <= capacity)
        {
            Buffer.BlockCopy(array, startIndex, buffer, 0, count);
        }
        else
        {
            var count1 = capacity - startIndex;
            Buffer.BlockCopy(array, startIndex, buffer, 0, count1);
            var count2 = count - count1;
            Buffer.BlockCopy(array, 0, buffer, count1, count2);
        }
    }

    public void Info()
    {
        LogUtils.I($"front:{front} rear:{rear} count:{length}");
    }
}
