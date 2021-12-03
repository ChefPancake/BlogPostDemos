namespace WarCardGame
module GameSetup =
    open System
    open Common
    open Components

    let rand = new Random ()

    let private shuffle (r : Random) = 
        List.sortBy (fun _ -> r.Next())

    let standardDeck =
        List.replicate 4 [2u..14u]
        |> List.concat
        |> List.map Card

    let shuffleDeck cards =
        shuffle rand cards

    let tryDealCardsToPlayers playerCount cards =
        let newPlayers =
            List.mapi (fun i card -> (i % playerCount, card)) cards
            |> List.groupBy tupleFirst
            |> List.map (
                tupleSecond 
                >> (List.map tupleSecond) 
                >> NonEmptyList.tryCreate)
            |> List.choose id
            |> List.map (Deck.newDeck >> ActivePlayer)
        match List.length newPlayers with
        | x when x < playerCount ->
            Error "Not enough cards"
        | _ -> Ok newPlayers

    let trySetupGame playerCount =
        standardDeck 
        |> shuffleDeck
        |> tryDealCardsToPlayers playerCount
        |> Result.map GameState.RunningGame 
