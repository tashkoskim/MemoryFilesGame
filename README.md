# Memory Files Game

A Windows Explorer-based memory game: match pairs by opening file shortcuts instead of cards in a traditional game window.
![MemoryFilesGame Screenshot](MemoryFilesGame_Screenshot.png) 

## How it works

1. The game selects expressive Unicode symbols (such as ☺, ♥, ♛, ♣, ...) and generates an SVG source, PNG image, and ICO icon for each pair.
2. Each pair is represented by two randomly named Windows shortcuts in `%TEMP%\MemoryFilesGame`.
3. All shortcuts initially use the same question-mark icon. Opening a shortcut launches a lightweight handler that reveals its symbol by changing its icon. Matching pairs remain revealed; mismatched pairs are flipped back.
4. The handler exits after each action—there is no always-running background process. Game state is stored in JSON, while generated images, SVG sources, and icons are kept separately under `%LOCALAPPDATA%\MemoryFilesGame`.

When all pairs are matched, the game displays the completion time and number of moves, with options to play again or exit.

## GIF Demo
![MemoryFilesGame Example](MemoryFilesGame_Example.gif) 


## Authors
- tashkoskim@yahoo.com
