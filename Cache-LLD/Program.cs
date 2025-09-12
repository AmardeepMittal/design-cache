using Cache_LLD;
using Cache_LLD.Locking;
using Cache_LLD.Logger;
using Cache_LLD.Policies;
using Cache_LLD.Storage;
// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");
var lockingManager = new ReaderWriterLockingManager<int>();
IEvictionPolicy<int> policy = new LruEvictionPolicy<int>();
DictionaryStorage<int, int> dictionaryStorage = new DictionaryStorage<int, int>();
ILog log = new ConsoleLogger();
Cache<int, int> cache = new Cache<int, int>(dictionaryStorage, policy, log, 50);

//cache.Put(1, 100);
//cache.Put(2, 200);
//var task4 = Task.Run(() => { cache.Put(3, 300); }); //Lru: 3,2,1
//var task5 = Task.Run(() => { cache.Get(1); });      //Lru: 1,3,2
//var task6 = Task.Run(() => { cache.Put(4, 300); }); //Lru: 4,1,3,2x
//var task7 = Task.Run(() => { cache.Get(3); });      //Lru: 3,4,1
//var task8 = Task.Run(() => { cache.Put(5, 300); }); //Lru: 5,3,4,1x
//var task9 = Task.Run(() => { cache.Get(4); });      //Lru: 4,5,3
//var task0 = Task.Run(() => { cache.Get(4); });      //Lru: 4,5,3


//cache.Put(2, 200);
//cache.Put(3, 300);
//cache.Get(1);
//cache.Put(2, 200);
//cache.Put(3, 300);
//cache.Put(4, 401);
//cache.Put(5, 501);
//cache.Put(6, 601);

Random rnd = new Random(1);

DateTime start = DateTime.UtcNow;
for (int i = 1; i < 500; i++)
{
    var task2 = Task.Run(() =>
    {
        int key = rnd.Next(1, 50);
        cache.Put(key, key + 1);
        log.Log($"Value for Key:{key} is updated to {key + 1}");
    });

    var task1 = Task.Run(() =>
    {
        int key = rnd.Next(1, 50);
        log.Log($"Value for Key:{key} is {cache.Get(key)}");
    });
}
Console.WriteLine($"Time spent: {DateTime.UtcNow.Subtract(start).TotalSeconds}");


//var task1 = Task.Run(() => {
//    cache.Put(7, 701);
//    log.Log($"Key: 7, Value: 701");
//});

//var task2 = Task.Run(() => {
//    cache.Put(8, 801);
//    log.Log($"Key: 8, Value: 801");
//});

//var task3 = Task.Run(() =>
//{
//    cache.Put(9, 901);
//    log.Log($"Key: 9, Value: {cache.Get(9)}");
//});

//var task4 = Task.Run(() =>
//{
//    cache.Put(10, 1001);
//    log.Log($"Key: 10, Value: {cache.Get(10)}");
//});

//var task5 = Task.Run(() =>
//{
//    cache.Put(7, 702);
//    log.Log($"Key: 7, Value: {cache.Get(7)}");
//});

//var task6 = Task.Run(() =>
//{
//    cache.Put(8, 802);
//    log.Log($"Key: 8, Value: {cache.Get(8)}");
//});

//var task7 = Task.Run(() =>
//{
//    cache.Put(10, 1002);
//    log.Log($"Key: 10, Value: {cache.Get(10)}");
//});

//var task8 = Task.Run(() => {
//    cache.Put(7, 703);
//    log.Log($"Key: 7, Value: 701");
//});

//var task9 = Task.Run(() => {
//    var value = cache.Get(1);
//    log.Log($"Key: 1, Value: {value}");
//});


//for (int i = 0; i < 20000; i++)
//{
//    int key = rnd.Next(1, 10000);
//    cache.Put(key, key);
//}

//DateTime start = DateTime.UtcNow;
//for (int i = 0; i < 10000000; i++)
//{
//    int key = rnd.Next(1,10000);
//    cache.Put(key, i);
//    key = cache.Get(key);
//    //Console.WriteLine(key);
//}
//Console.WriteLine($"Time spent: {DateTime.UtcNow.Subtract(start).TotalSeconds}");

Console.ReadLine();




