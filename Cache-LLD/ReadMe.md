
# Problem Statement

Design a cache system supporting the following operations:

- **Put:** Store a value for a key in the cache.
- **Get:** Retrieve a value by key from the cache.
- **Eviction:** Remove a key if the cache is full and a new key-value pair is added.

## Expectations

- Functionally correct code
- Modular, readable, and professional code
- Extensible and scalable design (easy to add new requirements)
- Good object-oriented design



---

## Locking Options in C#

C# provides several locking and synchronization primitives:


### 1. `lock` (Monitor)
- Most common, simple mutual exclusion for a code block.
- Best for short critical sections. For complex logic, prefer `Monitor.TryEnter` with a timeout to avoid long waits.

[Reference: DefaultLockingManager](./Locking/DefaultLockingManager.cs)
```csharp
public bool TryAcquireLock(TKey key, int millisecondsTimeout)
{
    var syncObj = GetSyncObject(key);
    bool isTaken = false;
    System.Threading.Monitor.TryEnter(syncObj, millisecondsTimeout, ref isTaken);
    return isTaken;
}
```


### 2. `ReaderWriterLockSlim`
- Allows multiple readers or a single writer.
- Useful for frequent reads and infrequent writes.
- Write lock waits for all read locks to be released before acquiring.


### 3. `Mutex`
- Can be used across processes, not just threads in the same process.
- Heavier than `lock`, but useful for inter-process synchronization.


### 4. `Semaphore` / `SemaphoreSlim`
- Controls access to a resource pool with a set number of slots.
- Useful for throttling or limiting concurrency.


### 5. `SpinLock`
- Lightweight lock for very short critical sections.
- Avoids context switches but can waste CPU if held too long.


### 6. `Monitor.TryEnter`
- Like `lock`, but allows timeouts and non-blocking attempts.


### 7. `Interlocked`
- For atomic operations on variables (increment, exchange, compare-and-swap).
- No blocking, but only for simple value types.


### 8. `ManualResetEvent` / `AutoResetEvent`
- For signaling between threads, not for mutual exclusion.


### 9. `Barrier`, `CountdownEvent`, etc.
- For more advanced thread coordination.

---


**Summary:**
- For most cache scenarios, `lock`, `ReaderWriterLockSlim`, or `Monitor.TryEnter` are practical.
- For advanced or scalable locking, consider partitioned locks or lock-free data structures.



# Design

## Cache Class
**Dependencies:**
- **Storage:** Stores key-value pairs.
- **EvictionPolicy:** Evicts key/value pairs as per the LRU algorithm.
- **Caching Capacity:** Number of values the cache can hold.

**Methods:**
- `Get(Key)`: Gets the value for a key from cache.
- `Put(Key, Value)`: Puts the key-value pair in cache.


## Storage
Uses [IStorage](./Storage/IStorage.cs) to abstract how/where data is stored,
allowing the cache to depend only on required operations and swap implementations without changes.
Implemented as a `ConcurrentDictionary` with generic types for key-value pairs.

- Must be a `ConcurrentDictionary` to prevent inconsistencies when multiple threads operate on it.
- Per-key locks prevent concurrent modification of the same key, but do not make the dictionary itself thread-safe.
- `ConcurrentDictionary` ensures thread safety for all dictionary operations, including adding/removing keys and enumerating.
- Use both per-key locks (for fine-grained control) and `ConcurrentDictionary` (for overall thread safety) in high-concurrency scenarios.

**Methods:**
- `Get`
- `Put`
- `Remove`


## Eviction Policy
Implemented using:
- **NodeList:** Maintains the order of key/value pairs; the node at the end is evicted if cache capacity is full.
- **ConcurrentDictionary:** Stores the Key/Node pair for quick node lookup.



---

## Concurrency Scenarios

To maintain consistency between `Storage` and `EvictionPolicy`, consider the following scenarios:

**Use Case 1:**
Requests at the same time:
- `ThreadA` => `GET(Key:1)`
- `ThreadB` => `PUT(Key:5)` (evicts `Key:1`)
`ThreadA` may throw a "key not found" exception. Synchronization is required. Since `Key:1` is the most recently accessed, it should not be evicted (strong consistency).

**Use Case 2:**
Requests at the same time:
- `ThreadA` => `PUT(Key:5)` (evicts `Key:1`)
- `ThreadB` => `PUT(Key:6)` (evicts `Key:1`)
Both threads try to evict the same key. One will throw an exception.

**Use Case 3:**
- `ThreadA` => `GET(Key:1)`
- `ThreadB` => `PUT(Key:5)`
Both try to insert their node as the first node in the list. This can make the `NodeList` inconsistent if not handled properly.

**Use Case 4:**
- `ThreadA` => `GET(Key:1)`
- `ThreadB` => `GET(Key:2)`
Reading different keys should not block each other, but `NodeList` needs synchronization as both threads try to insert their accessed node at the first position.

**Use Case 5:**
- `ThreadA` => `PUT(Key:5)`
- `ThreadB` => `PUT(Key:6)`
Writing different keys should not block each other, but `NodeList` needs synchronization as both threads try to insert their accessed node at the first position.

**Use Case 6:**
- `ThreadA` => `GET(Key:1)`
- `ThreadB` => `GET(Key:1)`
Reading the same key should be allowed simultaneously.

**Use Case 7:**
- `ThreadA` => `PUT(Key:5`, `Value:10)`
- `ThreadB` => `PUT(Key:5`, `Value:20)`
Writing the same key cannot be done simultaneously; these threads must be serialized for the `PUT` operation. `NodeList` also needs synchronization as both threads try to insert the accessed node at the first position.

----
Why we need `ConcurrentDictionary` for Storage and Key Map in LRU policy ?

- As these will be the shared resources where multiple threads might be reading or updating the values in the dictionary, so to keep the operations thread safe we have used `ConcurrentDictionary`.

---
How is Thread Safty Implemented and how did you sychronized the operation on different shared resources ?

- We have implemented `ReaderWriterLockingManager` class which has a static `ConcurrentDictionary` which maintains locks for all the keys stored in the cache.<br>
When a thread required to `GET/PUT` a value of `Key`, it first tries to `acquire` the lock for that key from `locks dictionary`. If its not present then that thread tries to create a new instance for `ReaderWriterLockSlim` object for the key and adds it to the locks dictionary.<br>
Once the thread gets the lock, it can read or write the key value accordingly based on the type of lock acquired. <br>
`Read Lock`: Its a shared lock, multiple threads can acquired a read lock for a key at a given time.<br>
`Write Lock`: There can only be one write lock for a key at a given time, once this lock is requested on a key, no more read lockes are issued and once all the existing read locks are released, then write lock is granted.
This same locks dictionary is used by `LRUEvictionPolicy` class when trying to evict the keys from `Key Map` dictionary.
So now all the operations are atomic, as locks are common b/w two shared resources.