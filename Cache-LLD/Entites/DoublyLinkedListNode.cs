namespace Cache_LLD.Entites;

public class DoublyLinkedListNode<TKey>
{
    public DoublyLinkedListNode<TKey> Prev;
    public DoublyLinkedListNode<TKey> Next;

    public DoublyLinkedListNode(TKey key)
    {
        Key = key;
    }

    public TKey Key { get; }
}