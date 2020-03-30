Since we don't have nice UI in there yet attached to any functionality for playing, here's how to playtest:

- Use controllers to control player
- Play SavingTest scene in Unity (don't build)
- Adjust Cup Settings on the MatchManager prefab to your liking (# players, # of rounds)
- Once ready, click play and press Alpha4 (4 on the number bar above all the letters on your keyboard)

It's going to go to Level Select, but automatically select level 1 after a second (I put a delay to show that the flow works). Once there, you can play normally. Once someone dies, the round will end and it'll go back to level select. This'll repeat until all rounds are played. 

Once all rounds are played, it'll bring you to GameEnd scene (incomplete). You can play again by pressing Alpha4.

All the save data is in the saves folder in the project. If you open it up, there's a bunch of info for each round, but you can minimize the rounds array section to see the important info.