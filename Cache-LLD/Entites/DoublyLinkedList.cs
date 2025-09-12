namespace Cache_LLD.Entites;

public class DoublyLinkedList<TKey>
{
    private readonly DoublyLinkedListNode<TKey> _head = new DoublyLinkedListNode<TKey>(default);
    private readonly DoublyLinkedListNode<TKey> _tail = new DoublyLinkedListNode<TKey>(default);
    private int _count;
    private readonly object sync = new object();
    

    public DoublyLinkedList()
    {
        _head.Next = _tail;
        _tail.Prev = _head;
        _count = 0;
    }

    public DoublyLinkedListNode<TKey>? First => _count > 0 ? _head.Next : null;

    public DoublyLinkedListNode<TKey>? Last => _count > 0 ? _tail.Prev : null;

    public DoublyLinkedListNode<TKey> AddFirst(DoublyLinkedListNode<TKey> node)
    {
        lock (sync) {
            var next = _head.Next;
            _head.Next = node;
            node.Prev = _head;

            node.Next = next;
            next.Prev = node;
            _count++;
        }
        return node;
    }

    public void Remove(DoublyLinkedListNode<TKey> node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (node.Prev == null || node.Next == null) return;

        lock (sync) {
            var prev = node.Prev;
            var next = node.Next;

            prev.Next = next;
            next.Prev = prev;
            _count--;
        }
    }
    
    
}