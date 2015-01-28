The Turk is chess engine that you can play under Winboard protocol supported user interfaces. The Turk has been written in Object Oriented design instead of traditional functional chess engines. Object Oriented Design has been choosen to simplify complexity of project for help others to understand and read code more easily.

Name of chess engine comes from chess Automaton in history. Detailed info is [here](http://en.wikipedia.org/wiki/The_Turk)

# İmplemented Features

* Winboard protocol support
* [8,8] 2 dimensional array for board representation (none-zero based)
* Fen notation support
* AlphaBeta Algorithim
* Quiescence Search with MVV/LVA Sorting
* Iterative Deepening
* Aspiration Windows
* Null Move Pruning
* Chess clock
* Late Move Reduction pruning
* History and Killer moves
* Principal Variation Search
* Zobrist Keys
* 3 fold repetetion rule

# Roadmap

- [ ] Support for UCI protocol
- [ ] More Winboard commands support
- [x] Chess clock
- [x] Iterative Deepening
- [x] Aspiration Windows
- [x] Principal Variation Search
- [x] Late Move Reduction pruning
- [x] Null Move Pruning
- [ ] Static Exchange Evaluation
- [x] History and Killer moves
- [ ] Transposition Tables
- [ ] Advanced Evaluation
- [ ] Tuning for Null move and Late move pruning
- [x] Zobrist Keys
- [x] 3 fold repetetion rule

# Requirements

* .NET framework 4.0
* User interface which support Winboard protocol. Such as [Arena](http://www.playwitharena.com/)
