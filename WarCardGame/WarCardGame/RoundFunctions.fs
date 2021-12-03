namespace WarCardGame
module RoundFunctions =
    open Common
    open Components

    type WinningsResult = {
        Winner: PlayerId
        Winnings: NonEmptyList<Card>
        Decks: (PlayerId * Card list) list
    }
    type RoundResult = 
        | DecisiveResult of WinningsResult
        | Draw of PlayerId list

    let private cardsWithHighestValue cards =
        let highestValue =
            NonEmptyList.max PlayedCard.value cards
        NonEmptyList.choose 
            (fun pc -> 
                match pc.Value = highestValue with
                | true -> Some pc
                | false -> None)
            cards

    let private drawResultFromPlayedCards cards =
        NonEmptyList.map 
            (fun (x: PlayedCard) -> x.Owner) 
            cards
        |> NonEmptyList.toList
        |> RoundResult.Draw

    let resolveTie decks winnings cardsToCompare drawAgain =
        let decksWithCardsRemaining = 
            List.choose 
                (fun (id, cs) -> 
                    match List.length cs with 
                    | 0 -> None
                    | _ -> Some (id, cs)) 
                decks
            |> NonEmptyList.tryCreate
        match decksWithCardsRemaining with
        | None -> 
            drawResultFromPlayedCards cardsToCompare
        | Some x when NonEmptyList.length x = 1 -> 
            {
                WinningsResult.Winner = tupleFirst x.Head
                WinningsResult.Decks = decks
                WinningsResult.Winnings = winnings }
            |> RoundResult.DecisiveResult
        | Some x -> 
            let nextCards = 
                NonEmptyList.map 
                    (tupleMapSecond 
                        (List.head 
                        >> Card.value)) 
                    x
                |> NonEmptyList.map 
                    (tupleApply PlayedCard.create)
            let nextDecks =
                List.map 
                    (tupleMapSecond List.tail) 
                    decks
            drawAgain
                nextCards
                (NonEmptyList.toList winnings)
                nextDecks

    let rec resolveCardDrawWithWinnings
            (cardsToCompare: NonEmptyList<PlayedCard>)
            (winnings: Card list)
            (decks: (PlayerId * Card list) list)
            : RoundResult =
        let cardsWithHighestValue = 
            cardsWithHighestValue cardsToCompare
        let winnings = 
            NonEmptyList.tryCreate winnings
            |> fun x -> 
                match x with 
                | None -> NonEmptyList.map PlayedCard.asCard cardsToCompare
                | Some x -> 
                    NonEmptyList.map PlayedCard.asCard cardsToCompare
                    |> NonEmptyList.append x
        match List.length cardsWithHighestValue with
        | 0 -> failwith "unreachable code hit"
        | 1 -> 
            {
                Decks = decks
                Winner = cardsWithHighestValue.Head.Owner
                Winnings = winnings
            }
            |> RoundResult.DecisiveResult
        | _ -> 
            resolveTie 
                decks 
                winnings
                cardsToCompare 
                resolveCardDrawWithWinnings
       
    let addWinningsToDeck 
            (chooseOrderOfCards: PlayerChooseCardOrder)
            (winningDeck: (PlayerId * Card list)) 
            (winnings: NonEmptyList<Card>)
            : Deck =
        let playerToChoose = tupleFirst winningDeck
        let orderedWinnings = 
            chooseOrderOfCards 
                playerToChoose 
                winnings
        let newCards = 
            List.append 
                (tupleSecond winningDeck)
                (NonEmptyList.toList orderedWinnings)
            |> NonEmptyList.tryCreate
            |> Option.get
        { Deck.Owner = playerToChoose; Deck.Cards = newCards }

    let private applyPlayerToCardList (playedCardAndDeck: (PlayedCard * Card list)) =
        let playerId = 
            tupleFirst playedCardAndDeck
            |> PlayedCard.owner
        tupleMapSecond (fun x -> (playerId, x)) playedCardAndDeck

    let private drawCardsFromDecks =
        (NonEmptyList.map Deck.draw)
        >> (NonEmptyList.map applyPlayerToCardList)
        >> tupleNonEmptyListSplit

    let private getResult decksToDrawFrom = 
        let (cards, decks) = 
            drawCardsFromDecks decksToDrawFrom
        resolveCardDrawWithWinnings 
            cards
            []
            (NonEmptyList.toList decks)

    let gameStateFromDecisiveResult chooseWinningsOrder result =
        let winningPlayerIndex =
            List.map tupleFirst result.Decks
            |> List.findIndex ((=) result.Winner)    
        let winningDeck =
            List.find (fun (id, _) -> id = result.Winner) result.Decks
        let deckWithWinnings =
            addWinningsToDeck
                chooseWinningsOrder
                winningDeck
                result.Winnings
        List.map (tupleMapSecond NonEmptyList.tryCreate) result.Decks
        |> List.map 
            (fun (id, l) -> 
                match l with
                | Some l -> 
                    Deck.create id l
                    |> ActivePlayer
                | None -> KnockedOutPlayer id)
        |> List.updateAt winningPlayerIndex (deckWithWinnings |> ActivePlayer)
        |> GameState.RunningGame

    let resolveRunningRound 
            (chooseWinningsOrder: PlayerChooseCardOrder)
            (players: PlayerState list)
            : GameState =
        let decks = 
            List.choose 
                (fun x -> 
                    match x with 
                    | ActivePlayer deck -> Some deck
                    | KnockedOutPlayer _ -> None) 
                players
            |> NonEmptyList.tryCreate
        match Option.map getResult decks with
        | None -> 
            List.map PlayerState.playerId players       
            |> GameState.CompletedGame
        | Some result ->
            match result with
            | RoundResult.Draw winners -> 
                GameState.CompletedGame winners
            | RoundResult.DecisiveResult result ->
                gameStateFromDecisiveResult 
                    chooseWinningsOrder 
                    result

    let resolveRound
            (chooseWinningsOrder: PlayerChooseCardOrder)
            (gameState: GameState)
            : GameState =
        GameState.mapRunning
            (resolveRunningRound chooseWinningsOrder)
            gameState