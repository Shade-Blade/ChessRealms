# Chess Realms
Very rough prototype of a big chess variant game with a lot of pieces.

# Chess Engine Information
It is probably very inefficient by chess engine standards though I don't know enough to optimize it much more than it already is. The perft test runs at ~200k nodes per second and the engine itself usually runs to depth 3-4 in the 4 seconds per move I give it. It's at a point where it isn't making obviously stupid moves but it also probably doesn't play very well. I can usually beat it and I'm not that good at chess (~1000 chess.com elo), though I can't tell how much of that is that the engine is low depth vs the engine is flawed

## Search features
- Transposition table with zobrist hashes (though this is less efficient as the last move is part of the board state)
	- Note that the last move can have an effect on the current position due to certain special pieces or restrictions (Arcana Fool piece copies your last moved piece for example)
- Iterative deepening
- Alpha beta search
- Killer moves
- Move ordering
- Quiesence search (Note that it searches any move that causes a material loss for the enemy instead of just straight captures)

## Evaluation terms
- Piece value (Bonus for special modifiers in the Modifier Test, also a small bonus for pieces that use "charges" for each of those charges)
	- Also a small bonus for Crystal pieces you control
- Endgame phase detection (based on piece values remaining on both sides)
- Piece square tables (Pawns / anything that can promote get a bias towards going upwards that gets larger near endgame)
	- King is biased to be near the corners (or near the top center in endgame)
- Castling bonus (if you castled you get a small bonus)
- King safety (Attempts to keep pawnlike pieces in front of the king if the king is near the corner, as well as keeping powerful enemy pieces out)
	- Not sure this works in practice, some self play tests show the king with several enemy pieces relatively close by (Though they were 3 away while the current king safety check only checks 2 away)
- Passed pawns
- Endgame king closeness (in a late endgame the evaluation gets a multiplier so the winning side is enticed to move the kings closer together?)
	- Not sure this works in practice? Self play games often see one king left in the corner while the other pieces do stuff

## Special rules
- Castling is very loose because it would be a lot of annoying complexity to implement (Only requires King to be 3 away from an ally non-pawn horizontally, can castle out of check or through check)
	- (Note that detecting check is not simple in practice due to the existence of explosion radii, pieces potentially disabling attacking pieces, etc)
- Pawns only promote to Queens (I didn't really want to deal with multiple moves between the same squares, also the various pawnlike pieces promote to specific pieces too)
- Also no en passant because that would be annoying to code too (plus would be weird to implement for all the weird pawnlike pieces with different kinds of moves)
- King on the back rank is a win
- Bare king is a win

## Special mechanics
- Crystal pieces: These pieces can be moved by whichever side is attacking them.
- Neutral pieces: Can be moved by either side.

## Piece classes / armies
There are about 324 pieces, with many having unique magical abilities or movements (different kinds of teleports and such). Look at the piece data table or the piece code file to see the list of them.