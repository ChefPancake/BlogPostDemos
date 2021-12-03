namespace WarCardGame

module Common =
    let tupleFirst (x, _) = x
    let tupleMapFirst f (x, y) = (f x, y)
    let tupleSecond (_, x) = x
    let tupleMapSecond f (x, y) = (x, f y)
    let tupleApply f (x, y) = f x y
    let tupleListSplit (items: ('a * 'b) list): ('a list * 'b list) = 
        let firsts = List.map tupleFirst items
        let seconds = List.map tupleSecond items
        (firsts, seconds)

    let rec chain (f: 'T -> 'T) (init: 'T): seq<'T> =
        seq {
            let next = f init
            yield next
            yield! chain f next
        }

    let takeWhileIncl 
            (predicate: 'T -> bool) 
            (items: seq<'T>)
            : seq<'T> =
        let head = Seq.head items
        Seq.pairwise items
        |> Seq.takeWhile (tupleFirst >> predicate)
        |> Seq.map tupleSecond
        |> Seq.append [head]

    type NonEmptyList<'T> = {
        Head: 'T
        Tail: 'T list
    }
    module NonEmptyList =
        let private fromListUnsafe l = {
            NonEmptyList.Head = List.head l
            NonEmptyList.Tail = List.tail l
        }
            
        let tryCreate items = 
            List.tryHead items
            |> Option.map
                (fun h -> { 
                    NonEmptyList.Head = h
                    NonEmptyList.Tail = List.tail items
                })
        let toList nonEmptyList =
            [nonEmptyList.Head] @ nonEmptyList.Tail

        let map f nonEmptyList = {
                NonEmptyList.Head = f nonEmptyList.Head
                NonEmptyList.Tail = List.map f nonEmptyList.Tail
            }

        let max projection nonEmptyList =
            toList nonEmptyList
            |> List.map projection
            |> List.max

        let maxBy projection nonEmptyList =
            toList nonEmptyList
            |> List.maxBy projection

        let sortBy keySelect nonEmptyList = 
            toList nonEmptyList
            |> List.sortBy keySelect
            |> fromListUnsafe

        let sortByDescending keySelect nonEmptyList = 
            toList nonEmptyList
            |> List.sortByDescending keySelect
            |> fromListUnsafe

        let groupBy keySelect nonEmptyList =
            toList nonEmptyList
            |> List.groupBy keySelect
            |> fromListUnsafe
            |> map (tupleMapSecond fromListUnsafe)

        let choose predicate nonEmptyList =
            toList nonEmptyList
            |> List.choose predicate

        let join innerKeySelector outerKeySelector inner outer =
            seq {
                for outer in (toList outer |> List.toSeq) do
                for inner in (List.toSeq inner) do
                    if outerKeySelector outer = innerKeySelector inner then
                        yield (inner, outer)
            }            

        let length nonEmptyList =
            List.length nonEmptyList.Tail
            |> (+) 1

        let append second first = {
            NonEmptyList.Head = first.Head
            NonEmptyList.Tail = first.Tail @ (toList second) }

        let head nonEmptyList = nonEmptyList.Head
        let tail nonEmptyList = nonEmptyList.Tail

    let tupleNonEmptyListSplit (items: NonEmptyList<('a * 'b)>): (NonEmptyList<'a> * NonEmptyList<'b>) = 
        let firsts = NonEmptyList.map tupleFirst items
        let seconds = NonEmptyList.map tupleSecond items
        (firsts, seconds)

module Components =
    open Common
    open System

    type PlayerId = PlayerId of Guid
    module PlayerId =
        let create () =
            Guid.NewGuid ()
            |> PlayerId

    type Card = Card of uint
    module Card =
        let value (Card value) = value

    type PlayedCard = {
        Owner: PlayerId
        Value: uint
    }
    module PlayedCard =
        let create owner value =
            {
                PlayedCard.Owner = owner
                PlayedCard.Value = value
            }
        let value card = card.Value
        let owner card = card.Owner
        let asCard card = Card card.Value

    type Deck = {
        Cards: NonEmptyList<Card>
        Owner: PlayerId
    }
    module Deck =
        let create owner cards = {
            Deck.Cards = cards
            Deck.Owner = owner
        }

        let newDeck cards = 
            create (PlayerId.create ()) cards

        let draw deck: (PlayedCard * Card list ) =
            let playedCard = {
                PlayedCard.Owner = deck.Owner
                PlayedCard.Value = Card.value deck.Cards.Head
            }
            let newDeck =
                NonEmptyList.tryCreate deck.Cards.Tail
                |> Option.map (create deck.Owner)
            (playedCard, deck.Cards.Tail)
    
    type PlayerState = 
        | ActivePlayer of Deck
        | KnockedOutPlayer of PlayerId
    module PlayerState =
        let mapActive f state =
            match state with
            | ActivePlayer a -> f a |> ActivePlayer
            | KnockedOutPlayer id -> KnockedOutPlayer id

        let bindActive f state =
            match state with
            | ActivePlayer a -> f a
            | KnockedOutPlayer id -> KnockedOutPlayer id

        let playerId state = 
            match state with
            | ActivePlayer ap -> ap.Owner
            | KnockedOutPlayer ko -> ko

    type GameState = 
        | RunningGame of PlayerState list
        | CompletedGame of PlayerId list
    module GameState =
        let isRunning state =
            match state with
            | RunningGame players -> true
            | CompletedGame winners -> false

        let mapRunning f state =
            match state with
            | RunningGame players -> f players
            | CompletedGame winners -> CompletedGame winners

    type AdvanceGameState = 
        GameState -> GameState

    type PlayerChooseCardOrder =
        PlayerId -> NonEmptyList<Card> -> NonEmptyList<Card>

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
