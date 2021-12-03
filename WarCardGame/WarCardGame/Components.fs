namespace WarCardGame
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