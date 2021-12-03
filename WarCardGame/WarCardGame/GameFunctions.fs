namespace WarCardGame
module GameFunctions =
    open Components
    open Common
    open RoundFunctions

    let resolveGame 
            (resolveRound: AdvanceGameState) 
            (init: GameState)
            : GameState =
        chain resolveRound init
        |> takeWhileIncl GameState.isRunning
        |> Seq.last 

    let resolveWarGame 
            (chooseWinningsOrder: PlayerChooseCardOrder)
            (init: GameState)
            : GameState =
         resolveGame
            (resolveRound chooseWinningsOrder)
            init

