This is a proof of concept for Host Migration in Mirror.

I am not the best programmer, so it might not be perfect, but i believe it can be upgraded or fixed for your own needs.

This works by when a new client joins, the server chooses the next host as backup and sends it to each client to store.
Once the host leaves, each client will store their own data, like position, rotation, healt, and set it themselves, you may want to make that server authorative but for this experiment i did not care.
The next host will start a new server, and a little bit after each client will then attempt to join.
