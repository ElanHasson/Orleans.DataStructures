# Orleans.DataStructures

A project to provide implementations of durable, distributed data structures that run on top of Microsoft Orleans.


## Array
### Capabilities
* Add items to the array.
* Read the value at an index of the array (`ArrayItemGrain`)
* Get the size of the array

### What's Next
* Remove Items
* Insert Items
* `IAsyncEnumerable` support to enumerate them (depends on https://github.com/dotnet/orleans/issues/6504) 
* Allow Reentrancy (need to implement atomic operations on the `ArrayGrain`
* Performance Enhancements
* Change notification on `ArrayGrain` and `ArrayItemGrain` to support Observers.


