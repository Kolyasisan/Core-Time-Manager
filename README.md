# Core-Time-Manager

Obsolete since Unity 2020.2 finally includes much needed Delta Time fixes

A custom Time Manager for Unity that has some neat little features.

NOTE: Requires Core Update Manager to work (https://github.com/Kolyasisan/Core-Update-Manager)

Core Time Manager is a custom Time Manager for Unity. It behaves a lot like Time class in Unity, but with new parameters. Of a primary note, there are Optimal variables (like optimalDeltaTime), which are filtered on the account of having potential Heartbeat Stutters, which can substantially help with the problem (in short, Heartbeat Stutter is a notorious hardware problem that occasionally makes the game think that the frametime wildly spikes up and down, which results in visible stutter due to deltaTime scaling while no actual frame drops occured).

Read ```the wiki``` for instructions.

This project is distributed under the ```Beerware License```  
![Beer](https://habrastorage.org/getpro/geektimes/post_images/78f/720/c75/78f720c75de7b8828353bc0cf8a254c4.png)
