# Design Cache

## Requirements for the Cache

- **R1** - User should be able to cache any type of data as a key/value pair
- **R2** - User should be able to retrieve data with very low latency (<10ms)
- **R3** - Cache should have an eviction policy
- **R5** - Different eviction strategies:
    - Least Recently Used (LRU)
    - Least Frequently Used (LFU)
    - TTL (Time to Live) / Expiry

The interviewer may ask to implement the cache and discuss how to ensure concurrency so that if multiple threads are trying to update the same key, the value remains consistent.

> **Note:** Usage of `ConcurrentDictionary` is not allowed, as it already handles concurrency. You must implement your own version.

---

## Low Level Design

The LRU design will be followed for the cache.

1. Use a `Dictionary` to hold all the key-value pairs, where the key is a string and the value is a doubly linked list.
2. The value in each linked list node will be a `CacheEntry<TKey, TValue>`.

---

## How the Design Handles Concurrency

### Write Operation

1. Maintain another `Dictionary<string, ReaderWriterLockSlim>` called `_locks` to hold all the locks on a per-key basis.
2. When a request comes to write a key-value pair:
    - Lock the `_locks` dictionary.
    - Get the `ReaderWriterLockSlim` lock object for that key; if not present, add it and return the lock object.
    - Call `rwLock.EnterWriteLock()`.
    - Perform the write operation.
    - In a `finally` block, release the lock by calling `rwLock.ExitWriteLock()`.

### Read Operation

1. When a request comes to read a key-value pair:
    - Lock the `_locks` dictionary.
    - Get the `ReaderWriterLockSlim` lock object for that key; if not present, add it and return the lock object.
    - Call `rwLock.EnterReadLock()`.
    - Perform the read operation.
    - In a `finally` block, release the lock by calling `rwLock.ExitReadLock()`.

This ensures better concurrency, as more than one thread can read the value concurrently.

---

## Important

If you are reading the value of a key from the cache and another thread tries to write/update the value for the same key, the writing thread will wait at `EnterWriteLock()` to acquire the lock on the key. No new read locks are issued until the write request is executed. This ensures that the cache dictionary is fully thread-safe, as only one thread is reading or writing for a given key at any time.












