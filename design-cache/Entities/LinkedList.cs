namespace design_cache.Entities;

public class LinkedList<TValue>
{
    public LinkedListNode<TValue> Head { get; set; }
    public LinkedListNode<TValue> Tail { get; set; }

    public LinkedList()
    {
        this.Head = new LinkedListNode<TValue>();
        this.Tail = new LinkedListNode<TValue>();
        
        this.Head.Next = this.Tail;
        this.Tail.Previous = this.Head;
    }

    public void Add(CacheEntry<string, TValue> entry)
    {
        var next = this.Head.Next;

        this.Head.Next = new LinkedListNode<TValue>(entry);
        next.Previous = this.Head.Next;

        this.Head.Next.Next = next;
    }
    
    public void Remove(LinkedListNode<TValue> node){}
    
    
}

public class LinkedListNode<TValue>
{
    public LinkedListNode(CacheEntry<string, TValue>? value = null, LinkedListNode<TValue>? next = null, LinkedListNode<TValue>? previous = null)
    {
        Next = next;
        Previous = previous;
        Value = value;
    }

    public LinkedListNode<TValue>? Next { get; set; }
    public LinkedListNode<TValue>? Previous { get; set; }
    public CacheEntry<string, TValue>? Value { get; set; }
}