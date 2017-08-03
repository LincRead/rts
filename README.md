# rts
Experimenting with squad-based game mechanics for an RTS to run on mobile platforms.

My approach on using advanced steering behaviour combined with fixed-point arithmetics for lockstep-multiplayer, proved to be a performance-breaker on mobile devices. The aim was to allow for 30 units per squad, with up to 8 sqauds on the battlefield. 

One example of a performance bottleneck that I introduced was preventing units from walking through each other, both ally and enemy units.

The steering behaviour required to keep this rule intact proved to be too performance-heavy for lower-tier mobile devices, especially when using fixed-point numbers.

The behaviours that were implemented looks really fantastic though.
