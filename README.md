# rts
Experimenting with squad-based game mechanics for an RTS to run on mobile platforms.

My approach on using advanced steering behaviour combined with fixed-point arithmetics for lockstep-multiplayer, proved to be a performance-breaker on mobile devices. Especially since the aim was to allow for 30 units per squad, with up to 8 sqauds. 

One example of a bottleneck I encountered, was calculating linear distance between units using fixed-point numbers.

The behaviours that were implemented looked really fantastic though.
